using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CoinFlip : MonoBehaviour
{
    private GameObject _panel;
    private Canvas _canvas;
    private bool _isFlipping;

    void Awake()
    {
        _canvas = FindAnyObjectByType<Canvas>();
        if (_canvas == null)
        {
            var cGO = new GameObject("CoinCanvas");
            _canvas = cGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cGO.AddComponent<CanvasScaler>();
            cGO.AddComponent<GraphicRaycaster>();
        }
    }

    void Update()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame && !_isFlipping)
            StartCoroutine(FlipRoutine());

        if (Keyboard.current.escapeKey.wasPressedThisFrame && _panel != null && !_isFlipping)
        {
            Destroy(_panel);
            _panel = null;
        }
    }

    IEnumerator FlipRoutine()
    {
        _isFlipping = true;
        if (_panel != null) Destroy(_panel);

        bool finalResult = Random.value > 0.5f;

        // Build panel
        _panel = new GameObject("CoinFlipPanel");
        _panel.transform.SetParent(_canvas.transform, false);

        var panelRT = _panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.35f, 0.35f);
        panelRT.anchorMax = new Vector2(0.65f, 0.65f);
        panelRT.sizeDelta = Vector2.zero;

        _panel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.97f);

        // Title
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(_panel.transform, false);
        var titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 0.86f);
        titleRT.anchorMax = Vector2.one;
        titleRT.sizeDelta = Vector2.zero;
        var titleText = titleGO.AddComponent<Text>();
        titleText.text      = "COIN FLIP";
        titleText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize  = 16;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color     = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;

        // Coin circle
        var coinGO = new GameObject("Coin");
        coinGO.transform.SetParent(_panel.transform, false);
        var coinRT = coinGO.AddComponent<RectTransform>();
        coinRT.anchorMin = new Vector2(0.2f, 0.3f);
        coinRT.anchorMax = new Vector2(0.8f, 0.85f);
        coinRT.sizeDelta = Vector2.zero;
        var coinImg = coinGO.AddComponent<Image>();

        var coinTextGO = new GameObject("CoinText");
        coinTextGO.transform.SetParent(coinGO.transform, false);
        var coinTextRT = coinTextGO.AddComponent<RectTransform>();
        coinTextRT.anchorMin = Vector2.zero;
        coinTextRT.anchorMax = Vector2.one;
        coinTextRT.sizeDelta = Vector2.zero;
        var coinText = coinTextGO.AddComponent<Text>();
        coinText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        coinText.fontSize  = 48;
        coinText.fontStyle = FontStyle.Bold;
        coinText.color     = Color.white;
        coinText.alignment = TextAnchor.MiddleCenter;

        // Result text
        var resultGO = new GameObject("Result");
        resultGO.transform.SetParent(_panel.transform, false);
        var resultRT = resultGO.AddComponent<RectTransform>();
        resultRT.anchorMin = new Vector2(0, 0.15f);
        resultRT.anchorMax = new Vector2(1, 0.32f);
        resultRT.sizeDelta = Vector2.zero;
        var resultText = resultGO.AddComponent<Text>();
        resultText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        resultText.fontSize  = 24;
        resultText.fontStyle = FontStyle.Bold;
        resultText.alignment = TextAnchor.MiddleCenter;

        // Instruction
        var instrGO = new GameObject("Instruction");
        instrGO.transform.SetParent(_panel.transform, false);
        var instrRT = instrGO.AddComponent<RectTransform>();
        instrRT.anchorMin = new Vector2(0, 0.02f);
        instrRT.anchorMax = new Vector2(1, 0.14f);
        instrRT.sizeDelta = Vector2.zero;
        var instrText = instrGO.AddComponent<Text>();
        instrText.text      = "Press C to flip again  |  ESC to close";
        instrText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instrText.fontSize  = 11;
        instrText.color     = new Color(0.6f, 0.6f, 0.6f);
        instrText.alignment = TextAnchor.MiddleCenter;

        // Animate — rapidly cycle H/T
        float flipDuration = 1.2f;
        float elapsed      = 0f;
        float interval     = 0.08f;
        float nextFlip     = 0f;
        bool  shown        = false;

        Color goldColor   = new Color(0.85f, 0.65f, 0.1f);
        Color silverColor = new Color(0.7f, 0.7f, 0.75f);

        resultText.text  = "Flipping...";
        resultText.color = Color.white;

        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;

            // Slow down toward the end
            interval = Mathf.Lerp(0.08f, 0.25f, elapsed / flipDuration);

            if (elapsed >= nextFlip)
            {
                shown = !shown;
                nextFlip = elapsed + interval;
                coinImg.color  = shown ? goldColor : silverColor;
                coinText.text  = shown ? "H" : "T";
            }

            yield return null;
        }

        // Show final result
        bool heads = finalResult;
        Color resultColor = heads ? goldColor : silverColor;
        coinImg.color    = resultColor;
        coinText.text    = heads ? "H" : "T";
        resultText.text  = heads ? "HEADS" : "TAILS";
        resultText.color = resultColor;

        _isFlipping = false;
    }
}