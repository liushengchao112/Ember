using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ClickHandler : UnityEngine.EventSystems.EventTrigger
{
    public delegate void ClickDelegate( GameObject go );

    public ClickDelegate onClickDown;
    public ClickDelegate onClickUp;
    public ClickDelegate onDrag;
    public ClickDelegate onEnter;
    public ClickDelegate onExit;

    public static ClickHandler Get( GameObject go )
    {
        ClickHandler listener = go.GetComponent<ClickHandler>();
        if ( listener == null ) listener = go.AddComponent<ClickHandler>();
        return listener;
    }

    public override void OnPointerDown( PointerEventData eventData )
    {
        if ( onClickDown != null ) onClickDown( gameObject );
    }

    public override void OnPointerUp( PointerEventData eventData )
    {
        if ( onClickUp != null ) onClickUp( gameObject );
    }

    public override void OnDrag( PointerEventData eventData )
    {
        if ( onDrag != null ) onDrag( gameObject );
    }

    public override void OnPointerEnter( PointerEventData eventData )
    {
        if ( onEnter != null )
        {
            onEnter( gameObject );
        }
    }

    public override void OnPointerExit( PointerEventData eventData )
    {
        if ( onExit != null )
        {
            onExit( gameObject );
        }
    }
}
