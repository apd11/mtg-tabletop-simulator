using UnityEngine;

public class CardHoverRelay : MonoBehaviour
{
    private CardHover _hover;

    void Update()
    {
        if (_hover == null)
            _hover = GetComponentInParent<CardHover>();
    }

    void OnMouseEnter() => _hover?.OnHoverEnter();
    void OnMouseExit()  => _hover?.OnHoverExit();
}