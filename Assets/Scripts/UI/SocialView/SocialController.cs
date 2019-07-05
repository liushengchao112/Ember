using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class SocialController : ControllerBase
	{
		private SocialView view;

		public SocialController( SocialView view )
		{
			this.view = view;
		}

		public override void OnResume()
		{
			base.OnResume();
		}

		public override void OnPause()
		{
			base.OnPause();
		}
	}
}
