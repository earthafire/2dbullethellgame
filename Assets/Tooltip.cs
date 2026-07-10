using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Diablo/RotMG-style item tooltip. The whole layout (border, title, stat rows,
// ability line, description) is built procedurally in Awake so this can sit on
// any RectTransform without hand-authoring a deep prefab hierarchy - tweak the
// colors/sizes below and the layout regenerates itself.
public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance { get; private set; }

    [SerializeField] RectTransform _canvasRectTransform;

    private const float PanelWidth = 300f;

    private static readonly Color BorderColor = new Color(0.78f, 0.63f, 0.24f, 1f);
    private static readonly Color PanelColor = new Color(0.07f, 0.07f, 0.09f, 0.96f);
    private static readonly Color TitleColor = new Color(0.96f, 0.85f, 0.55f, 1f);
    private static readonly Color SubtitleColor = new Color(0.6f, 0.6f, 0.63f, 1f);
    private static readonly Color BuffColor = new Color(0.31f, 0.8f, 0.47f, 1f);
    private static readonly Color DebuffColor = new Color(0.86f, 0.33f, 0.33f, 1f);
    private static readonly Color AbilityColor = new Color(0.51f, 0.81f, 1f, 1f);
    private static readonly Color DescriptionColor = new Color(0.72f, 0.72f, 0.75f, 1f);
    private static readonly Color KeyStatColor = new Color(0.95f, 0.76f, 0.35f, 1f);

    private RectTransform _rect;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _subtitleText;
    private Transform _keyStatsContainer;
    private Transform _statsContainer;
    private TextMeshProUGUI _abilityText;
    private TextMeshProUGUI _descriptionText;

    private void Awake()
    {
        Instance = this;
        BuildLayout();
        Hide();
    }

    private void Update()
    {
        FollowMouseStayVisible();
    }

    private void BuildLayout()
    {
        _rect = GetComponent<RectTransform>();

        // Clear out any placeholder children from earlier prototypes.
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        Image panelImage = GetComponent<Image>();
        if (panelImage == null) panelImage = gameObject.AddComponent<Image>();
        panelImage.sprite = Resources.GetBuiltinResource<Sprite>("UISprite.psd");
        panelImage.type = Image.Type.Sliced;
        panelImage.color = PanelColor;
        panelImage.raycastTarget = false;

        Outline border = GetComponent<Outline>();
        if (border == null) border = gameObject.AddComponent<Outline>();
        border.effectColor = BorderColor;
        border.effectDistance = new Vector2(1.5f, 1.5f);
        border.useGraphicAlpha = false;

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        VerticalLayoutGroup layout = GetComponent<VerticalLayoutGroup>();
        if (layout == null) layout = gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 12, 12);
        layout.spacing = 4;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        _rect.sizeDelta = new Vector2(PanelWidth, _rect.sizeDelta.y);

        _titleText = CreateLabel("Title", 22, FontStyles.Bold, TitleColor, false, transform);
        _subtitleText = CreateLabel("Subtitle", 14, FontStyles.Italic, SubtitleColor, false, transform);
        CreateDivider();
        _keyStatsContainer = CreateVerticalContainer("KeyStats");
        _statsContainer = CreateVerticalContainer("Stats");
        _abilityText = CreateLabel("Ability", 15, FontStyles.Normal, AbilityColor, true, transform);
        _descriptionText = CreateLabel("Description", 13, FontStyles.Italic, DescriptionColor, true, transform);
    }

    private static TextMeshProUGUI CreateLabel(string name, int fontSize, FontStyles style, Color color, bool wrap, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.color = color;
        label.alignment = TextAlignmentOptions.TopLeft;
        label.enableWordWrapping = wrap;
        label.overflowMode = TextOverflowModes.Overflow;
        label.raycastTarget = false;
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1;
        return label;
    }

    private void CreateDivider()
    {
        GameObject go = new GameObject("Divider", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(BorderColor.r, BorderColor.g, BorderColor.b, 0.5f);
        img.raycastTarget = false;
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.minHeight = 1.5f;
        le.preferredHeight = 1.5f;
        le.flexibleWidth = 1;
    }

    private Transform CreateVerticalContainer(string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(transform, false);
        VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 2;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = go.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1;
        return go.transform;
    }

    private void FollowMouseStayVisible()
    {
        Vector2 anchoredPosition = Input.mousePosition / _canvasRectTransform.localScale.x;

        if (anchoredPosition.x + _rect.rect.width > _canvasRectTransform.rect.width)
            anchoredPosition.x = _canvasRectTransform.rect.width - _rect.rect.width;
        if (anchoredPosition.y + _rect.rect.height > _canvasRectTransform.rect.height)
            anchoredPosition.y = _canvasRectTransform.rect.height - _rect.rect.height;

        _rect.anchoredPosition = anchoredPosition;
    }

    private void Populate(string itemName, ItemType slotType, Dictionary<Attribute, float> buffs, ActivatableAbilityType ability, float abilityDamage, float abilityCooldown, string description)
    {
        _titleText.text = itemName;
        _subtitleText.text = NicifyName(slotType.ToString());

        foreach (Transform child in _keyStatsContainer)
            Destroy(child.gameObject);

        bool hasAbilityStats = ability != ActivatableAbilityType.NULL && abilityCooldown > 0f;
        if (hasAbilityStats)
        {
            float dps = abilityDamage / abilityCooldown;
            CreateLabel("Damage", 16, FontStyles.Bold, KeyStatColor, false, _keyStatsContainer).text = $"Damage: {abilityDamage:0.#}";
            CreateLabel("Cooldown", 16, FontStyles.Bold, KeyStatColor, false, _keyStatsContainer).text = $"Cooldown: {abilityCooldown:0.##}s";
            CreateLabel("DPS", 16, FontStyles.Bold, KeyStatColor, false, _keyStatsContainer).text = $"Damage/Sec: {dps:0.#}";
        }

        foreach (Transform child in _statsContainer)
            Destroy(child.gameObject);

        foreach (KeyValuePair<Attribute, float> pair in buffs)
        {
            bool isPercent = pair.Key == Attribute.damage || pair.Key == Attribute.cooldown;
            string sign = pair.Value >= 0 ? "+" : "";
            string valueText = isPercent ? $"{sign}{pair.Value:0.#}%" : $"{sign}{pair.Value:0.#}";
            Color rowColor = pair.Value >= 0 ? BuffColor : DebuffColor;
            TextMeshProUGUI row = CreateLabel(pair.Key.ToString(), 15, FontStyles.Normal, rowColor, false, _statsContainer);
            row.text = $"{valueText} {NicifyName(pair.Key.ToString())}";
        }

        bool hasAbility = ability != ActivatableAbilityType.NULL;
        _abilityText.gameObject.SetActive(hasAbility);
        if (hasAbility)
            _abilityText.text = "Grants Ability: " + NicifyName(ability.ToString());

        bool hasDescription = !string.IsNullOrWhiteSpace(description);
        _descriptionText.gameObject.SetActive(hasDescription);
        if (hasDescription)
            _descriptionText.text = description;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);
    }

    private static string NicifyName(string rawName)
    {
        string spaced = Regex.Replace(rawName, "(?<!^)([A-Z])", " $1").Replace('_', ' ');
        return spaced.Length == 0 ? spaced : char.ToUpper(spaced[0]) + spaced.Substring(1);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public static void ShowTooltip(Item item, ItemType slotType)
    {
        Instance.Populate(item.Name, slotType, item.Buffs, item.Ability, item.AbilityDamage, item.AbilityCooldown, item.Description);
        Instance.Show();
    }

    public static void HideTooltip()
    {
        Instance.Hide();
    }
}
