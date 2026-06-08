using System.Collections;
using UnityEngine;

public class CardHover : MonoBehaviour
{
    public float hoverSpeed             = 8f;
    public float handHoverDelay         = 0f;
    public float battlefieldHoverDelay  = 0.4f;

    private Vector3 _restPosition;
    private bool _hovered;
    private Coroutine _hideCoroutine;
    private Coroutine _showCoroutine;

    void Update()
    {
        if (GetComponent<CardObject>().CurrentZone == ZoneType.Hand)
        {
            Vector3 target = _hovered
                ? _restPosition + Vector3.up * 0.3f
                : _restPosition;
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * hoverSpeed);
        }
    }

    public void SetRestPosition(Vector3 pos)
    {
        _restPosition = pos;
    }

    public void OnHoverEnter()
    {
        var zone = GetComponent<CardObject>().CurrentZone;
        if (zone != ZoneType.Hand && zone != ZoneType.Battlefield) return;

        if (_hideCoroutine != null) { StopCoroutine(_hideCoroutine); _hideCoroutine = null; }

        float delay = zone == ZoneType.Hand ? handHoverDelay : battlefieldHoverDelay;
        if (_showCoroutine != null) StopCoroutine(_showCoroutine);
        _showCoroutine = StartCoroutine(DelayedShow(delay));
    }

    public void OnHoverExit()
    {
        _hovered = false;
        if (_showCoroutine != null) { StopCoroutine(_showCoroutine); _showCoroutine = null; }

        var zone = GetComponent<CardObject>().CurrentZone;
        if (zone == ZoneType.Hand)
            CardPreview.Instance.HidePreview();
        else
        {
            if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
            _hideCoroutine = StartCoroutine(DelayedHide());
        }
    }

    IEnumerator DelayedShow(float delay)
    {
        yield return new WaitForSeconds(delay);
        _hovered = true;
        CardPreview.Instance.ShowPreview(GetComponent<CardObject>());
    }

    IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(0.15f);
        if (!_hovered)
            CardPreview.Instance.HidePreview();
    }
}