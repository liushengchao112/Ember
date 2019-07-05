using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Utils;
using Network;

namespace UI
{
    public class BattleCommandControler : ControllerBase
    {
        private BattleCommandControlView view;
        public BattleCommandControler( BattleCommandControlView v )
        {
            viewBase = v;
            view = v;
        }

        public void SendOrderNotice( OrderType orderType )
        {
            NoticeC2S message = new NoticeC2S();

            message.type = NoticeType.NoticeOperation;
            message.noticeOperation = (int)orderType;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.NoticeMessage , data );
        }

        public void SelectAllUnit()
        {
            MessageDispatcher.PostMessage( Constants.MessageType.SelectAll );
        }

        public void SendGreeting( GreetingType type )
        {

        }
    }
}
