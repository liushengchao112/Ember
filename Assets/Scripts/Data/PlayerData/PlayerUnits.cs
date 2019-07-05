using System.Collections;
using System.Collections.Generic;

using Network;
using Utils;

namespace Data
{
    public class PlayerUnits
    {
        public List<SoldierInfo> soldiers;

        public PlayerUnits()
        {
            soldiers = new List<SoldierInfo>();
        }

        public void RegisterPlayerUnitsServerMessageHandler()
        {
            NetworkManager.RegisterServerMessageHandler( MsgCode.GainMessage, HandleGainSoldierFeedback );
            //NetworkManager.RegisterServerMessageHandler( MsgCode.RefreshSoldierMessage, HandleRefreshSoldierFeedback );
        }

        public void RegisterPlayerUnitsSocialServerMessageHandler()
        {
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.GainMessage, HandleGainSoldierFeedback );
        }

        private void HandleGainSoldierFeedback( byte[] data )
        {
            GainS2C feedback = ProtobufUtils.Deserialize<GainS2C>( data );

            if ( feedback == null )
            {
                DebugUtils.LogError( DebugUtils.Type.UI, "GainSoldier~~~~Feedback is null" );
                return;
            }

            if ( feedback.result )
            {
                PlayerUnits army = DataManager.GetInstance().GetPlayerUnits();
                for ( int i = 0; i < feedback.soldiers.Count; i++ )
                {
                    int id = feedback.soldiers[i].metaId;
                    SoldierInfo info = army.soldiers.Find( p => p.metaId == id );

                    if ( info == null )
                        army.soldiers.Add( feedback.soldiers[i] );
                    else
                        army.soldiers.Find( p => p.metaId == info.metaId ).count += feedback.soldiers[i].count;
                }

                MessageDispatcher.PostMessage( Constants.MessageType.RefreshPlayerUnitsData );
            }
        }

        private void HandleRefreshSoldierFeedback( byte[] data )
        {

            //RefreshSoldierS2C message = ProtobufUtils.Deserialize<RefreshSoldierS2C>( data );
            //DebugUtils.Log( DebugUtils.Type.Data, " Refresh soldier result:" + message.result );

            //if ( message.result )
            //{

            //    PlayerUnits army = DataManager.GetInstance().GetPlayerUnits();
                
            //    for ( int i = 0; i < army.soldiers.Count; i++ )
            //    {
            //        if ( army.soldiers[i].metaId == message.soldier.metaId )
            //        {
            //            army.soldiers[i] = message.soldier;
            //        }
            //    }

            //    MessageDispatcher.PostMessage( Constants.MessageType.RefeshSoldier, message.soldier );
            //}
        }
    }
}
