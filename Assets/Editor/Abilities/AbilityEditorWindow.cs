using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// Scaffolds new abilities and edits ability stats on items. The ability system here is
// code-based, not data-driven (see AbilityObject.cs / ActivatableAbility.cs / PlayerAbilityManager.cs) -
// a new ability normally requires hand-writing a script pair AND manually adding matching
// entries to the ActivatableAbilityType enum and the abilities dictionary in
// PlayerAbilityManager.cs. This window automates that wiring so the two never drift apart.
public class AbilityEditorWindow : EditorWindow
{
    const string AbilityManagerPath = "Assets/Scripts/Player/PlayerAbilityManager.cs";
    const string ScriptsRoot = "Assets/Scripts/Abilities";
    const string PrefabsRoot = "Assets/Resources/Prefabs/Abilities";

    int _tab;
    readonly string[] _tabLabels = { "Create New Ability", "Item Ability Stats" };

    // --- Create tab state ---
    string _displayName = "";
    float _cooldown = 1.5f;
    bool _createInstanceScript = true;
    float _duration = 1f;
    float _speed = 1f;
    float _damage = 10f;
    float _pierce = 1f;
    float _knockback = 0.5f;
    float _tickRate = 0.1f;

    // --- Item stats tab state ---
    Vector2 _itemsScroll;
    ItemObject[] _items = Array.Empty<ItemObject>();

    [MenuItem("Tools/Abilities/Ability Editor")]
    static void Open()
    {
        var window = GetWindow<AbilityEditorWindow>("Ability Editor");
        window.minSize = new Vector2(440, 380);
    }

    void OnEnable()
    {
        RefreshItems();
    }

    void OnGUI()
    {
        _tab = GUILayout.Toolbar(_tab, _tabLabels);
        EditorGUILayout.Space(8);

        if (_tab == 0)
            DrawCreateTab();
        else
            DrawItemStatsTab();
    }

    // ---------------------------------------------------------------
    // Create New Ability
    // ---------------------------------------------------------------

    void DrawCreateTab()
    {
        EditorGUILayout.HelpBox(
            "Scaffolds a new ability: an ActivatableAbility subclass (caster side) and, " +
            "optionally, an AbilityObject subclass (the spawned projectile/AoE), then registers the " +
            "new ActivatableAbilityType enum entry and PlayerAbilityManager.abilities dictionary " +
            "entry automatically. You still need to build the prefab and fill in the ability's " +
            "actual behaviour (targeting, VFX, on-hit effects).",
            MessageType.Info);
        EditorGUILayout.Space(6);

        _displayName = EditorGUILayout.TextField(
            new GUIContent("Ability Name", "e.g. \"Ice Spike\" - spaces allowed, converted to the project's naming style automatically (class: IceSpike, enum: Ice_Spike)."),
            _displayName);
        _cooldown = EditorGUILayout.FloatField("Cooldown (seconds)", _cooldown);

        EditorGUILayout.Space(6);
        _createInstanceScript = EditorGUILayout.ToggleLeft(
            "Also create an AbilityObject instance script (the spawned projectile/AoE, with OnHit/OnStay)",
            _createInstanceScript);

        if (_createInstanceScript)
        {
            EditorGUI.indentLevel++;
            _duration = EditorGUILayout.FloatField("Duration", _duration);
            _speed = EditorGUILayout.FloatField("Speed", _speed);
            _damage = EditorGUILayout.FloatField("Damage", _damage);
            _pierce = EditorGUILayout.FloatField("Pierce", _pierce);
            _knockback = EditorGUILayout.FloatField("Knockback", _knockback);
            _tickRate = EditorGUILayout.FloatField("Tick Rate", _tickRate);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        bool validName = IsValidDisplayName(_displayName);
        GUI.enabled = validName;
        if (GUILayout.Button("Create Ability", GUILayout.Height(30)))
            CreateAbility();
        GUI.enabled = true;

        if (!string.IsNullOrEmpty(_displayName) && !validName)
            EditorGUILayout.HelpBox("Name must start with a letter and contain only letters, digits and spaces.", MessageType.Warning);
    }

    void CreateAbility()
    {
        var (className, enumName) = ToNames(_displayName);

        string folder = $"{ScriptsRoot}/{className}";
        string classFile = $"{folder}/{className}.cs";
        string instanceFile = $"{folder}/{className}Instance.cs";

        if (Directory.Exists(folder) || File.Exists(classFile))
        {
            EditorUtility.DisplayDialog("Ability Editor", $"'{className}' already exists at {folder}. Choose a different name.", "OK");
            return;
        }

        string managerText = File.ReadAllText(AbilityManagerPath);
        if (Regex.IsMatch(managerText, $@"\b{Regex.Escape(enumName)}\b"))
        {
            EditorUtility.DisplayDialog("Ability Editor", $"'{enumName}' already exists in ActivatableAbilityType. Choose a different name.", "OK");
            return;
        }

        try
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(classFile, BuildActivatableAbilitySource(className));
            if (_createInstanceScript)
                File.WriteAllText(instanceFile, BuildAbilityObjectSource(className));

            PatchPlayerAbilityManager(className, enumName);

            EnsureFolder(PrefabsRoot);
            if (!AssetDatabase.IsValidFolder($"{PrefabsRoot}/{className}"))
                AssetDatabase.CreateFolder(PrefabsRoot, className);

            AssetDatabase.Refresh();

            Debug.Log($"[AbilityEditor] Created '{className}' at {folder} (Resources.Load path \"Prefabs/Abilities/{className}/{className}\"), " +
                $"registered ActivatableAbilityType.{enumName} in PlayerAbilityManager.cs, and created an empty prefab folder at {PrefabsRoot}/{className}. " +
                $"Next: build the prefab there and flesh out Activated()" + (_createInstanceScript ? "/OnHit()/OnStay()" : "()") + ".");

            _displayName = "";
        }
        catch (Exception e)
        {
            Debug.LogError($"[AbilityEditor] Failed to create ability '{className}': {e}");
            EditorUtility.DisplayDialog("Ability Editor", $"Failed to create ability: {e.Message}", "OK");
        }
    }

