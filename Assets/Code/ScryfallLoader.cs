using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ScryfallLoader : MonoBehaviour
{
    public static ScryfallLoader Instance;

    void Awake()
    {
        Instance = this;
    }

    public void LoadArt(CardObject card, string cardName)
    {
        StartCoroutine(FetchCard(card, cardName));
    }

    IEnumerator FetchCard(CardObject card, string cardName)
    {
        string url = $"https://api.scryfall.com/cards/named?fuzzy={UnityWebRequest.EscapeURL(cardName)}";

        int maxRetries = 3;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            using var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("User-Agent", "MTGSimulator/0.1 contact@youremail.com");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var json = request.downloadHandler.text;

                if (json.Contains("\"card_faces\""))
                {
                    string frontUrl = ParseFaceImageUrl(json, 0);
                    string backUrl  = ParseFaceImageUrl(json, 1);

                    if (!string.IsNullOrEmpty(frontUrl))
                        yield return DownloadArt(card, frontUrl, isFront: true);
                    if (!string.IsNullOrEmpty(backUrl))
                        yield return DownloadArt(card, backUrl, isFront: false);
                }
                else
                {
                    string frontUrl = ParseImageUrl(json);
                    if (!string.IsNullOrEmpty(frontUrl))
                        yield return DownloadArt(card, frontUrl, isFront: true);

                    yield return DownloadCardBack(card);
                }
                yield break;
            }
            else if (request.responseCode == 429)
            {
                Debug.LogWarning($"[Scryfall] Rate limited on {cardName}, retrying in {(attempt + 1) * 2}s...");
                yield return new WaitForSeconds((attempt + 1) * 2f);
            }
            else
            {
                Debug.LogWarning($"[Scryfall] Failed to fetch {cardName}: {request.error}");
                yield break;
            }
        }
    }

    IEnumerator DownloadArt(CardObject card, string imageUrl, bool isFront = true)
    {
        using var request = UnityWebRequestTexture.GetTexture(imageUrl);
        request.SetRequestHeader("User-Agent", "MTGSimulator/0.1 contact@youremail.com");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[Scryfall] Image download failed: {request.error}");
            yield break;
        }

        var tex = DownloadHandlerTexture.GetContent(request);
        tex = DuplicateTexture(tex);

        if (isFront) card.SetCardTexture(tex);
        else         card.SetCardBackTexture(tex);
    }

    IEnumerator DownloadCardBack(CardObject card)
    {
        string backUrl = "https://upload.wikimedia.org/wikipedia/en/a/aa/Magic_the_gathering-card_back.jpg";
        using var request = UnityWebRequestTexture.GetTexture(backUrl);
        request.SetRequestHeader("User-Agent", "MTGSimulator/0.1 contact@youremail.com");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[Scryfall] Card back download failed: {request.error}");
            yield break;
        }

        var tex = DownloadHandlerTexture.GetContent(request);
        tex = DuplicateTexture(tex);
        card.SetCardBackTexture(tex);
    }

    Texture2D DuplicateTexture(Texture2D source)
    {
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height);
        Graphics.Blit(source, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D readable = new Texture2D(source.width, source.height);
        readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readable.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return readable;
    }

    string ParseFaceImageUrl(string json, int faceIndex)
    {
        const string facesKey = "\"card_faces\"";
        int facesIdx = json.IndexOf(facesKey);
        if (facesIdx < 0) return null;

        int searchFrom = facesIdx;
        for (int i = 0; i <= faceIndex; i++)
        {
            searchFrom = json.IndexOf("{", searchFrom + 1);
            if (searchFrom < 0) return null;
        }

        const string key = "\"png\":\"";
        int idx = json.IndexOf(key, searchFrom);
        if (idx < 0) return null;
        int start = idx + key.Length;
        int end = json.IndexOf("\"", start);
        return json.Substring(start, end - start);
    }

    string ParseImageUrl(string json)
    {
        const string key = "\"png\":\"";
        int idx = json.IndexOf(key);
        if (idx < 0) return null;
        int start = idx + key.Length;
        int end = json.IndexOf("\"", start);
        return json.Substring(start, end - start);
    }
}