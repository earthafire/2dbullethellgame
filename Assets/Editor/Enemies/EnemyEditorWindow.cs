using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Scaffolds new enemies. Unlike abilities (see AbilityEditorWindow.cs), the enemy system
// is data-driven rather than enum/code-driven: every enemy is just the shared Enemy
// MonoBehaviour + an Attributes ScriptableObject (health/speed/damage) on a prefab, and
// gets wired into spawning by dropping that prefab into gameSupervisorController's
// regularEnemies/bossEnemies arrays - there's no type registry to keep in sync. This
// window creates the Attributes asset, assembles the prefab shell (matching the shape of
// existing prefabs like Slime.prefab: SpriteRenderer/Rigidbody2D/CircleCollider2D/Animator/
// hit-particles child/nested Shadow child), optionally builds an AnimatorOverrideController
// from the shared Base.controller, and can register the result for spawning.
public class EnemyEditorWindow : EditorWindow
{
    const string AttributesFolder = "Assets/Resources/Data/Enemies";
    const string PrefabsRoot = "Assets/Resources/Prefabs/Enemies";
    const string ShadowFolder = "Assets/Resources/Prefabs/Enemies/Shadow";
    const string AnimFolder = "Assets/Art/Animation/Enemies";
    const string BaseControllerPath = "Assets/Art/Animation/Enemies/Base/Base.controller";

    enum EnemyCategory { Ground, Flying }

    [Serializable]
    class ClipPreviewState
    {
        public bool playing;
        public double playStart;
        public float scrub;
    }

    int _tab;
    readonly string[] _tabLabels = { "Create New Enemy", "Register For Spawning" };

    // --- Create tab state ---
    string _name = "";
    EnemyCategory _category = EnemyCategory.Ground;
    bool _isBoss;
    float _maxHealth = 20f;
    float _moveSpeed = 2f;
    float _damage = 5f;
    Sprite _sprite;
    EnemyXpObjectData _xpData;
    GameObject _lootBag;
    AnimationClip _baseClip;
    AnimationClip _getHitClip;
    readonly ClipPreviewState _basePreview = new ClipPreviewState();
    readonly ClipPreviewState _getHitPreview = new ClipPreviewState();

    // --- Register tab state ---
    GameObject _quickAssignPrefab;
    int _quickAssignTier;

    [MenuItem("Tools/Enemies/Enemy Editor")]
    static void Open()
    {
        var window = GetWindow<EnemyEditorWindow>("Enemy Editor");
        window.minSize = new Vector2(460, 420);
    }

    void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    void OnEditorUpdate()
    {
        if (_basePreview.playing || _getHitPreview.playing)
            Repaint();
    }

    void OnGUI()
    {
        _tab = GUILayout.Toolbar(_tab, _tabLabels);
        EditorGUILayout.Space(8);

        if (_tab == 0)
            DrawCreateTab();
        else
            DrawSpawnRegistrationTab();
    }

    // ---------------------------------------------------------------
    // Create New Enemy
    // ---------------------------------------------------------------

    void DrawCreateTab()
    {
        EditorGUILayout.HelpBox(
            "Creates an Attributes data asset, assembles a prefab (SpriteRenderer / Rigidbody2D / " +
            "CircleCollider2D / Animator / hit-particles child / shadow child / Enemy component) in " +
            "the right layer and folder, and - if you link animations below - builds an " +
            "AnimatorOverrideController from the shared Base.controller. You'll still want to fine-tune " +
            "the collider and run the Navigation setup tool for pathfinding.",
            MessageType.Info);
        EditorGUILayout.Space(6);

        _name = EditorGUILayout.TextField("Enemy Name", _name);
        _category = (EnemyCategory)EditorGUILayout.EnumPopup("Category", _category);
        _isBoss = EditorGUILayout.ToggleLeft("Boss (informational only - place it in bossEnemies[] in the Register tab)", _isBoss);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
        _maxHealth = EditorGUILayout.FloatField("Max Health", _maxHealth);
        _moveSpeed = EditorGUILayout.FloatField("Move Speed", _moveSpeed);
        _damage = EditorGUILayout.FloatField("Damage", _damage);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Visuals & Loot", EditorStyles.boldLabel);
        _sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _sprite, typeof(Sprite), false);
        _xpData = (EnemyXpObjectData)EditorGUILayout.ObjectField("XP Data (tier asset)", _xpData, typeof(EnemyXpObjectData), false);
        _lootBag = (GameObject)EditorGUILayout.ObjectField("Loot Bag Prefab (optional)", _lootBag, typeof(GameObject), false);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Animations (optional - builds an AnimatorOverrideController)", EditorStyles.boldLabel);
        DrawClipField("Base / Movement Animation", ref _baseClip, _basePreview);
        using (new EditorGUI.DisabledScope(_baseClip == null))
            DrawClipField("Get Hit Animation (falls back to the base placeholder if empty)", ref _getHitClip, _getHitPreview);

