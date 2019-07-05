using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Resource;
using Data;

namespace UI
{
    public enum KillDesType
    {
        None = 0,
        Kill = 1,
    }

    public class NoticeView : MonoBehaviour
    {

        private Transform noticeRoot;
        private Image killerHead;
        //private Image textBgImageBlue;
        //private Image textBgImageRed;
        private Text desText;
        private Image beKillerHead;
        private Image killImage;
        private Image killerHeadFrameBgBlue;
        private Image killerHeadFrameBgRed;
        private Image beKillerHeadFrameBgRed;
        private Image beKillerHeadFrameBgBlue;

        private List<UnitsProto.Unit> unitsProto;
        private MatchSide selfSide;
        private MatchSide enemySide;

        private bool isShowingNotice = false;
        private float showingTime = 0;

        public void Init()
        {
            noticeRoot = transform.Find( "Ani/NoticeRoot" ).transform;
            killerHead = noticeRoot.Find( "KillerHeadBg/KillerHead" ).GetComponent<Image>();
            //textBgImageBlue = noticeRoot.Find( "TextBgBlue" ).GetComponent<Image>();
            //textBgImageRed = noticeRoot.Find( "TextBgRed" ).GetComponent<Image>();
            desText = noticeRoot.Find( "DesText" ).GetComponent<Text>();
            beKillerHead = noticeRoot.Find( "BeKillerHeadBg/BeKillerHead" ).GetComponent<Image>();
            killImage = noticeRoot.Find( "BeKillerHeadBg/KillImage" ).GetComponent<Image>();
            killerHeadFrameBgBlue = noticeRoot.Find( "KillerHeadFrameBgBlue" ).GetComponent<Image>();
            killerHeadFrameBgRed = noticeRoot.Find( "KillerHeadFrameBgRed" ).GetComponent<Image>();
            beKillerHeadFrameBgRed = noticeRoot.Find( "BeKillerHeadFrameBgRed" ).GetComponent<Image>();
            beKillerHeadFrameBgBlue = noticeRoot.Find( "BeKillerHeadFrameBgBlue" ).GetComponent<Image>();

            DataManager client = DataManager.GetInstance();
            unitsProto = client.unitsProtoData;
            selfSide = client.GetMatchSide();
            SetEnemySide( selfSide );

            desText.text = "";
            noticeRoot.gameObject.SetActive( false );
        }


        public void ShowKillIdolNotice( ForceMark mark , int iconId )
        {
            // TODO:
        }

        public void ShowKillUnitNotice( ForceMark killerMark , int killerIcon , ForceMark beKillerMark , int beKillerIcon )
        {
            // TODO: Temp Code
            if ( killerIcon == 0 || beKillerIcon == 0 )
            {
                return;
            }
            if ( killerIcon == -1 || beKillerIcon  == -1 )
            {
                return;
            }

            ShowKillUnitNotice( KillDesType.Kill , killerIcon , beKillerIcon , GetIsSameSide( killerMark , beKillerMark ) );
        }

        private void ShowKillUnitNotice( KillDesType type ,int killerIconId , int beKillerIconId ,bool isSameSide)
        {
            SetBgColor( isSameSide );
            desText.text = GetStrByType( type );
            SetIconImage( killerHead , killerIconId );
            SetIconImage( beKillerHead , beKillerIconId );
            noticeRoot.gameObject.SetActive( true );
            isShowingNotice = true;
            showingTime = 0;
        }

        private bool GetIsSameSide( ForceMark killerMark , ForceMark beKillerMark )
        {
            if ( GetSideFromMark(killerMark) == selfSide && GetSideFromMark(beKillerMark) == enemySide )
            {
                return true;
            }
            else if ( GetSideFromMark( beKillerMark ) == selfSide && GetSideFromMark( killerMark ) == enemySide )
            {
                return false;
            }
            return false;
        }

        public MatchSide GetSideFromMark( ForceMark mark )
        {
            if ( mark <= ForceMark.BottomRedForce )
            {
                return MatchSide.Red;
            }
            else if ( mark <= ForceMark.BottomBlueForce )
            {
                return MatchSide.Blue;
            }
            else
            {
                return MatchSide.NoSide;
            }
        }

        private void SetBgColor(bool isSameSide)
        {
            killerHeadFrameBgBlue.gameObject.SetActive( isSameSide );
            killerHeadFrameBgRed.gameObject.SetActive( !isSameSide );
            beKillerHeadFrameBgRed.gameObject.SetActive( isSameSide );
            beKillerHeadFrameBgBlue.gameObject.SetActive( !isSameSide );
            //textBgImageBlue.gameObject.SetActive( isSameSide );
            //textBgImageRed.gameObject.SetActive( !isSameSide );
        }

        private void SetIconImage( Image image , int iconId )
        {
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( iconId, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
            {
                image.SetSprite( atlasSprite );
            }, true );
        }

        private string GetStrByType( KillDesType type)
        {
            switch ( type )
            {
                case KillDesType.None:
                {
                    return "";
                }
                case KillDesType.Kill:
                {
                    return "击杀";
                }
                default:
                {
                    return "击杀";
                }                
            }
        }

        void Update()
        {
            if ( isShowingNotice )
            {
                showingTime += Time.deltaTime;
                if ( showingTime >= 1.5f )
                {
                    noticeRoot.gameObject.SetActive( false );
                    isShowingNotice = false;
                    showingTime = 0;
                }
            }
        }

        private void SetEnemySide( MatchSide side )
        {
            switch ( side )
            {
                case MatchSide.NoSide:
                break;
                case MatchSide.Observe:
                break;
                case MatchSide.Red:
                enemySide = MatchSide.Blue;
                break;
                case MatchSide.Blue:
                enemySide = MatchSide.Red;
                break;
                default:
                break;
            }
        }

    }
}

