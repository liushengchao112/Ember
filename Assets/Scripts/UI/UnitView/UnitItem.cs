using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Resource;

using Utils;
using System;
using UI;

public class UnitItem : MonoBehaviour
{
	public int id;
	public int icon;
	public int number;
	public string unitName;
	public Action<int> onClickEvent;

	private Image unitImage;
	private Button unitButton;
	private Text unitNameText;
	private Text unitNumberText;
	private Transform content;

	void Init()
	{
        if( unitImage == null )
        {
            unitButton = transform.Find( "UnitBoxButton" ).GetComponent<Button>();
            unitImage = transform.Find( "UnitImage" ).GetComponent<Image>();
            unitNameText = transform.Find( "UnitNameText" ).GetComponent<Text>();
            unitNumberText = transform.Find( "Content/UnitNumberText" ).GetComponent<Text>();
            content = transform.Find( "Content" );
            unitButton.onClick.AddListener( OnClickShowUnitInformationButton );
        }
    }

	public void RefreshItem()
	{
        Init();

        if ( number > 0 )
		{
			content.gameObject.SetActive ( true );
            unitNumberText.text = "X" + number;
		}
		else
		{
			content.gameObject.SetActive ( false );
		}

        unitNameText.text = unitName;

        AtlasSprite aSprite = GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon );
        if ( aSprite == null )
        {
            DebugUtils.LogError( DebugUtils.Type.UI, string.Format( "Can't find unit item icon : {0}, please check this!", icon ) );
        }
        unitImage.SetSprite( aSprite );
        //GameResourceLoadManager.GetInstance().LoadAtlasSprite( icon, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
        //{
        //    if( unitImage == null )
        //    {
        //        return;
        //    }
        //    unitImage.SetSprite( atlasSprite );

        //    if ( atlasSprite == null )
        //    {
        //        DebugUtils.LogError( DebugUtils.Type.UI, "Can't find unit item icon, please check this." );
        //    }
        //}, true );
    }

	private void OnClickShowUnitInformationButton()
	{
		onClickEvent ( id );
	}

    void OnDestroy()
    {
        unitImage = null;
    }
}

