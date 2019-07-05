using UnityEngine;
using System.Collections;

namespace UI
{
	public class UINetworkSelectView : MonoBehaviour
	{
		[HideInInspector]
		public GameObject aliCloudBtn;
		[HideInInspector]
		public GameObject sunMacServerBtn;
		[HideInInspector]
		public GameObject jiawenServerBtn;
		[HideInInspector]
		public GameObject ancherServerBtn;
        [HideInInspector]
        public GameObject fangJSServerBtn;

        private GameObject boot;
        private UINetworkSelectController uiNetworkSelectController;

        private UILabel versionLabel;

		void Start()
		{
            boot = GameObject.Find( "Boot" );
			uiNetworkSelectController = new UINetworkSelectController( this, boot );

			aliCloudBtn = transform.Find( "ExtanalServer_Btn/Sprite" ).gameObject;
			sunMacServerBtn = transform.Find( "SunMacServer_Btn/Sprite" ).gameObject;
			jiawenServerBtn = transform.Find( "JiawenServer_Btn/Sprite" ).gameObject;
			ancherServerBtn = transform.Find( "AncherServer_Btn/Sprite" ).gameObject;
            fangJSServerBtn = transform.Find( "FangJSServer_Btn/Sprite" ).gameObject;

            versionLabel = transform.Find( "Version_Label" ).gameObject.GetComponent<UILabel>();
			versionLabel.text = string.Format( "GameVersion : {0}", Utils.VersionUtil.Instance.curVersion );

            UIEventListener.Get( aliCloudBtn ).onClick = uiNetworkSelectController.GoIAliCloudNetwork;
			UIEventListener.Get( sunMacServerBtn ).onClick = uiNetworkSelectController.GoSunMacServerNetwork;
			UIEventListener.Get( jiawenServerBtn ).onClick = uiNetworkSelectController.GoJiawenServerNetwork;
			UIEventListener.Get( ancherServerBtn ).onClick = uiNetworkSelectController.GoArcherServerNetwork;
            UIEventListener.Get( fangJSServerBtn ).onClick = uiNetworkSelectController.GoFangJSServerNetwork;
        }
	}
}