    string BuildActivatableAbilitySource(string className)
    {
        string field = ToCamelCase(className) + "Obj";
        return
$@"using UnityEngine;

public class {className} : ActivatableAbility
{{
    public GameObject {field};

    void Start()
    {{
        cooldownTimeMax = {F(_cooldown)}f;

        {field} = (GameObject)Resources.Load(""Prefabs/Abilities/{className}/{className}"", typeof(GameObject));
    }}

    public override void Activated()
    {{
        // Default spawn-on-player behaviour, matching AoE-style abilities like FrostPulse.
        // For a directional/projectile ability, aim at the cursor and spawn at a fire point instead.
        Instantiate({field}, player.transform);
    }}
}}
";
    }

    string BuildAbilityObjectSource(string className)
    {
        return
$@"using UnityEngine;

public class {className}Instance : AbilityObject
{{
    public override float duration {{ get; set; }} = {F(_duration)}f;
    public override float speed {{ get; set; }} = {F(_speed)}f;
    public override float damage {{ get; set; }} = {F(_damage)}f;
    public override float pierce {{ get; set; }} = {F(_pierce)}f;
    public override float knockback {{ get; set; }} = {F(_knockback)}f;
    public override float tickRate {{ get; set; }} = {F(_tickRate)}f;

    public override void OnHit(Enemy enemy)
    {{
        enemy.TakeDamage((int)damage);
        enemy.GetKnockbacked(player.transform, knockback);
    }}

    public override void OnStay(Enemy enemy)
    {{
        // override this to customize on-stay behaviour (e.g. damage-over-time)
    }}
}}
";
    }

    static string F(float v) => v.ToString(CultureInfo.InvariantCulture);

    static string ToCamelCase(string pascal) =>
        pascal.Length == 0 ? pascal : char.ToLowerInvariant(pascal[0]) + pascal.Substring(1);

    static bool IsValidDisplayName(string name)
    {
        name = name?.Trim();
        return !string.IsNullOrEmpty(name) && Regex.IsMatch(name, @"^[A-Za-z][A-Za-z0-9 ]*$");
    }

