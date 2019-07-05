using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using Utils;

namespace UI
{
    public class TapTowerPopUp : MonoBehaviour
    {
        public Transform headerTra;

        private long towerId;

        private Button recycleButton, cancelButton;
        private Text costText;

        private void Awake()
        {
            costText = transform.Find( "CostText" ).GetComponent<Text>();
            recycleButton = transform.Find( "RecycleButton" ).GetComponent<Button>();
            cancelButton = transform.Find( "CancelButton" ).GetComponent<Button>();
        }

        private void Start()
        {
            recycleButton.AddListener( OnClickRecycleButton );
            cancelButton.AddListener( OnClickCancelButton );
        }

        private void OnClickRecycleButton()
        {
            MessageDispatcher.PostMessage( Constants.MessageType.RecylingTower, towerId );

            this.gameObject.SetActive( false );
        }

        private void OnClickCancelButton()
        {
            this.gameObject.SetActive( false );
        }

        public void RefreshItem(int cost,long playerId)
        {
            this.transform.position = Camera.main.WorldToScreenPoint( headerTra.position );

            this.gameObject.SetActive( true );
            this.towerId = playerId;
            costText.text = cost.ToString();
        }

        private void Update()
        {
            this.transform.position = Camera.main.WorldToScreenPoint( headerTra.position );
        }

    }
}
