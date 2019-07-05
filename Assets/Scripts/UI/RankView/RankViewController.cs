using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Utils;
using Network;

using RankMemberInfo = Data.RankS2C.RankMemberInfo;

namespace UI
{
    public class RankViewController : ControllerBase
    {
        private RankView view;
        private RankType currentType = RankType.GradeRank;
        public RankType CurrentType
        {
            get
            {
                return currentType;
            }
            set
            {
                if ( currentType != value )
                {
                    currentType = value;
                    ClearMembersInfo();
                }
            }
        }

        private int pageId = 0;
        private bool isFriendRank = false;
        private List<RankMemberInfo> members;
        private RankMemberInfo myRankInfo;

        public RankType GetCurrentRankType()
        {
            return currentType;
        }

        public int GetPageId()
        {
            return pageId;
        }

        public bool GetIsFriendRank()
        {
            return isFriendRank;
        }

        public List<RankMemberInfo> GetMembersInfo()
        {
            return members;
        }

        public void ClearMembersInfo()
        {
            members.Clear();
        }

        public RankMemberInfo GetMyRankInfo()
        {
            return myRankInfo;
        }

        public RankViewController( RankView v )
        {
            view = v;
            viewBase = v;
            members = new List<RankMemberInfo>();

            NetworkManager.RegisterServerMessageHandler( ServerType.GameServer , MsgCode.RankMessage , RankRequestFeedBack );
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            NetworkManager.RemoveServerMessageHandler( ServerType.GameServer , MsgCode.RankMessage , RankRequestFeedBack );
        }

        private void RankRequestFeedBack( byte[] data )
        {
            RankS2C feedback = ProtobufUtils.Deserialize<RankS2C>( data );

            if ( feedback.result )
            {
                CurrentType = feedback.rankType;
                pageId = feedback.pageId;
                isFriendRank = feedback.isFriendRank;

                for ( int i = 0; i < feedback.members.Count; i++ )
                {
                    members.Add( feedback.members[i] );
                }

                myRankInfo = feedback.selfInfo;

                if ( feedback.members.Count < 10 )
                {
                    view.isCanRequest = false;
                }
                else
                {
                    view.isCanRequest = true;
                }

                //if ( feedback.members.Count > 0 )
                {
                    view.ReFreshUI();
                }
            }
        }

        public void SendRankRequest( RankType type , int pageId , bool isFriendRank )
        {
            RankC2S message = new RankC2S();
            message.rankType = type;
            message.pageId = pageId;
            message.isFriendRank = isFriendRank;

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.RankMessage , data );

            view.StartRequestCD();
        }
    }
}
