using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Data;
using Resource;
using Utils;

namespace UI
{
    public class RankView : ViewBase
    {
        private RankViewController controller;

        //private const string RANK_ITEM_PATH = "Prefabs/UI/RankViewItem/RankViewItem";
        //private const string PLAYER_ICON_PATH = "UITexture/Avatar_icon/";

        private Transform rankUIRoot;
        private Transform bigBg;
        private Transform playersInfo;
        private ScrollRect scrollRect;
        private Transform content;
        private Transform myInfoRoot;
        private Image headImage;
        private Text lvText;
        private Text nameText;
        private Text desText;
        private Text rankText;
        private Text gradeText;
        private Text rateText1v1;
        private Text rateText2v2;
        private Transform fiveStar;
        private GameObject[] fiveStars = new GameObject[5];
        private Transform fourStar;
        private GameObject[] fourStars = new GameObject[4];
        private Transform threeStar;
        private GameObject[] threeStars = new GameObject[3];
        private Transform btnRoot;
        private Button friendBtn;
        private GameObject friendUnSelect;
        private Button districtBtn;
        private GameObject districtUnSelect;
        private Button changeRankBtn;
        private GameObject changeRankUnSelect;
        private Transform textRoot;
        private Text rankDesText;
        private Transform changeRankTypeRoot;
        private Transform rankTypeContentRoot;
        private Button gradeRankBtn;
        private Button unitQuantityRankBtn;
        private Button skinRankBtn;
        private Button winStreakRankBtn;
        private Button winRankBtn;
        private Button battleTimesRankBtn;
        private Text rankTypeTitleText;

        private RankType rankType = RankType.UnitQuantityRank;
        public RankType RankType
        {
            get
            {
                return rankType;
            }
            set
            {
                if ( rankType != value )
                {
                    rankType = value;
                }
            }
        }

        private bool isFriendRank = false;
        private int pageId = 1;
        public bool isCanRequest = true;
        private bool isRequestTime = true;
        private float requestTime = 0;

        private RankScrollView rankScrollView;

