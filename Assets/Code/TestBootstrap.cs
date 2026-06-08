using System.Collections;
using UnityEngine;

public class TestBootstrap : MonoBehaviour
{
    public ZoneManager player1Zone;
    public GameObject cardPrefab;

    [TextArea(10, 30)]
    public string decklist = @"4 Lightning Bolt
2 Counterspell
1 Black Lotus
2 Dark Ritual
1 Serra Angel
2 Giant Growth
1 Shivan Dragon
4 Island
4 Mountain
1 Sol Ring";

    IEnumerator Start()
    {
        yield return StartCoroutine(
            DeckLoader.Instance.LoadDeck(decklist, player1Zone, cardPrefab)
        );

        player1Zone.DrawCards(7);
    }
}