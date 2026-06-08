using System.Collections.Generic;
using UnityEngine;

public enum ZoneType
{
    Hand,
    Battlefield,
    Library,
    Graveyard,
    Exile,
    Command,
    Stack
}

[System.Serializable]
public class CardZone
{
    public ZoneType zoneType;
    public Transform anchor;
    public bool isFaceDown;
    public bool isOrdered;
    public int maxCards = -1;

    [HideInInspector]
    public List<CardObject> cards = new List<CardObject>();

    public float cardSpacing = 0.03f;
    public float fanAngle = 8f;
}

public class ZoneManager : MonoBehaviour
{
    [Header("Zone Anchors — assign in Inspector")]
    public CardZone hand = new CardZone { zoneType = ZoneType.Hand, isOrdered = false, isFaceDown = false };
    public CardZone battlefield = new CardZone { zoneType = ZoneType.Battlefield, isOrdered = false, isFaceDown = false, cardSpacing = 0.8f };
    public CardZone library = new CardZone { zoneType = ZoneType.Library, isOrdered = true, isFaceDown = true };
    public CardZone graveyard = new CardZone { zoneType = ZoneType.Graveyard, isOrdered = true, isFaceDown = false };
    public CardZone exile = new CardZone { zoneType = ZoneType.Exile, isOrdered = false, isFaceDown = false };
    public CardZone command = new CardZone { zoneType = ZoneType.Command, isOrdered = false, isFaceDown = false };

    [Header("Animation")]
    public float cardMoveSpeed = 5f;
    public bool animateCardMoves = true;

    private Dictionary<ZoneType, CardZone> _zoneMap;

    void Awake()
    {
        _zoneMap = new Dictionary<ZoneType, CardZone>
        {
            { ZoneType.Hand,        hand        },
            { ZoneType.Battlefield, battlefield },
            { ZoneType.Library,     library     },
            { ZoneType.Graveyard,   graveyard   },
            { ZoneType.Exile,       exile       },
            { ZoneType.Command,     command     },
        };
    }

    public void MoveCard(CardObject card, ZoneType destination, bool toTop = true)
    {
        foreach (var zone in _zoneMap.Values)
            zone.cards.Remove(card);

        var dest = _zoneMap[destination];
        if (toTop) dest.cards.Add(card);
        else dest.cards.Insert(0, card);

        card.CurrentZone = destination;
        card.IsFaceDown = dest.isFaceDown;

        RefreshZoneLayout(destination);
    }

    public void DrawCards(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            if (library.cards.Count == 0)
            {
                Debug.LogWarning($"[ZoneManager] {gameObject.name}: Library empty.");
                return;
            }
            MoveCard(library.cards[^1], ZoneType.Hand);
        }
    }

    public void SetTapped(CardObject card, bool tapped)
    {
        if (card.CurrentZone != ZoneType.Battlefield) return;
        card.IsTapped = tapped;
        card.ApplyTapRotation();
    }

    public void UntapAll()
    {
        foreach (var card in battlefield.cards)
        {
            card.IsTapped = false;
            card.ApplyTapRotation();
        }
    }

    public void ShuffleLibrary()
    {
        var cards = library.cards;
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
        RefreshZoneLayout(ZoneType.Library);
        Debug.Log("[ZoneManager] Library shuffled.");
    }

    public IReadOnlyList<CardObject> GetCards(ZoneType zone) => _zoneMap[zone].cards;

    public void RefreshZoneLayout(ZoneType zone) => LayoutZone(_zoneMap[zone]);

    public void RefreshAllLayouts()
    {
        foreach (var z in _zoneMap.Values) LayoutZone(z);
    }

    void LayoutZone(CardZone zone)
    {
        if (zone.anchor == null) return;
        switch (zone.zoneType)
        {
            case ZoneType.Hand: LayoutHand(zone); break;
            case ZoneType.Battlefield: break; // free placement — don't auto layout
            default: LayoutPile(zone); break;
        }
    }

    void LayoutHand(CardZone zone)
    {
        int n = zone.cards.Count;
        if (n == 0) return;

        for (int i = 0; i < n; i++)
        {
            Vector3 offset = new Vector3(
                (i - (n - 1) / 2f) * zone.cardSpacing,
                0,
                0
            );

            Vector3 pos = zone.anchor.TransformPoint(offset);
            MoveCardToPosition(zone.cards[i], pos,
                zone.anchor.rotation * Quaternion.Euler(90, 0, 0));

            var hover = zone.cards[i].GetComponent<CardHover>();
            if (hover == null) hover = zone.cards[i].gameObject.AddComponent<CardHover>();
            hover.SetRestPosition(pos);
        }
    }

    void LayoutPile(CardZone zone)
    {
        for (int i = 0; i < zone.cards.Count; i++)
        {
            Vector3 offset = new Vector3(0, i * 0.0005f, 0);
            MoveCardToPosition(zone.cards[i],
                zone.anchor.TransformPoint(offset),
                zone.anchor.rotation * Quaternion.Euler(90, 0, 0));
        }
    }

    void MoveCardToPosition(CardObject card, Vector3 pos, Quaternion rot)
    {
        if (animateCardMoves) card.AnimateTo(pos, rot, cardMoveSpeed);
        else { card.transform.position = pos; card.transform.rotation = rot; }
    }
}