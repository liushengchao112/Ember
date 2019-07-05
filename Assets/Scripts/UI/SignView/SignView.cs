using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using SignInfo = Data.DailyLoginS2C.SignInfo;

namespace UI
{
    public class SignView : ViewBase
    {
        private SignControler controler;

        #region UIComponet
        private Transform aniTran;
        private Text titleText;
        private Transform itemRoot;
        private Button signButton;
        private Text signButtonText;
        private GameObject enableImage;
        private Button closeButton;
        #endregion

        private SignItem[] signItem = new SignItem[7];
        private List<SignInfo> signInfos = new List<SignInfo>();
        private bool signResquestType = false;

        public override void OnInit()
        {
            base.OnInit();
            _controller = new SignControler( this );
            controler = _controller as SignControler;

            #region FindUIComponet
            aniTran = transform.Find( "Ani" );
            titleText = aniTran.Find( "TitleText" ).GetComponent<Text>();
            itemRoot = aniTran.Find( "ItemRoot" );
            for ( int i = 0; i < 7; i++ )
            {
                signItem[i] = itemRoot.Find( "SignItem" + i ).gameObject.AddComponent<SignItem>();
            }
            signButton = aniTran.Find( "SignButton" ).GetComponent<Button>();
            signButton.AddListener( OnClickSignButton );
            signButtonText = signButton.transform.Find( "Text" ).GetComponent<Text>();
            enableImage = signButton.transform.Find( "EnableImage" ).gameObject;
            enableImage.SetActive( false );
            closeButton = aniTran.Find( "CloseButton" ).GetComponent<Button>();
            closeButton.AddListener( OnClickCloseButton );
            #endregion
        }

        public override void OnEnter()
        {
            base.OnEnter();
            controler.SendDailySignResquest( false );
            signResquestType = false;
        }

        public void RefreshItemUI()
        {
            signInfos = controler.GetSignInfo();
            for ( int i = 0; i < signInfos.Count; i++ )
            {
                signItem[i].UpdateData( signInfos[i] );
            }
            if ( controler.GetIsSignToday() )
            {
                signButton.enabled = false;
                enableImage.SetActive( true );
            }
        }

        public void SignHandler()
        {
            if ( !signResquestType )
            {
                return;
            }
            for ( int i = 0; i < signInfos.Count; i++ )
            {
                if ( !signInfos[i].isGet )
                {
                    signInfos[i].isGet = true;
                    signItem[i].UpdateData( signInfos[i] );
                    break;
                }
            }
            signButton.enabled = false;
            enableImage.SetActive( true );
        }

        private void OnClickSignButton()
        {
            controler.SendDailySignResquest( true );
            signResquestType = true;
        }

        private void OnClickCloseButton()
        {
            OnExit( false );
        }
    }
}