        public override void OnInit()
        {
            base.OnInit();

            controller = new RankViewController( this );
            _controller = controller;

            rankUIRoot = transform.Find( "Ani/RankUIRoot" );
            bigBg = rankUIRoot.Find( "BigBg" );
            playersInfo = bigBg.Find( "PlayersInfo" );
            scrollRect = playersInfo.Find( "ScrollView" ).GetComponent<ScrollRect>();

            rankScrollView = scrollRect.gameObject.AddComponent<RankScrollView>();
            rankScrollView.OnCreateItemHandler = OnCreateItem;
            GameResourceLoadManager.GetInstance().LoadResource( "RankViewItem", OnLoadRankItem , true );

            scrollRect.onValueChanged.AddListener( ScrollRectDragHandler );
            content = scrollRect.transform.Find( "Viewport/Content" );
            myInfoRoot = bigBg.Find( "MyInfo" );
            headImage = myInfoRoot.Find( "MyInfoBg/HeadBg/HeadImage" ).GetComponent<Image>();
            lvText = myInfoRoot.Find( "MyInfoBg/PlayerLvText" ).GetComponent<Text>();
            nameText = myInfoRoot.Find( "MyInfoBg/PlayerNameText" ).GetComponent<Text>();
            desText = myInfoRoot.Find( "MyInfoBg/PlayerDesText" ).GetComponent<Text>();
            rankText = myInfoRoot.Find( "MyInfoBg/RankText" ).GetComponent<Text>();
            gradeText = myInfoRoot.Find( "MyInfoBg/GradeDesText" ).GetComponent<Text>();
            rateText1v1 = myInfoRoot.Find( "MyInfoBg/1v1RateText" ).GetComponent<Text>();
            rateText2v2 = myInfoRoot.Find( "MyInfoBg/2v2RateText" ).GetComponent<Text>();
            fiveStar = myInfoRoot.Find( "MyInfoBg/FiveStar" );
            for ( int i = 0; i < 5; i++ )
            {
                fiveStars[i] = fiveStar.Find( "Star" + i ).Find( "ShowImage" ).gameObject;
            }
            fourStar = myInfoRoot.Find( "MyInfoBg/FourStar" );
            for ( int i = 0; i < 4; i++ )
            {
                fourStars[i] = fourStar.Find( "Star" + i ).Find( "ShowImage" ).gameObject;
            }
            threeStar = myInfoRoot.Find( "MyInfoBg/FourStar" );
            for ( int i = 0; i < 3; i++ )
            {
                threeStars[i] = threeStar.Find( "Star" + i ).Find( "ShowImage" ).gameObject;
            }
            btnRoot = bigBg.Find( "BtnRoot" );
            friendBtn = btnRoot.Find( "FriendBtn" ).GetComponent<Button>();
            friendUnSelect = friendBtn.transform.Find( "UnSelect" ).gameObject;            
            friendBtn.AddListener( OnFriendBtnClick );
            districtBtn = btnRoot.Find( "DistrictBtn" ).GetComponent<Button>();
            districtUnSelect = districtBtn.transform.Find( "UnSelect" ).gameObject;
            districtBtn.AddListener( OnDistrictBtnClick );
            changeRankBtn = btnRoot.Find( "ChangeRankBtn" ).GetComponent<Button>();
            changeRankUnSelect = changeRankBtn.transform.Find( "UnSelect" ).gameObject;
            changeRankBtn.AddListener( OnChangeRankBtnClick );
            changeRankTypeRoot = rankUIRoot.Find( "ChangeRankTypeRoot" );
            rankTypeContentRoot = changeRankTypeRoot.Find( "ScrollView/Viewport/Content" );
            gradeRankBtn = rankTypeContentRoot.Find( "GradeRank" ).GetComponent<Button>();
            gradeRankBtn.AddListener( OnGradeRankBtnClick );
            unitQuantityRankBtn = rankTypeContentRoot.Find( "UnitQuantityRank" ).GetComponent<Button>();
            unitQuantityRankBtn.AddListener( OnUnitQuantityRankBtnClick );
            skinRankBtn = rankTypeContentRoot.Find( "SkinRank" ).GetComponent<Button>();
            skinRankBtn.AddListener( OnSkinRankBtnClick );
            winStreakRankBtn = rankTypeContentRoot.Find( "WinStreakRank" ).GetComponent<Button>();
            winStreakRankBtn.AddListener( OnWinStreakRankBtnClick );
            winRankBtn = rankTypeContentRoot.Find( "WinRank" ).GetComponent<Button>();
            winRankBtn.AddListener( OnWinRankBtnClick );
            battleTimesRankBtn = rankTypeContentRoot.Find( "BattleTimesRank" ).GetComponent<Button>();
            battleTimesRankBtn.AddListener( OnBattleTimesRankBtnClick );
            rankTypeTitleText = rankUIRoot.Find( "RankTypeBg/Text" ).GetComponent<Text>();
            textRoot = bigBg.Find( "TextRoot" );
            rankDesText = textRoot.Find( "RankDesText" ).GetComponent<Text>();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            OnFriendBtnClick();
        }

        private void Reset()
        {
            pageId = 1;
            isCanRequest = true;
            controller.ClearMembersInfo();
            controller.SendRankRequest( RankType , pageId , isFriendRank );
        }

        public void ReFreshUI()
        {
            RefreshUIText();

            if ( pageId == 1 )
            {
                RefreshMyInfo();
            }

            rankScrollView.InitializeWithData( controller.GetMembersInfo() );
        }

        private void RefreshUIText()
        {
            RefreshTitleNameText();
            RefreshRankDesText();
        }

        private void RefreshTitleNameText()
        {
            SetTitleText( GetTitleStrByRankType( RankType ) );
        }

        private string GetTitleStrByRankType( RankType type )
        {
            string str = "";
            switch ( type )
            {
                case RankType.GradeRank:
                {
                    str = "段位排行";
                }
                break;
                case RankType.UnitQuantityRank:
                {
                    str = "英雄排行";
                }
                break;
                case RankType.SkinRank:
                {
                    str = "皮肤排行";
                }
                break;
                case RankType.WinStreakRank:
                {
                    str = "连胜排行";
                }
                break;
                case RankType.WinRank:
                {
                    str = "胜场排行";
                }
                break;
                case RankType.BattleTimesRank:
                {
                    str = "场次排行";
                }
                break;
                default:
                {
                    str = "排行榜";
                }
                break;
            }
            return str;
        }

