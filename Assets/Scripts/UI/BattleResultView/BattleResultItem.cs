using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using Resource;
using Data;

namespace UI
{
    public class BattleResultItem : MonoBehaviour
    {
        public bool isMVP;
        public bool isVictory;
        public float mvpValue;
        public string playerName, icon;
        public int killCount, killedCount, resourcesCount;
        public MatchSide side;

        public long playerID;

        private Image mvpImage, backgroundImage, vicSignImage, failSignImage, playerIcon , blueHeadImage , redHeadImage;
        private Text mvpValueText, nameText, killCountText, killedCountText, resourcesCountText;
        private Button addFriendButton , informButton;

        #region Player HeadIcon Image Path
        //private const string PLAYER_ICON_PATH = "UITexture/Avatar_icon/";
        #endregion

        private void Awake()
        {
            mvpImage = transform.Find( "MVPImage" ).GetComponent<Image>();
            backgroundImage = transform.Find( "Background" ).GetComponent<Image>();
            vicSignImage = transform.Find( "VicSignImage" ).GetComponent<Image>();
            failSignImage = transform.Find( "FailSignImage" ).GetComponent<Image>();
            playerIcon = transform.Find( "HeadBgImage/PlayerIcon" ).GetComponent<Image>();
            vicSignImage.gameObject.SetActive( false );
            failSignImage.gameObject.SetActive( false );
            mvpValueText = transform.Find( "ValueText" ).GetComponent<Text>();
            nameText = transform.Find( "NameText" ).GetComponent<Text>();
            killCountText = transform.Find( "KillCountText" ).GetComponent<Text>();
            killedCountText = transform.Find( "KilledCountText" ).GetComponent<Text>();
            resourcesCountText = transform.Find( "ResourcesCountText" ).GetComponent<Text>();
            addFriendButton = transform.Find( "AddFriendButton" ).GetComponent<Button>();
            informButton = transform.Find( "InformButton" ).GetComponent<Button>();
            blueHeadImage = transform.Find( "BlueHeadImage" ).GetComponent<Image>();
            redHeadImage = transform.Find( "RedHeadImage" ).GetComponent<Image>();
            informButton.AddListener( OnClickInformButton );
            informButton.gameObject.SetActive( false );
        }

        public void SetAddFriendButton(bool show)
        {
            if (show && playerID != Data.DataManager.GetInstance().GetPlayerId())
            {
                addFriendButton.gameObject.SetActive( true );
                addFriendButton.AddListener( OnClickAddFriendButton );
            }
            else
            {
                addFriendButton.gameObject.SetActive( false );
            }
        }

        private void OnClickAddFriendButton()
        {

        }

        private void OnClickInformButton()
        {
            //TODO
        }

        public void ShowAddFriendButton()
        {            
            if ( playerID == Data.DataManager.GetInstance().GetPlayerId() )
            {
                addFriendButton.gameObject.SetActive( false );
                informButton.gameObject.SetActive( false );
                return;
            }
            if ( DataManager.GetInstance().CurBattleIsPVE() )
            {
                addFriendButton.gameObject.SetActive( false );
                informButton.gameObject.SetActive( false );
                return;
            }
            addFriendButton.gameObject.SetActive( true );
            informButton.gameObject.SetActive( false );
        }

        public void ShowInformButton()
        {            
            if ( playerID == Data.DataManager.GetInstance().GetPlayerId() )
            {
                addFriendButton.gameObject.SetActive( false );
                informButton.gameObject.SetActive( false );
                return;
            }
            if ( DataManager.GetInstance().CurBattleIsPVE() )
            {
                addFriendButton.gameObject.SetActive( false );
                informButton.gameObject.SetActive( false );
                return;
            }
            addFriendButton.gameObject.SetActive( false );
            informButton.gameObject.SetActive( true );
        }

        public void HideInteractButton()
        {
            addFriendButton.gameObject.SetActive( false );
            informButton.gameObject.SetActive( false );
        }

        public void RefreshBattleResultItem()
        {
            mvpImage.gameObject.SetActive( isMVP );
            mvpValueText.text = mvpValue.ToString();
            nameText.text = playerName;
            killCountText.text = killCount.ToString();
            killedCountText.text = killedCount.ToString();
            resourcesCountText.text = resourcesCount.ToString();

            //if (playerID == Data.DataManager.GetInstance().GetPlayerId())
            //{
            //    vicSignImage.gameObject.SetActive( isVictory );
            //    failSignImage.gameObject.SetActive( !isVictory );
            //}

            if ( side == MatchSide.Blue )
            {
                blueHeadImage.gameObject.SetActive( true );
                redHeadImage.gameObject.SetActive( false );
                vicSignImage.gameObject.SetActive( true );
                failSignImage.gameObject.SetActive( false );
                nameText.text = "<color=#507CEA>" + playerName + "</color>";
            }
            else
            {
                blueHeadImage.gameObject.SetActive( false );
                redHeadImage.gameObject.SetActive( true );
                failSignImage.gameObject.SetActive( true );
                vicSignImage.gameObject.SetActive( false );
                nameText.text = "<color=#D02522>" + playerName + "</color>";
            }

            if ( playerID == DataManager.GetInstance().GetPlayerId() )
            {
                nameText.text = "<color=#FAEB91>" + playerName + "</color>";
            }

            if ( !string.IsNullOrEmpty( icon ) )
            {
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
                {
                    playerIcon.SetSprite( atlasSprite );
                }, true );
            }
        }

        public void UpdateBattleInfo( BattleInfo info )
        {
            mvpImage.gameObject.SetActive( isMVP );
            mvpValueText.text = info.battleScore.ToString();
            nameText.text = info.playerName;
            killCountText.text = info.killQuantity.ToString();
            killedCountText.text = info.dieQuantity.ToString();
            resourcesCountText.text = info.resouce.ToString();

            if ( info.matchSide == MatchSide.Blue )
            {
                blueHeadImage.gameObject.SetActive( true );
                redHeadImage.gameObject.SetActive( false );
                vicSignImage.gameObject.SetActive( true );
                failSignImage.gameObject.SetActive( false );
                nameText.text = "<color=#507CEA>" + info.playerName + "</color>";
            }
            else
            {
                blueHeadImage.gameObject.SetActive( false );
                redHeadImage.gameObject.SetActive( true );
                failSignImage.gameObject.SetActive( true );
                vicSignImage.gameObject.SetActive( false );
                nameText.text = "<color=#D02522>" + info.playerName + "</color>";
            }

            if ( info.playerId == DataManager.GetInstance().GetPlayerId() )
            {
                nameText.text = "<color=#FAEB91>" + info.playerName + "</color>";
            }

            if ( !string.IsNullOrEmpty( info.portrait ) )
            {
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( info.portrait , delegate ( string name , AtlasSprite atlasSprite , System.Object param )
                {
                    playerIcon.SetSprite( atlasSprite );
                } , true );
            }
        }
    }
}
