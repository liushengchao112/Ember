using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class LongPressButton : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,IPointerEnterHandler
{
	public Action<int> longPressEvent;
	public Action pressDownEvent;
	public Action pressUpEvent;
	public Action leaveEvent;
	public Action EnterEvent;

	private bool isPointDown;

	private float timer;

	private float interval = 1; 

	private int MaxLongTime = 30;
	private int remainingTime = 30;

	void Update()
	{
		if( isPointDown && remainingTime > 0 )
		{
			timer += Time.deltaTime;
			if( timer >= interval )
			{
				timer = 0;
				longPressEvent ( --remainingTime );
			}
		}
	}

	public void OnPointerDown (PointerEventData eventData)
	{
		if( pressDownEvent != null )
		{
			pressDownEvent ();
		}

		remainingTime = MaxLongTime;
		isPointDown = true;
	}

	public void OnPointerUp (PointerEventData eventData)
	{
		if( pressUpEvent != null )
		{
			pressUpEvent ();
		}
		
		isPointDown = true;
		timer = 0;
	}

	public void OnPointerExit (PointerEventData eventData)
	{
		if( leaveEvent != null )
		{
			leaveEvent ();
		}

		isPointDown = false;
	}

	public void OnPointerEnter( PointerEventData eventData )
	{
		if( EnterEvent != null )
		{
			EnterEvent ();
		}

		isPointDown = true;
	}

}