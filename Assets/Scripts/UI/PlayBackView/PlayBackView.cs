using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using Data;
using Resource;
using Constants;

namespace UI
{
    public class PlayBackView : ViewBase
    {
        private PlayBackViewControler controler;

        private Transform aniTran;
        private Transform topPanel;
        private Text redSideKillCountText, blueSideKillCountText, timerText;
        private Transform leftTopPanel;
        private Transform miniMapBg;
        private Transform leftBottomPanel;
        private Transform blueHeadTran0, blueHeadTran1;
        private Button blueHead0, blueHead1;
        private Image blueHeadIcon0, blueHeadIcon1;
        private Button blueArrowHead0, blueArrowHead1;
        private Transform blueUnitCardRoot;
        private Image[] blueUnitCard = new Image[9];
        private Button blueUnitLeftArrowHead, blueUnitRightArrowHead;
        private Transform bottomPanel;
        private Transform bottomBg;
        private Button speedBtn, pauseBtn;
        private GameObject speedOne, speedTwo, speedFour, speedEight;
        private Image timeSlider;
        private Text battleDurationText;
        private Transform rightBottomPanel;
        private Transform redHeadTran0, redHeadTran1;
        private Button redHead0, redHead1;
        private Image redHeadIcon0, redHeadIcon1;
        private Button redArrowHead0, redArrowHead1;
        private Transform redUnitCardRoot;
        private Image[] redUnitCard = new Image[9];
        private Button redUnitLeftArrowHead, redUnitRightArrowHead;
        private Transform rightPanel;
        private RectTransform buildingBuffPanelRect;
        private float originalPanelHeight;
        private RectTransform attributeTextRect;
        private Text attributeText;
        private float originalTextHeight;
        private Button buffPanelArrowButton;
        private float spaceText;
        private bool isSpread = false;
        private Transform rightTopPanel;
        private Transform topRightBg;
        private Text killCountText, deadCountText, emberText;
        private Button quitBtn;
        private Transform centerPanel;
        public NoticeView noticeView;
        public CommandNoticeView commandNoticeView;

        private bool isShowBlue0 = false;
        private bool isShowBlue1 = false;
        private bool isShowRed0 = false;
        private bool isShowRed1 = false;
        private int currentSpeed = 1;
        private int currentPlay = 1;
        private List<int> blueHeadList0 = new List<int>();
        private List<int> blueHeadList1 = new List<int>();
        private List<int> redHeadList0 = new List<int>();
        private List<int> redHeadList1 = new List<int>();
        private string blueHeadStr0;
        private string blueHeadStr1;
        private string redHeadStr0;
        private string redHeadStr1;
        private ForceMark followMark;
        private long battleDurationTime = 0;
        private float batleDurationFloat = 0;
        private string battleDurationStr = "";

