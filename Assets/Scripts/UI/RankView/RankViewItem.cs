using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Data;
using Resource;
using RankMemberInfo = Data.RankS2C.RankMemberInfo;
using Utils;

namespace UI
{
    public class RankViewItem : ScrollViewItemBase
    {
        public delegate void OnItemClick( RankMemberInfo info );
        public OnItemClick OnItemClickHnadler;
        public RankViewController controler;

        public RankMemberInfo info;
        public RankType rankType;

        //private const string PLAYER_ICON_PATH = "UITexture/Avatar_icon/";

        private Image rankIconImage;
        private Text rankText;
        private Image headImage;
        private Text playerLvText;
        private Text playerNameText;
        private Text unionDesText;
        private Text unionNameText;
        private Transform gradingRoot;
        private Image gradingIcon;
        private Text gradingText;
        private Image emberMedalNone;
        private Image emberMedal;
        private Text emberMedalCountText;
        private Text countText;
        private Button addFriendBtn;
        private Toggle itemToggle;

        void Awake()
        {
            itemToggle = transform.GetComponent<Toggle>();
            itemToggle.onValueChanged.AddListener( OnItemObjClick );
            rankIconImage = transform.Find( "RankIconImage" ).GetComponent<Image>();
            rankText = transform.Find( "RankText" ).GetComponent<Text>();
            headImage = transform.Find( "HeadBg/HeadImage" ).GetComponent<Image>();
            playerLvText = transform.Find( "PlayerLvText" ).GetComponent<Text>();
            playerNameText = transform.Find( "PlayerNameText" ).GetComponent<Text>();
            unionDesText = transform.Find( "UnionDesText" ).GetComponent<Text>();
            unionNameText = transform.Find( "UnionNameText" ).GetComponent<Text>();
            gradingRoot = transform.Find( "GradingRoot" );
            gradingIcon = gradingRoot.Find( "GradingIcon" ).GetComponent<Image>();
            gradingText = gradingRoot.Find( "GradingText" ).GetComponent<Text>();
            emberMedalNone = gradingRoot.Find( "EmberMedalNone" ).GetComponent<Image>();
            emberMedal = gradingRoot.Find( "EmberMedal" ).GetComponent<Image>();
            emberMedalCountText = gradingRoot.Find( "EmberMedalCountText" ).GetComponent<Text>();
            countText = transform.Find( "CountText" ).GetComponent<Text>();
            addFriendBtn = transform.Find( "AddFriendBtn" ).GetComponent<Button>();
            addFriendBtn.AddListener( OnAddFriendBtnClick );
        }

        private void OnItemObjClick( bool isOn )
        {
            if ( OnItemClickHnadler != null && info != null )
            {
                OnItemClickHnadler( info );
            }
        }

        private void OnAddFriendBtnClick()
        {

        }

        public void RefreshPlayerHeadIcon( string orderIconStr )
        {
            if ( string.IsNullOrEmpty( orderIconStr ) )
            {
                DebugUtils.LogError( DebugUtils.Type.Resource , string.Format( "PlayerHeadIcon string IsNullOrEmpty! Please check!" ) );
                return;
            }
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( orderIconStr , delegate ( string name , AtlasSprite atlasSprite , System.Object param )
            {
                headImage.SetSprite( atlasSprite );
            } , true );
        }

        public void UpdateInfo()
        {
            if ( info == null )
            {
                return;
            }
            rankType = controler.GetCurrentRankType();
            if ( rankType == RankType.GradeRank )
            {
                gradingRoot.gameObject.SetActive( true );
                countText.gameObject.SetActive( false );

                if ( info.score > 0 )
                {
                    emberMedalNone.gameObject.SetActive( false );
                    emberMedal.gameObject.SetActive( true );
                    emberMedalCountText.text = "x" + info.score.ToString();
                }
                else
                {
                    emberMedalNone.gameObject.SetActive( true );
                    emberMedal.gameObject.SetActive( false );
                }
            }
            else
            {
                gradingRoot.gameObject.SetActive( false );
                countText.gameObject.SetActive( true );
            }

            if ( info.rank <= 3 )
            {
                rankIconImage.gameObject.SetActive( true );
                rankText.gameObject.SetActive( false );
                SetRankIconImage( info.rank );
            }
            else
            {
                rankIconImage.gameObject.SetActive( false );
                rankText.gameObject.SetActive( true );
            }

            rankText.text = info.rank.ToString();
            RefreshPlayerHeadIcon( info.portrait );
            playerLvText.text = info.level.ToString();
            playerNameText.text = info.playerName;
            unionNameText.text = info.unionName;
            countText.text = info.score.ToString();

            if ( info.isFriend )
            {
                addFriendBtn.gameObject.SetActive( false );
            }
            else
            {
                addFriendBtn.gameObject.SetActive( true );
            }

            gradingText.text = info.grading.ToString();
        }


        public override void UpdateItemData( object dataObj )
        {
            base.UpdateItemData( dataObj );
            info = (RankMemberInfo)dataObj;
            UpdateInfo();
        }

        private void SetRankIconImage(int rank)
        {
            string rankStr = "";
            if ( rank == 1 )
            {
                rankStr = "I_T_paihangicon01";
            }
            else if( rank == 2 )
            {
                rankStr = "I_T_paihangicon02";
            }
            else if ( rank == 3 )
            {
                rankStr = "I_T_paihangicon03";
            }
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( rankStr , delegate ( string name , AtlasSprite atlasSprite , System.Object param )
            {
                rankIconImage.SetSprite( atlasSprite );
            } , true );
        }
    }
}

