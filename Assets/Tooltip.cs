using TMPro;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance {  get; private set; }


    [SerializeField] RectTransform _canvasRectTransform;
    private RectTransform background;
    private TextMeshProUGUI _textMeshProUGUI;

    private System.Func<string> getToolTipTextFunc;

    private void Awake()
    {
        Instance = this;

        background = GetComponent<RectTransform>();
        _textMeshProUGUI = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        HideTooltip();
    }

    private void Update()
    {
        //SetText(getToolTipTextFunc());
        FollowMouseStayVisible();
    }

    private void FollowMouseStayVisible()
    {
        Vector2 anchoredPosition = Input.mousePosition / _canvasRectTransform.localScale.x;

        if (anchoredPosition.x + background.rect.width > _canvasRectTransform.rect.width)
        {
            anchoredPosition.x = _canvasRectTransform.rect.width -
                background.rect.width;
        }
        if (anchoredPosition.y + background.rect.height > _canvasRectTransform.rect.height)
        {
            anchoredPosition.y = _canvasRectTransform.rect.height -
                background.rect.height;
        }
        background.anchoredPosition = anchoredPosition;
    }

    private void SetText(string text)
    {
        _textMeshProUGUI.SetText(text);
        _textMeshProUGUI.ForceMeshUpdate();

        Vector2 textSize = _textMeshProUGUI.GetRenderedValues();
        Vector2 padding = new Vector2(12, 12);

        background.sizeDelta = textSize + padding;
    }

    private void Show(string text)
    {
        gameObject.SetActive(true);
        SetText(text);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public static void ShowTooltip(string text)
    {
        Instance.Show(text);
    }
    private void ShowTooltip(System.Func<string> getTooltipTextFunc)
    {
        this.getToolTipTextFunc = getTooltipTextFunc;
        gameObject.SetActive(true);
        SetText(getToolTipTextFunc());
    }
   /* System.Func<string> getTooltipTextFunc = () => { };
    Tooltip.ShowTooltip_Static(getTooltipTextFunc);*/
    public static void ShowTooltip_Static(System.Func<string> getTooltipTextFunc)
    {
        Instance.ShowTooltip(getTooltipTextFunc);
    }
    public static void HideTooltip()
    {
        Instance.Hide();
    }
}
