using UnityEngine;

public class ZoneCountDisplay : MonoBehaviour
{
    private ZoneManager _zoneManager;

    [Header("Zone Anchors")]
    public Transform libraryAnchor;
    public Transform graveyardAnchor;
    public Transform exileAnchor;
    public Transform commandAnchor;
    public Transform handAnchor;

    private TextMesh _libraryText;
    private TextMesh _graveyardText;
    private TextMesh _exileText;
    private TextMesh _commandText;
    private TextMesh _handText;

    void Awake()
    {
        _zoneManager = FindAnyObjectByType<ZoneManager>();
    }

    void Start()
    {
        _libraryText   = CreateLabel(libraryAnchor,   "Library");
        _graveyardText = CreateLabel(graveyardAnchor, "Graveyard");
        _exileText     = CreateLabel(exileAnchor,     "Exile");
        _commandText   = CreateLabel(commandAnchor,   "Command");
        _handText      = CreateLabel(handAnchor,      "Hand");
    }

    void Update()
    {
        if (_zoneManager == null) return;

        UpdateLabel(_libraryText,   _zoneManager.GetCards(ZoneType.Library).Count);
        UpdateLabel(_graveyardText, _zoneManager.GetCards(ZoneType.Graveyard).Count);
        UpdateLabel(_exileText,     _zoneManager.GetCards(ZoneType.Exile).Count);
        UpdateLabel(_commandText,   _zoneManager.GetCards(ZoneType.Command).Count);
        UpdateLabel(_handText,      _zoneManager.GetCards(ZoneType.Hand).Count);
    }

    void UpdateLabel(TextMesh label, int count)
    {
        if (label == null) return;
        string text = count > 0 ? count.ToString() : "";
        label.text = text;

        foreach (Transform child in label.transform)
        {
            var shadow = child.GetComponent<TextMesh>();
            if (shadow != null) shadow.text = text;
        }
    }

    TextMesh CreateLabel(Transform anchor, string zoneName)
    {
        if (anchor == null) return null;

        var go = new GameObject($"{zoneName}Count");
        go.transform.position = anchor.position + new Vector3(0.23f, 0.02f, -0.36f);
        go.transform.rotation = Quaternion.Euler(90, 0, 0);

        // Black outline — 4 offset copies behind main text
        float offset = 0.008f;
        Vector3[] offsets = {
            new Vector3( offset, 0,  offset),
            new Vector3(-offset, 0,  offset),
            new Vector3( offset, 0, -offset),
            new Vector3(-offset, 0, -offset)
        };

        foreach (var off in offsets)
        {
            var shadowGO = new GameObject("Shadow");
            shadowGO.transform.SetParent(go.transform, false);
            shadowGO.transform.localPosition = off;
            shadowGO.transform.localRotation = Quaternion.identity;
            shadowGO.transform.localScale    = Vector3.one;

            var shadowTM = shadowGO.AddComponent<TextMesh>();
            shadowTM.fontSize      = 60;
            shadowTM.characterSize = 0.02f;
            shadowTM.anchor        = TextAnchor.MiddleCenter;
            shadowTM.alignment     = TextAlignment.Center;
            shadowTM.color         = Color.black;
            shadowTM.fontStyle     = FontStyle.Bold;

            // Push shadow behind main text
            shadowGO.GetComponent<MeshRenderer>().sortingOrder = -1;
        }

        // Main white text
        var tm = go.AddComponent<TextMesh>();
        tm.fontSize      = 60;
        tm.characterSize = 0.02f;
        tm.anchor        = TextAnchor.MiddleCenter;
        tm.alignment     = TextAlignment.Center;
        tm.color         = Color.green;
        tm.fontStyle     = FontStyle.Bold;
        go.GetComponent<MeshRenderer>().sortingOrder = 0;

        return tm;
    }
}