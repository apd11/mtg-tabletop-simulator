using UnityEngine;
using UnityEngine.InputSystem;

public class CardDrag : MonoBehaviour
{
    private Camera _cam;
    private CardObject _card;
    private ZoneManager _zoneManager;
    private bool _dragging;
    private float _dragHeight = 0.5f;
    private static int _layerCounter = 0;

    void Awake()
    {
        _cam = Camera.main;
        _card = GetComponentInParent<CardObject>();
        _zoneManager = FindAnyObjectByType<ZoneManager>();
    }

    void OnMouseDown()
    {
        if (CardContextMenu.IsOpen) return;

        var zone = _card.CurrentZone;
        if (zone != ZoneType.Hand && zone != ZoneType.Battlefield) return;
        _dragging = true;
        CardPreview.Instance.HidePreview();

        _card.transform.position = new Vector3(
            _card.transform.position.x,
            0.1f,
            _card.transform.position.z
        );

        if (zone == ZoneType.Hand)
        {
            _zoneManager.hand.cards.Remove(_card);
            _card.CurrentZone = ZoneType.Battlefield;
            _zoneManager.RefreshZoneLayout(ZoneType.Hand);
        }
    }

    void OnMouseDrag()
    {
        if (!_dragging) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _cam.ScreenPointToRay(mousePos);
        Plane plane = new Plane(Vector3.up, Vector3.up * _dragHeight);

        if (plane.Raycast(ray, out float dist))
        {
            _card.transform.position = ray.GetPoint(dist);
        }
    }

    void OnMouseUp()
    {
        if (!_dragging) return;
        _dragging = false;

        ZoneType dropZone = GetDropZone();

        if (dropZone == ZoneType.Battlefield)
        {
            if (!_zoneManager.battlefield.cards.Contains(_card))
            {
                foreach (var z in new[] { _zoneManager.hand, _zoneManager.battlefield, _zoneManager.graveyard, _zoneManager.exile, _zoneManager.command })
                    z.cards.Remove(_card);

                _zoneManager.battlefield.cards.Add(_card);
                _card.CurrentZone = ZoneType.Battlefield;
                _card.IsFaceDown = false;
            }

            float tappedY = _card.IsTapped ? 90f : 0f;
            _card.transform.rotation = Quaternion.Euler(90, tappedY, 0);

            _layerCounter++;
            _card.transform.position = new Vector3(
                _card.transform.position.x,
                _layerCounter * 0.001f,
                _card.transform.position.z
            );

            var hover = _card.GetComponent<CardHover>();
            if (hover == null) hover = _card.gameObject.AddComponent<CardHover>();
        }
        else if (dropZone == ZoneType.Hand)
        {
            _zoneManager.battlefield.cards.Remove(_card);
            _card.CurrentZone = ZoneType.Hand;
            _zoneManager.hand.cards.Add(_card);
            _zoneManager.RefreshZoneLayout(ZoneType.Hand);
        }
        else
        {
            _zoneManager.MoveCard(_card, dropZone);
        }
    }

    ZoneType GetDropZone()
    {
        Vector3 pos = _card.transform.position;

        if (IsNearAnchor(_zoneManager.graveyard.anchor, pos)) return ZoneType.Graveyard;
        if (IsNearAnchor(_zoneManager.exile.anchor, pos))     return ZoneType.Exile;
        if (IsNearAnchor(_zoneManager.command.anchor, pos))   return ZoneType.Command;
        if (IsOverBattlefield())                              return ZoneType.Battlefield;

        return ZoneType.Hand;
    }

    bool IsNearAnchor(Transform anchor, Vector3 pos, float radius = 0.5f)
    {
        if (anchor == null) return false;
        float dist = Vector3.Distance(new Vector3(pos.x, 0, pos.z),
                                      new Vector3(anchor.position.x, 0, anchor.position.z));
        return dist < radius;
    }

    bool IsOverBattlefield()
    {
        var bf = _zoneManager.battlefield.anchor;
        if (bf == null) return false;

        Vector3 local = bf.InverseTransformPoint(_card.transform.position);
        return local.x < 2.9f && local.x > -2.5f && Mathf.Abs(local.z) < 2.5f;
    }
}