using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZoneBrowser : MonoBehaviour
{
    public static ZoneBrowser Instance;

    private GameObject _panel;
    private ZoneManager _zoneManager;
    private bool _isOpen;

    void Awake()
    {
        Instance = this;
        _zoneManager = FindAnyObjectByType<ZoneManager>();
    }

    public void OpenBrowser(ZoneType zone)
    {
        if (_isOpen) CloseBrowser();

        var cards = _zoneManager.GetCards(zone);
        Debug.Log($"[ZoneBrowser] Opening {zone} with {cards.Count} cards");

        if (cards.Count == 0)
        {
            Debug.Log($"[ZoneBrowser] {zone} is empty.");
            return;
        }

        _isOpen = true;

        var canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            var cGO = new GameObject("BrowserCanvas");
            canvas = cGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cGO.AddComponent<CanvasScaler>();
            cGO.AddComponent<GraphicRaycaster>();
        }

        canvas.sortingOrder = 10;

        _panel = new GameObject("ZoneBrowserPanel");
        _panel.transform.SetParent(canvas.transform, false);

        var panelRect = _panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        var panelBg = _panel.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.05f, 0.6f);

        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(_panel.transform, false);
        var titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.85f);
        titleRect.anchorMax = new Vector2(0.9f, 0.95f);
        titleRect.sizeDelta = Vector2.zero;
        var titleText = titleGO.AddComponent<Text>();
        titleText.text      = $"{zone} ({cards.Count} cards)";
        titleText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize  = 28;
        titleText.color     = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;

        CreateCloseButton();
        CreateCardGrid(cards, zone);
    }

    void CreateCloseButton()
    {
        var closeGO = new GameObject("CloseButton");
        closeGO.transform.SetParent(_panel.transform, false);

        var rect = closeGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.9f, 0.88f);
        rect.anchorMax = new Vector2(0.98f, 0.96f);
        rect.sizeDelta = Vector2.zero;

        var bg = closeGO.AddComponent<Image>();
        bg.color = new Color(0.6f, 0.1f, 0.1f);

        var button = closeGO.AddComponent<Button>();
        button.targetGraphic = bg;
        var colors = button.colors;
        colors.highlightedColor = new Color(0.8f, 0.2f, 0.2f);
        button.colors = colors;
        button.onClick.AddListener(CloseBrowser);

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(closeGO.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        var text = textGO.AddComponent<Text>();
        text.text      = "X";
        text.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize  = 20;
        text.color     = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
    }

    void CreateCardGrid(IReadOnlyList<CardObject> cards, ZoneType zone)
    {
        int cardWidth  = 120;
        int cardHeight = 168;
        int padding    = 10;
        int cols       = 6;
        int startX     = padding;
        int startY     = -padding;

        for (int i = 0; i < cards.Count; i++)
        {
            int col = i % cols;
            int row = i / cols;

            var cardGO = new GameObject($"Card_{i}");
            cardGO.transform.SetParent(_panel.transform, false);

            var cardRT = cardGO.AddComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0, 1);
            cardRT.anchorMax = new Vector2(0, 1);
            cardRT.pivot     = new Vector2(0, 1);
            cardRT.sizeDelta = new Vector2(cardWidth, cardHeight);
            cardRT.anchoredPosition = new Vector2(
                startX + col * (cardWidth + padding),
                startY - row * (cardHeight + padding)
            );

            var bg = cardGO.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f);

            var tex = cards[i].FrontTexture;
            if (tex != null && tex.width > 0 && tex.height > 0)
            {
                try
                {
                    bg.sprite = Sprite.Create(tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f));
                    bg.color = Color.white;
                }
                catch
                {
                    bg.color = new Color(0.2f, 0.2f, 0.2f);
                }
            }

            var button = cardGO.AddComponent<Button>();
            var capturedCard = cards[i];
            var capturedZone = zone;
            button.onClick.AddListener(() => OpenCardOptions(capturedCard, capturedZone));

            var nameGO = new GameObject("Name");
            nameGO.transform.SetParent(cardGO.transform, false);
            var nameRT = nameGO.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0);
            nameRT.anchorMax = new Vector2(1, 0.2f);
            nameRT.sizeDelta = Vector2.zero;
            nameGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);

            var nameTextGO = new GameObject("NameText");
            nameTextGO.transform.SetParent(nameGO.transform, false);
            var nameTextRT = nameTextGO.AddComponent<RectTransform>();
            nameTextRT.anchorMin = Vector2.zero;
            nameTextRT.anchorMax = Vector2.one;
            nameTextRT.sizeDelta = Vector2.zero;
            var nameText = nameTextGO.AddComponent<Text>();
            nameText.text      = cards[i].cardName;
            nameText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize  = 10;
            nameText.color     = Color.white;
            nameText.alignment = TextAnchor.MiddleCenter;
        }
    }

    void OpenCardOptions(CardObject card, ZoneType zone)
    {
        var existing = _panel.transform.Find("CardOptions");
        if (existing != null) Destroy(existing.gameObject);

        // Dim overlay
        var overlayGO = new GameObject("CardOptions");
        overlayGO.transform.SetParent(_panel.transform, false);
        var overlayRT = overlayGO.AddComponent<RectTransform>();
        overlayRT.anchorMin = Vector2.zero;
        overlayRT.anchorMax = Vector2.one;
        overlayRT.sizeDelta = Vector2.zero;
        overlayGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);

        // Modal box
        var modalGO = new GameObject("Modal");
        modalGO.transform.SetParent(overlayGO.transform, false);
        var modalRT = modalGO.AddComponent<RectTransform>();
        modalRT.anchorMin = new Vector2(0.35f, 0.3f);
        modalRT.anchorMax = new Vector2(0.65f, 0.75f);
        modalRT.sizeDelta = Vector2.zero;
        modalGO.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.12f, 1f);

        // Card art
        var artGO = new GameObject("Art");
        artGO.transform.SetParent(modalGO.transform, false);
        var artRT = artGO.AddComponent<RectTransform>();
        artRT.anchorMin = new Vector2(0.05f, 0.55f);
        artRT.anchorMax = new Vector2(0.95f, 0.95f);
        artRT.sizeDelta = Vector2.zero;
        var artImg = artGO.AddComponent<Image>();
        artImg.color = new Color(0.2f, 0.2f, 0.2f);
        var tex = card.FrontTexture;
        if (tex != null && tex.width > 0 && tex.height > 0)
        {
            try
            {
                artImg.sprite = Sprite.Create(tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f));
                artImg.color = Color.white;
                artImg.preserveAspect = true;
            }
            catch { }
        }

        // Card name
        var nameGO = new GameObject("CardName");
        nameGO.transform.SetParent(modalGO.transform, false);
        var nameRT = nameGO.AddComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0.05f, 0.44f);
        nameRT.anchorMax = new Vector2(0.95f, 0.54f);
        nameRT.sizeDelta = Vector2.zero;
        var nameText = nameGO.AddComponent<Text>();
        nameText.text      = card.cardName;
        nameText.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize  = 18;
        nameText.color     = Color.white;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.fontStyle = FontStyle.Bold;

        // Divider
        var divGO = new GameObject("Divider");
        divGO.transform.SetParent(modalGO.transform, false);
        var divRT = divGO.AddComponent<RectTransform>();
        divRT.anchorMin = new Vector2(0.05f, 0.425f);
        divRT.anchorMax = new Vector2(0.95f, 0.435f);
        divRT.sizeDelta = Vector2.zero;
        divGO.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);

        // Action buttons
        string[]   labels  = { "To Hand", "Battlefield", "Graveyard", "Exile" };
        Color[]    colors  = {
            new Color(0.2f, 0.6f, 0.2f),
            new Color(0.2f, 0.4f, 0.8f),
            new Color(0.5f, 0.15f, 0.15f),
            new Color(0.4f, 0.2f, 0.6f)
        };
        ZoneType[] targets = { ZoneType.Hand, ZoneType.Battlefield, ZoneType.Graveyard, ZoneType.Exile };

        var validButtons = new List<int>();
        for (int i = 0; i < labels.Length; i++)
            if (targets[i] != zone) validButtons.Add(i);

        float btnHeight  = 0.08f;
        float btnSpacing = 0.02f;
        float totalH     = validButtons.Count * (btnHeight + btnSpacing) - btnSpacing;
        float startY     = 0.05f + totalH;

        for (int b = 0; b < validButtons.Count; b++)
        {
            int i = validButtons[b];

            var btnGO = new GameObject(labels[i]);
            btnGO.transform.SetParent(modalGO.transform, false);

            float yMax = startY - b * (btnHeight + btnSpacing);
            float yMin = yMax - btnHeight;

            var btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.05f, yMin);
            btnRT.anchorMax = new Vector2(0.95f, yMax);
            btnRT.sizeDelta = Vector2.zero;

            var btnBg = btnGO.AddComponent<Image>();
            btnBg.color = colors[i];

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnBg;
            var btnColors = btn.colors;
            btnColors.normalColor      = colors[i];
            btnColors.highlightedColor = colors[i] * 1.4f;
            btnColors.pressedColor     = colors[i] * 0.7f;
            btn.colors = btnColors;

            var capturedTarget = targets[i];
            var capturedCard   = card;

            btn.onClick.AddListener(() =>
            {
                if (capturedTarget == ZoneType.Battlefield)
                {
                    foreach (var z in new[] {
                        _zoneManager.hand,
                        _zoneManager.battlefield,
                        _zoneManager.graveyard,
                        _zoneManager.exile,
                        _zoneManager.command })
                        z.cards.Remove(capturedCard);

                    _zoneManager.battlefield.cards.Add(capturedCard);
                    capturedCard.CurrentZone = ZoneType.Battlefield;
                    capturedCard.IsFaceDown  = false;
                    capturedCard.transform.rotation = Quaternion.Euler(90, 0, 0);
                    capturedCard.transform.position = new Vector3(0, 0.01f, 0);

                    var hover = capturedCard.GetComponent<CardHover>();
                    if (hover == null) capturedCard.gameObject.AddComponent<CardHover>();
                }
                else
                {
                    _zoneManager.MoveCard(capturedCard, capturedTarget);
                }
                CloseBrowser();
            });

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            var text = textGO.AddComponent<Text>();
            text.text      = labels[i];
            text.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize  = 15;
            text.color     = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
        }

        // Close button
        var closeGO = new GameObject("Close");
        closeGO.transform.SetParent(modalGO.transform, false);
        var closeRT = closeGO.AddComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(0.82f, 0.92f);
        closeRT.anchorMax = new Vector2(0.97f, 0.99f);
        closeRT.sizeDelta = Vector2.zero;
        var closeBg = closeGO.AddComponent<Image>();
        closeBg.color = new Color(0.5f, 0.1f, 0.1f);
        var closeBtn = closeGO.AddComponent<Button>();
        closeBtn.targetGraphic = closeBg;
        var closeColors = closeBtn.colors;
        closeColors.highlightedColor = new Color(0.8f, 0.2f, 0.2f);
        closeBtn.colors = closeColors;
        closeBtn.onClick.AddListener(() => Destroy(overlayGO));

        var closeTxtGO = new GameObject("Text");
        closeTxtGO.transform.SetParent(closeGO.transform, false);
        var closeTxtRT = closeTxtGO.AddComponent<RectTransform>();
        closeTxtRT.anchorMin = Vector2.zero;
        closeTxtRT.anchorMax = Vector2.one;
        closeTxtRT.sizeDelta = Vector2.zero;
        var closeTxt = closeTxtGO.AddComponent<Text>();
        closeTxt.text      = "✕";
        closeTxt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeTxt.fontSize  = 14;
        closeTxt.color     = Color.white;
        closeTxt.alignment = TextAnchor.MiddleCenter;
    }

    public void CloseBrowser()
    {
        if (_panel != null) Destroy(_panel);
        _isOpen = false;
    }
}