using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

using DetailedRecordData = Data.BattleInfo;

namespace UI
{
    public class DetailedRecordItem : ScrollViewItemBase, IBeginDragHandler
    {
        public delegate void OnClickBattleResultCallBack( DetailedRecordItem item );
        public OnClickBattleResultCallBack clickBattleResultEvent;

        public DetailedRecordData info;

        private Button bgButton, playBackButton, battleResultButton;
        private Image playerIcon, winIcon, failIcon;
        private Text deathCountText, battleModeText, battleScoreText, dateText;
        private GameObject unitTransformObj;
        private Image[] unitIconArray = new Image[9];
        private List<Data.UnitsProto.Unit> unitsProtoData;

        private const float timeLimit = 0.5f;
        private float clickTime;
        private bool clickBgButton = false;
        private bool isDrag = false;

        private bool isHaveBattleInfoInLocal = false;

        #region Player HeadIcon Image Path
        private const string PLAYER_ICON_PATH = "UITexture/Avatar_icon/";
        #endregion

        private void Awake()
        {
            unitsProtoData = Data.DataManager.GetInstance().unitsProtoData;

            bgButton = transform.Find( "BgButton" ).GetComponent<Button>();
            playBackButton = transform.Find( "PlayBackButton" ).GetComponent<Button>();
            battleResultButton = transform.Find( "BattleResultButton" ).GetComponent<Button>();
            playerIcon = transform.Find( "PlayerIcon" ).GetComponent<Image>();
            winIcon = transform.Find( "WinIcon" ).GetComponent<Image>();
            failIcon = transform.Find( "FailIcon" ).GetComponent<Image>();
            deathCountText = transform.Find( "DeathCountText" ).GetComponent<Text>();
            battleModeText = transform.Find( "BattleModeText" ).GetComponent<Text>();
            battleScoreText = transform.Find( "BattleScoreText" ).GetComponent<Text>();
            dateText = transform.Find( "DateText" ).GetComponent<Text>();
            unitTransformObj = transform.Find( "UnitTransform" ).gameObject;
            unitTransformObj.SetActive( false );
            unitIconArray[0] = transform.Find( "UnitTransform/UnitGroup/UnitIcon1/UnitIcon" ).GetComponent<Image>();
            unitIconArray[1] = transform.Find( "UnitTransform/UnitGroup/UnitIcon2/UnitIcon" ).GetComponent<Image>();
            unitIconArray[2] = transform.Find( "UnitTransform/UnitGroup/UnitIcon3/UnitIcon" ).GetComponent<Image>();
            unitIconArray[3] = transform.Find( "UnitTransform/UnitGroup/UnitIcon4/UnitIcon" ).GetComponent<Image>();
            unitIconArray[4] = transform.Find( "UnitTransform/UnitGroup/UnitIcon5/UnitIcon" ).GetComponent<Image>();
            unitIconArray[5] = transform.Find( "UnitTransform/UnitGroup/UnitIcon6/UnitIcon" ).GetComponent<Image>();
            unitIconArray[6] = transform.Find( "UnitTransform/UnitGroup/UnitIcon7/UnitIcon" ).GetComponent<Image>();
            unitIconArray[7] = transform.Find( "UnitTransform/UnitGroup/UnitIcon8/UnitIcon" ).GetComponent<Image>();
            unitIconArray[8] = transform.Find( "UnitTransform/UnitGroup/UnitIcon9/UnitIcon" ).GetComponent<Image>();

            playBackButton.AddListener( ClickPlayBackButton );
            battleResultButton.AddListener( ClickBattleResultButton );

            ClickHandler.Get( bgButton.gameObject ).onClickDown = ClickBgButtonDown;
            ClickHandler.Get( bgButton.gameObject ).onClickUp = ClickBgButtonUp;
        }

        #region Button Event

        private void ClickBgButtonDown( GameObject obj )
        {
            clickBgButton = true;
            clickTime = Time.time;
        }

        private void ClickBgButtonUp( GameObject obj )
        {
            clickBgButton = false;
            isDrag = false;
            unitTransformObj.SetActive( false );
        }

        private void ClickPlayBackButton()
        {
            //TODO : PlayBack
            PlayBackManager.GetInstance().PlayBattleBack( info.battleId );
        }

        private void ClickBattleResultButton()
        {
            if ( clickBattleResultEvent != null )
                clickBattleResultEvent( this );
        }

        #endregion

        public override void UpdateItemData( object dataObj )
        {
            base.UpdateItemData( dataObj );
            if ( dataObj == null )
                return;
            info = (DetailedRecordData)dataObj;
            RefreshItem();
        }

        public void RefreshItem()
        {
            winIcon.gameObject.SetActive( info.matchSide==info.winners );
            failIcon.gameObject.SetActive( info.matchSide != info.winners );

            deathCountText.text = string.Format( "{0}/{1}", info.killQuantity, info.dieQuantity );
            battleModeText.text = GetBattleMode( info.battleType );
            battleScoreText.text = info.resouce.ToString();
            dateText.text = GetDateString( info.battleStartDate);

            if ( !string.IsNullOrEmpty( info.portrait ) )
                Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( info.portrait, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    playerIcon.SetSprite( atlasSprite );
                }, true );

            for ( int i = 0; i < unitIconArray.Length; i++ )
            {
                if ( unitsProtoData.Find( p => p.ID == info.unitId[i] ) == null )
                    return;

                int icon = unitsProtoData.Find( p => p.ID == info.unitId[i] ).Icon;
                Image unitIcon = unitIconArray[i];

                if ( icon != 0 )
                    Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                        unitIcon.SetSprite( atlasSprite );
                    }, true );
            }

            isHaveBattleInfoInLocal = false;
            long[] localBattleid =  PlayBackManager.GetInstance().GetBattleIdsFromPlaybackLocalCache();
            for ( int i = 0; i < localBattleid.Length; i++ )
            {
                if ( info.battleId == localBattleid[i] )
                {                    
                    isHaveBattleInfoInLocal = true;
                    break;
                }
            }
            if ( isHaveBattleInfoInLocal )
            {
                playBackButton.gameObject.SetActive( true );
            }
            else
            {
                playBackButton.gameObject.SetActive( false );
            }
        }
        
        private string GetDateString( long time )
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime( new DateTime( 1970, 1, 1 ) );
            long iTime = long.Parse( time + "0000" );
            TimeSpan toNow = new TimeSpan( iTime );
            DateTime dTime = dtStart.Add( toNow );

            return dTime.ToString( "yyyy/MM/dd\nHH:mm:ss" );
        }

        private string GetBattleMode( Data.BattleType type )
        {
            switch ( type )
            {
                case Data.BattleType.BattleP1vsP1:
                    return "1v1";
                case Data.BattleType.BattleP2vsP2:
                    return "2v2";
                case Data.BattleType.Survival:
                    return "生存";
                case Data.BattleType.Tranining:
                    return "训练";
            }
            return "";
        }

        public void Update()
        {
            if ( clickBgButton && !isDrag )
            {
                if ( Time.time - clickTime >= timeLimit )
                    unitTransformObj.gameObject.SetActive( true );
            }
        }

        public void OnBeginDrag( PointerEventData eventData )
        {
            isDrag = true;
        }
    }
}
