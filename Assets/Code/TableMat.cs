using UnityEngine;

public class TableMat : MonoBehaviour
{
    [Header("Mat Settings")]
    public int textureWidth  = 2048;
    public int textureHeight = 2048;

    private Color _feltGreen   = new Color(0.13f, 0.37f, 0.18f);
    private Color _zoneOutline = new Color(0.8f, 0.75f, 0.5f, 0.6f);
    private Color _labelColor  = new Color(0.9f, 0.85f, 0.6f);

    private Texture2D _matTexture;
    private GameObject _matQuad;

    void Awake()
    {
        GenerateMat();
    }

    void GenerateMat()
    {
        _matTexture = new Texture2D(textureWidth, textureHeight);

        FillBackground();
        DrawFeltNoise();

        // Bottom to top: Library, Graveyard, Exile, Commander
        DrawZone(0.04f, 0.72f, 0.09f, 0.126f, "CMD");
        DrawZone(0.04f, 0.57f, 0.09f, 0.126f, "EXL");
        DrawZone(0.04f, 0.42f, 0.09f, 0.126f, "GRV");
        DrawZone(0.04f, 0.26f, 0.09f, 0.126f, "LIB");

        // Battlefield - center
        DrawZone(0.16f, 0.26f, 0.80f, 0.60f, "BATTLEFIELD");

        // Hand - bottom
        DrawZone(0.04f, 0.05f, 0.92f, 0.18f, "HAND");

        _matTexture.Apply();

        _matQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _matQuad.name = "TableMat";
        _matQuad.transform.position = new Vector3(0, -0.001f, 0);
        _matQuad.transform.rotation = Quaternion.Euler(90, 0, 0);
        _matQuad.transform.localScale = new Vector3(7f, 7f, 1f);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.SetTexture("_BaseMap", _matTexture);
        mat.SetColor("_BaseColor", Color.white);
        _matQuad.GetComponent<Renderer>().material = mat;

        Destroy(_matQuad.GetComponent<MeshCollider>());
    }

    void FillBackground()
    {
        for (int x = 0; x < textureWidth; x++)
            for (int y = 0; y < textureHeight; y++)
                _matTexture.SetPixel(x, y, _feltGreen);
    }

