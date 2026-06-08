using UnityEngine;

public class CardPreview : MonoBehaviour
{
    public static CardPreview Instance;

    private GameObject _previewQuad;
    private Renderer _previewRenderer;
    private CardObject _currentCard;
    private float _hideTimer = 0f;
    private bool _hiding = false;

    void Awake()
    {
        Instance = this;

        _previewQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _previewQuad.name = "CardPreviewQuad";
        _previewQuad.transform.position = new Vector3(4.45f, 0.01f, 0f);
        _previewQuad.transform.rotation = Quaternion.Euler(90, 0, 0);
        _previewQuad.transform.localScale = new Vector3(1.8f, 2.52f, 1f);
        _previewQuad.SetActive(false);

        _previewRenderer = _previewQuad.GetComponent<Renderer>();
        _previewRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        _previewRenderer.material.SetColor("_BaseColor", Color.white);
    }

    void Update()
    {
        if (_hiding)
        {
            _hideTimer -= Time.deltaTime;
            if (_hideTimer <= 0f)
            {
                _previewQuad.SetActive(false);
                _currentCard = null;
                _hiding = false;
            }
        }
    }

    public void ShowPreview(CardObject card)
    {
        _hiding = false;
        _hideTimer = 0f;
        _currentCard = card;

        var tex = card.frontRenderer?.material?.GetTexture("_BaseMap");
        if (tex != null)
        {
            _previewRenderer.material.SetTexture("_BaseMap", tex);
            _previewQuad.SetActive(true);
        }
    }

    public void HidePreview()
    {
        _hiding = true;
        _hideTimer = 0.3f;
    }
}