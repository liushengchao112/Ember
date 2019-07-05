using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Network;
using Data;
using Utils;

namespace UI
{
    public class FriendController
    {
        private FriendView view;

        //FriendList Panel item data.
        private List<FriendListDataStruct> friendListPlayerDataList = new List<FriendListDataStruct>();

        //FindFriend Panel item data.
        private List<FriendListDataStruct> systemRecommendedPlayerDataList = new List<FriendListDataStruct>();

        //ApplicationInformation Panel item data;
        private List<FriendListDataStruct> applicationPlayerDataList = new List<FriendListDataStruct>();

        //BlackList Panel item data;
        private List<FriendListDataStruct> blackListPlayerDataList = new List<FriendListDataStruct>();

        public enum PlayerListType
        {
            Friend = 1,
            Black = 2,
            Appliation = 3
        }

        private PlayerListType currentListType;

        public FriendController(FriendView view)
        {
            this.view = view;
        }

        #region MsgCode

        public void RegisterServerMessage()
        {
            NetworkManager.RegisterServerMessageHandler(ServerType.SocialServer, MsgCode.RelationApplicationMessage, HandleRelationApplicationFeedback);
            NetworkManager.RegisterServerMessageHandler(ServerType.SocialServer, MsgCode.RelationListMessage, HandleRelationListFeedback);
            NetworkManager.RegisterServerMessageHandler(ServerType.SocialServer, MsgCode.RelationSearchMessage, HandleSearchPlayerFeedback);
            NetworkManager.RegisterServerMessageHandler(ServerType.SocialServer, MsgCode.RelationRecommendMessage, HandleRecommendPlayerFeedback);
            NetworkManager.RegisterServerMessageHandler(ServerType.SocialServer, MsgCode.GIFT_CURRENCIES, HandleGiftCurrenciesFeedback);
            NetworkManager.RegisterServerMessageHandler(ServerType.SocialServer, MsgCode.RelationScreenedMessage, HandleRelationScreenedFeedback);

            MessageDispatcher.AddObserver(RefreshRedBubble, Constants.MessageType.RefreshSocialServerRedBubble);
        }

        public void RemoveServerMessage()
        {
            NetworkManager.RemoveServerMessageHandler(ServerType.SocialServer, MsgCode.RelationApplicationMessage, HandleRelationApplicationFeedback);
            NetworkManager.RemoveServerMessageHandler(ServerType.SocialServer, MsgCode.RelationListMessage, HandleRelationListFeedback);
            NetworkManager.RemoveServerMessageHandler(ServerType.SocialServer, MsgCode.RelationSearchMessage, HandleSearchPlayerFeedback);
            NetworkManager.RemoveServerMessageHandler(ServerType.SocialServer, MsgCode.RelationRecommendMessage, HandleRecommendPlayerFeedback);
            NetworkManager.RemoveServerMessageHandler(ServerType.SocialServer, MsgCode.GIFT_CURRENCIES, HandleGiftCurrenciesFeedback);
            NetworkManager.RemoveServerMessageHandler(ServerType.SocialServer, MsgCode.RelationScreenedMessage, HandleRelationScreenedFeedback);

            MessageDispatcher.RemoveObserver(RefreshRedBubble, Constants.MessageType.RefreshSocialServerRedBubble);
        }

        #endregion

        #region Send

        public void SendRelationApplicationMessage(RelationApplicationType type, long friendId)
        {
            RelationApplicationC2S message = new RelationApplicationC2S();
            message.friendId = friendId;
            message.applciationType = type;
            byte[] stream = ProtobufUtils.Serialize(message);
            NetworkManager.SendRequest(ServerType.SocialServer, MsgCode.RelationApplicationMessage, stream);
        }

        public void SendRelationList(PlayerListType listType)
        {
            currentListType = listType;
            RelationListC2S message = new RelationListC2S();
            message.listType = (int)listType;
            byte[] stream = ProtobufUtils.Serialize(message);
            NetworkManager.SendRequest(ServerType.SocialServer, MsgCode.RelationListMessage, stream);
        }

