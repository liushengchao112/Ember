using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using UI;
using Constants;
using Utils;
using Data;
using Resource;

namespace UI
{
    public class UnitCardItemTypeTwo : UnitCardItemBase
    {
        public long unitId;
        public SquadData data;
        private BattleUIControlType tyep;
        private int unitMaxHp;
        public int UnitMaxHp
        {
            get
            {
                return unitMaxHp;
            }
        }
        private int unitcurrentHp;
        public int UnitCurrentHp
        {
            get
            {
                return unitcurrentHp;
            }
        }
        private bool isSelected;
        public bool iSeleceted
        {
            get
            {
                return isSelected;
            }
        }        

        private Image headImage;
        private Image hpImage;
        private GameObject selectedImage;
        private GameObject bigRoot;
        private Image bigHeadImageRoot;
        private Image bigHeadImage;
        private Text nameText;
        private Image bigHpImage;

        private Sprite bgHeadImage;

        private bool isClickDownHeadImage;
        private float clickDownTime;
        private bool isEnter;
        private float enterTime;

        public UnitCardItemTypeTwo( long unitId , SquadData squadData )
        {
            this.unitId = unitId;
            data = squadData;
        }

        public override void AdjustHealth( int hp , int maxHp )
        {

            bigHpImage.fillAmount = hpImage.fillAmount = hp  / (float)maxHp;

            unitcurrentHp = hp;
            unitMaxHp = maxHp;
        }

        public override void OnInit()
        {
            headImage = transform.Find( "HeadBgImage/HeadImage" ).GetComponent<Image>();
            bgHeadImage = headImage.sprite;
            ClickHandler.Get( headImage.gameObject ).onClickDown = OnHeadImageClickDown;
            ClickHandler.Get( headImage.gameObject ).onClickUp = OnHeadImageClickUp;
            hpImage = transform.Find( "HpImage" ).GetComponent<Image>();
            selectedImage = transform.Find( "SelectedImage" ).gameObject;
            bigRoot = transform.Find( "BigRoot" ).gameObject;
            bigHeadImageRoot = bigRoot.transform.Find( "BigHeadImageRoot" ).GetComponent<Image>();
            bigHeadImage = bigHeadImageRoot.transform.Find( "BigHeadImage" ).GetComponent<Image>();
            ClickHandler.Get( bigHeadImage.gameObject ).onEnter = OnBigHeadEnter;
            ClickHandler.Get( bigHeadImage.gameObject ).onExit = OnBigHeadExit;
            nameText = bigRoot.transform.Find( "NameBgImage/Text" ).GetComponent<Text>();
            bigHpImage = bigRoot.transform.Find( "BigHpImage" ).GetComponent<Image>();
        }

        private void OnHeadImageClickDown( GameObject go )
        {
            if ( unitId == 0 )
            {
                return;
            }
            isClickDownHeadImage = true;
            clickDownTime = Time.time;
        }

        private void OnHeadImageClickUp( GameObject go )
        {
            if ( unitId == 0 )
            {
                return;
            }
            isClickDownHeadImage = false;
            bigRoot.gameObject.SetActive( false );
        }

        private void OnBigHeadEnter( GameObject go )
        {
            if ( unitId == 0 )
            {
                return;
            }
            isEnter = true;
            enterTime = Time.time;
        }

        private void OnBigHeadExit( GameObject go )
        {
            if ( unitId == 0 )
            {
                return;
            }
            isEnter = false;            
        }

        void Update()
        {
            if ( isClickDownHeadImage )
            {
                if ( Time.time - clickDownTime > 0.5f )
                {
                    bigRoot.gameObject.SetActive( true );
                    RereshBigHeadUIInfo();
                    isClickDownHeadImage = false;
                }
            }

            if ( isEnter )
            {
                if ( Time.time - enterTime > 0.5f )
                {
                    MessageDispatcher.PostMessage( Constants.MessageType.SelectObjectInBattle , unitId );
                    isEnter = false;
                }
            }
        }

        void Awake()
        {
            MessageDispatcher.AddObserver( SelectFeedback , MessageType.BattleUIOperationFeedBack );
            tyep = (BattleUIControlType)DataManager.GetInstance().unitOperationChoose;
        }

        void OnDestroy()
        {
            MessageDispatcher.RemoveObserver( SelectFeedback , MessageType.BattleUIOperationFeedBack );
        }

        public void ResetUI()
        {
            //--TODO
            unitId = 0;
            data = null;
            bigHpImage.fillAmount = hpImage.fillAmount = 0;
            Seleceted( false );
            bigRoot.gameObject.SetActive( false );
            headImage.sprite = bgHeadImage;
        }

        private void RereshBigHeadUIInfo()
        {
            SetIconImage( bigHeadImage , data.protoData.Icon );
            nameText.text = data.protoData.Name;
            bigHpImage.fillAmount = hpImage.fillAmount;
        }

        public void UpdateData( long unitId , SquadData squadData )
        {
            this.unitId = unitId;
            data = squadData;

            if ( unitMaxHp == 0 )
            {
                AdjustHealth( 1, 1 );
            }
            else
            {
                AdjustHealth( unitMaxHp , unitMaxHp );
            }
            
            SetIconImage( headImage , data.protoData.Icon );

            RereshBigHeadUIInfo();
        }

        private void SelectFeedback( object typeObj , object idObj , object stateObj )
        {
            BattleUIOperationType type = (BattleUIOperationType)typeObj;
            if ( type != BattleUIOperationType.SelectedUnitResult )
                return;

            long id = (long)idObj;
            if ( id != unitId )
                return;

            bool state = (bool)stateObj;

            Seleceted( state );
        }

        public void Seleceted( bool isSelected )
        {
            selectedImage.SetActive( isSelected );
            this.isSelected = isSelected;
        }

        private void SetIconImage( Image image , int iconId )
        {
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( iconId , delegate ( string name , AtlasSprite atlasSprite , System.Object param )
            {
                image.SetSprite( atlasSprite );
            } , true );
        }        
    }
}