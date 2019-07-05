using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Utils;

namespace UI
{
    public class FriendView : MonoBehaviour
    {
        public enum FriendScreenStatus
        {
            ShowFriendList,
            ShowFindFriend,
            ShowApplicationInfomation,
            ShowBlaickList,
            ShowWeiChatFriendList
        }

        Resource.GameResourceLoadManager resourceLoadManager;

        #region FriendPanel values

        public FriendScreenStatus screenStatus = FriendScreenStatus.ShowFriendList;

        #endregion

        #region Component

        private FriendController friendController;

        private Toggle friendsToggle;
        private Toggle findFrindsToggle;
        private Toggle applicationInformationToggle;
        private Toggle blackListToggle;
        private Toggle weiChatFriendsToggle;

        private GameObject friendListPanel;
        private GameObject findFriendPanel;
        private GameObject applicationInformationPanel;
        private GameObject blackListPanel;
        private GameObject weiChatFriendListPanel;

        //Friend panel

        private ScrollRect dragFriendPanel;
        private FindFriendListScrollView findfriendListView;
        private FriendListScrollView friendListScrollView;

        private bool isLoadFriendItemFinish = false;

        private Text notHaveFriendNoticeText;

        //FindFriend panel

        private Button searchFriendButton;
        private InputField searchFriendInput;
        private Text findFriendNoticeTitleText;
        private Text findFriendNoticeText;
        private ScrollRect dragSystemRecommendedFriendsPanel;
        private List<FindFriendItem> findFriendPanelItemList;

        private string findingPlayerStr = "未搜索到玩家";
        private string findPlayerInputStr = "请输入用户名";
        private string systemRecommendedFriendsStr = "系统推荐:";
        private string findFriendResult = "查找结果";
        private string recommendFridens = "推荐好友";

        //ApplicationInformation panel

        private ScrollRect dragApplicationInformationPanel;
        private ApplicationInformationScrollView applicationInformationScrollView;
        private GameObject applicationInformationRedBubble;
        private Toggle refuseAllApplicationToggle;
        private Image friendToggleRedBubble;

        //BlackList panel

        private ScrollRect dragBlackListPanel;
        private BlackListScrollView blackListScrollView;

        //WeiChat panel

        private ScrollRect dragWeiChatFriendListPanel;
        private WeiChatFriendListScrollView weiChatFriendScrollView;

        private Text notHaveWeiChatFriendNoticeText;

        #endregion

        #region FriendScreen Initialize