        public void SendSearchPlayer(string playerName)
        {
            SearchPlayerC2S message = new SearchPlayerC2S();
            message.playerName = playerName;
            byte[] stream = ProtobufUtils.Serialize(message);
            NetworkManager.SendRequest(ServerType.SocialServer, MsgCode.RelationSearchMessage, stream);
        }

        public void SendRecommendPlayer()
        {
            RecommendPlayerC2S message = new RecommendPlayerC2S();
            byte[] stream = ProtobufUtils.Serialize(message);
            NetworkManager.SendRequest(ServerType.SocialServer, MsgCode.RelationRecommendMessage, stream);
        }


        private long currentSendFriendId = -1;
        public void SendGiftCurrencies(long playerId)
        {
            GiftCurrenciesC2S message = new GiftCurrenciesC2S();
            message.playerId = playerId;
            currentSendFriendId = playerId;
            byte[] stream = ProtobufUtils.Serialize(message);
            NetworkManager.SendRequest(ServerType.SocialServer, MsgCode.GIFT_CURRENCIES, stream);
        }

        public void SendRelationScreenedMessage(bool isScreened)
        {
            RelationScreenedC2S message = new RelationScreenedC2S();
            message.isScreened = isScreened;
            byte[] stream = ProtobufUtils.Serialize(message);
            NetworkManager.SendRequest(ServerType.SocialServer, MsgCode.RelationScreenedMessage, stream);
        }

        #endregion

        #region Handle

        private void HandleRelationApplicationFeedback(byte[] data)
        {
            RelationApplicationS2C feedback = ProtobufUtils.Deserialize<RelationApplicationS2C>(data);
            if (feedback.result)
            {
                switch (feedback.applciationType)
                {
                    case RelationApplicationType.AcceptRelation:
                        FriendListDataStruct acceptRelationData = applicationPlayerDataList.Find(p => p.playerUID == feedback.friendId);
                        applicationPlayerDataList.Remove(acceptRelationData);
                        friendListPlayerDataList.Add(acceptRelationData);
                        view.RefreshApplicationInfomationPlayerList();
                        DataManager.GetInstance().SetRedBubbleNum(CaptionType.RelationApplicationCaption, DataManager.GetInstance().GetRedBubbleNum(CaptionType.EmailCaption) - 1);
                        view.RefresRedBubble();
                        
                        break;

                    case RelationApplicationType.BlockList:
                        FriendListDataStruct blockData = friendListPlayerDataList.Find(p => p.playerUID == feedback.friendId);
                        friendListPlayerDataList.Remove(blockData);
                        view.RefreshFriendList();
                        break;

                    case RelationApplicationType.Unblock:
                        FriendListDataStruct unblockData = blackListPlayerDataList.Find(p => p.playerUID == feedback.friendId);
                        blackListPlayerDataList.Remove(unblockData);
                        view.RefreshBlackList();
                        break;

                    case RelationApplicationType.RefuseRelation:
                        FriendListDataStruct refuseRelationData = applicationPlayerDataList.Find(p => p.playerUID == feedback.friendId);
                        applicationPlayerDataList.Remove(refuseRelationData);
                        view.RefreshApplicationInfomationPlayerList();
                        DataManager.GetInstance().SetRedBubbleNum(CaptionType.RelationApplicationCaption, DataManager.GetInstance().GetRedBubbleNum(CaptionType.EmailCaption) - 1);
                        view.RefresRedBubble();
                        break;

                    case RelationApplicationType.RemoveRelation:
                        FriendListDataStruct removeRelationData = friendListPlayerDataList.Find(p => p.playerUID == feedback.friendId);
                        friendListPlayerDataList.Remove(removeRelationData);
                        view.RefreshFriendList();
                        break;
                }
            }
        }

