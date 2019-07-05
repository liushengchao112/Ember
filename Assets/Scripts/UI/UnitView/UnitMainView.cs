using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Resource;
using Utils;
using DG.Tweening;
using Data;

namespace UI
{
	public class UnitMainView : ViewBase
	{
		private UnitMainViewController controller;

		private Transform unitMain, unitTop, unitTagGroup, unitGroup;
		private Toggle isHavaToggle;
		private Text havaNumberText;

		private int[] UnitTags = { -1, 1, 2, 3, 4, 5, 6 };

		private int currentUnitTagId = -1;

		private Dictionary<int, int> currentUnits;

		private Toggle[] unitTagToggles;

		private List<UnitItem> unitCacheList = new List<UnitItem> ();
        private ObjectPool<UnitItem> unitItemPool;
        private Dictionary<int, int>.Enumerator unitEnumerator;
        private UnitItem goUnitItem;
        private bool preLoadModel = true;

        public override void OnEnter()
		{
			base.OnEnter ();

            LoadUnitItem ();
        }

		public override void OnInit()
		{
			base.OnInit ();

			controller = new UnitMainViewController ( this );

			InitComponent ();

			InitOnClickListener ();
		}

		private void InitComponent()
		{
			unitMain = transform.Find ( "UnitMain" );
			unitTop = transform.Find ( "UnitMain/UnitTop" );

			isHavaToggle = unitTop.Find ( "IsHavaToggle" ).GetComponent<Toggle> ();
			havaNumberText = unitTop.Find ( "HavaNumberText" ).GetComponent<Text> ();
				
			unitTagGroup = unitTop.Find ( "UnitTagGroup" );
			unitGroup = unitMain.Find ( "UnitMiddle/UnitGroup" );
		}

		private void InitOnClickListener()
		{
			isHavaToggle.AddListener ( OnClickIsHavaToggle );

			unitTagToggles = new Toggle[ unitTagGroup.childCount];
			for ( int i = 0; i < unitTagGroup.childCount; i++ )
			{
				unitTagToggles[ i ] = unitTagGroup.GetChild ( i ).GetComponent<Toggle> ();
				unitTagToggles[ i ].AddListener ( OnClickSelectUnitTagToggle );
			}
		}

		#region ButtonEvent and ToggleEvent

		private void OnClickSelectUnitTagToggle(bool on)
		{
			for ( int i = 0; i < unitTagToggles.Length; i++ )
			{
				if( unitTagToggles[ i ].isOn )
				{
					int unitTagId = UnitTags[ i ];

					if( currentUnitTagId != unitTagId )
					{
						currentUnitTagId = unitTagId;
						LoadUnitItem ();
					}
				}
			}
		}

		private void OnClickIsHavaToggle(bool on)
		{
			LoadUnitItem ();
		}

		#endregion

		private void LoadUnitItem()
		{
            if ( unitItemPool == null )
            {
                unitItemPool = new ObjectPool<UnitItem>( CreateUnitItem, DistroyUnitItem );
            }
            currentUnits = controller.GetUnitList( currentUnitTagId, isHavaToggle.isOn );
            unitEnumerator = currentUnits.GetEnumerator();
            havaNumberText.text = string.Format( "数量{0}/{1}", controller.GetUnitList( currentUnitTagId, true ).Count, controller.GetUnitList( currentUnitTagId, false ).Count );

            if( goUnitItem == null )
            {
                GameResourceLoadManager.GetInstance().LoadAssetAsync<GameObject>( "UnitItem", delegate ( GameObject go )
                {
                    goUnitItem = GameObject.Instantiate( go ).AddComponent<UnitItem>();
                    ShowList();
                } );
            }
            else
            {
                ShowList();
            }
        }

        private UnitItem CreateUnitItem() { return GameObject.Instantiate<UnitItem>( goUnitItem ); }

        private void DistroyUnitItem( UnitItem item ) { GameObject.Destroy( item.gameObject ); item = null; }

        #region Match Item

        private void ShowList()
        {
            ClearUnitList();
            ShowListHandler();
        }

        private void ShowListHandler()
        {
            if ( unitItemPool == null ) return;

            if ( !unitEnumerator.MoveNext() )
            {
                unitEnumerator.Dispose();
                return;
            }

            int unitId = unitEnumerator.Current.Key;

            UnitItem unitItem = unitItemPool.GetObject();
            unitItem.transform.SetParent( unitGroup, false );
            unitItem.transform.localPosition = Vector3.zero;
            unitItem.transform.localScale = Vector3.zero;

            UnitsProto.Unit unitData = controller.GetUnitData( unitId );
            unitItem.id = unitId;
            unitItem.icon = unitData.Icon_bust;
            unitItem.unitName = unitData.Name;
            unitItem.number = unitEnumerator.Current.Value;
            unitItem.RefreshItem();
            unitItem.onClickEvent = OnClickEnterUnitDetailsCallBack;

            unitItem.transform.DOScale( Vector3.one, 0.05f ).SetEase( Ease.OutCirc, 0.05f ).OnComplete( ShowListHandler );

            unitCacheList.Add( unitItem );
        }

        private void ClearUnitList()
        {
            for ( int i = 0; i < unitCacheList.Count; i++ )
            {
                unitCacheList[i].transform.DOKill();
                unitItemPool.DisposeObject( unitCacheList[i] );
                unitCacheList[i].transform.SetParent( null, false );
            }
            unitCacheList.Clear();
        }

        private void OnClickEnterUnitDetailsCallBack( int unitId )
		{
			UIManager.Instance.GetUIByType ( UIType.UnitInfoUI , (ui, param ) =>
			{
				if ( ui != null )
				{
					if ( !ui.openState )
					{
                        UnitInfoView view = ui as UnitInfoView;
                        view.unitId = unitId;
                        view.currentUnits = currentUnits;
                        view.OnEnter();
                    }
				}
			} );
		}

        #endregion

        public override void OnExit( bool isGoBack )
        {
            base.OnExit( isGoBack );

            for ( int i = 0; i < unitCacheList.Count; i++ )
            {
                DistroyUnitItem( unitCacheList[i] );
            }
            unitCacheList.Clear();

            if ( unitItemPool != null )
            {
                unitItemPool.Clear();
                unitItemPool = null;
            }

            if( goUnitItem != null )
            {
                DistroyUnitItem( goUnitItem );
            }
        }
    }
}