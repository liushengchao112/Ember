using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using Resource;
using MatchFriendData = Data.InvitationFriendInfo;

namespace UI
{
    public class MatchFriendItem : ScrollViewItemBase
    {
        public delegate void InviationCallBack( MatchFriendItem item );
        public InviationCallBack inviationEvent;

        public MatchFriendData info;

        private Image iconImage;
        private Text nameText, stateText;
        private Button inviteButton;
        private int refreshIconNum = 0;

        private Color onlineColor = new Color( 21 / (float)255, 241 / (float)255, 83 / (float)255 );
        private Color offlineColor = new Color( 209 / (float)255, 207 / (float)255, 210 / (float)255 );

        private void Awake()
        {
            iconImage = transform.Find( "IconImage" ).GetComponent<Image>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
            stateText = transform.Find( "StateText" ).GetComponent<Text>();
            inviteButton = transform.Find( "InviteButton" ).GetComponent<Button>();
        }

        private void Start()
        {
            inviteButton.AddListener( OnClickInviteButton, UIEventGroup.Middle, UIEventGroup.Middle );
        }

        private void OnClickInviteButton()
        {
            if ( inviationEvent != null )
                inviationEvent( this );
        }

        public override void UpdateItemData( object dataObj )
        {
            base.UpdateItemData( dataObj );
            if ( dataObj == null )
                return;
            info = (MatchFriendData)dataObj;
            RefreshMatchFriendItem();
        }

        public void RefreshMatchFriendItem()
        {
            if ( refreshIconNum == 0 && string.IsNullOrEmpty( info.portrait ) )
            {
                refreshIconNum += 1;
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( info.portrait, delegate ( string name, AtlasSprite atlasSprite, System.Object param ) {
                    iconImage.SetSprite( atlasSprite );
                }, true );
            }
            stateText.color = info.online ? onlineColor : offlineColor;

            nameText.text = info.playerName;
            stateText.text = GetFriendState( info );
        }

        private string GetFriendState( MatchFriendData data )
        {
            if ( data.battleId == 1 )
            {
                SetInvationButtonState( false );
                return "组队中";
            }
            if ( data.battleId != 0 )
            {
                SetInvationButtonState( false );
                return "战斗中";
            }
            if ( data.online )
            {
                SetInvationButtonState( true );
                return "在线";
            }
            SetInvationButtonState( false );
            return "离线";
        }

        public void SetInvationButtonState( bool invationState )
        {
            inviteButton.interactable = invationState;
        }

        public void SetFriendInBattle( bool state )
        {
            inviteButton.interactable = state;
            stateText.text = state ? GetFriendState( info ) : "战斗中";
        }

        public void SetFriendInMatching( bool state )
        {
            inviteButton.interactable = state;
            stateText.text = state ? GetFriendState( info ) : "组队中";
        }
    }
}