        public void Init()
        {
            friendController = new FriendController(this);
            //FriendListPanel
            dragFriendPanel = transform.Find("FriendListPanel/DragFriendsPanel").GetComponent<ScrollRect>();
            friendListScrollView = dragFriendPanel.GetComponent<FriendListScrollView>();
            friendListScrollView.onCreateItemHandler = CreateFriendListItem;

            friendsToggle = transform.Find("FriendToggleListBG/GameFriends").GetComponent<Toggle>();
            friendListPanel = transform.Find("FriendListPanel").gameObject;
            friendsToggle.onValueChanged.AddListener(GameFriendClicked);

            notHaveFriendNoticeText = transform.Find("FriendListPanel/NoticeText").GetComponent<Text>();

            //FindFriendPanel
            dragSystemRecommendedFriendsPanel = transform.Find("FindFriendPanel/DragFriendPanel").GetComponent<ScrollRect>();
            findfriendListView = dragSystemRecommendedFriendsPanel.GetComponent<FindFriendListScrollView>();
            findfriendListView.onCreateItemHandler = CreateFindFriendListItem;


            findFrindsToggle = transform.Find("FriendToggleListBG/FindFriend").GetComponent<Toggle>();
            findFriendPanel = transform.Find("FindFriendPanel").gameObject;
            findFrindsToggle.onValueChanged.AddListener(FindFriendClicked);

            searchFriendButton = transform.Find("FindFriendPanel/SearchButton").GetComponent<Button>();
            searchFriendInput = transform.Find("FindFriendPanel/InputField").GetComponent<InputField>();
            searchFriendButton.onClick.AddListener(SearchFriendButtonClicked);

            findFriendNoticeTitleText = transform.Find("FindFriendPanel/FindFriendNoticeTitleText").GetComponent<Text>();
            findFriendNoticeText = transform.Find("FindFriendPanel/FindFriendNoticeText").GetComponent<Text>();

            findFriendPanelItemList = new List<FindFriendItem>();
            Transform itemGroup = transform.Find("FindFriendPanel/DragFriendPanel/ViewPoint/ItemGroup").transform;

            //ApplicationInformation Panel
            applicationInformationToggle = transform.Find("FriendToggleListBG/ApplicationInformation").GetComponent<Toggle>();
            applicationInformationToggle.onValueChanged.AddListener(ApplicationInfomationClicked);

            applicationInformationPanel = transform.Find("ApplicationInformationPanel").gameObject;
            applicationInformationRedBubble = transform.Find("FriendToggleListBG/ApplicationInformation/RedBubble").gameObject;

            dragApplicationInformationPanel = transform.Find("ApplicationInformationPanel/DragApplicationPanel").GetComponent<ScrollRect>();
            applicationInformationScrollView = dragApplicationInformationPanel.GetComponent<ApplicationInformationScrollView>();
            applicationInformationScrollView.onCreateItemHandler = CreateApplicationInformationItem;

            refuseAllApplicationToggle = transform.Find("ApplicationInformationPanel/RefuseAllApplicationBG/RefuseToggle").GetComponent<Toggle>();
            friendToggleRedBubble = transform.parent.Find("ToggleGroup/FriendToggle/RedBubble").GetComponent<Image>();
            refuseAllApplicationToggle.AddListener(RefuseAllApplicationToggleEvent);

            //BlackList Panel
            blackListToggle = transform.Find("FriendToggleListBG/BlackList").GetComponent<Toggle>();
            blackListToggle.onValueChanged.AddListener(BlackListClicked);
            blackListPanel = transform.Find("BlackListPanel").gameObject;

            dragBlackListPanel = transform.Find("BlackListPanel/DragBlackListPanel").GetComponent<ScrollRect>();
            blackListScrollView = dragBlackListPanel.GetComponent<BlackListScrollView>();
            blackListScrollView.onCreatItemHandler = CreateBlackListItem;

            //WeiChatList Panel
            dragWeiChatFriendListPanel = transform.Find("WeiChatFriendListPanel/DragFriendsPanel").GetComponent<ScrollRect>();
            weiChatFriendScrollView = dragWeiChatFriendListPanel.GetComponent<WeiChatFriendListScrollView>();

            weiChatFriendsToggle = transform.Find("FriendToggleListBG/WeiChatFriends").GetComponent<Toggle>();
            weiChatFriendsToggle.onValueChanged.AddListener(WeiChatFriendClicked);
            weiChatFriendListPanel = transform.Find("WeiChatFriendListPanel").gameObject;

            notHaveWeiChatFriendNoticeText = transform.Find("WeiChatFriendListPanel/NoticeText").GetComponent<Text>();
            resourceLoadManager = Resource.GameResourceLoadManager.GetInstance();

            resourceLoadManager.LoadResource("FriendList_Item", OnLoadFriendListItem, true);
            resourceLoadManager.LoadResource("ApplicationInformation_Item", OnLoadApplicationInformationListItem, true);
            resourceLoadManager.LoadResource("BlackList_Item", OnLoadBlackListItem, true);
            resourceLoadManager.LoadResource("FindFriend_Item", OnLoadFindFriendItem, true);

            SwitchPanle();
        }

        public void MyOnEnable()
        {
            friendsToggle.isOn = true;
            RefresRedBubble();
            GameFriendClicked(friendsToggle.isOn);

        }