        public override void OnInit()
        {
            base.OnInit();
            controler = new PlayBackViewControler( this );
            _controller = controler as PlayBackViewControler;

            aniTran = transform.Find( "Ani" );
            topPanel = aniTran.Find( "TopPanel" );
            redSideKillCountText = topPanel.Find( "RedSideKillCountText" ).GetComponent<Text>();
            blueSideKillCountText = topPanel.Find( "BlueSideKillCountText" ).GetComponent<Text>();
            timerText = topPanel.Find( "TimeText" ).GetComponent<Text>();
            leftTopPanel = aniTran.Find( "LeftTopPanel" );
            miniMapBg = leftTopPanel.Find( "MiniMapBg" );
            leftBottomPanel = aniTran.Find( "LeftBottomPanel" );
            blueHeadTran0 = leftBottomPanel.Find( "BlueHead0" );                      
            blueHeadIcon0 = blueHeadTran0.Find( "HeadImage" ).GetComponent<Image>();
            blueHead0 = blueHeadIcon0.GetComponent<Button>();
            blueHead0.AddListener( OnClickBlueHead0 );
            blueArrowHead0 = blueHeadTran0.Find( "ArrowHead" ).GetComponent<Button>();
            blueArrowHead0.AddListener( OnClickBlueArrowHead0 );
            blueHeadTran1 = leftBottomPanel.Find( "BlueHead1" );                        
            blueHeadIcon1 = blueHeadTran1.Find( "HeadImage" ).GetComponent<Image>();
            blueHead1 = blueHeadIcon1.GetComponent<Button>();
            blueHead1.AddListener( OnClickBlueHead1 );
            blueArrowHead1 = blueHeadTran1.Find( "ArrowHead" ).GetComponent<Button>();
            blueArrowHead1.AddListener( OnClickBlueArrowHead1 );
            blueUnitCardRoot = leftBottomPanel.Find( "UnitCardRoot" );
            for ( int i = 0; i < 9; i++ )
            {
                blueUnitCard[i] = blueUnitCardRoot.Find( "CardRoot/HeadIcon" + i + "/HeadIcon" ).GetComponent<Image>();
            }
            blueUnitLeftArrowHead = blueUnitCardRoot.Find( "LeftArrowHead" ).GetComponent<Button>();
            blueUnitRightArrowHead = blueUnitCardRoot.Find( "RightArrowHead" ).GetComponent<Button>();
            blueUnitLeftArrowHead.AddListener( OnClickBlueUnitArrowHead );
            blueUnitRightArrowHead.AddListener( OnClickBlueUnitArrowHead );
            bottomPanel = aniTran.Find( "BottomPanel" );
            bottomBg = bottomPanel.Find( "BottomBg" );
            speedBtn = bottomBg.Find( "SpeedBtn" ).GetComponent<Button>();
            speedBtn.AddListener( OnClickSpeedBtn );
            speedOne = speedBtn.transform.Find( "One" ).gameObject;
            speedTwo = speedBtn.transform.Find( "Two" ).gameObject;
            speedFour = speedBtn.transform.Find( "Four" ).gameObject;
            speedEight = speedBtn.transform.Find( "Eight" ).gameObject;
            HideAllSpeedMarkBut( 1 );
            timeSlider = bottomBg.Find( "TimeSlider" ).GetComponent<Image>();
            SetTimeSliderValue( 0 );
            battleDurationText = bottomBg.Find( "BattleDurationText" ).GetComponent<Text>();
            pauseBtn = bottomBg.Find( "PauseBtn" ).GetComponent<Button>();
            pauseBtn.AddListener( OnClickPauseBtn );
            rightBottomPanel = aniTran.Find( "RightBottomPanel" );
            redHeadTran0 = rightBottomPanel.Find( "RedHead0" );                              
            redHeadIcon0 = redHeadTran0.transform.Find( "HeadImage" ).GetComponent<Image>();
            redHead0 = redHeadIcon0.GetComponent<Button>();
            redHead0.AddListener( OnClickRedHead0 );
            redArrowHead0 = redHeadTran0.transform.Find( "ArrowHead" ).GetComponent<Button>();
            redArrowHead0.AddListener( OnClickRedArrowHead0 );
            redHeadTran1 = rightBottomPanel.Find( "RedHead1" );
            redHeadIcon1 = redHeadTran1.Find( "HeadImage" ).GetComponent<Image>();
            redHead1 = redHeadIcon1.GetComponent<Button>();
            redHead1.AddListener( OnClickRedHead1 );
            redArrowHead1 = redHeadTran1.Find( "ArrowHead" ).GetComponent<Button>();
            redArrowHead1.AddListener( OnClickRedArrowHead1 );
            redUnitCardRoot = rightBottomPanel.Find( "UnitCardRoot" );
            for ( int i = 0; i < 9; i++ )
            {
                redUnitCard[i] = redUnitCardRoot.Find( "CardRoot/HeadIcon" + i + "/HeadIcon" ).GetComponent<Image>();
            }
            redUnitLeftArrowHead = redUnitCardRoot.Find( "LeftArrowHead" ).GetComponent<Button>();
            redUnitRightArrowHead = redUnitCardRoot.Find( "RightArrowHead" ).GetComponent<Button>();
            redUnitLeftArrowHead.AddListener( OnClickRedUnitArrowHead );
            redUnitRightArrowHead.AddListener( OnClickRedUnitArrowHead );
            rightPanel = aniTran.Find( "RightPanel" );
            buildingBuffPanelRect = rightPanel.Find( "BuildingBuffPanel" ).GetComponent<RectTransform>();
            originalPanelHeight = buildingBuffPanelRect.rect.height;
            attributeTextRect = buildingBuffPanelRect.transform.Find( "AttributeText" ).GetComponent<RectTransform>();
            attributeText = attributeTextRect.transform.GetComponent<Text>();
            originalTextHeight = attributeTextRect.rect.height;
            spaceText = originalPanelHeight - originalTextHeight;
            buffPanelArrowButton = buildingBuffPanelRect.transform.Find( "BuffPanelArrowButton" ).GetComponent<Button>();
            buffPanelArrowButton.AddListener( BuffPanelArrowButtonEvent );
            buildingBuffPanelRect.gameObject.SetActive( false );
            rightTopPanel = aniTran.Find( "RightTopPanel" );
            topRightBg = rightTopPanel.Find( "TopRightBg" );
            killCountText = topRightBg.Find( "KillCountText" ).GetComponent<Text>();
            deadCountText = topRightBg.Find( "KilledCountText" ).GetComponent<Text>();
            emberText = topRightBg.Find( "EmberText" ).GetComponent<Text>();
            quitBtn = topRightBg.Find( "QuitButton" ).GetComponent<Button>();
            quitBtn.AddListener( OnClickQuitBtn );
            centerPanel = aniTran.Find( "CenterPanel" );
            noticeView = centerPanel.Find( "NoticePanel" ).gameObject.AddComponent<NoticeView>();
            noticeView.Init();
            commandNoticeView = centerPanel.Find( "CommandNoticePanel" ).gameObject.AddComponent<CommandNoticeView>();
            commandNoticeView.OnInit();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            SetBattleHeadIcon();
            battleDurationTime = controler.GetBattleDurationTime();
            batleDurationFloat = battleDurationTime * 0.001f;
            SetBattleDurationStr();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void SetBattleDurationStr()
        {
            float duratiion = battleDurationTime * 0.001f;
            battleDurationStr = "/" + string.Format( "{0:00} : {1:00}" , duratiion / 60 , duratiion % 60 );
        }

        private void SetBattleHeadIcon()
        {
            GetBattleHeadIcon();
            if ( !string.IsNullOrEmpty( blueHeadStr0 ) )
            {
                SetImageIcon( blueHeadIcon0 , blueHeadStr0 );
            }
            else
            {
                blueHeadTran0.gameObject.SetActive( false );
            }
            if ( !string.IsNullOrEmpty( blueHeadStr1 ) )
            {
                SetImageIcon( blueHeadIcon1 , blueHeadStr1 );
            }
            else
            {
                blueHeadTran1.gameObject.SetActive( false );
            }
            if ( !string.IsNullOrEmpty( redHeadStr0 ) )
            {
                SetImageIcon( redHeadIcon0 , redHeadStr0 );
            }
            else
            {
                redHeadTran0.gameObject.SetActive( false );
            }
            if ( !string.IsNullOrEmpty( redHeadStr1 ) )
            {
                SetImageIcon( redHeadIcon1 , redHeadStr1 );
            }
            else
            {
                redHeadTran1.gameObject.SetActive( false );
            }
        }

        private void GetBattleHeadIcon()
        {
            blueHeadStr0 = controler.GetBattleHeadIconStr( MatchSide.Blue , 0 );
            blueHeadStr1 = controler.GetBattleHeadIconStr( MatchSide.Blue , 1 );
            redHeadStr0 = controler.GetBattleHeadIconStr( MatchSide.Red , 0 );
            redHeadStr1 = controler.GetBattleHeadIconStr( MatchSide.Red , 1 );
        }

        private void HideBlueUnitCardRoot()
        {
            blueUnitCardRoot.gameObject.SetActive( false );
            isShowBlue0 = false;
            isShowBlue1 = false;
            blueUnitLeftArrowHead.gameObject.SetActive( false );
            blueUnitRightArrowHead.gameObject.SetActive( false );
            blueArrowHead0.gameObject.SetActive( true );
            blueArrowHead1.gameObject.SetActive( true );
        }

        private void OnClickBlueHead0()
        {
            followMark = controler.GetForceMark( MatchSide.Blue , 0 );
            SetFollowForce();
        }

        private void OnClickBlueArrowHead0()
        {
            if ( isShowBlue0 )
            {
                HideBlueUnitCardRoot();
                return;
            }
            if ( !SetHeadIcon( MatchSide.Blue , 0 ) )
            {
                return;
            }
            blueUnitCardRoot.gameObject.SetActive( true );
            blueArrowHead0.gameObject.SetActive( false );
            blueUnitLeftArrowHead.gameObject.SetActive( true );
            isShowBlue0 = true;
            isShowBlue1 = false;
        }

        private void OnClickBlueHead1()
        {
            followMark = controler.GetForceMark( MatchSide.Blue, 1 );
            SetFollowForce();
        }

        private void OnClickBlueArrowHead1()
        {
            if ( isShowBlue1 )
            {
                HideBlueUnitCardRoot();
                return;
            }
            if ( !SetHeadIcon( MatchSide.Blue , 1 ) )
            {
                return;
            }
            blueUnitCardRoot.gameObject.SetActive( true );
            blueArrowHead1.gameObject.SetActive( false );
            blueUnitRightArrowHead.gameObject.SetActive( true );
            isShowBlue1 = true;
            isShowBlue0 = false;
        }

        private void OnClickBlueUnitArrowHead()
        {
            HideBlueUnitCardRoot();
        }

        private void SetFollowForce()
        {
            controler.SetFollowForce( followMark );
        }

        private bool SetHeadIcon( MatchSide side,int index )
        {
            if ( side == MatchSide.Blue )
            {
                if ( index == 0 )
                {
                    if ( blueHeadList0.Count <= 0 )
                    {
                        blueHeadList0 = controler.GetUnitHeadIcon( MatchSide.Blue , 0 );
                    }
                    if ( blueHeadList0.Count <= 0 )
                    {
                        return false;
                    }
                    for ( int i = 0; i < blueUnitCard.Length; i++ )
                    {
                        SetImageIcon( blueUnitCard[i], blueHeadList0[i] );
                    }
                }
                else if ( index == 1 )
                {
                    if ( blueHeadList1.Count <= 0 )
                    {
                        blueHeadList1 = controler.GetUnitHeadIcon( MatchSide.Blue, 1 );
                    }
                    if ( blueHeadList1.Count <= 0 )
                    {
                        return false;
                    }
                    for ( int i = 0; i < blueUnitCard.Length; i++ )
                    {
                        SetImageIcon( blueUnitCard[i] , blueHeadList1[i] );
                    }
                }
            }
            else if ( side == MatchSide.Red )
            {
                if ( index == 0 )
                {
                    if ( redHeadList0.Count <= 0 )
                    {
                        redHeadList0 = controler.GetUnitHeadIcon( MatchSide.Red, 0 );
                    }
                    if ( redHeadList0.Count <= 0 )
                    {
                        return false;
                    }
                    for ( int i = 0; i < redUnitCard.Length; i++ )
                    {
                        SetImageIcon( redUnitCard[i] , redHeadList0[i] );
                    }
                }
                else if ( index == 1 )
                {
                    if ( redHeadList1.Count <= 0 )
                    {
                        redHeadList1 = controler.GetUnitHeadIcon( MatchSide.Red , 1 );
                    }
                    if ( redHeadList1.Count <= 0 )
                    {
                        return false;
                    }
                    for ( int i = 0; i < redUnitCard.Length; i++ )
                    {
                        SetImageIcon( redUnitCard[i] , redHeadList1[i] );
                    }
                }
            }
            return true;
        }

        private void SetImageIcon( Image image,int icon )
        {
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon , delegate ( string name , AtlasSprite atlasSprite , System.Object param )
            {
                image.SetSprite( atlasSprite );
            } , true );
        }