    // "ice spike" -> className "IceSpike", enumName "Ice_Spike" (matches existing
    // Frost_Pulse / Electric_Spin / Wind_Shield underscore-joined enum style).
    static (string className, string enumName) ToNames(string displayName)
    {
        var words = displayName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpperInvariant(w[0]) + (w.Length > 1 ? w.Substring(1) : ""))
            .ToList();
        return (string.Concat(words), string.Join("_", words));
    }

    static void EnsureFolder(string assetsPath)
    {
        var parts = assetsPath.Split('/');
        string cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }

    // Inserts the new enum entry and dictionary entry into PlayerAbilityManager.cs via
    // regex, rather than requiring the enum/dictionary to be maintained by hand in two
    // places (see the "ALSO ADD TO `abilities` dictionary ABOVE!" comment in that file).
    static void PatchPlayerAbilityManager(string className, string enumName)
    {
        string text = File.ReadAllText(AbilityManagerPath);

        var enumMatch = Regex.Match(text, @"enum\s+ActivatableAbilityType\s*\{(.*?)\}", RegexOptions.Singleline);
        if (!enumMatch.Success)
            throw new InvalidOperationException("Could not find 'enum ActivatableAbilityType { ... }' in PlayerAbilityManager.cs - add the entry manually.");

        var dictMatch = Regex.Match(text, @"abilities\s*=\s*new Dictionary<ActivatableAbilityType,\s*ActivatableAbility>\(\)\s*\{(.*?)\}\s*;", RegexOptions.Singleline);
        if (!dictMatch.Success)
            throw new InvalidOperationException("Could not find the 'abilities' dictionary initializer in PlayerAbilityManager.cs - add the entry manually.");

        var enumBody = enumMatch.Groups[1];
        var dictBody = dictMatch.Groups[1];
        string dictEntry = $"{{ActivatableAbilityType.{enumName}, gameObject.AddComponent<{className}>()}}";

        // Patch whichever region occurs later in the file first, so the earlier region's
        // captured index/length stays valid for the second replacement.
        string result;
        if (dictBody.Index > enumBody.Index)
        {
            result = ReplaceRange(text, dictBody.Index, dictBody.Length, AppendEntry(dictBody.Value, "            ", dictEntry));
            result = ReplaceRange(result, enumBody.Index, enumBody.Length, AppendEntry(enumBody.Value, "    ", enumName));
        }
        else
        {
            result = ReplaceRange(text, enumBody.Index, enumBody.Length, AppendEntry(enumBody.Value, "    ", enumName));
            result = ReplaceRange(result, dictBody.Index, dictBody.Length, AppendEntry(dictBody.Value, "            ", dictEntry));
        }

        File.WriteAllText(AbilityManagerPath, result);
    }

    static string ReplaceRange(string text, int index, int length, string replacement) =>
        text.Substring(0, index) + replacement + text.Substring(index + length);

    static string AppendEntry(string body, string indent, string entry)
    {
        string trimmed = body.TrimEnd();
        if (trimmed.Length > 0 && !trimmed.EndsWith(","))
            trimmed += ",";
        return trimmed + "\n" + indent + entry + "\n";
    }

    // ---------------------------------------------------------------
    // Item Ability Stats
    // ---------------------------------------------------------------

    void RefreshItems()
    {
        _items = AssetDatabase.FindAssets("t:ItemObject")
            .Select(guid => AssetDatabase.LoadAssetAtPath<ItemObject>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(i => i != null)
            .OrderBy(i => i.name)
            .ToArray();
    }

    void DrawItemStatsTab()
    {
        EditorGUILayout.HelpBox(
            "Edits the ability wired onto each equippable item (ItemObject.ability / abilityDamage / abilityCooldown). " +
            "An ability's own base stats (duration, speed, pierce, knockback...) live in its AbilityObject subclass " +
            "source, not here - these fields only control which ability an item grants and its per-item damage/cooldown.",
            MessageType.Info);

        if (GUILayout.Button("Refresh"))
            RefreshItems();

        EditorGUILayout.Space(6);

        if (_items.Length == 0)
        {
            EditorGUILayout.LabelField("No ItemObject assets found in the project.");
            return;
        }

        _itemsScroll = EditorGUILayout.BeginScrollView(_itemsScroll);
        foreach (var item in _items)
        {
            if (item == null)
                continue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(item.name, EditorStyles.boldLabel);

            // Read/write the fields directly rather than via SerializedObject/FindProperty:
            // ItemObject is an Odin SerializedScriptableObject, and simple fields like these
            // are only guaranteed to appear on Unity's native SerializedProperty path under
            // Odin's default serialization policy - direct field access works regardless.
            EditorGUI.BeginChangeCheck();
            var ability = (ActivatableAbilityType)EditorGUILayout.EnumPopup("Ability", item.ability);
            float damage = item.abilityDamage;
            float cooldown = item.abilityCooldown;
            using (new EditorGUI.DisabledScope(ability == ActivatableAbilityType.NULL))
            {
                damage = EditorGUILayout.FloatField("Ability Damage", damage);
                cooldown = EditorGUILayout.FloatField("Ability Cooldown", cooldown);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(item, "Edit Ability Stats");
                item.ability = ability;
                item.abilityDamage = damage;
                item.abilityCooldown = cooldown;
                EditorUtility.SetDirty(item);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }
        EditorGUILayout.EndScrollView();
    }
}
