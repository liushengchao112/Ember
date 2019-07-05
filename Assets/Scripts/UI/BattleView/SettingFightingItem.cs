using Resource;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Data;

namespace UI
{
    public class SettingFightingItem : MonoBehaviour
    {
        //private const string PLAYER_ICON_PATH = "UITexture/Avatar_icon/";

        public string playerName , playerIcon;
        public int killCount, deathCount, resourceCount;
        public bool isRedSide;
        public long playerId;

        private Text playerNameText, killCountText, deathCountText, resourceCountText;
        private GameObject blueImageObj, redImageObj;
        private Image playerHead;

        private void Awake()
        {
            playerNameText = transform.Find( "PlayerNameText" ).GetComponent<Text>();
            killCountText = transform.Find( "KillCountText" ).GetComponent<Text>();
            deathCountText = transform.Find( "DeathCountText" ).GetComponent<Text>();
            resourceCountText = transform.Find( "ResourceCountText" ).GetComponent<Text>();
            blueImageObj = transform.Find( "BlueImage" ).gameObject;
            redImageObj = transform.Find( "RedImage" ).gameObject;
            playerHead = transform.Find( "HeadBgImage/Image" ).GetComponent<Image>();
        }

        public void SetSettingFightItem()
        {
            playerNameText.text = playerName;

            if ( isRedSide )
            {
                playerNameText.text = "<color=#D02522>" + playerName + "</color>";
            }
            else
            {
                playerNameText.text = "<color=#507CEA>" + playerName + "</color>";
            }

            if ( playerId == DataManager.GetInstance().GetPlayerId() )
            {
                playerNameText.text = "<color=#FAEB91>" + playerName + "</color>";
            }

            killCountText.text = killCount.ToString();
            deathCountText.text = deathCount.ToString();
            resourceCountText.text = resourceCount.ToString();

            blueImageObj.SetActive( !isRedSide );
            redImageObj.SetActive( isRedSide );
            RefreshPlayerHeadIcon( playerIcon );
        }

        private void RefreshPlayerHeadIcon( string orderIconStr )
        {
            if ( string.IsNullOrEmpty( orderIconStr ) )
            {
                return;
            }
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( orderIconStr , delegate ( string name , AtlasSprite atlasSprite , System.Object param )
            {
                playerHead.SetSprite( atlasSprite );
            } , true );
        }
    }

    public struct SettingFightingItemVo
    {
        public string name;    
        public int killCount;
        public int deathCount;
        public int resourceCount;
        public string portrait;  //头像
        public bool isRedSide;
        public long playerId;
    }
}