        private void SetImageIcon( Image image , string icon )
        {
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon , delegate ( string name , AtlasSprite atlasSprite , System.Object param )
            {
                image.SetSprite( atlasSprite );
            } , true );
        }

        private void HideRedUnitCardRoot()
        {
            redUnitCardRoot.gameObject.SetActive( false );
            isShowRed0 = false;
            isShowRed1 = false;
            redArrowHead0.gameObject.SetActive( true );
            redArrowHead1.gameObject.SetActive( true );
            redUnitLeftArrowHead.gameObject.SetActive( false );
            redUnitRightArrowHead.gameObject.SetActive( false );
        }

        private void OnClickRedUnitArrowHead()
        {
            HideRedUnitCardRoot();
        }

        private void OnClickRedHead0()
        {
            followMark = controler.GetForceMark( MatchSide.Red , 0 );
            SetFollowForce();
        }

        private void OnClickRedArrowHead0()
        {
            if ( isShowRed0 )
            {
                HideRedUnitCardRoot();
                return;
            }
            if ( !SetHeadIcon( MatchSide.Red , 0 ) )
            {
                return;
            }
            redUnitCardRoot.gameObject.SetActive( true );
            isShowRed0 = true;
            isShowRed1 = false;
            redArrowHead0.gameObject.SetActive( false );
            redUnitRightArrowHead.gameObject.SetActive( true );
        }