        private List<FriendListDataStruct> GetFriendList(List<FriendInfo> list)
        {
            List<FriendListDataStruct> dataList = new List<FriendListDataStruct>();
            for (int i = 0; i < list.Count; i++)
            {
                FriendListDataStruct friendData = new FriendListDataStruct();
                friendData.canGiveGold = list[i].canGiveGold;
                friendData.playerUID = list[i].friendId;
                friendData.playerName = list[i].name;
                friendData.portrait = list[i].portrait;
                friendData.grade = list[i].grade;
                friendData.playerLevel = list[i].level;
                friendData.vipLevel = list[i].vipLevel;
                friendData.isPlayerJionedGuild = list[i].isOnline;
                friendData.lastLoginTime = list[i].lastLoginTime;
                friendData.playerNetWorkStatus = GetPlayerNetworkStatus(list[i].lastLoginTime, list[i].isOnline);
                dataList.Add(friendData);
            }
            return dataList;

        }
        private void HandleRelationListFeedback(byte[] data)
        {
            RelationListS2C feedback = ProtobufUtils.Deserialize<RelationListS2C>(data);
            if (feedback.result)
            {
                List<FriendListDataStruct> dataList = GetFriendList(feedback.friends);
                dataList.Sort(FriendInfoSort);
                switch (currentListType)
                {
                    case PlayerListType.Friend:
                        friendListPlayerDataList = dataList;
                        view.RefreshFriendList();
                        break;

                    case PlayerListType.Black:
                        blackListPlayerDataList = dataList;
                        view.RefreshBlackList();
                        break;

                    case PlayerListType.Appliation:
                        applicationPlayerDataList = dataList;
                        view.RefresRedBubble();
                        view.RefreshRedrefuseAllApplicationToggle(feedback.isRefuseApply);
                        if (view.screenStatus == UI.FriendView.FriendScreenStatus.ShowApplicationInfomation)
                        {
                            view.RefreshApplicationInfomationPlayerList();
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void HandleRecommendPlayerFeedback(byte[] data)
        {
            RecommendPlayerS2C feedback = ProtobufUtils.Deserialize<RecommendPlayerS2C>(data);
            if (feedback.result)
            {
                systemRecommendedPlayerDataList.Clear();
                List<FriendInfo> list = feedback.friends;
                for (int i = 0; i < list.Count; i++)
                {
                    FriendListDataStruct friendData = new FriendListDataStruct();
                    friendData.playerUID = list[i].friendId;
                    friendData.playerName = list[i].name;
                    friendData.portrait = list[i].portrait;
                    friendData.grade = list[i].grade;
                    friendData.playerLevel = list[i].level;
                    friendData.vipLevel = list[i].vipLevel;
                    friendData.isOnline = list[i].isOnline;
                    friendData.lastLoginTime = list[i].lastLoginTime;
                    friendData.alreadyApplication = list[i].alreadyApplication;
                    systemRecommendedPlayerDataList.Add(friendData);
                }
                view.RefreshSystemRecommendedFriendList();
            }
        }

        private static int FriendInfoSort(FriendListDataStruct x, FriendListDataStruct y)
        {
            if (y.isOnline.CompareTo(x.isOnline) != 0)
            {
                return y.isOnline.CompareTo(x.isOnline);
            }
            else if (y.grade.CompareTo(x.grade) != 0)
            {
                return y.grade.CompareTo(x.grade);
            }
            else if (y.playerLevel.CompareTo(x.playerLevel) != 0)
            {
                return y.playerLevel.CompareTo(x.playerLevel);
            }
            else if (y.playerUID.CompareTo(x.playerUID) != 0)
            {
                return x.playerUID.CompareTo(y.playerUID);
            }

            return 1;
        }

        private void HandleSearchPlayerFeedback(byte[] data)
        {
            SearchPlayerS2C feedback = ProtobufUtils.Deserialize<SearchPlayerS2C>(data);
            if (feedback.result)
            {
                systemRecommendedPlayerDataList.Clear();
                systemRecommendedPlayerDataList = GetFriendList(feedback.friends);
                view.RefreshSystemRecommendedFriendList();
            }
        }


        private void HandleGiftCurrenciesFeedback(byte[] data)
        {
            GiftCurrenciesS2C feedback = ProtobufUtils.Deserialize<GiftCurrenciesS2C>(data);
            if (feedback.result)
            {
                FriendListDataStruct friend = friendListPlayerDataList.Find(t => t.playerUID == currentSendFriendId);
                int index= friendListPlayerDataList.IndexOf(friend);
                friend.canGiveGold = false;
                friendListPlayerDataList[index]= friend;             
                view.RefreshFriendList();
                MessageDispatcher.PostMessage(Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, "金币赠送成功", "提示");
            }
        }

        /*
         * 不接受好友申请
         * TODO：??
         *       此处代码需要注意是否可删除
         * 
         */
        private void HandleRelationScreenedFeedback(byte[] data)
        {
            RelationScreenedS2C feedback = ProtobufUtils.Deserialize<RelationScreenedS2C>(data);
            if (feedback.result)
            {

            }
        }

        #endregion

        #region FriendListFunctions

        public List<FriendListDataStruct> GetFriendListPlayersData()
        {
            return friendListPlayerDataList;
        }

        #endregion

        #region FindFriendFunctions

        public List<FriendListDataStruct> GetSystemRecommendedPlayersData()
        {
            return systemRecommendedPlayerDataList;
        }

        #endregion

        #region ApplicationInformationFunctions

        public List<FriendListDataStruct> GetApplicationPlayersData()
        {
            return applicationPlayerDataList;
        }

        #endregion

        #region BlackListFunctions

        public List<FriendListDataStruct> GetBlackListPlayersData()
        {
            return blackListPlayerDataList;
        }

        #endregion

        #region GetPlayerNetWorkStatusFunctions

        private string GetPlayerNetworkStatus(long time, bool isOnline)
        {
            if (isOnline)
            {
                return "<color=#00FF00>在线</color>";
            }

            string timeStr ;
            long timeInterva = (GetTimeStamp() - time) / 1000;

            if (timeInterva / (3600*24)!=0)
            {
                timeStr = timeInterva / (3600 * 24)+"天前";
            }
            else if (timeInterva/3600!=0)
            {
                timeStr = timeInterva / 3600+"小时前";
            }
            else 
            {
                timeStr = timeInterva / 600+"分前";
            }         

            return timeStr;
        }

        public static long GetTimeStamp(bool bflag = false)
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long ret;
            if (bflag)
            {
                ret = Convert.ToInt64(ts.TotalSeconds);
            }
            else
            {
                ret = Convert.ToInt64(ts.TotalMilliseconds);
            }
            return ret;
        }

        #endregion
        public static DateTime ConvertTimeStampToDateTime(long timeStamp)
        {
            DateTime defaultTime = new DateTime(1970, 1, 1, 0, 0, 0);
            long defaultTick = defaultTime.Ticks;
            long timeTick = defaultTick + timeStamp * 10000;
            //// 东八区 要加上8个小时
            DateTime dt = new DateTime(timeTick).AddHours(8);
            return dt;

        }

        public bool GetSocialRedBubbleState()
        {
            int relationApplicationCaptionNum = Data.DataManager.GetInstance().GetRedBubbleNum(CaptionType.RelationApplicationCaption);

            return relationApplicationCaptionNum > 0;
        }

        #region Observer

        private void RefreshRedBubble()
        {
            if (GetSocialRedBubbleState())
            {
                SendRelationList(PlayerListType.Appliation);
            }
        }

        #endregion

    }

    #region struct datas

    public struct FriendListDataStruct
    {
        public long playerUID;
        public string playerName;
        public string portrait;
        public int grade;
        public int playerLevel;
        public int vipLevel;
        public bool isOnline;
        public long lastLoginTime;
        public string playerNetWorkStatus;
        public bool canGiveGold;
        public string NetworkStatus;
        public bool createTime;
        public bool alreadyApplication;
        public bool isPlayerJionedGuild;
    }

    #endregion

}