        #region 注册事件/删除时间

        public void RegisterFriendScreenMessageHandler()
        {
            friendController.RegisterServerMessage();
        }

        public void RemoveFriendScreenMessageHandler()
        {
            friendController.RemoveServerMessage();
        }

        #endregion

        #endregion

        #region OnCreateItemHandler

        private void CreateApplicationInformationItem(ApplicationInformationItem item)
        {
            item.agreeEvent = OnAgreeFriendApplicationClickCallBack;
            item.refuseEvent = OnRefuseFriendApplicationClickCallBack;
        }

        private void CreateFriendListItem(FriendListItem item)
        {
            item.deleteFriendEvent = OnDeleteFriendClickCallBack;
            item.pullBlackEvent = OnClickPullBlackCallBack;
            item.giveGiftEvent = OnClickGiveGiftCallBack;
            item.giveMoneyEvent = OnClickGiveMoneyCallBack;
        }

        private void CreateBlackListItem(BlackListItem item)
        {
            item.addPlayerInFriendListEvent = FriendClick;
            item.deleteBlackListPlayerEvent = OnClickUnblockCallBack;
        }
        #endregion

        #region OnClick Call Back


       public  void  CreateFindFriendListItem(FindFriendItem item)
        {
            item.addPlayerInFriendList = FriendClick;
        }

        private void FriendClick(long id)
        {
            FriendListDataStruct data = friendController.GetFriendListPlayersData().Find(p => p.playerUID == id);
            if (data.playerUID == 0)
            {
                friendController.SendRelationApplicationMessage(Data.RelationApplicationType.ApplyingRelation, id);
                MessageDispatcher.PostMessage(Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, "好友申请已发送，请耐心等待对方确认", "提示");
            }
            else
            {
                MessageDispatcher.PostMessage(Constants.MessageType.OpenAlertWindow, null, UI.AlertType.ConfirmAlone, "该玩家已经在好友列表中，无法再次添加", "提示");
            }
        }

		private void OnAgreeFriendApplicationClickCallBack( long id )
		{
			friendController.SendRelationApplicationMessage( Data.RelationApplicationType.AcceptRelation, id );
		}

		private void OnRefuseFriendApplicationClickCallBack( long id )
		{
			friendController.SendRelationApplicationMessage( Data.RelationApplicationType.RefuseRelation, id );
		}

		private void OnDeleteFriendClickCallBack( long id )
		{
			friendController.SendRelationApplicationMessage( Data.RelationApplicationType.RemoveRelation, id );
		}

		private void OnClickUnblockCallBack( long id )
		{
			friendController.SendRelationApplicationMessage( Data.RelationApplicationType.Unblock, id );
		}

		private void OnClickPullBlackCallBack( long id )
		{
			friendController.SendRelationApplicationMessage( Data.RelationApplicationType.BlockList, id );
		}

		private void OnClickGiveGiftCallBack( long playerId )
		{		
			UIManager.Instance.GetUIByType( UIType.MainLeftBar, ( ViewBase ui, System.Object param ) =>
			{
				( ( MainLeftBarView )ui ).SetStoreClick();
			} );

			UIManager.Instance.GetUIByType( UIType.StoreScreen, ( ViewBase ui, System.Object param ) =>
			{
				( ( StoreView )ui ).OpenStore( playerId );
			} );
		}

		private void OnClickGiveMoneyCallBack( long playerId )
		{
			friendController.SendGiftCurrencies( playerId );
		}

		#endregion

		#region FriendPanel switch toggle functions

		private void GameFriendClicked( bool isOn )
		{
			friendsToggle.interactable = !isOn;
			screenStatus = FriendScreenStatus.ShowFriendList;
			SwitchPanle();
			if( isOn )
			{
                friendController.SendRelationList(FriendController.PlayerListType.Friend);
            }
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, "GameFriend toggle is clicked." );
		}

