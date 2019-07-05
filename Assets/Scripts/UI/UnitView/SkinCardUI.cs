using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkinCardUI : MonoBehaviour, IEndDragHandler, IDragHandler
{
	public float centerSpeed = 9f;

	public System.Action<SkinCardItem> onCenter;

	private ScrollRect scrollView;
	private HorizontalLayoutGroup group;
	private Transform skinCardGroup;

	private List<float> centerPosList = new List<float> ();
	private List<SkinCardItem> skinCardList = new List<SkinCardItem>();

	private float targetPos;
	private bool centering = false;
	private float firstPosDistance;
	private int currentSelectedIndex = -1;

	private int itemWidth = 172;

	public AnimationCurve scaleCurve;

	void Awake ()
	{
		scrollView = GetComponent<ScrollRect> ();
		skinCardGroup = scrollView.content;

		group = skinCardGroup.GetComponent<HorizontalLayoutGroup> ();

		scrollView.movementType = ScrollRect.MovementType.Unrestricted;

		firstPosDistance = scrollView.GetComponent<RectTransform> ().rect.width * 0.5f - itemWidth * 0.5f;

		Vector3 startPos = skinCardGroup.localPosition;
		startPos.x = firstPosDistance;
		skinCardGroup.localPosition = startPos;

		Keyframe[] kf = new Keyframe[2];
		kf[ 0 ] = new Keyframe ( 0 , 1.5f );
		kf[ 1 ] = new Keyframe ( 222 , 1f );
		scaleCurve = new AnimationCurve( kf );
		scaleCurve.preWrapMode = WrapMode.Once;
	
		Refresh ( skinCardGroup.childCount );
	}

	public void Refresh(int number)
	{
		centerPosList.Clear ();
		skinCardList.Clear ();

		float centerPosX;

		for (int i = 0; i < number; i++)
		{
			centerPosX = firstPosDistance - ( itemWidth + group.spacing ) * i;
			centerPosList.Add ( centerPosX );

			SkinCardItem skinCardItem = skinCardGroup.GetChild ( i ).GetComponent<SkinCardItem> ();
			skinCardList.Add ( skinCardItem );

			skinCardItem.onClickEvent = OnClickSkinCardItemCallBack;
		}

		//Used this code when Card get from server
		//targetPos = FindClosestPos ( firstPosDistance );
		//centering = true;
	}

	public void OnClickSkinCardItemCallBack( int index )
	{
		SetTargetPos ( index );
	}

	void Update ()
	{
		if (centering)
		{
			Vector3 v = skinCardGroup.localPosition;

			v.x = Mathf.Lerp (skinCardGroup.localPosition.x, targetPos, centerSpeed * Time.deltaTime);

			skinCardGroup.localPosition = v;

			if ( Mathf.Abs ( skinCardGroup.localPosition.x - targetPos ) < 0.01f )
			{
				centering = false;
			}
		}

		for ( int i = 0; i < skinCardList.Count; i++ )
		{
			SkinCardItem item = skinCardList[ i ];
			float distance = Mathf.Abs ( skinCardGroup.localPosition.x - centerPosList[ i ] );
			float scale = scaleCurve.Evaluate ( distance );
			item.UpdateScrollViewItems ( scale );
		}
	}

	public void OnEndDrag (PointerEventData eventData)
	{
		centering = true;
		targetPos = FindClosestPos (skinCardGroup.localPosition.x);
	}

	public void OnDrag (PointerEventData eventData)
	{
		centering = false;
	}

	private float FindClosestPos (float currentPos)
	{
		int childIndex = 0;
		float closest = 0;
		float distance = Mathf.Infinity;

		for (int i = 0; i < centerPosList.Count; i++)
		{
			float p = centerPosList[i];
			float d = Mathf.Abs (p - currentPos);
			if (d < distance)
			{
				distance = d;
				closest = p;
				childIndex = i;
			}
		}

		SelectedCardItem ( childIndex );

		return closest;
	}

	private void SelectedCardItem( int index )
	{
		if( currentSelectedIndex == index )
		{
			return;	
		}

		currentSelectedIndex = index;

		SkinCardItem item = skinCardList[ index ];

		if( onCenter != null )
		{
			onCenter ( item );
		}
	}

	public void SetTargetPos(int index)
	{
		centering = true;
		targetPos = centerPosList[ index ];
		SelectedCardItem ( index );
	}
}