        private void SetTitleText( string str )
        {
            rankTypeTitleText.text = str;
        }

        private void RefreshRankDesText()
        {
            SetRankDesText( GetRankDesStrByRankType( RankType ) );
        }

        private string GetRankDesStrByRankType( RankType type )
        {
            string str = "";
            switch ( type )
            {
                case RankType.GradeRank:
                {
                    str = "玩家段位";
                }
                break;
                case RankType.UnitQuantityRank:
                {
                    str = "英雄数量";
                }
                break;
                case RankType.SkinRank:
                {
                    str = "皮肤数量";
                }
                break;
                case RankType.WinStreakRank:
                {
                    str = "连胜数量";
                }
                break;
                case RankType.WinRank:
                {
                    str = "胜场数量";
                }
                break;
                case RankType.BattleTimesRank:
                {
                    str = "游戏场数";
                }
                break;
                default:
                {
                    str = "排行";
                }
                break;
            }
            return str;
        }

        private void SetRankDesText( string str )
        {
            rankDesText.text = str;
        }

        private void ScrollRectDragHandler( Vector2 vt )
        {
            if ( !isCanRequest || isRequestTime )
            {
                return;
            }

            if ( vt.y <= 0 )
            {
                pageId++;
                controller.SendRankRequest( RankType , pageId , false );
            }
        }

        public void StartRequestCD()
        {
            isRequestTime = true;
            requestTime = 0;
        }

        void Update()
        {
            if ( isRequestTime )
            {
                requestTime += Time.deltaTime;
                if ( requestTime >= 1f )
                {
                    isRequestTime = false;
                    requestTime = 0;
                }
            }
        }

        private void OnLoadRankItem( string name , UnityEngine.Object obj , System.Object param )
        {
            rankScrollView.InitDataBase( scrollRect , obj , 1 , 788 , 105 , 5 , new Vector3( 398 , -59 , 0 ) );
        }

        private void RefreshMyInfo()
        {
            RankS2C.RankMemberInfo myInfo = controller.GetMyRankInfo();
            UpdateRightInfo( myInfo );
        }

        private void UpdateRightInfo( RankS2C.RankMemberInfo info )
        {
            RefreshPlayerHeadIcon( info.portrait );
            lvText.text = "Lv" + info.level.ToString();
            nameText.text = info.playerName;
            rankText.text = info.rank.ToString();
            //gradeText.text = info.grading.ToString();
            rateText1v1.text = info.winRate1v1.ToString() + "%";
            rateText2v2.text = info.winRate2v2.ToString() + "%";
            //SetStars( info.score );
        }

        private void ShowStarsByGrade( int grade )
        {
            fiveStar.gameObject.SetActive( false );
            fourStar.gameObject.SetActive( false );
            threeStar.gameObject.SetActive( false );
            if ( grade <= 6 )
            {
                threeStar.gameObject.SetActive( true );
            }
            else if ( grade <= 16 )
            {
                fourStar.gameObject.SetActive( true );
            }
            else
            {
                fiveStar.gameObject.SetActive( true );
            }
        }

        private void SetStars( int num )
        {
            for ( int i = 0; i < fiveStars.Length; i++ )
            {
                fiveStars[i].SetActive( false );
            }
            if ( num > fiveStars.Length )
            {
                return;
            }
            for ( int i = 0; i < num; i++ )
            {
                fiveStars[i].SetActive( true );
            }
        }