        private void OnClickRedHead1()
        {
            followMark = controler.GetForceMark( MatchSide.Red , 1 );
            SetFollowForce();
        }

        private void OnClickRedArrowHead1()
        {
            if ( isShowRed1 )
            {
                HideRedUnitCardRoot();
                return;
            }
            if ( !SetHeadIcon( MatchSide.Red , 1 ) )
            {
                return;
            }
            redUnitCardRoot.gameObject.SetActive( true );
            isShowRed1 = true;
            isShowRed0 = false;
            redArrowHead1.gameObject.SetActive( false );
            redUnitLeftArrowHead.gameObject.SetActive( true );
        }

        private void OnClickSpeedBtn()
        {
            currentSpeed++;
            if ( currentSpeed >= 5 )
            {
                currentSpeed = 1;
            }
            SetSpeed( currentSpeed );
        }

        private void OnClickPauseBtn()
        {
            currentPlay++;
            if ( currentPlay >= 3 )
            {
                currentPlay = 1;
            }
            SetPlayType( currentPlay );
        }

        private void OnClickQuitBtn()
        {
            controler.EnterMainMenu();
        }

        private void SetSpeed( int speed )
        {
            if ( speed == 1 )
            {
                HideAllSpeedMarkBut( 1 );
            }
            else if ( speed == 2 )
            {
                HideAllSpeedMarkBut( 2 );
            }
            else if ( speed == 3 )
            {
                HideAllSpeedMarkBut( 4 );
            }
            else if ( speed == 4 )
            {
                HideAllSpeedMarkBut( 8 );
            }
        }

