using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainBottomBarView : ViewBase
    {
        private Text chatText;
        private Button chatBt, settingBt, socialBt, guildBt, bulletinBt,watchMatchBt;
        private Image chatRedBubble,guildRedBubble,socialRedBubble,signRedBubble,watchMatchBubble;

        private MainBottomBarController controller;

        public override void OnInit()
        {
            base.OnInit();

            controller = new MainBottomBarController( this );
            _controller = controller;

			chatText = transform.Find( "ChatText" ).GetComponent<Text>();
            chatBt = transform.Find( "ChatBt" ).GetComponent<Button>();

            Transform btGroup = transform.Find( "ButtonGroup" );
            chatRedBubble = transform.Find( "ChatRedBubble" ).GetComponent<Image>();
            guildRedBubble = btGroup.Find( "GuildButton/RedBubble" ).GetComponent<Image>();
            socialRedBubble = btGroup.Find( "SocialButton/RedBubble" ).GetComponent<Image>();
            signRedBubble = btGroup.Find( "BulletinButton/RedBubble" ).GetComponent<Image>();
            watchMatchBubble = btGroup.Find( "WatchMatchButton/RedBubble" ).GetComponent<Image>();
            settingBt = btGroup.Find( "SettingButton" ).GetComponent<Button>();
            socialBt  = btGroup.Find( "SocialButton" ).GetComponent<Button>();
            guildBt = btGroup.Find( "GuildButton" ).GetComponent<Button>();
            bulletinBt = btGroup.Find( "BulletinButton" ).GetComponent<Button>();
            watchMatchBt = btGroup.Find( "WatchMatchButton" ).GetComponent<Button>();

            chatBt.AddListener ( OnClickChatButton );
            settingBt.AddListener( OnClickSettingBt );
            socialBt.AddListener( OnClickSocialBt );
            guildBt.AddListener( OnClickGuildBt );
            bulletinBt.AddListener( OnClickBulletinBtn );
            watchMatchBt.AddListener( OnClickWatchMatchBt );
        }

        public override void OnEnter()
        {
            base.OnEnter();

            SetSocialRedBubble( controller.GetSocialRedBubbleState() );
        }

        #region Button Event

        private void OnRankBtnClick()
        {
            UIManager.Instance.GetUIByType( UIType.MainLeftBar, ( ViewBase ui, System.Object param ) => { ( ui as MainLeftBarView ).SetLeftBarNoClick(); } );
            UIManager.Instance.GetUIByType( UIType.RankView, EnterUI );
        }

        private void OnClickSettingBt()
        {            
            UIManager.Instance.GetUIByType( UIType.SettingScreen, EnterUI );
        }

        private void OnClickBulletinBtn()
        {
            UIManager.Instance.GetUIByType( UIType.BulletinBoardUI , EnterUI );
        }

        private void OnClickSocialBt()
        {
            UIManager.Instance.GetUIByType( UIType.MainLeftBar, ( ViewBase ui, System.Object param ) => { ( ui as MainLeftBarView ).SetLeftBarNoClick(); } );
            UIManager.Instance.GetUIByType( UIType.SocialScreen, EnterUI );
        }

        private void OnClickGuildBt()
        {
            //UIManager.Instance.GetUIByType( UIType.MainLeftBar, ( ViewBase ui, System.Object param ) => { ( ui as MainLeftBarView ).SetLeftBarNoClick(); } );
            //UIManager.Instance.GetUIByType( UIType.GuildScreen, EnterUI );
        }

        private void OnClickWatchMatchBt()
        {
            //TODO : ClickWatchMatch

        }

        private void OnClickChatButton()
        {
            UIManager.Instance.GetUIByType( UIType.MainLeftBar, ( ViewBase ui, System.Object param ) => { ( ui as MainLeftBarView ).SetLeftBarNoClick(); } );
            UIManager.Instance.GetUIByType( UIType.ChatMainUI, EnterUI );
		}

        private void EnterUI( ViewBase ui, System.Object param )
        {
            if ( ui != null )
            {
                if ( !ui.openState )
                {
                    ui.OnEnter();
                }
            }
        }

        #endregion

        #region Set RedBubble State

        public void SetChatRedBubble( bool state )
        {
            chatRedBubble.gameObject.SetActive( state );
        }

        public void SetGuildRedBubble( bool state )
        {
            guildRedBubble.gameObject.SetActive( state );
        }
        
        public void SetSocialRedBubble( bool state )
        {
            socialRedBubble.gameObject.SetActive( state );
        }

        public void SetSignRedBubble( bool state )
        {
            signRedBubble.gameObject.SetActive( state );
        }

        public void SetWatchMatchRedBubble( bool state )
        {
            watchMatchBubble.gameObject.SetActive( state );
        }

        #endregion
    }
}
