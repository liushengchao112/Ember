using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using Resource;

namespace UI
{
    public class MatchSucceedItem : MonoBehaviour
    {
        public string icon;
        public string nameStr;
        public bool isReady;
        public bool isMyself;

        private Image itemImage, glowImage, background;
        private Text nameText;

        private Color myColor = new Color( 1, 235 / (float)255, 145 / (float)255 );
        
        private void Awake()
        {
            itemImage = transform.Find( "ItemImage" ).GetComponent<Image>();
            glowImage = transform.Find( "GlowImage" ).GetComponent<Image>();
            background = transform.Find( "Background" ).GetComponent<Image>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
        }

        public void RefreshMatch2V2SucceedItem()
        {
            itemImage.gameObject.SetActive( true );
            nameText.gameObject.SetActive( true );
            background.gameObject.SetActive( true );

            GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
            {
                itemImage.SetSprite( atlasSprite );
            }, true );

            nameText.text = nameStr;

            itemImage.SetGray( !isReady );
            glowImage.gameObject.SetActive( isMyself );
            nameText.color = isMyself ? myColor : Color.white;
        }

        public void SetItemCancell()
        {
            //itemImage.gameObject.SetActive( false );
            //glowImage.gameObject.SetActive( false );
            //nameText.gameObject.SetActive( false );
            //background.gameObject.SetActive( false );
        }
    }
}
