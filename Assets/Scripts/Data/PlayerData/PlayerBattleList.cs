using UnityEngine;
using System.Collections.Generic;

using Network;
using Utils;

namespace Data
{
    public class PlayerBattleList
    {
        public List<ArmyInfo> ArmyList;

        public PlayerBattleList()
        {
            ArmyList = new List<ArmyInfo>();
        }

        public void RegisterPlayerBattleListServerMessageHandler()
        {
            NetworkManager.RegisterServerMessageHandler( MsgCode.RefreshBattleListMessage, HandleRefreshBattleListFeedback );
        }

        private void HandleRefreshBattleListFeedback( byte[] data )
        {
            RefreshArmyS2C feedback = ProtobufUtils.Deserialize<RefreshArmyS2C>( data );

            if ( feedback.result )
            {
                DataManager.GetInstance().SetBattleArmyList( feedback.armyInfos );

                MessageDispatcher.PostMessage( Constants.MessageType.RefreshPlayerBattleListData );
            }
        }
    }
}
