using UnityEngine;
using UnityEngine.InputSystem;

public class CardInteract : MonoBehaviour
{
    private CardObject _card;
    private ZoneManager _zoneManager;

    void Awake()
    {
        _card = GetComponentInParent<CardObject>();
        _zoneManager = FindAnyObjectByType<ZoneManager>();
    }

    void OnMouseOver()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (_card.CurrentZone == ZoneType.Battlefield)
            {
                _zoneManager.SetTapped(_card, !_card.IsTapped);
            }
        }
    }
}