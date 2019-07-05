using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

using Data;
using Resource;
using Utils;
using System.Collections;

namespace UI
{
	public class UnitBuyPanel : MonoBehaviour
	{
		public string unitName;

		public int icon;
		public int goldCost;
		public int diamondCost;
		public int crystalCost;
		public int boughtNumber;
		public int boughtNumberLimit;
		public int playerGold;
		public int playerCrystal;
		public int playerDiamond;

		public string descripe;
		public Action<int, CurrencyType> onClickEvent;

		private  int buyCount = 1;

		private Button runeSellButton, addCountButton, reduceCountButton, exitButton, diamondButton, goldButton;

		private Text unitNameText, unitNumberText, describeText, diamondCostText, goldCostText;
		private InputField buyNumberInputField;

		private Image runeIcon;

		#region warning pop

		private string warningTitle = "提示";
		private string limitedStr = "已经达到购买上限";
		private string needMoreMoney = "您的金币不够";
		private string needMoreDiamond = "您的钻石不够";

		#endregion

		public void Awake()
		{
			describeText = transform.Find ( "DescribeText" ).GetComponent<Text> ();
			diamondCostText = transform.Find ( "CrystalCostText" ).GetComponent<Text> ();
			goldCostText = transform.Find ( "GoldCostText" ).GetComponent<Text> ();

			addCountButton = transform.Find ( "AddButton" ).GetComponent<Button> ();
			exitButton = transform.Find ( "MaskImageButton" ).GetComponent<Button> ();
			reduceCountButton = transform.Find ( "ReduceButton" ).GetComponent<Button> ();
			goldButton = transform.Find ( "GoldBuyButton" ).GetComponent<Button> ();
			diamondButton = transform.Find ( "CrystalBuyButton" ).GetComponent<Button> ();

			runeIcon = transform.Find ( "UnitIcon" ).GetComponent<Image> ();
			buyNumberInputField = transform.Find ( "InputField" ).GetComponent<InputField> ();
			unitNameText = transform.Find ( "UnitNameText" ).GetComponent<Text> ();

			addCountButton.AddListener ( OnClickAddButton );
			reduceCountButton.AddListener ( OnClickReduceButton );
			exitButton.AddListener ( OnClickExitButton );
			diamondButton.AddListener ( OnClickDiamondButton );
			goldButton.AddListener ( OnClickGoldButton );
			buyNumberInputField.onEndEdit.AddListener ( InputFileEvent );
		}

		public void RefreshPanel()
		{
            GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
            {
                runeIcon.SetSprite( atlasSprite );
            }, true );

			buyCount = 1;

            unitNameText.text = unitName;
			describeText.text = descripe;
			RefreshText ();
		}

		public void OnClickAddButton()
		{
			buyCount++;
			RefreshText ();
		}

		public void OnClickReduceButton()
		{
			buyCount--;

			if( buyCount < 1 )
			{
				buyCount = 1;
			}

			RefreshText ();
		}

		public void InputFileEvent( string text )
		{
			buyCount = int.Parse ( text );
			RefreshText ();
		}

		//MayBe need ember button.
		/*private void OnClickCrystalButton()
		{
			if( boughtNumberLimit != 0 && ( boughtNumber + buyCount ) > boughtNumberLimit )
			{
				MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, AlertType.ConfirmAlone, limitedStr, warningTitle );
				return;
			}

			if( playerCrystal < (buyCount * crystalCost) )
			{
				MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, AlertType.ConfirmAlone, needMoreDiamond, warningTitle );
				return;
			}

			if( onClickEvent != null )
			{
				onClickEvent ( buyCount , CurrencyType.EMBER );
			}
		}*/

		private void OnClickDiamondButton()
		{
			if( boughtNumberLimit != 0 && ( boughtNumber + buyCount ) > boughtNumberLimit )
			{
				MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, AlertType.ConfirmAlone, limitedStr, warningTitle );
				return;
			}

			if( playerDiamond < ( buyCount * diamondCost ) )
			{
				MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, AlertType.ConfirmAlone, needMoreDiamond, warningTitle );
				return;
			}

			if( onClickEvent != null )
			{
				onClickEvent ( buyCount , CurrencyType.DIAMOND );
			}
		}

		private void OnClickGoldButton()
		{
			if( boughtNumberLimit != 0 && ( boughtNumber + buyCount) > boughtNumberLimit )
			{
				MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, AlertType.ConfirmAlone, limitedStr, warningTitle );
				return;
			}

			if( playerGold < ( buyCount * goldCost ) )
			{
				MessageDispatcher.PostMessage( Constants.MessageType.OpenAlertWindow, null, AlertType.ConfirmAlone, needMoreMoney, warningTitle );
				return;
			}

			if( onClickEvent != null )
			{
				onClickEvent ( buyCount , CurrencyType.GOLD );
			}
		}

		private void RefreshText()
		{
			goldCostText.text = goldCost * buyCount + "";
			diamondCostText.text = diamondCost * buyCount + "";
			buyNumberInputField.text = buyCount.ToString ();
		}
			
		public void OnClickExitButton()
		{
			gameObject.SetActive ( false );
		}
	}
}