		private void FindFriendClicked( bool isOn )
		{
            searchFriendInput.text = "";
            findFrindsToggle.interactable = !isOn;
			screenStatus = FriendScreenStatus.ShowFindFriend;
			SwitchPanle();
			if( isOn )
			{
                findFriendNoticeTitleText.text = recommendFridens;
                friendController.SendRecommendPlayer();
			}				
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, "FindFriend toggle is clicked." );
		}

		private void ApplicationInfomationClicked( bool isOn )
		{
			applicationInformationToggle.interactable = !isOn;
			screenStatus = FriendScreenStatus.ShowApplicationInfomation;
			SwitchPanle();
			if( isOn )
			{
				friendController.SendRelationList( FriendController.PlayerListType.Appliation );
			}

			RefreshApplicationInfomationPlayerList();
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, "ApplicaitonInfomation toggle is clicked." );
		}

		private void BlackListClicked( bool isOn )
		{
			blackListToggle.interactable = !isOn;

			screenStatus = FriendScreenStatus.ShowBlaickList;
			SwitchPanle();

			if( isOn )
			{
				friendController.SendRelationList( FriendController.PlayerListType.Black );
			}
				
			DebugUtils.Log( DebugUtils.Type.UI, "BlackList toggle is clicked." );
		}

		private void WeiChatFriendClicked( bool isOn )
		{
			weiChatFriendsToggle.interactable = !isOn;
			screenStatus = FriendScreenStatus.ShowWeiChatFriendList;
			SwitchPanle();

			DebugUtils.Log( DebugUtils.Type.UI, "WeiChatFriends toggle is clicked." );
		}

		private void SwitchPanle()
		{
            friendListPanel.SetActive(false);
            findFriendPanel.SetActive(false);
            applicationInformationPanel.SetActive(false);
            blackListPanel.SetActive(false);
            weiChatFriendListPanel.SetActive(false);
            switch (screenStatus)
            {
                case FriendScreenStatus.ShowFriendList:
                    friendListPanel.SetActive(true);
                    break;
                case FriendScreenStatus.ShowFindFriend:
                    findFriendPanel.SetActive(true);
                    break;
                case FriendScreenStatus.ShowApplicationInfomation:
                    applicationInformationPanel.SetActive(true);
                    break;
                case FriendScreenStatus.ShowBlaickList:
                    blackListPanel.SetActive(true);
                    break;
                case FriendScreenStatus.ShowWeiChatFriendList:
                    weiChatFriendListPanel.SetActive(true);
                    break;
                default:
                    break;
            }
		}

		#endregion

		#region FriendList Panel functions

		public void RefreshFriendList()
		{
			List<FriendListDataStruct> friendList = friendController.GetFriendListPlayersData();
			if( friendList.Count < 1 )
			{
				notHaveFriendNoticeText.gameObject.SetActive( true );
			}
			else
			{
				notHaveFriendNoticeText.gameObject.SetActive( false );
			}
            friendListScrollView.InitializeWithData( friendList );
		}

		private void OnLoadFriendListItem( string name, UnityEngine.Object obj, System.Object param )
		{
			friendListScrollView.InitDataBase( dragFriendPanel, obj, 1, 904, 102, 2, new Vector2( 501f, -50f ));
            RefreshFriendList();
		}

 
        private void OnLoadFindFriendItem(string name, UnityEngine.Object obj, System.Object param)
        {
            findfriendListView.InitDataBase(dragSystemRecommendedFriendsPanel,obj,3,290,178,2,new Vector2(165f,-90f));
            RefreshSystemRecommendedFriendList();
        }

		#endregion

		#region FindFriend Panel functions

		private void SearchFriendButtonClicked()
		{
			//TODO:Send server a findfriend operation.Then active network waiting icon.
			DebugUtils.Log( DebugUtils.Type.UI_SocialScreen, "SearchFriendButton clicked." );
            dragSystemRecommendedFriendsPanel.gameObject.SetActive(false);
            findFriendNoticeTitleText.text = this.findFriendResult;
            friendController.SendSearchPlayer ( searchFriendInput.text );
		}

		public void RefreshSystemRecommendedFriendList()
		{     
			List<FriendListDataStruct> playerList = friendController.GetSystemRecommendedPlayersData();
            if ( playerList.Count > 0 )
			{
				dragSystemRecommendedFriendsPanel.gameObject.SetActive( true );
				findFriendNoticeText.gameObject.SetActive( false );
			}
			else
			{
				findFriendNoticeText.gameObject.SetActive( true );
				dragSystemRecommendedFriendsPanel.gameObject.SetActive( false );
				return;
			}
            findfriendListView.InitializeWithData(playerList);
        }

        private void HidFindPlayerNotice()
		{
			findFriendNoticeText.gameObject.SetActive( false );
			searchFriendInput.text = findPlayerInputStr;
			findFriendNoticeTitleText.text = systemRecommendedFriendsStr;
		}
			
		private void ShowFindedPlayerItem( FriendListDataStruct dummy )
		{
			for( int i = 0; i < findFriendPanelItemList.Count;i++ )
			{
				findFriendPanelItemList[i].gameObject.SetActive( false );
			}
			findFriendPanelItemList[0].SetData(dummy);
            findFriendPanelItemList[0].Show();
		}

		#endregion

		#region ApplicationInformation Panel functions

		public void RefreshApplicationInfomationPlayerList()
		{
			List<FriendListDataStruct> applicationPlayerList = friendController.GetApplicationPlayersData();
			applicationInformationScrollView.InitializeWithData( applicationPlayerList );
		}

		public void RefresRedBubble()
		{
			if(DataManager.GetInstance().GetRedBubbleNum(CaptionType.RelationApplicationCaption) > 0 )
			{
				ShowFriendToggleRedBubble();
				ShowApplicationInformationRedBubble();
			}
			else
			{
				HideFriendToggleRedBubble();
				HideApplicationInformationRedBubble();
			}
		}

		public void RefreshRedrefuseAllApplicationToggle( bool isRefuseApply )
		{
			if( refuseAllApplicationToggle.isOn != isRefuseApply )
			{
				refuseAllApplicationToggle.isOn = isRefuseApply;
			}
		}

		private void OnLoadApplicationInformationListItem( string name, UnityEngine.Object obj, System.Object param )
		{
			applicationInformationScrollView.InitDataBase( dragApplicationInformationPanel, obj, 1, 904, 102, 2, new Vector2( 456.4f, -51.7f ) );
		}

		private void ShowFriendToggleRedBubble()
		{
			friendToggleRedBubble.gameObject.SetActive( true );
		}

		private void HideFriendToggleRedBubble()
		{
			friendToggleRedBubble.gameObject.SetActive( false );
		}

		public void ShowApplicationInformationRedBubble()
		{
			applicationInformationRedBubble.SetActive( true );
		}

		public void HideApplicationInformationRedBubble()
		{
			applicationInformationRedBubble.SetActive( false );
		}

		public void RefuseAllApplicationToggleEvent( bool isOn )
		{
			friendController.SendRelationScreenedMessage( isOn );
		}

		#endregion

		#region BlackList Panel functions

		private void ResetBlackListPanel()
		{
			RefreshBlackList();
		}

		public void RefreshBlackList()
		{
			List<FriendListDataStruct> blackListPlayerList = friendController.GetBlackListPlayersData();

			blackListScrollView.InitializeWithData( blackListPlayerList );
		}

		private void OnLoadBlackListItem( string name, UnityEngine.Object obj, System.Object param )
		{
			blackListScrollView.InitDataBase( dragBlackListPanel, obj, 1, 951, 135, 2, new Vector2( 502, -53f ) );
		}

		#endregion

	}
}