using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CardContextMenu : MonoBehaviour
{
    private CardObject _card;
    private ZoneManager _zoneManager;

    private static GameObject _menuInstance;
    public static bool IsOpen => _menuInstance != null;

    void Awake()
    {
        _card = GetComponentInParent<CardObject>();
        _zoneManager = FindAnyObjectByType<ZoneManager>();
    }

    void OnMouseOver()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CloseMenu();
            OpenMenu();
        }
    }

    void OpenMenu()
    {
        var canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGO = new GameObject("ContextCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        _menuInstance = new GameObject("ContextMenu");
        _menuInstance.transform.SetParent(canvas.transform, false);

        var rect = _menuInstance.AddComponent<RectTransform>();
        Vector2 mousePos = Mouse.current.position.ReadValue();
        rect.pivot = new Vector2(0, 1);
        rect.anchorMin = rect.anchorMax = Vector2.zero;
        rect.anchoredPosition = mousePos;

        var bg = _menuInstance.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        string[] labels;
        ZoneType[] zones;

        if (_card.CurrentZone == ZoneType.Library)
        {
            labels = new[] { "Draw Card", "Browse Library", "Shuffle Library" };
            zones  = new[] { ZoneType.Hand, ZoneType.Library, ZoneType.Library };
        }
        else if (_card.CurrentZone == ZoneType.Graveyard)
        {
            labels = new[] { "Browse Graveyard", "Return to Hand", "Send to Exile" };
            zones  = new[] { ZoneType.Graveyard, ZoneType.Hand, ZoneType.Exile };
        }
        else if (_card.CurrentZone == ZoneType.Exile)
        {
            labels = new[] { "Browse Exile", "Return to Hand" };
            zones  = new[] { ZoneType.Exile, ZoneType.Hand };
        }
        else
        {
            labels = new[] { "Send to Graveyard", "Send to Exile", "Send to Command", "Return to Hand" };
            zones  = new[] { ZoneType.Graveyard, ZoneType.Exile, ZoneType.Command, ZoneType.Hand };
        }

        int buttonHeight = 40;
        rect.sizeDelta = new Vector2(180, buttonHeight * labels.Length);

        for (int i = 0; i < labels.Length; i++)
            CreateMenuItem(labels[i], zones[i], i, buttonHeight);
    }

    void CreateMenuItem(string label, ZoneType zone, int index, int buttonHeight)
    {
        var itemGO = new GameObject(label);
        itemGO.transform.SetParent(_menuInstance.transform, false);

        var rect = itemGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(0, -index * buttonHeight);
        rect.sizeDelta = new Vector2(0, buttonHeight);

        var bg = itemGO.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        var button = itemGO.AddComponent<Button>();
        button.targetGraphic = bg;

        var colors = button.colors;
        colors.normalColor      = new Color(0.15f, 0.15f, 0.15f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.8f, 0.3f, 1f);
        colors.pressedColor     = new Color(0.1f, 0.4f, 0.1f);
        button.colors = colors;

        var targetCard    = _card;
        var targetZone    = zone;
        var targetManager = _zoneManager;
        var targetLabel   = label;

        button.onClick.AddListener(() =>
        {
            if (targetLabel == "Browse Library" || 
                targetLabel == "Browse Graveyard" || 
                targetLabel == "Browse Exile")
            {
                ZoneBrowser.Instance.OpenBrowser(targetZone);
            }
            else if (targetLabel == "Shuffle Library")
            {
                targetManager.ShuffleLibrary();
            }
            else
            {
                targetManager.MoveCard(targetCard, targetZone);
            }
            CloseMenu();
        });

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(itemGO.transform, false);

        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(10, 0);

        var text = textGO.AddComponent<Text>();
        text.text      = label;
        text.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleLeft;
        text.color     = Color.white;
        text.fontSize  = 16;
    }

    public static void CloseMenu()
    {
        if (_menuInstance != null)
        {
            Destroy(_menuInstance);
            _menuInstance = null;
        }
    }

    void Update()
    {
        if (_menuInstance != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            var menuRect = _menuInstance.GetComponent<RectTransform>();
            if (!RectTransformUtility.RectangleContainsScreenPoint(menuRect, mousePos))
            {
                CloseMenu();
            }
        }
    }
}