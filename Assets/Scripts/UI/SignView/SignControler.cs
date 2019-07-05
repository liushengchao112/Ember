using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using SignInfo = Data.DailyLoginS2C.SignInfo;
using Network;
using Utils;

namespace UI
{
    public class SignControler : ControllerBase
    {
        private SignView signView;

        public SignControler( SignView v )
        {
            viewBase = v;
            signView = v;
            MessageDispatcher.AddObserver( ViewRefresh , Constants.MessageType.RefreshSignView );
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            MessageDispatcher.RemoveObserver( ViewRefresh , Constants.MessageType.RefreshSignView );
        }

        private void ViewRefresh( object isSign )
        {
            if ( (bool)isSign )
            {
                signView.SignHandler();                
            }
            signView.RefreshItemUI();
        }

        public List<SignInfo> GetSignInfo()
        {
            return DataManager.GetInstance().GetSignInfo();
        }

        public bool GetIsSignToday()
        {
            return DataManager.GetInstance().GetIsSignToday();
        }

        public void SendDailySignResquest( bool isSign )
        {
            DailyLoginC2S cs = new DailyLoginC2S();
            cs.isSign = isSign;
            byte[] data = ProtobufUtils.Serialize( cs );
            NetworkManager.SendRequest( ServerType.GameServer , MsgCode.DailyLoginMessage , data );
        }
    }
}
