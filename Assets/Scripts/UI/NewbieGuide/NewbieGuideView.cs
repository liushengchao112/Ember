using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using Utils;
using Resource;
using Data;
using Map;
using DG.Tweening;
using TutorialModeStage = PVE.TutorialModeManager.TutorialModeStage;

namespace UI
{
    public class NewbieGuideView : MonoBehaviour
    {
        private NewbieGuideController controler;

        #region Component
        private Transform aniTran;
        private Image markImage;
        private Image transparentImage;
        private Image showImage;
        private Button circleImage;
        private Image arrowImage;
        private Image targetImage;
        private Image handImage;

        private GameObject targetObj;
        private Canvas canvas;
        private Vector3 arrowOffset = new Vector3( 33, -30, 0 );
        private Vector3 moveOffset = new Vector3( 200, 0, 0 );
        private int currentGuideIndex = 0;
        private int lastGuideIndex = -1;

        //BigTextureRoot
        private Transform bigTextureRoot;
        private Button bigTextureBg;
        private Image bigTexture0, bigTexture1;
        private Image[] titleImages = new Image[5];
        private Transform moveToPos;
        #endregion

        private bool isCanDrag = true;
        private float dragDistance = 5.0f;

        private BattleType battleType;
        private TutorialModeStage currentModeStage;

        private PassEventHadler passEventHadler;
        private Tween loopTween;

        private void Awake()
        {
            controler = new NewbieGuideController( this );

            aniTran = transform.Find( "Ani" );
            markImage = aniTran.Find( "MarkImage" ).GetComponent<Image>();
            transparentImage = aniTran.Find( "TransparentImage" ).GetComponent<Image>();
            transparentImage.gameObject.SetActive( false );
            showImage = aniTran.Find( "ShowImage" ).GetComponent<Image>();
            circleImage = aniTran.Find( "CircleImage" ).GetComponent<Button>();
            circleImage.GetComponent<PassEventHadler>().OnButtonClick = OncirCleImageClick;
            circleImage.GetComponent<PassEventHadler>().OnBeginDragEvent = OnCircleBeginDrag;
            //circleImage.GetComponent<PassEventHadler>().OnButtonDrag = OnCircleImageDrag;
            passEventHadler = circleImage.GetComponent<PassEventHadler>();
            arrowImage = aniTran.Find( "ArrowImage" ).GetComponent<Image>();
            targetImage = aniTran.Find( "TargetImage" ).GetComponent<Image>();
            handImage = aniTran.Find( "HandImage" ).GetComponent<Image>();
            //BigTextureRoot
            bigTextureRoot = aniTran.Find( "BigTextureRoot" );
            bigTextureBg = bigTextureRoot.GetComponent<Button>();
            bigTexture0 = bigTextureRoot.Find( "Image0" ).GetComponent<Image>();
            bigTexture1 = bigTextureRoot.Find( "Image1" ).GetComponent<Image>();
            titleImages[0] = bigTextureRoot.Find( "TitleImages/Image1" ).GetComponent<Image>();
            titleImages[1] = bigTextureRoot.Find( "TitleImages/Image2" ).GetComponent<Image>();
            titleImages[2] = bigTextureRoot.Find( "TitleImages/Image3" ).GetComponent<Image>();
            titleImages[3] = bigTextureRoot.Find( "TitleImages/Image4" ).GetComponent<Image>();
            titleImages[4] = bigTextureRoot.Find( "TitleImages/Image5" ).GetComponent<Image>();
            moveToPos = bigTextureRoot.Find( "MoveToPos" );
            bigTextureBg.AddListener( OnClickBackground );
            bigTextureRoot.gameObject.SetActive( false );

            battleType = controler.GetBattleType();
        }

        void OnDestroy()
        {
            controler.OnDestroy();
        }

        private void SetIsCanDrag( bool isCan )
        {
            passEventHadler.isCanDrag = isCan;
        }

        private void SetIsTap3D( bool isTap3D )
        {
            passEventHadler.isTap3D = isTap3D;
        }

        public void Open( object index, object lastIndex )
        {
            currentModeStage = DataManager.GetInstance().GetTutorialStage();

            controler.OnShowNewbieUI( (int)index, (int)lastIndex );
        }

        private void MoveTexture( Image texture )
        {
            DOTween.To( () => texture.transform.position, value => texture.transform.position = value, moveToPos.position, 1f );
        }

