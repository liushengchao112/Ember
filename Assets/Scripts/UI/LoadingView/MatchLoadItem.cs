using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

namespace UI
{
    public class MatchLoadItem : MonoBehaviour
    {
        private Image itemImage,golwImage;
        private Text nameText, rateText;

        //private const string PLAYER_ICON_PATH = "UITexture/Avatar_icon/";
        private MatchLoadItemVo data;
        private Data.MatchSide mySide;
        public bool isLoadComplted;
        private int targetNum;

        private Color myColor = new Color( 1, 235 / (float)255, 145 / (float)255 );

        private void Awake()
        {
            golwImage = transform.Find( "GlowImage" ).GetComponent<Image>();
            itemImage = transform.Find( "ItemImage" ).GetComponent<Image>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
            rateText = transform.Find( "RateText" ).GetComponent<Text>();
        }

        public void Init(MatchLoadItemVo data)
        {
            this.data = data;

            AtlasSprite sprite = Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( data.playerIcon );
            itemImage.SetSprite( sprite );
            //Resource.GameResourceLoadManager.GetInstance().LoadAtlasSprite( data.playerIcon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
            //{
            //    itemImage.SetSprite( atlasSprite );
            //}, true );

            if (this.data.playerID != Data.DataManager.GetInstance().GetPlayerId())
            {
                golwImage.gameObject.SetActive( false );
                nameText.color = Color.white;
                rateText.color = Color.white;
            }
            else
            {
                golwImage.gameObject.SetActive( true );
                nameText.color = myColor;
                rateText.color = myColor;
            }
            nameText.text = this.data.playerName;

            targetNum = 0;
            OnProgressHandler();
        }
       
        public int Progress
        {
            set
            {
                DOTween.To( () => targetNum, x => targetNum = x, value, 1 ).OnUpdate( OnProgressHandler );
            }
        }
        private void OnProgressHandler()
        {
            rateText.text = targetNum + "%";
        }

        public long PlayerID
        {
            get
            {
                return data.playerID;
            }
        }
    }

    public struct MatchLoadItemVo
    {
        public long playerID;      
        public string playerName;  
        public string playerIcon;    
        public Data.MatchSide sideIndex;
    }
}
