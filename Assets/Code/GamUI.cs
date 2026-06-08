using UnityEngine;
using UnityEngine.InputSystem;

public class GameUI : MonoBehaviour
{
    private ZoneManager _zoneManager;

    void Awake()
    {
        _zoneManager = FindAnyObjectByType<ZoneManager>();
    }

    void Update()
    {
        if (Keyboard.current.uKey.wasPressedThisFrame)
            _zoneManager.UntapAll();

        if (Keyboard.current.dKey.wasPressedThisFrame)
            _zoneManager.DrawCards(1);
            if (Keyboard.current.sKey.wasPressedThisFrame)
            _zoneManager.ShuffleLibrary();
    }
}