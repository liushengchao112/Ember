using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace UI
{
	public class ShoppingMallView : ViewBase
	{
		private Transform shoppingMallTop;

		private Button shopTopExitButon;

		public override void OnInit()
		{
			base.OnInit();
			shoppingMallTop = transform.Find( "ShoppingMallTop" );
			shopTopExitButon = shoppingMallTop.Find( "ShoppingMallTopExitButon" ).GetComponent<Button>();

			InitOnClickListener();
		}

		public void InitOnClickListener()
		{
			shopTopExitButon.AddListener( () =>
			{
				OnExit( true );
			} );
		}
	}
}