        private string GetGradeDesStr( int gradeNum )
        {
            string gradeStr = "";
            if ( gradeNum == 1 )
            {
                gradeStr = "荣誉青铜III";
            }
            else if ( gradeNum == 2 )
            {
                gradeStr = "荣誉青铜II";
            }
            else if ( gradeNum == 3 )
            {
                gradeStr = "荣誉青铜I";
            }
            else if ( gradeNum == 4 )
            {
                gradeStr = "尊贵白银VI";
            }
            else if ( gradeNum == 5 )
            {
                gradeStr = "尊贵白银III";
            }
            else if ( gradeNum == 6 )
            {
                gradeStr = "尊贵白银II";
            }
            else if ( gradeNum == 7 )
            {
                gradeStr = "尊贵白银I";
            }
            else if ( gradeNum == 8 )
            {
                gradeStr = "华贵黄金V";
            }
            else if ( gradeNum == 9 )
            {
                gradeStr = "华贵黄金VI";
            }
            else if ( gradeNum == 10 )
            {
                gradeStr = "华贵黄金III";
            }
            else if ( gradeNum == 11 )
            {
                gradeStr = "华贵黄金II";
            }
            else if ( gradeNum == 12 )
            {
                gradeStr = "华贵黄金I";
            }
            else if ( gradeNum == 13 )
            {
                gradeStr = "耀眼白金V";
            }
            else if ( gradeNum == 14 )
            {
                gradeStr = "耀眼白金VI";
            }
            else if ( gradeNum == 15 )
            {
                gradeStr = "耀眼白金III";
            }
            else if ( gradeNum == 16 )
            {
                gradeStr = "耀眼巴金II";
            }
            else if ( gradeNum == 17 )
            {
                gradeStr = "耀眼白金I";
            }
            else if ( gradeNum == 18 )
            {
                gradeStr = "璀璨钻石V";
            }
            else if ( gradeNum == 19 )
            {
                gradeStr = "璀璨钻石VI";
            }
            else if ( gradeNum == 20 )
            {
                gradeStr = "璀璨钻石III";
            }
            else if ( gradeNum == 21 )
            {
                gradeStr = "璀璨钻石II";
            }
            else if ( gradeNum == 22 )
            {
                gradeStr = "璀璨钻石I";
            }
            else if ( gradeNum == 23 )
            {
                gradeStr = "余烬大师V";
            }
            else if ( gradeNum == 24 )
            {
                gradeStr = "余烬大师VI";
            }
            else if ( gradeNum == 25 )
            {
                gradeStr = "余烬大师III";
            }
            else if ( gradeNum == 26 )
            {
                gradeStr = "余烬大师II";
            }
            else if ( gradeNum == 27 )
            {
                gradeStr = "余烬大师I";
            }
            else if ( gradeNum == 28 )
            {
                gradeStr = "战争王者";
            }
            return gradeStr;
        }

        private void OnCreateItem(RankViewItem item)
        {
            item.OnItemClickHnadler = UpdateRightInfo;
            item.controler = controller;
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

        private void OnFriendBtnClick()
        {
            isFriendRank = true;
            Reset();
            ShowAllUnSelectImage();            
            friendUnSelect.SetActive( false );
        }

        private void OnDistrictBtnClick()
        {
            isFriendRank = false;
            Reset();
            ShowAllUnSelectImage();
            districtUnSelect.SetActive( false );
        }

        private void OnChangeRankBtnClick()
        {
            if ( changeRankTypeRoot.gameObject.activeInHierarchy )
            {
                ShowChangeRankTypePanel( false );
            }
            else
            {
                ShowChangeRankTypePanel( true );
            }            
        }

        private void ShowAllUnSelectImage()
        {
            friendUnSelect.SetActive( true );
            districtUnSelect.SetActive( true );            
        }

        private void OnGradeRankBtnClick()
        {
            RankType = RankType.GradeRank;
            Reset();
            ShowChangeRankTypePanel( false );
        }

        private void OnUnitQuantityRankBtnClick()
        {
            RankType = RankType.UnitQuantityRank;
            Reset();
            ShowChangeRankTypePanel( false );
        }

        private void OnSkinRankBtnClick()
        {
            RankType = RankType.SkinRank;
            Reset();
            ShowChangeRankTypePanel( false );
        }

        private void OnWinStreakRankBtnClick()
        {
            RankType = RankType.WinStreakRank;
            Reset();
            ShowChangeRankTypePanel( false );
        }

        private void OnWinRankBtnClick()
        {
            RankType = RankType.WinRank;
            Reset();
            ShowChangeRankTypePanel( false );
        }

        private void OnBattleTimesRankBtnClick()
        {
            RankType = RankType.BattleTimesRank;
            Reset();
            ShowChangeRankTypePanel( false );
        }

        private void ShowChangeRankTypePanel( bool isShow )
        {
            changeRankTypeRoot.gameObject.SetActive( isShow );
            changeRankUnSelect.SetActive( !isShow );
        }
    }
}
