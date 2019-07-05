using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UI;
using System;

public class SkinCardItem : MonoBehaviour
{
	private Vector3 scale;
	private Button cardItemButton;
	public Action<int> onClickEvent;
	public int icon;

	public void Awake()
	{
		scale = transform.localScale;
		cardItemButton = transform.GetComponent<Button> ();
		cardItemButton.AddListener ( OnCardItemClickButton );
	}

	public void UpdateScrollViewItems( float scaleValue )
	{
		scale.y = scaleValue;
		scale.x = scaleValue;
		transform.localScale = scale;
	}

	private void OnCardItemClickButton()
	{
		if( onClickEvent != null )
		{
			onClickEvent ( transform.GetSiblingIndex() );
		}
	}
}