using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Network;
using Data;
using Utils;

namespace UI
{
    public class MainBottomBarController : ControllerBase
    {
        private MainBottomBarView view;

        public MainBottomBarController( MainBottomBarView v )
        {
            view = v;
            viewBase = v;
        }

        public override void OnResume()
        {
            base.OnResume();

            MessageDispatcher.AddObserver( RefreshRedBubble, Constants.MessageType.RefreshSocialServerRedBubble );
        }

        public override void OnPause()
        {
            base.OnPause();

            MessageDispatcher.RemoveObserver( RefreshRedBubble, Constants.MessageType.RefreshSocialServerRedBubble );
        }

        public bool GetSocialRedBubbleState()
        {
            DataManager dataManager = Data.DataManager.GetInstance();

            int emailCaptionNum = dataManager.GetRedBubbleNum( CaptionType.EmailCaption );
            int relationApplicationCaptionNum = dataManager.GetRedBubbleNum( CaptionType.RelationApplicationCaption );

            return emailCaptionNum + relationApplicationCaptionNum > 0;
        }

        private void RefreshRedBubble()
        {
            view.SetSocialRedBubble( GetSocialRedBubbleState() );
        }
    }
}
