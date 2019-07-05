using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class BackgroundStoryPanel : MonoBehaviour
	{

		public string descripe;

		private Button exitButton;

		private Text describeText;


		public void Awake()
		{
			describeText = transform.Find ( "UnitDescribeText" ).GetComponent<Text> ();
			exitButton = transform.Find ( "MaskImageButton" ).GetComponent<Button> ();
			exitButton.AddListener ( OnClickExitButton );
		}

		public void RefreshPanel()
		{
			describeText.text = descripe;
		}
			
		public void OnClickExitButton()
		{
			gameObject.SetActive ( false );
		}
	}
}