        private void SetImageIcon( Image image, string icon )
        {
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                image.SetSprite( atlasSprite );
            }, true );
        }

        public void SetGuideIndex( int index, int lastIndex )
        {
            currentGuideIndex = index;
            lastGuideIndex = lastIndex;

            if ( currentModeStage == TutorialModeStage.NormallyControlOperation_Stage )
            {
                SetBigTexture0( "B_T_NewbieGuide1" );
                SetBigTexture1( "B_S_NewbieGuide2" );
            }
            else if ( currentModeStage == TutorialModeStage.BuildingControlOperation_Stage )
            {
                SetBigTexture0( "B_S_NewbieGuide4" );
                SetBigTexture1( "B_S_NewbieGuide5" );
            }
        }

        #region Set UI

        private void HideBigTexture()
        {
            bigTextureRoot.gameObject.SetActive( false );
        }

        private void ShowBigTexture()
        {
            bigTextureRoot.gameObject.SetActive( true );
        }

        private void SetBigTexture0( string icon )
        {
            bigTexture0.gameObject.SetActive( false );
            bigTexture0.transform.localPosition = Vector3.zero;
            SetImageIcon( bigTexture0, icon );
        }

        private void SetBigTexture1( string icon )
        {
            bigTexture1.gameObject.SetActive( false );
            bigTexture1.transform.localPosition = Vector3.zero;
            SetImageIcon( bigTexture1, icon );
        }

        private void HideGuidePanel()
        {
            aniTran.gameObject.SetActive( false );
        }

        private void ShowGuidePanel()
        {
            aniTran.gameObject.SetActive( true );
        }

        private void SetMaskImageState( bool state )
        {
            markImage.gameObject.SetActive( state );
            transparentImage.gameObject.SetActive( !state );
        }

        public void ShowNewbieUI()
        {
            gameObject.SetActive( true );

            #region NormallyControlOperation_Stage
            if ( currentModeStage == TutorialModeStage.NormallyControlOperation_Stage )
            {
                if ( currentGuideIndex == 0 )
                {
                    bigTextureRoot.gameObject.SetActive( true );
                    bigTexture0.gameObject.SetActive( true );
                    SetTitleImages( currentGuideIndex );
                }
                else if ( currentGuideIndex == 6 )
                {
                    bigTextureRoot.gameObject.SetActive( true );
                    bigTexture1.gameObject.SetActive( true );
                    SetTitleImages( currentGuideIndex );
                }
                else if ( currentGuideIndex == 9 )
                {
                    bigTextureRoot.gameObject.SetActive( true );
                    bigTexture0.gameObject.SetActive( true );
                    SetTitleImages( currentGuideIndex );
                }
                else
                    GetTargetImage();
            }
            #endregion
            #region BuildingControlOperation_Stage
            else if ( currentModeStage == TutorialModeStage.BuildingControlOperation_Stage )
            {
                if ( currentGuideIndex == 0 )
                {
                    bigTextureRoot.gameObject.SetActive( true );
                    bigTexture0.gameObject.SetActive( true );
                    SetTitleImages( currentGuideIndex );
                }
                else if ( currentGuideIndex == 1 )
                {
                    bigTextureRoot.gameObject.SetActive( true );
                    bigTexture1.gameObject.SetActive( true );
                    SetTitleImages( currentGuideIndex );
                }
                else
                    GetTargetImage();
            }
            #endregion
        }

        private void SetTitleImages( int guideIndex )
        {
            foreach ( Image item in titleImages )
            {
                item.gameObject.SetActive( false );
            }
            #region NormallyControlOperation_Stage
            if ( currentModeStage == TutorialModeStage.NormallyControlOperation_Stage )
            {
                if ( guideIndex == 0 )
                    titleImages[0].gameObject.SetActive( true );
                else if ( guideIndex == 6 )
                    titleImages[1].gameObject.SetActive( true );
                else if ( guideIndex == 9 )
                    titleImages[2].gameObject.SetActive( true );
            }
            #endregion
            #region BuildingControlOperation_Stage
            else if ( currentModeStage == TutorialModeStage.BuildingControlOperation_Stage )
            {
                if ( guideIndex == 0 )
                    titleImages[3].gameObject.SetActive( true );
                else if ( guideIndex == 1 )
                    titleImages[4].gameObject.SetActive( true );
            }
            #endregion
        }

        public void TutorialFinish()
        {
            Debug.Log( "------------TutorialFinish" );
            this.gameObject.SetActive( false );
            SetMaskImageState( true );
        }

        public void TutorialStop()
        {
            Debug.Log( "------------TutorialStop" );
            SetMaskImageState( false );
        }

        public void MoveGuideOutsideTheScreen()
        {
            if ( loopTween != null )
            {
                loopTween.Kill( true );
                loopTween = null;
            }
            showImage.transform.position = new Vector3( 0, 2000, 0 );
            circleImage.transform.position = new Vector3( 0, 2000, 0 );
            arrowImage.transform.position = new Vector3( 0, 2000, 0 );
            targetImage.transform.position = new Vector3( 0, 2000, 0 );
        }

        public void HideMarkImage()
        {
            markImage.gameObject.SetActive( false );
        }

        #endregion

        private void OnClickBackground()
        {
            bigTextureRoot.gameObject.SetActive( false );
            OncirCleImageClick();
        }

        private void OnCircleBeginDrag()
        {
            if ( currentGuideIndex == 8 )
            {

            }
        }

        private void OncirCleImageClick()
        {
            if ( currentModeStage == TutorialModeStage.NormallyControlOperation_Stage )
            {
                if ( currentGuideIndex == 8 )
                    return;
                else if ( currentGuideIndex == 9 )
                {
                    lastGuideIndex = 12;
                }
            }
            else if ( currentModeStage == TutorialModeStage.BuildingControlOperation_Stage )
            {

            }
            MessageDispatcher.PostMessage( Constants.MessageType.TutorialUpData );
        }

        public void GoNext( object guideIndex )
        {
            currentGuideIndex = (int)guideIndex;
            controler.ResetNewbieGuide( currentGuideIndex, lastGuideIndex );
            //HideGuidePanel();

            MoveGuideOutsideTheScreen();

            DebugUtils.Log( DebugUtils.Type.Tutorial, "CurrentGuideIndex is [ " + currentGuideIndex + " ]  ,  CurrentTutorialModeStage is [ " + currentModeStage + " ]" );

            #region NormallyControlOperation_Stage
            if ( currentModeStage == TutorialModeStage.NormallyControlOperation_Stage )
            {
                if ( currentGuideIndex == 0 )
                {
                    SetIsCanDrag( false );
                    SetIsTap3D( false );
                    ShowNewbieUI();
                }
                else if ( currentGuideIndex == 5 )
                {
                    SetIsCanDrag( false );
                    SetIsTap3D( true );
                    StartCoroutine( DelayGetTarget3D( controler.GetUnitPostion( 1 ), false, new Vector2( 100, 100 ) ) );
                }
                else if ( currentGuideIndex == 6 )
                {
                    SetBigTexture0( "B_S_NewbieGuide3" );
                    ShowNewbieUI();
                }
                else if ( currentGuideIndex == 7 )
                {
                    SetIsCanDrag( false );
                    SetIsTap3D( true );
                    StartCoroutine( DelayGetTarget3D( controler.GetBoxPostion( 0 ), false, new Vector2( 100, 100 ) ) );
                }
                else if ( currentGuideIndex == 8 )
                {
                    SetIsCanDrag( true );
                    SetIsTap3D( true );
                    SetTargetPos( controler.GetBoxPostion( 0 ) );
                    StartCoroutine( DelayGetTarget3D( controler.GetUnitPostion( 2 ), true, new Vector2( 500, 250 ) ) );
                }
                else if ( currentGuideIndex == 9 )
                {
                    SetBigTexture1( "B_S_NewbieGuide4" );
                    ShowNewbieUI();
                }
                else if ( currentGuideIndex == 11 )
                {
                    SetIsTap3D( false );
                    SetIsCanDrag( false );
                    StartCoroutine( DelayGetTarget3D( controler.GetBoxPostion( 1 ), false, new Vector2( 100, 100 ) ) );
                }
                else if ( currentGuideIndex == 12 )
                {
                    SetIsTap3D( true );
                    SetIsCanDrag( false );
                }
                else if ( currentGuideIndex <= lastGuideIndex )
                {
                    SetIsTap3D( false );
                    SetIsCanDrag( false );
                    GetTargetImage();
                }
            }
            #endregion

            #region BuildingControlOperation_Stage
            if ( currentModeStage == TutorialModeStage.BuildingControlOperation_Stage )
            {
                if ( currentGuideIndex == 0 || currentGuideIndex == 1 )
                {
                    ShowNewbieUI();
                }
                else if ( currentGuideIndex == 2 )
                {
                    SetIsCanDrag( false );
                    SetIsTap3D( true );
                    StartCoroutine( DelayGetTarget3D( controler.GetInstitutePostion( 0 ), false, new Vector2( 250, 220 ) ) );
                }
                else if ( currentGuideIndex == 4 )
                {
                    SetIsCanDrag( false );
                    SetIsTap3D( true );
                    StartCoroutine( DelayGetTarget3D( controler.GetInstitutePostion( 0 ), false, new Vector2( 250, 220 ) ) );
                }
                else if ( currentGuideIndex == 8 )
                {
                    SetIsCanDrag( false );
                    SetIsTap3D( true );
                    StartCoroutine( DelayGetTarget3D( controler.GetTowerPostion( 0 ), false, new Vector2( 100, 100 ) ) );
                }
                else if ( currentGuideIndex <= lastGuideIndex )
                {
                    SetIsTap3D( false );
                    SetIsCanDrag( false );
                    GetTargetImage();
                }
            }
            #endregion
        }

        private void OnCircleImageDrag( Vector3 v )
        {
            Vector3 vv = circleImage.transform.position;
            if ( isCanDrag )
            {
                //circleImage.transform.position = v;
                float x = vv.x - v.x;
                float y = vv.y - v.y;
                float z = vv.z - v.z;
                if ( ( x * x + y * y + z * z ) >= dragDistance )
                {
                    //-- todo
                }
            }
        }

        public void GetTargetImage()
        {
            ShowGuidePanel();
            StartCoroutine( DelayGetTarget() );
        }

        private void SetTargetPos( Vector3 pos )
        {
            targetImage.transform.position = pos;
        }

        private IEnumerator DelayGetShowImageTarget()
        {
            yield return new WaitForSeconds( 0.5f );
            GameObject targetUI = GameObject.Find( controler.GetUIPrefabName() );
            targetObj = targetUI.transform.Find( controler.GetUIPath() ).gameObject;

            Vector2 sizeDelta = targetObj.GetComponent<RectTransform>().sizeDelta;
            showImage.rectTransform.sizeDelta = sizeDelta;

            showImage.transform.position = targetObj.transform.position;// camera2D.ScreenToWorldPoint( scr );
            showImage.transform.position = new Vector3( showImage.transform.position.x, showImage.transform.position.y, 10 );
        }

        private IEnumerator DelayGetTarget()
        {
            yield return new WaitForSeconds( 0.5f );
            GameObject targetUI = GameObject.Find( controler.GetUIPrefabName() );
            targetObj = targetUI.transform.Find( controler.GetUIPath() ).gameObject;

            Vector2 sizeDelta = targetObj.GetComponent<RectTransform>().sizeDelta;
            showImage.rectTransform.sizeDelta = sizeDelta;

            float circleSize = sizeDelta.x > sizeDelta.y ? sizeDelta.y : sizeDelta.x;
            circleImage.GetComponent<RectTransform>().sizeDelta = new Vector2( circleSize, circleSize );
            //canvas = transform.root.gameObject.GetComponentInChildren<Canvas>();
            //Camera camera2D = GameObject.Find( "Camera" ).GetComponent<Camera>();//--UIcamera
            //Vector3 scr = RectTransformUtility.WorldToScreenPoint( camera2D , targetObj.transform.position );
            //scr.z = 0;
            //scr.z = Mathf.Abs( camera2D.transform.position.z - transform.position.z );
            showImage.transform.position = targetObj.transform.position;// camera2D.ScreenToWorldPoint( scr );
            showImage.transform.position = new Vector3( showImage.transform.position.x, showImage.transform.position.y, 10 );
            circleImage.transform.position = showImage.transform.position;
            arrowImage.transform.position = showImage.transform.position + arrowOffset;
        }

        private IEnumerator DelayGetTarget3D( Vector3 v, bool isDarg, Vector2 showSize )
        {
            yield return new WaitForSeconds( 0.5f );
            if ( Camera.main )
            {
                showImage.transform.position = Camera.main.WorldToScreenPoint( v );
                showImage.transform.position = new Vector3( showImage.transform.position.x, showImage.transform.position.y, 10 );
                showImage.rectTransform.sizeDelta = showSize;
                circleImage.transform.position = showImage.transform.position;
                circleImage.GetComponent<RectTransform>().sizeDelta = new Vector2( 50, 50 );
                arrowImage.transform.position = showImage.transform.position + arrowOffset;

                if ( isDarg )
                    loopTween = arrowImage.transform.DOMove( Camera.main.WorldToScreenPoint( controler.GetBoxPostion( 0 ) ) + arrowOffset, 2f ).SetLoops( -1 );
            }
        }
    }
}
