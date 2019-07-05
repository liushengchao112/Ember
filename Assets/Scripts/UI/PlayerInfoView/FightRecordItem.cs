using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using FightRecordData = Data.BattleInfo;

namespace UI
{
    public class FightRecordItem : ScrollViewItemBase
    {
        public FightRecordData info;

        private Text dateText, modeText, scoreText;
        private Image winIcon, failIcon;

        private void Awake()
        {
            dateText = transform.Find( "DateText" ).GetComponent<Text>();
            modeText = transform.Find( "ModeText" ).GetComponent<Text>();
            scoreText = transform.Find( "ScoreText" ).GetComponent<Text>();
            winIcon = transform.Find( "WinImage" ).GetComponent<Image>();
            failIcon = transform.Find( "FailImage" ).GetComponent<Image>();
        }

        public override void UpdateItemData( object dataObj )
        {
            base.UpdateItemData( dataObj );
            if ( dataObj == null )
                return;
            info = (FightRecordData)dataObj;
            RefreshItem();
        }

        public void RefreshItem()
        {
            dateText.text = GetDateString( info.battleStartDate );
            modeText.text = GetBattleMode( info.battleType );
            scoreText.text = info.resouce.ToString();

            winIcon.gameObject.SetActive( info.matchSide == info.winners );
            failIcon.gameObject.SetActive( info.matchSide != info.winners );
        }

        private string GetDateString( long time )
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime( new DateTime( 1970, 1, 1 ) );
            long iTime = long.Parse( time + "0000" );
            TimeSpan toNow = new TimeSpan( iTime );
            DateTime dTime = dtStart.Add( toNow );

            return dTime.ToString( "yyyy/MM/dd \t\t HH:mm" );
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
    }
}