        private void HideAllSpeedMarkBut( int speed )
        {
            speedOne.SetActive( false );
            speedTwo.SetActive( false );
            speedFour.SetActive( false );
            speedEight.SetActive( false );

            if ( speed == 1 )
            {
                speedOne.SetActive( true );
            }
            else if ( speed == 2 )
            {
                speedTwo.SetActive( true );
            }
            else if ( speed == 4 )
            {
                speedFour.SetActive( true );
            }
            else if ( speed == 8 )
            {
                speedEight.SetActive( true );
            }

            controler.SetSpeed( speed );
        }

        private void SetPlayType( int type )
        {
            controler.SetPauseAndPlay( type );
        }

        private void SetTimeSliderValue( float value )
        {
            timeSlider.fillAmount = value;
        }

        private void SetBattleDurationText( int time )
        {
            battleDurationText.text = string.Format( "{0:00} : {1:00}" , time / 60 , time % 60 ) + battleDurationStr;
            SetTimeSliderValue( time / batleDurationFloat );
        }

        private void BuffPanelArrowButtonEvent()
        {
            isSpread = !isSpread;

            if ( isSpread && attributeText.preferredHeight > originalTextHeight )
            {
                buildingBuffPanelRect.sizeDelta = new Vector2( buildingBuffPanelRect.sizeDelta.x , attributeText.preferredHeight + spaceText );
                attributeTextRect.sizeDelta = new Vector2( attributeTextRect.sizeDelta.x , attributeText.preferredHeight );
            }
            else
            {
                buildingBuffPanelRect.sizeDelta = new Vector2( buildingBuffPanelRect.sizeDelta.x , originalPanelHeight );
                attributeTextRect.sizeDelta = new Vector2( attributeTextRect.sizeDelta.x , originalTextHeight );
            }

            buffPanelArrowButton.transform.Rotate( new Vector3( 0 , 0 , 180 ) );
        }

