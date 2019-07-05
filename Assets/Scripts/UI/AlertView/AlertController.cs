using UnityEngine;
using System.Collections;
using System;

using Utils;

namespace UI
{
    public enum AlertType
    {
        ConfirmAlone = 1,
        ConfirmAndCancel = 2,
    }

    public class AlertController
    {
        private AlertView view;

        public AlertController( AlertView v )
        {
            view = v;

            //MessageDispatcher.AddObserver( OpenAlertWindow, Constants.MessageType.OpenAlertWindow );
        }

        public void Destroy()
        {
            //MessageDispatcher.RemoveObserver( OpenAlertWindow, Constants.MessageType.OpenAlertWindow );
        }

        //private void OpenAlertWindow( object confirmEvent, object type, object contentText, object titleText )
        //{
        //    view.confirmEvent = ( confirmEvent ) as Action;
        //    view.alertType = (AlertType)type;
        //    view.content = contentText.ToString();
        //    view.title = titleText.ToString();

        //    view.RefreshAlertView();
        //}
    }
}
