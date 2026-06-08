using System.Collections;
using UnityEngine;

public class CardObject : MonoBehaviour
{
    [Header("Card Identity")]
    public string cardName;
    public string scryfallId;
    public Texture2D cardArt;

    public ZoneType CurrentZone { get; set; } = ZoneType.Library;

    private bool _isTapped;
    public bool IsTapped
    {
        get => _isTapped;
        set { _isTapped = value; }
    }

    private bool _isFaceDown;
    public bool IsFaceDown
    {
        get => _isFaceDown;
        set
        {
            _isFaceDown = value;
            UpdateFaceVisibility();
        }
    }

    public int PowerModifier     { get; set; } = 0;
    public int ToughnessModifier { get; set; } = 0;

    public Texture2D FrontTexture { get; private set; }

    [Header("Visual References")]
    public GameObject frontFace;
    public GameObject backFace;
    public Renderer  frontRenderer;
    public Renderer  backRenderer;

    private Coroutine _moveCoroutine;

    private Texture2D _pendingTexture;
    private Material  _cardMaterial;

    private Texture2D _pendingBackTexture;
    private Material  _cardBackMaterial;

    void Awake()
    {
        if (frontFace == null)     frontFace     = transform.Find("Front")?.gameObject;
        if (backFace == null)      backFace      = transform.Find("Back")?.gameObject;
        if (frontRenderer == null) frontRenderer = frontFace?.GetComponent<Renderer>();
        if (backRenderer == null)  backRenderer  = backFace?.GetComponent<Renderer>();

        if (frontRenderer != null)
        {
            _cardMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _cardMaterial.SetColor("_BaseColor", Color.white);
            frontRenderer.material = _cardMaterial;
        }

        if (backRenderer != null)
        {
            _cardBackMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _cardBackMaterial.SetColor("_BaseColor", Color.white);
            backRenderer.material = _cardBackMaterial;
        }
    }

    void Update()
    {
        if (frontRenderer != null && frontRenderer.material != _cardMaterial)
            frontRenderer.material = _cardMaterial;

        if (_pendingTexture != null && _cardMaterial != null)
        {
            _cardMaterial.SetTexture("_BaseMap", _pendingTexture);
            _pendingTexture = null;
        }

        if (backRenderer != null && backRenderer.material != _cardBackMaterial)
            backRenderer.material = _cardBackMaterial;

        if (_pendingBackTexture != null && _cardBackMaterial != null)
        {
            _cardBackMaterial.SetTexture("_BaseMap", _pendingBackTexture);
            _pendingBackTexture = null;
        }
    }

    public void SetCardTexture(Texture2D tex)
    {
        _pendingTexture = tex;
        FrontTexture    = tex;
    }

    public void SetCardBackTexture(Texture2D tex)
    {
        _pendingBackTexture = tex;
    }

    public void ApplyTapRotation()
    {
        float targetY = _isTapped ? 90f : 0f;
        StopCoroutine_Safe(ref _moveCoroutine);
        _moveCoroutine = StartCoroutine(AnimateRotationTo(Quaternion.Euler(90, targetY, 0), 0.15f));
    }

    public void AnimateTo(Vector3 targetPos, Quaternion targetRot, float speed)
    {
        StopCoroutine_Safe(ref _moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveRoutine(targetPos, targetRot, speed));
    }

    IEnumerator MoveRoutine(Vector3 targetPos, Quaternion targetRot, float speed)
    {
        float arcHeight = 0.15f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            float eased = EaseInOut(Mathf.Clamp01(t));

            Vector3 linearPos = Vector3.Lerp(startPos, targetPos, eased);
            float arc = Mathf.Sin(eased * Mathf.PI) * arcHeight;
            transform.position = linearPos + Vector3.up * arc;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, eased);
            yield return null;
        }
        transform.position = targetPos;
        transform.rotation = targetRot;
    }

    IEnumerator AnimateRotationTo(Quaternion target, float duration)
    {
        Quaternion start = transform.localRotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localRotation = Quaternion.Slerp(start, target, EaseInOut(Mathf.Clamp01(t)));
            yield return null;
        }
        transform.localRotation = target;
    }

    void UpdateFaceVisibility()
    {
        if (frontFace != null) frontFace.SetActive(!_isFaceDown);
        if (backFace  != null) backFace.SetActive(_isFaceDown);
    }

    static float EaseInOut(float t) => t < 0.5f
        ? 2f * t * t
        : -1f + (4f - 2f * t) * t;

    void StopCoroutine_Safe(ref Coroutine c)
    {
        if (c != null) { StopCoroutine(c); c = null; }
    }
}