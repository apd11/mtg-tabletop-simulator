using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckLoader : MonoBehaviour
{
    public static DeckLoader Instance;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Parse a decklist string in MTGO format (e.g. "4 Lightning Bolt")
    /// Returns a flat list of card names (duplicates included)
    /// </summary>
    public List<string> ParseDecklist(string decklist)
    {
        var cards = new List<string>();
        var lines = decklist.Split('\n');

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            if (trimmed.StartsWith("//")) continue; // skip comments

            // Split on first space only
            int spaceIndex = trimmed.IndexOf(' ');
            if (spaceIndex < 0) continue;

            var countStr = trimmed.Substring(0, spaceIndex).Trim();
            var cardName = trimmed.Substring(spaceIndex + 1).Trim();

            if (!int.TryParse(countStr, out int count)) continue;

            for (int i = 0; i < count; i++)
                cards.Add(cardName);
        }

        return cards;
    }

    /// <summary>
    /// Spawn a full deck into the library zone and load art for each unique card
    /// </summary>
    public IEnumerator LoadDeck(string decklist, ZoneManager zoneManager, GameObject cardPrefab)
    {
        var cardNames = ParseDecklist(decklist);
        Debug.Log($"[DeckLoader] Loading {cardNames.Count} cards...");

        // Track unique names to avoid duplicate Scryfall requests
        var seen = new HashSet<string>();

        foreach (var cardName in cardNames)
        {
            var cardGO = Instantiate(cardPrefab);
            var card   = cardGO.GetComponent<CardObject>();
            card.cardName = cardName;
            zoneManager.MoveCard(card, ZoneType.Library);
        }

        // Load art for unique cards only
        foreach (var cardName in cardNames)
        {
            if (seen.Contains(cardName)) continue;
            seen.Add(cardName);

            // Find all cards with this name in library
            foreach (var card in zoneManager.GetCards(ZoneType.Library))
            {
                if (card.cardName == cardName)
                    ScryfallLoader.Instance.LoadArt(card, cardName);
            }

            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log("[DeckLoader] Deck loaded.");
    }
}