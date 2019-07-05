using UnityEngine;
using UnityEngine.UI;
using Resource;

namespace UI
{
	public class UnitAttackInfoPanel : MonoBehaviour
	{

		public string descripe;
		public int icon;

		private Button exitButton;

		private Text describeText;

		private Image attackImage;

		public void Awake()
		{
			describeText = transform.Find ( "AttackDescribeText" ).GetComponent<Text> ();
			exitButton = transform.Find ( "MaskImageButton" ).GetComponent<Button> ();
			attackImage = transform.Find ( "AttackInfoIcon" ).GetComponent<Image> ();
			exitButton.AddListener ( OnClickExitButton );
		}

		public void RefreshPanel()
		{
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
            {
                attackImage.SetSprite( atlasSprite );
            }, true );

            describeText.text = descripe;
		}
			
		public void OnClickExitButton()
		{
			gameObject.SetActive ( false );
		}
	}
}