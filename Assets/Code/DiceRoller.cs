using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DiceRoller : MonoBehaviour
{
    private GameObject _panel;
    private Canvas _canvas;
    private bool _isRolling;

    void Awake()
    {
        _canvas = FindAnyObjectByType<Canvas>();
        if (_canvas == null)
        {
            var cGO = new GameObject("DiceCanvas");
            _canvas = cGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cGO.AddComponent<CanvasScaler>();
            cGO.AddComponent<GraphicRaycaster>();
        }
    }

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame && !_isRolling)
            StartCoroutine(RollRoutine());

        if (Keyboard.current.escapeKey.wasPressedThisFrame && _panel != null && !_isRolling)
        {
            Destroy(_panel);
            _panel = null;
        }
    }

    IEnumerator RollRoutine()
    {
        _isRolling = true;
        if (_panel != null) Destroy(_panel);

        int finalResult = Random.Range(1, 21);

        // Build panel
        _panel = new GameObject("DicePanel");
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
        titleText.text      = "D20 ROLL";
        titleText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize  = 16;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color     = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;

        // Dice display
        var diceGO = new GameObject("Dice");
        diceGO.transform.SetParent(_panel.transform, false);
        var diceRT = diceGO.AddComponent<RectTransform>();
        diceRT.anchorMin = new Vector2(0.15f, 0.3f);
        diceRT.anchorMax = new Vector2(0.85f, 0.85f);
        diceRT.sizeDelta = Vector2.zero;
        var diceImg = diceGO.AddComponent<Image>();
        diceImg.color = new Color(0.2f, 0.2f, 0.6f);

        var diceTextGO = new GameObject("DiceText");
        diceTextGO.transform.SetParent(diceGO.transform, false);
        var diceTextRT = diceTextGO.AddComponent<RectTransform>();
        diceTextRT.anchorMin = Vector2.zero;
        diceTextRT.anchorMax = Vector2.one;
        diceTextRT.sizeDelta = Vector2.zero;
        var diceText = diceTextGO.AddComponent<Text>();
        diceText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        diceText.fontSize  = 52;
        diceText.fontStyle = FontStyle.Bold;
        diceText.color     = Color.white;
        diceText.alignment = TextAnchor.MiddleCenter;

        // Result label
        var resultGO = new GameObject("Result");
        resultGO.transform.SetParent(_panel.transform, false);
        var resultRT = resultGO.AddComponent<RectTransform>();
        resultRT.anchorMin = new Vector2(0, 0.15f);
        resultRT.anchorMax = new Vector2(1, 0.3f);
        resultRT.sizeDelta = Vector2.zero;
        var resultText = resultGO.AddComponent<Text>();
        resultText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        resultText.fontSize  = 18;
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
        instrText.text      = "Press R to roll again  |  ESC to close";
        instrText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instrText.fontSize  = 11;
        instrText.color     = new Color(0.6f, 0.6f, 0.6f);
        instrText.alignment = TextAnchor.MiddleCenter;

        // Animate — rapidly cycle numbers
        float rollDuration = 1.2f;
        float elapsed      = 0f;
        float interval     = 0.06f;
        float nextRoll     = 0f;

        resultText.text  = "Rolling...";
        resultText.color = Color.white;

        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;
            interval = Mathf.Lerp(0.06f, 0.2f, elapsed / rollDuration);

            if (elapsed >= nextRoll)
            {
                nextRoll = elapsed + interval;
                int randNum = Random.Range(1, 21);
                diceText.text = randNum.ToString();
                diceImg.color = GetDiceColor(randNum);
            }

            yield return null;
        }

        // Show final result
        diceText.text    = finalResult.ToString();
        diceImg.color    = GetDiceColor(finalResult);
        resultText.text  = GetResultLabel(finalResult);
        resultText.color = GetDiceColor(finalResult);

        _isRolling = false;
    }

    Color GetDiceColor(int result)
    {
        if (result == 20) return new Color(1f, 0.8f, 0.1f);   // gold — nat 20
        if (result == 1)  return new Color(0.8f, 0.1f, 0.1f); // red — nat 1
        if (result >= 15) return new Color(0.2f, 0.7f, 0.3f); // green — high
        if (result <= 5)  return new Color(0.7f, 0.3f, 0.2f); // orange — low
        return new Color(0.2f, 0.2f, 0.6f);                   // default blue
    }

    string GetResultLabel(int result)
    {
        if (result == 20) return "NATURAL 20!";
        if (result == 1)  return "NATURAL 1...";
        if (result >= 15) return "Great Roll!";
        if (result <= 5)  return "Rough Roll.";
        return $"Rolled a {result}";
    }
}