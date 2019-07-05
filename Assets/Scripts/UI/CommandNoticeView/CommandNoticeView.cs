using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

using Data;
using Resource;
using Utils;
using Network;

namespace UI
{

    public enum OrderType
    {
        Attack = 1,        
        Defense = 2,
        Retreat = 3,
    }

    public class CommandNoticeView : MonoBehaviour
    {
        //private const string PLAYER_ICON_PATH = "UITexture/Avatar_icon/";

        private Transform noticeRoot;
        private Image orderHead;
        private Text desText;

        private bool isShowingNotice = false;
        private float showingTime = 0;

        private MatchSide selfSide;

        void OnDestroy()
        {
            NetworkManager.RemoveServerMessageHandler( ServerType.BattleServer, MsgCode.NoticeMessage, OnNoticeRespond );
        }

        private void OnNoticeRespond( byte[] data )
        {
            NoticeS2C msg = ProtobufUtils.Deserialize<NoticeS2C>( data );

            if ( msg.type == NoticeType.NoticeOperation )
            {
                ShowOrderNotice( DataManager.GetInstance().GetPlayerHeadIcon() , (OrderType)msg.noticeOperation );
            }
        }

        public void OnInit()
        {
            NetworkManager.RegisterServerMessageHandler( ServerType.BattleServer, MsgCode.NoticeMessage , OnNoticeRespond );

            noticeRoot = transform.Find( "Ani/NoticeRoot" ).transform;
            orderHead = noticeRoot.Find( "OrderHeadBg/OrderHead" ).GetComponent<Image>();
            desText = noticeRoot.Find( "DesText" ).GetComponent<Text>();

            DataManager client = DataManager.GetInstance();
            selfSide = client.GetMatchSide();

            desText.text = "";
            noticeRoot.gameObject.SetActive( false );
        }

        public void HideOrderNotice()
        {
            noticeRoot.gameObject.SetActive( false );
        }

        private void ShowRootObj()
        {
            noticeRoot.gameObject.SetActive( true );
        }

        public void ShowOrderNotice( string orderIcon , OrderType orderType )
        {
            if ( string.IsNullOrEmpty( orderIcon ) )
            {
                return;
            }
            desText.text = GetStrByType( orderType );
            RefreshPlayerHeadIcon( orderIcon );
            isShowingNotice = true;
            showingTime = 0;
            ShowRootObj();
        }

        public void RefreshPlayerHeadIcon( string orderIconStr )
        {
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( orderIconStr , delegate ( string name , AtlasSprite atlasSprite , System.Object param )
            {
                orderHead.SetSprite( atlasSprite );
            } , true );
        }

        private bool GetIsSameSide( ForceMark orderForceMark )
        {
            if ( GetSideFromMark( orderForceMark ) == selfSide )
            {
                return true;
            }
            return false;
        }

        private MatchSide GetSideFromMark( ForceMark mark )
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

        private string GetStrByType( OrderType type )
        {
            string str = "";
            switch ( type )
            {
                case OrderType.Retreat:
                {
                    str = "撤退";
                }
                break;
                case OrderType.Defense:
                {
                    str = "回防";
                }
                break;
                case OrderType.Attack:
                {
                    str = "进攻";
                }
                break;
                default:
                {
                    str = "";
                }
                break;
            }
            return str;
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


    }
}