    void DrawFeltNoise()
    {
        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                float noise = Mathf.PerlinNoise(x * 0.02f, y * 0.02f) * 0.04f;
                Color base_ = _matTexture.GetPixel(x, y);
                _matTexture.SetPixel(x, y, new Color(
                    base_.r + noise,
                    base_.g + noise,
                    base_.b + noise
                ));
            }
        }
    }

    void DrawZone(float nx, float ny, float nw, float nh, string label)
    {
        int x = Mathf.RoundToInt(nx * textureWidth);
        int y = Mathf.RoundToInt(ny * textureHeight);
        int w = Mathf.RoundToInt(nw * textureWidth);
        int h = Mathf.RoundToInt(nh * textureHeight);

        // Zone overlay
        for (int px = x; px < x + w; px++)
            for (int py = y; py < y + h; py++)
                if (px < textureWidth && py < textureHeight)
                    _matTexture.SetPixel(px, py,
                        Color.Lerp(_matTexture.GetPixel(px, py), Color.black, 0.12f));

        // Border
        int border = 4;
        for (int px = x; px < x + w; px++)
        {
            for (int b = 0; b < border; b++)
            {
                SetPixelSafe(px, y + b, _zoneOutline);
                SetPixelSafe(px, y + h - 1 - b, _zoneOutline);
            }
        }
        for (int py = y; py < y + h; py++)
        {
            for (int b = 0; b < border; b++)
            {
                SetPixelSafe(x + b, py, _zoneOutline);
                SetPixelSafe(x + w - 1 - b, py, _zoneOutline);
            }
        }

        DrawLabel(label, x + w / 2, y + h / 2);
    }

    void DrawLabel(string text, int cx, int cy)
    {
        int scale      = 3;
        int charWidth  = 6 * scale;
        int charHeight = 8 * scale;
        int totalWidth = text.Length * charWidth;
        int startX = cx - totalWidth / 2;
        int startY = cy - charHeight / 2;

        for (int i = 0; i < text.Length; i++)
        {
            bool[,] glyph = GetGlyph(text[i]);
            for (int row = 0; row < 7; row++)
                for (int col = 0; col < 5; col++)
                    if (glyph[row, col])
                        for (int sx = 0; sx < scale; sx++)
                            for (int sy = 0; sy < scale; sy++)
                                SetPixelSafe(
                                    startX + i * charWidth + col * scale + sx,
                                    startY + (6 - row) * scale + sy,
                                    _labelColor);
        }
    }

    void SetPixelSafe(int x, int y, Color c)
    {
        if (x >= 0 && x < textureWidth && y >= 0 && y < textureHeight)
            _matTexture.SetPixel(x, y, c);
    }

    bool[,] GetGlyph(char c)
    {
        switch (char.ToUpper(c))
        {
            case 'A': return new bool[,] {
                {false,true,true,true,false},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,true,true,true,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true}};
            case 'B': return new bool[,] {
                {true,true,true,true,false},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,true,true,true,false},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,true,true,true,false}};
            case 'C': return new bool[,] {
                {false,true,true,true,false},
                {true,false,false,false,true},
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,false,false,false,true},
                {false,true,true,true,false}};
            case 'D': return new bool[,] {
                {true,true,true,false,false},
                {true,false,false,true,false},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,true,false},
                {true,true,true,false,false}};
            case 'E': return new bool[,] {
                {true,true,true,true,true},
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,true,true,true,false},
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,true,true,true,true}};
            case 'F': return new bool[,] {
                {true,true,true,true,true},
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,true,true,true,false},
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,false,false,false,false}};
            case 'G': return new bool[,] {
                {false,true,true,true,false},
                {true,false,false,false,true},
                {true,false,false,false,false},
                {true,false,true,true,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {false,true,true,true,false}};
            case 'H': return new bool[,] {
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,true,true,true,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true}};
            case 'I': return new bool[,] {
                {true,true,true,true,true},
                {false,false,true,false,false},
                {false,false,true,false,false},
                {false,false,true,false,false},
                {false,false,true,false,false},
                {false,false,true,false,false},
                {true,true,true,true,true}};
            case 'L': return new bool[,] {
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,false,false,false,false},
                {true,true,true,true,true}};
            case 'M': return new bool[,] {
                {true,false,false,false,true},
                {true,true,false,true,true},
                {true,false,true,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true}};
            case 'N': return new bool[,] {
                {true,false,false,false,true},
                {true,true,false,false,true},
                {true,false,true,false,true},
                {true,false,false,true,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true}};
            case 'O': return new bool[,] {
                {false,true,true,true,false},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {false,true,true,true,false}};
            case 'R': return new bool[,] {
                {true,true,true,true,false},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,true,true,true,false},
                {true,false,true,false,false},
                {true,false,false,true,false},
                {true,false,false,false,true}};
            case 'S': return new bool[,] {
                {false,true,true,true,false},
                {true,false,false,false,true},
                {true,false,false,false,false},
                {false,true,true,true,false},
                {false,false,false,false,true},
                {true,false,false,false,true},
                {false,true,true,true,false}};
            case 'T': return new bool[,] {
                {true,true,true,true,true},
                {false,false,true,false,false},
                {false,false,true,false,false},
                {false,false,true,false,false},
                {false,false,true,false,false},
                {false,false,true,false,false},
                {false,false,true,false,false}};
            case 'V': return new bool[,] {
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {false,true,false,true,false},
                {false,false,true,false,false}};
            case 'W': return new bool[,] {
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,true,false,true},
                {true,false,true,false,true},
                {true,true,false,true,true},
                {true,false,false,false,true}};
            case 'X': return new bool[,] {
                {true,false,false,false,true},
                {true,false,false,false,true},
                {false,true,false,true,false},
                {false,false,true,false,false},
                {false,true,false,true,false},
                {true,false,false,false,true},
                {true,false,false,false,true}};
            case 'Y': return new bool[,] {
                {true,false,false,false,true},
                {true,false,false,false,true},
                {false,true,false,true,false},
                {false,false,true,false,false},
                {false,false,true,false,false},
                {false,false,true,false,false},
                {false,false,true,false,false}};
            case 'Z': return new bool[,] {
                {true,true,true,true,true},
                {false,false,false,false,true},
                {false,false,false,true,false},
                {false,false,true,false,false},
                {false,true,false,false,false},
                {true,false,false,false,false},
                {true,true,true,true,true}};
            case ' ': return new bool[,] {
                {false,false,false,false,false},
                {false,false,false,false,false},
                {false,false,false,false,false},
                {false,false,false,false,false},
                {false,false,false,false,false},
                {false,false,false,false,false},
                {false,false,false,false,false}};
            default: return new bool[,] {
                {true,true,true,true,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,false,false,false,true},
                {true,true,true,true,true}};
        }
    }
}