using UnityEngine;
using UnityEngine.UI;

public class LifeCounter : MonoBehaviour
{
    public static LifeCounter Instance;

    private int _life            = 40;
    private int _poison          = 0;
    private int _commanderDamage = 0;

    private Text _lifeText;
    private Text _poisonText;
    private Text _commanderText;

    private InputField _lifeInput;
    private InputField _poisonInput;
    private InputField _commanderInput;

    void Awake()
    {
        Instance = this;
        CreateUI();
    }

    void CreateUI()
    {
        var canvasGO = new GameObject("LifeCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        var panelGO = new GameObject("LifePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);

        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.25f, 0.92f);
        panelRT.anchorMax = new Vector2(0.75f, 1f);
        panelRT.sizeDelta = Vector2.zero;

        panelGO.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 0.95f);

        CreateSection(panelGO, "LIFE",    ref _lifeText,      ref _lifeInput,      0f,    0.33f, new Color(0.2f, 0.6f, 0.2f), _life.ToString(),            OnLifeChange);
        CreateSection(panelGO, "POISON",  ref _poisonText,    ref _poisonInput,    0.33f, 0.66f, new Color(0.5f, 0.2f, 0.7f), _poison.ToString(),          OnPoisonChange);
        CreateSection(panelGO, "CMD DMG", ref _commanderText, ref _commanderInput, 0.66f, 1f,    new Color(0.7f, 0.3f, 0.1f), _commanderDamage.ToString(), OnCommanderChange);
    }

    void CreateSection(GameObject parent, string label, ref Text valueText, ref InputField inputField,
                       float xMin, float xMax, Color accentColor,
                       string initialValue, System.Action<int> onChange)
    {
        var sectionGO = new GameObject(label);
        sectionGO.transform.SetParent(parent.transform, false);

        var sectionRT = sectionGO.AddComponent<RectTransform>();
        sectionRT.anchorMin = new Vector2(xMin, 0);
        sectionRT.anchorMax = new Vector2(xMax, 1);
        sectionRT.sizeDelta = Vector2.zero;

        // Accent bar
        var accentGO = new GameObject("Accent");
        accentGO.transform.SetParent(sectionGO.transform, false);
        var accentRT = accentGO.AddComponent<RectTransform>();
        accentRT.anchorMin = new Vector2(0, 0.85f);
        accentRT.anchorMax = Vector2.one;
        accentRT.sizeDelta = Vector2.zero;
        accentGO.AddComponent<Image>().color = accentColor;

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(accentGO.transform, false);
        var labelRT = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.sizeDelta = Vector2.zero;
        var labelText = labelGO.AddComponent<Text>();
        labelText.text      = label;
        labelText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize  = 11;
        labelText.fontStyle = FontStyle.Bold;
        labelText.color     = Color.white;
        labelText.alignment = TextAnchor.MiddleCenter;

        // Minus button
        CreateCounterButton(sectionGO, "-", new Vector2(0, 0.2f), new Vector2(0.3f, 0.85f), accentColor, () => onChange(-1));

        // InputField value
        var valueGO = new GameObject("Value");
        valueGO.transform.SetParent(sectionGO.transform, false);
        var valueRT = valueGO.AddComponent<RectTransform>();
        valueRT.anchorMin = new Vector2(0.3f, 0.2f);
        valueRT.anchorMax = new Vector2(0.7f, 0.85f);
        valueRT.sizeDelta = Vector2.zero;

        valueGO.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f);

        var field = valueGO.AddComponent<InputField>();
        field.contentType = InputField.ContentType.IntegerNumber;

        var inputTextGO = new GameObject("Text");
        inputTextGO.transform.SetParent(valueGO.transform, false);
        var inputTextRT = inputTextGO.AddComponent<RectTransform>();
        inputTextRT.anchorMin = Vector2.zero;
        inputTextRT.anchorMax = Vector2.one;
        inputTextRT.sizeDelta = Vector2.zero;
        inputTextRT.offsetMin = new Vector2(4, 0);
        inputTextRT.offsetMax = new Vector2(-4, 0);
        var inputText = inputTextGO.AddComponent<Text>();
        inputText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        inputText.fontSize  = 28;
        inputText.fontStyle = FontStyle.Bold;
        inputText.color     = Color.white;
        inputText.alignment = TextAnchor.MiddleCenter;

        field.textComponent = inputText;
        field.text = initialValue;

        valueText  = inputText;
        inputField = field;

        var capturedLabel = label;
        field.onEndEdit.AddListener((val) =>
        {
            if (int.TryParse(val, out int newVal))
            {
                int current = GetCurrentValue(capturedLabel);
                onChange(newVal - current);
            }
        });

        // Plus button
        CreateCounterButton(sectionGO, "+", new Vector2(0.7f, 0.2f), new Vector2(1f, 0.85f), accentColor, () => onChange(1));

        // Divider
        if (xMax < 1f)
        {
            var divGO = new GameObject("Divider");
            divGO.transform.SetParent(sectionGO.transform, false);
            var divRT = divGO.AddComponent<RectTransform>();
            divRT.anchorMin = new Vector2(0.98f, 0.05f);
            divRT.anchorMax = new Vector2(1f, 0.95f);
            divRT.sizeDelta = Vector2.zero;
            divGO.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
        }

        // Reset button
        var resetGO = new GameObject("Reset");
        resetGO.transform.SetParent(sectionGO.transform, false);
        var resetRT = resetGO.AddComponent<RectTransform>();
        resetRT.anchorMin = new Vector2(0.1f, 0.02f);
        resetRT.anchorMax = new Vector2(0.9f, 0.18f);
        resetRT.sizeDelta = Vector2.zero;
        var resetBg = resetGO.AddComponent<Image>();
        resetBg.color = new Color(0.2f, 0.2f, 0.2f);
        var resetBtn = resetGO.AddComponent<Button>();
        resetBtn.targetGraphic = resetBg;
        var resetColors = resetBtn.colors;
        resetColors.highlightedColor = new Color(0.35f, 0.35f, 0.35f);
        resetBtn.colors = resetColors;
        var capturedLabelReset = label;
        resetBtn.onClick.AddListener(() => ResetSection(capturedLabelReset));

        var resetTxtGO = new GameObject("Text");
        resetTxtGO.transform.SetParent(resetGO.transform, false);
        var resetTxtRT = resetTxtGO.AddComponent<RectTransform>();
        resetTxtRT.anchorMin = Vector2.zero;
        resetTxtRT.anchorMax = Vector2.one;
        resetTxtRT.sizeDelta = Vector2.zero;
        var resetTxt = resetTxtGO.AddComponent<Text>();
        resetTxt.text      = "RESET";
        resetTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        resetTxt.fontSize  = 9;
        resetTxt.color     = new Color(0.7f, 0.7f, 0.7f);
        resetTxt.alignment = TextAnchor.MiddleCenter;
    }

    void CreateCounterButton(GameObject parent, string label,
                             Vector2 anchorMin, Vector2 anchorMax,
                             Color color, System.Action onClick)
    {
        var btnGO = new GameObject(label == "+" ? "PlusBtn" : "MinusBtn");
        btnGO.transform.SetParent(parent.transform, false);

        var btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = anchorMin;
        btnRT.anchorMax = anchorMax;
        btnRT.sizeDelta = Vector2.zero;
        btnRT.offsetMin = new Vector2(4, 4);
        btnRT.offsetMax = new Vector2(-4, -4);

        var btnBg = btnGO.AddComponent<Image>();
        btnBg.color = new Color(0.15f, 0.15f, 0.15f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnBg;
        var colors = btn.colors;
        colors.highlightedColor = color * 0.8f;
        colors.pressedColor     = color * 0.5f;
        btn.colors = colors;
        btn.onClick.AddListener(() => onClick());

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;
        var text = textGO.AddComponent<Text>();
        text.text      = label;
        text.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize  = 22;
        text.fontStyle = FontStyle.Bold;
        text.color     = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
    }

    int GetCurrentValue(string label)
    {
        switch (label)
        {
            case "LIFE":    return _life;
            case "POISON":  return _poison;
            case "CMD DMG": return _commanderDamage;
            default:        return 0;
        }
    }

    void OnLifeChange(int delta)
    {
        _life = Mathf.Max(0, _life + delta);
        _lifeText.text  = _life.ToString();
        _lifeInput.text = _life.ToString();
        _lifeText.color = _life <= 0 ? Color.red : Color.white;
    }

    void OnPoisonChange(int delta)
    {
        _poison = Mathf.Clamp(_poison + delta, 0, 10);
        _poisonText.text  = _poison.ToString();
        _poisonInput.text = _poison.ToString();
        _poisonText.color = _poison >= 10 ? new Color(0.8f, 0.2f, 0.8f) : Color.white;
    }

    void OnCommanderChange(int delta)
    {
        _commanderDamage = Mathf.Max(0, _commanderDamage + delta);
        _commanderText.text  = _commanderDamage.ToString();
        _commanderInput.text = _commanderDamage.ToString();
        _commanderText.color = _commanderDamage >= 21 ? new Color(1f, 0.3f, 0.1f) : Color.white;
    }

    void ResetSection(string label)
    {
        switch (label)
        {
            case "LIFE":
                _life = 40;
                _lifeText.text  = _life.ToString();
                _lifeInput.text = _life.ToString();
                _lifeText.color = Color.white;
                break;
            case "POISON":
                _poison = 0;
                _poisonText.text  = _poison.ToString();
                _poisonInput.text = _poison.ToString();
                _poisonText.color = Color.white;
                break;
            case "CMD DMG":
                _commanderDamage = 0;
                _commanderText.text  = _commanderDamage.ToString();
                _commanderInput.text = _commanderDamage.ToString();
                _commanderText.color = Color.white;
                break;
        }
    }
}