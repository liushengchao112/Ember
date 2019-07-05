using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FriendInvationAlertView:MonoBehaviour
    {
        #region Component
        private Image playerIcon;
        private Button refuseButton, acceptButton;
        private Text playerNameText, contentText;
        #endregion

        private long currentFriendId;
        private string currentFriendPortrait;
        private string currentFriendName;
        private Data.BattleType currentBattleType;

        private FriendInvationAlertController controller;

        public  void Awake()
        {
            controller = new FriendInvationAlertController( this );

            playerIcon = transform.Find( "PlayerIcon" ).GetComponent<Image>();
            playerNameText = transform.Find( "PlayerNameText" ).GetComponent<Text>();
            contentText = transform.Find( "ContentText" ).GetComponent<Text>();
            refuseButton = transform.Find( "RefuseButton" ).GetComponent<Button>();
            acceptButton = transform.Find( "AcceptButton" ).GetComponent<Button>();

            refuseButton.AddListener( OnClickRefuseButton, UIEventGroup.Middle, UIEventGroup.Middle );
            acceptButton.AddListener( OnClickAcceptButton, UIEventGroup.Middle, UIEventGroup.Middle );
        }

        public void OnDestroy()
        {
            if ( controller != null )
                controller.OnDestroy();
        }

        public void OnEnterAlert( long friendId, Data.BattleType type, string friendName, string friendPortrait )
        {
            controller.OnEnter();
            this.gameObject.SetActive( true );

            currentFriendId = friendId;
            currentBattleType = type;
            currentFriendName = friendName;
            currentFriendPortrait = friendPortrait;

            RefreshView();
            StartCoroutine( "WaitAMinuteRefuse" );
        }

        public void CloseView(  )
        {
            StopCoroutine( "WaitAMinuteRefuse" );
            controller.OnExit();
            this.gameObject.SetActive( false );
        }

        #region Button Event

        private void OnClickRefuseButton()
        {
            controller.SendRefuse( currentFriendId, currentBattleType );
        }

        private void OnClickAcceptButton()
        {
            controller.SendAccept( currentFriendId, currentBattleType );
        }

        #endregion

        private void RefreshView()
        {
            if ( !string.IsNullOrEmpty( currentFriendPortrait ) )
                Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( currentFriendPortrait, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    playerIcon.SetSprite( atlasSprite );
                }, true );

            playerNameText.text = currentFriendName;
            contentText.text = string.Format( "邀请您组队 <color=#ffe800>{0}</color>", GetBattleTypeString() );
        }

        private string GetBattleTypeString()
        {
            switch ( currentBattleType )
            {
                case Data.BattleType.BattleP2vsP2:
                    return "2v2匹配";
                case Data.BattleType.Survival:
                    return "生存模式";
                case Data.BattleType.Tranining:
                    return "训练模式";
            }
            return "";
        }

        public void OpenFightMatchView( List<Data.MatcherReadyData> dataList )
        {
            UIManager.Instance.GetUIByType( UIType.FightMatchScreen, ( ViewBase ui, System.Object param ) => { ( ui as FightMatchView ).EnterFightUI( false, currentBattleType, true, dataList ); } );
            CloseView();
        }

        private IEnumerator WaitAMinuteRefuse()
        {
            yield return new WaitForSeconds( 60f );
            controller.SendRefuse( currentFriendId, currentBattleType );
        }
    }
}