        public void SetBuffPanelText( string text )
        {
            attributeText.text = text;
            buildingBuffPanelRect.gameObject.SetActive( true );
        }

        public void SetRedSideKillCountText( int redKill )
        {
            redSideKillCountText.text = redKill.ToString();
        }

        public void SetBlueSideKillCountText( int blueKill )
        {
            blueSideKillCountText.text = blueKill.ToString();
        }

        public void SetTimerText( int time )
        {
            timerText.text = string.Format( "{0:00} : {1:00}", time / 60, time % 60 );
            SetBattleDurationText( time );
        }

        public void SetKillCountText( int killCount )
        {
            killCountText.text = killCount.ToString();
        }

        public void SetDeadCountText( int deadCount )
        {
            deadCountText.text = deadCount.ToString();
        }

        public void SetEmberText( int emberCount )
        {
            emberText.text = emberCount.ToString();
        }

        public void OpenBattleResult( NoticeType resType , BattleResultData resInfo )
        {            
            StartCoroutine( GenerateBanner( resType , resInfo ) );
        }

        private IEnumerator GenerateBanner( NoticeType resType , BattleResultData resInfo )
        {
            yield return new WaitForSeconds( 3.0f );
            GameObject banner;

            switch ( resType )
            {
                case NoticeType.BattleResultBlueWin:
                {
                    if ( DataManager.GetInstance().GetMatchSide() == MatchSide.Red )
                    {
                        banner = Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "BannerDefeat" ) );

                        SoundManager.Instance.PlayMusic( GameConstants.SOUND_DEFEAT_ID , false );
                    }
                    else
                    {
                        banner = Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "BannerVictory" ) );

                        SoundManager.Instance.PlayMusic( GameConstants.SOUND_VICTORY_ID , false );
                    }
                    break;
                }
                case NoticeType.BattleResultRedWin:
                {
                    if ( DataManager.GetInstance().GetMatchSide() == MatchSide.Red )
                    {
                        banner = Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "BannerVictory" ) );

                        SoundManager.Instance.PlayMusic( GameConstants.SOUND_VICTORY_ID , false );
                    }
                    else
                    {
                        banner = Instantiate( GameResourceLoadManager.GetInstance().LoadAsset<GameObject>( "BannerDefeat" ) );

                        SoundManager.Instance.PlayMusic( GameConstants.SOUND_DEFEAT_ID , false );
                    }
                    break;
                }
                default:
                {
                    banner = Instantiate( Resources.Load( "Prefabs/UI/BattleScreenItem/banners/banner_draw" ) as GameObject );

                    SoundManager.Instance.PlayMusic( GameConstants.SOUND_VICTORY_ID , false );
                    break;
                }
            }

            //banner.transform.Find( "Image" ).gameObject.SetActive( false );
            banner.transform.SetParent( transform.parent , false );

            StartCoroutine( EnableBattleResult( resType , resInfo , banner ) );
        }

        private IEnumerator EnableBattleResult( NoticeType resType , BattleResultData resInfo , GameObject banner )
        {
            yield return new WaitForSeconds( 3f );

            Destroy( banner );

            UIManager.Instance.GetUIByType( UIType.BattleResultScreen , ( ViewBase ui , System.Object param ) => {
                if ( !ui.openState )
                {
                    ui.OnEnter();
                    ( ui as BattleResultView ).SetIsMainUIOpen( false );
                    ( ui as BattleResultView ).SetBattleResultData( resType, resInfo, true);
                }
            } );
        }
    }
}