        EditorGUILayout.Space(10);
        GUI.enabled = !string.IsNullOrWhiteSpace(_name);
        if (GUILayout.Button("Create Enemy", GUILayout.Height(30)))
            CreateEnemy();
        GUI.enabled = true;
    }

    // Compact clip field: ObjectField + a small sprite preview sampled from the clip's
    // sprite-swap curve, with independent Play/Stop + scrub. Enemy animations in this
    // project are simple SpriteRenderer.m_Sprite object-reference curves (confirmed on
    // Move.anim), so sampling that curve directly is enough for a working preview without
    // needing a live Animator/AnimationMode simulation.
    void DrawClipField(string label, ref AnimationClip clip, ClipPreviewState state)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        clip = (AnimationClip)EditorGUILayout.ObjectField(label, clip, typeof(AnimationClip), false);

        using (new EditorGUILayout.HorizontalScope())
        {
            Rect box = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64));
            float previewTime = clip != null ? GetPreviewTime(clip, state) : 0f;
            DrawSpritePreview(box, clip != null ? GetSpriteAtTime(clip, previewTime) : null);

            using (new EditorGUILayout.VerticalScope())
            {
                GUI.enabled = clip != null;
                if (GUILayout.Button(state.playing ? "Stop" : "Play", GUILayout.Width(60)))
                {
                    state.playing = !state.playing;
                    if (state.playing)
                        state.playStart = EditorApplication.timeSinceStartup;
                }
                if (clip != null)
                    state.scrub = EditorGUILayout.Slider(state.scrub, 0f, Mathf.Max(clip.length, 0.01f));
                GUI.enabled = true;
            }
        }
        EditorGUILayout.EndVertical();
    }

    static float GetPreviewTime(AnimationClip clip, ClipPreviewState state)
    {
        if (!state.playing)
            return state.scrub;
        float length = Mathf.Max(clip.length, 0.01f);
        return (float)((EditorApplication.timeSinceStartup - state.playStart) % length);
    }

    static Sprite GetSpriteAtTime(AnimationClip clip, float time)
    {
        foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
        {
            if (binding.propertyName != "m_Sprite")
                continue;

            var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            if (keyframes.Length == 0)
                continue;

            Sprite result = keyframes[0].value as Sprite;
            foreach (var kf in keyframes)
            {
                if (kf.time > time)
                    break;
                result = kf.value as Sprite;
            }
            return result;
        }
        return null;
    }

    static void DrawSpritePreview(Rect rect, Sprite sprite)
    {
        EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.2f));
        if (sprite == null || sprite.texture == null)
            return;

        var texRect = sprite.textureRect;
        var tex = sprite.texture;
        var uv = new Rect(texRect.x / tex.width, texRect.y / tex.height, texRect.width / tex.width, texRect.height / tex.height);

        float aspect = texRect.width / texRect.height;
        Rect fit = rect;
        if (aspect > 1f)
            fit.height = rect.width / aspect;
        else
            fit.width = rect.height * aspect;
        fit.x = rect.x + (rect.width - fit.width) / 2f;
        fit.y = rect.y + (rect.height - fit.height) / 2f;

        GUI.DrawTextureWithTexCoords(fit, tex, uv);
    }

    void CreateEnemy()
    {
        string safeName = SanitizeFileName(_name.Trim());
        string categoryFolder = _category == EnemyCategory.Ground ? "Ground Enemies" : "Flying Enemies";
        string prefabPath = $"{PrefabsRoot}/{categoryFolder}/{safeName}.prefab";
        string attributesPath = $"{AttributesFolder}/{safeName}.asset";

        if (File.Exists(prefabPath) || File.Exists(attributesPath))
        {
            EditorUtility.DisplayDialog("Enemy Editor", $"'{safeName}' already exists (prefab or Attributes asset). Choose a different name.", "OK");
            return;
        }

        GameObject root = null;
        try
        {
            var attributes = ScriptableObject.CreateInstance<Attributes>();
            attributes.default_attributes[Attribute.maxHealth] = _maxHealth;
            attributes.default_attributes[Attribute.moveSpeed] = _moveSpeed;
            attributes.default_attributes[Attribute.damage] = _damage;
            EnsureFolder(AttributesFolder);
            AssetDatabase.CreateAsset(attributes, attributesPath);

            RuntimeAnimatorController overrideController = _baseClip != null
                ? BuildOverrideController(safeName, _baseClip, _getHitClip)
                : null;

            root = new GameObject(safeName);
            int layer = LayerMask.NameToLayer(_category == EnemyCategory.Ground ? "Enemy" : "FlyingEnemy");
            if (layer < 0)
                layer = _category == EnemyCategory.Ground ? 7 : 9;
            root.layer = layer;
            try { root.tag = "Enemy"; }
            catch (UnityException) { Debug.LogWarning($"[EnemyEditor] Tag 'Enemy' not found in Project Settings - left '{safeName}' untagged."); }

            // Shadow must be sibling index 0 - Enemy.Start() reads transform.GetChild(0) as the shadow.
            string shadowPrefabPath = $"{ShadowFolder}/{(_category == EnemyCategory.Ground ? "Ground Shadow" : "Flying Shadow")}.prefab";
            var shadowAsset = AssetDatabase.LoadAssetAtPath<GameObject>(shadowPrefabPath);
            if (shadowAsset != null)
            {
                var shadowInstance = (GameObject)PrefabUtility.InstantiatePrefab(shadowAsset);
                shadowInstance.transform.SetParent(root.transform, false);
                shadowInstance.transform.SetSiblingIndex(0);
            }
            else
            {
                Debug.LogWarning($"[EnemyEditor] Couldn't find {shadowPrefabPath} - created '{safeName}' without a shadow child; add one manually as child index 0.");
            }

            // Enemy.Awake() does GetComponentInChildren<ParticleSystem>() for the hit-VFX burst.
            var particlesGO = new GameObject("Hit Particles");
            particlesGO.transform.SetParent(root.transform, false);
            var ps = particlesGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;

            var sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = _sprite;

            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.mass = 10f;
            rb.linearDamping = 100f;
            rb.angularDamping = 0.05f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var col = root.AddComponent<CircleCollider2D>();
            col.isTrigger = false;
            col.radius = _sprite != null ? Mathf.Max(_sprite.bounds.extents.x, _sprite.bounds.extents.y) : 0.15f;

            var animator = root.AddComponent<Animator>();
            if (overrideController != null)
                animator.runtimeAnimatorController = overrideController;

            var enemy = root.AddComponent<Enemy>();
            enemy.attributes = attributes;
            enemy._xpData = _xpData;

            var enemySO = new SerializedObject(enemy);
            enemySO.FindProperty("_lootBag").objectReferenceValue = _lootBag;
            enemySO.ApplyModifiedPropertiesWithoutUndo();

            EnsureFolder($"{PrefabsRoot}/{categoryFolder}");
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Selection.objects = new UnityEngine.Object[] { savedPrefab, attributes };
            EditorGUIUtility.PingObject(savedPrefab);

            Debug.Log($"[EnemyEditor] Created '{safeName}': prefab at {prefabPath}, Attributes at {attributesPath}" +
                (overrideController != null ? $", override controller at {AnimFolder}/{safeName}/{safeName}.overrideController" : ", no animations linked - Animator has no controller yet") +
                $". Next: run Tools > Navigation > Add Agent Navigation To Selected Prefabs if it should pathfind, and register it for spawning in the 'Register For Spawning' tab.");

            _name = "";
        }
        catch (Exception e)
        {
            Debug.LogError($"[EnemyEditor] Failed to create enemy '{safeName}': {e}");
            EditorUtility.DisplayDialog("Enemy Editor", $"Failed to create enemy: {e.Message}", "OK");
        }
        finally
        {
            if (root != null)
                UnityEngine.Object.DestroyImmediate(root);
        }
    }

    // Base.controller has exactly two states - "Base" (movement loop) and "GetHit" - each
    // with a placeholder motion clip; every enemy's override controller replaces those two
    // clips with its own. Matched by state name rather than clip name since the placeholder
    // clips (borrowed from Green Cube) aren't descriptively named.
    static RuntimeAnimatorController BuildOverrideController(string safeName, AnimationClip baseClip, AnimationClip getHitClip)
    {
        var baseController = AssetDatabase.LoadAssetAtPath<AnimatorController>(BaseControllerPath);
        if (baseController == null)
        {
            Debug.LogWarning($"[EnemyEditor] Couldn't find base controller at {BaseControllerPath} - skipping override controller; assign an Animator Controller manually.");
            return null;
        }

        AnimationClip originalBase = null, originalGetHit = null;
        foreach (var childState in baseController.layers[0].stateMachine.states)
        {
            if (childState.state.name == "Base")
                originalBase = childState.state.motion as AnimationClip;
            else if (childState.state.name == "GetHit")
                originalGetHit = childState.state.motion as AnimationClip;
        }

        var overrideController = new AnimatorOverrideController(baseController);
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);
        for (int i = 0; i < overrides.Count; i++)
        {
            var original = overrides[i].Key;
            if (original == originalBase)
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, baseClip);
            else if (original == originalGetHit && getHitClip != null)
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, getHitClip);
        }
        overrideController.ApplyOverrides(overrides);

        string folder = $"{AnimFolder}/{safeName}";
        EnsureFolder(folder);
        AssetDatabase.CreateAsset(overrideController, $"{folder}/{safeName}.overrideController");
        return overrideController;
    }

    static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c.ToString(), "");
        return name.Trim();
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

    // ---------------------------------------------------------------
    // Register For Spawning
    // ---------------------------------------------------------------

    void DrawSpawnRegistrationTab()
    {
        var supervisor = FindAnyObjectByType<gameSupervisorController>();
        if (supervisor == null)
        {
            EditorGUILayout.HelpBox(
                "No gameSupervisorController found in the currently open scene(s). Open the gameplay " +
                "scene that contains it to register a new enemy for spawning here.",
                MessageType.Warning);
            return;
        }

        EditorGUILayout.HelpBox($"Editing '{supervisor.gameObject.name}' in scene '{supervisor.gameObject.scene.name}'. regularEnemies/bossEnemies are indexed by tier - spawnEntity() picks regularEnemies[enemyTier] and bossEnemies[enemyTier].", MessageType.Info);
        EditorGUILayout.Space(6);

        var so = new SerializedObject(supervisor);
        var regularProp = so.FindProperty("regularEnemies");
        var bossProp = so.FindProperty("bossEnemies");

        EditorGUILayout.PropertyField(regularProp, true);
        EditorGUILayout.Space(4);
        EditorGUILayout.PropertyField(bossProp, true);
        so.ApplyModifiedProperties();

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Quick Assign", EditorStyles.boldLabel);
        _quickAssignPrefab = (GameObject)EditorGUILayout.ObjectField("Enemy Prefab", _quickAssignPrefab, typeof(GameObject), false);
        _quickAssignTier = EditorGUILayout.IntField("Tier Index", _quickAssignTier);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.enabled = _quickAssignPrefab != null && _quickAssignTier >= 0;
            if (GUILayout.Button("Set as Regular Enemy for Tier"))
                AssignAtTier(supervisor, "regularEnemies");
            if (GUILayout.Button("Set as Boss for Tier"))
                AssignAtTier(supervisor, "bossEnemies");
            GUI.enabled = true;
        }
    }

    void AssignAtTier(gameSupervisorController supervisor, string arrayFieldName)
    {
        Undo.RecordObject(supervisor, "Assign Enemy Tier");
        var so = new SerializedObject(supervisor);
        var prop = so.FindProperty(arrayFieldName);
        if (_quickAssignTier >= prop.arraySize)
            prop.arraySize = _quickAssignTier + 1;
        prop.GetArrayElementAtIndex(_quickAssignTier).objectReferenceValue = _quickAssignPrefab;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(supervisor);
    }
}
