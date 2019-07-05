using UnityEngine;
using System.Collections;

using Utils;
using Data;

namespace UI
{
    public class SettingController : ControllerBase
    {
        private SettingView view;

        public SettingController( SettingView v )
        {
            viewBase = v;
            view = v;
        }

        public override void OnResume()
        {
            base.OnResume();

            ReadPlayerPrefsData();
        }

        public override void OnPause()
        {
            base.OnPause();

        }

        #region PlayerPrefs Data
        public void SavePlayerPrefsData()
        {
            DataManager.GetInstance().SaveSettingDataToPlayerPrefs();
        }

        private void ReadPlayerPrefsData()
        {
            DataManager.GetInstance().ReadSettingDataFromPlayerPrefs();
        }

        #endregion
    }
}
