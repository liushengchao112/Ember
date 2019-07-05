using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;

using Data;
using Utils;
using Constants;

namespace UI
{
    public class UnitCardItem : UnitCardItemBase, IPointerEnterHandler, IPointerExitHandler
    {
        public long unitId;
        public SquadData data;
        private BattleView battleView;

        private Image itemImage;
        private Image unitHealth;
        private Button clickButton;
        private GameObject selectionOverlay;
        private ShakeCard shakeCark;
        private Text unitNameText;

        private float timeOfLastTap = -1;

        private int unitcurrentHp;
        private int unitMaxHp;

        private bool HealthThresholdReached { get { return ( unitcurrentHp / unitMaxHp ) <= 0.2f; } }

        private bool isEnter = false;
        private bool isEnoughTime = false;
        private float enterTime;
        private bool isSelected = false;
        private BattleUIControlType type;

		public void InitComponent()
        {
            //MessageDispatcher.AddObserver( ToggleSelect, MessageType.SelectObjectInBattle );
            MessageDispatcher.AddObserver( SelectFeedback, MessageType.BattleUIOperationFeedBack );
            type = (BattleUIControlType)DataManager.GetInstance().unitOperationChoose;
            itemImage = transform.Find( "ItemImage" ).GetComponent<Image>();
            selectionOverlay = transform.Find( "ItemImage/SelectionOverlay" ).gameObject;
            unitHealth = transform.Find( "Item_HP" ).GetComponent<Image>();
            clickButton = transform.Find( "OnClickButton" ).GetComponent<Button>();
            unitNameText = transform.Find( "UnitName" ).GetComponent<Text>();
            unitHealth.gameObject.SetActive( false );
            selectionOverlay.SetActive( false );
            battleView = transform.root.GetComponentInChildren<BattleView>();

            shakeCark = gameObject.AddComponent<ShakeCard>();
            clickButton.AddListener( OnClickButton );
        }

        void OnDestroy()
        {
            //MessageDispatcher.RemoveObserver( ToggleSelect, MessageType.SelectObjectInBattle );
            MessageDispatcher.RemoveObserver( SelectFeedback, MessageType.BattleUIOperationFeedBack );
        }

        void OnEnable()
        {
            isEnter = false;
            isEnoughTime = false;
        }

        void Update()
        {
            if ( isEnter && !isEnoughTime && type == BattleUIControlType.TypeOne )
            {
                if ( Time.time - enterTime > 0.5f )
                {
                    isEnoughTime = true;
                    EnoughTimeHandler();
                }
            }
        }

        public void OnPointerEnter( PointerEventData eventData )
        {
            isEnter = true;
            isEnoughTime = false;
            enterTime = Time.time;
        }

        public void OnPointerExit( PointerEventData eventData )
        {
            isEnter = false;
            isEnoughTime = false;
        }

        private void EnoughTimeHandler()
        {
            MessageDispatcher.PostMessage( Constants.MessageType.SelectObjectInBattle , unitId );
        }

        private void SelectFeedback( object typeObj, object idObj, object stateObj )
        {
            BattleUIOperationType type = (BattleUIOperationType)typeObj;
            if ( type != BattleUIOperationType.SelectedUnitResult ) return;

            long id = (long)idObj;
            if ( id != unitId ) return;

            bool state = (bool)stateObj;

            if ( state )
                Select();
            else
                Unselect();
        }

        public void SetValues( SquadData data, long unitId )
        {
            this.unitId = unitId;
            this.data = data;
            //gameObject.name = unitId.ToString();

            AdjustHealth( unitMaxHp, unitMaxHp );

            Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( data.protoData.Icon_box, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
            {
                itemImage.SetSprite( atlasSprite );
            }, true );

            unitNameText.text = data.protoData.Name;
        }

        public bool IsSelected { get { return selectionOverlay.activeInHierarchy; } }

        public void Unselect()
        {
            selectionOverlay.SetActive( false );
            isSelected = false;
        }

        private void Select()
        {
            selectionOverlay.SetActive( true );
            isSelected = true;
        }

        public override void AdjustHealth( int hp, int maxHp )
        {
            if ( hp < unitcurrentHp )
            {
                shakeCark.Shake();
            }

            if ( hp < maxHp )
            {
                unitHealth.gameObject.SetActive( true );
            }
            else
            {
                unitHealth.gameObject.SetActive( false );
            }

            unitHealth.fillAmount = ( maxHp - hp ) / (float)maxHp;
            //_unitHealth.gameObject.SetActive( HealthThresholdReached );

            unitcurrentHp = hp;
            unitMaxHp = maxHp;
        }

        private void OnClickButton()
        {
            float tapTime = Time.realtimeSinceStartup;
            if ( timeOfLastTap < 0 )
            {
                timeOfLastTap = tapTime;
                MessageDispatcher.PostMessage( Constants.MessageType.SelectObjectInBattle, unitId );
            }
            else
            {
                if ( ( tapTime - timeOfLastTap ) <= GameConstants.DOUBLE_TAP_THRESHOLD )
                {
                    timeOfLastTap = -1;
                    MessageDispatcher.PostMessage( Constants.MessageType.SelectAll );
                }
                else
                {
                    timeOfLastTap = tapTime;
                    MessageDispatcher.PostMessage( Constants.MessageType.SelectObjectInBattle, unitId );
                }
            }            
        }
    }
}
