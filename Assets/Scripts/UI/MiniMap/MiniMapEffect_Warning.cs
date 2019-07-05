using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UI
{
	public class MiniMapEffect_Warning:MiniMapEffect
	{
		public void Init()
		{
			effectType = MiniMapEffectType.Warning;
			effectScale = Vector3.one * 2.5f;
			lifeTime = 0.8f;
		}

		public override void EffectStart()
		{
			this.transform.localScale = Vector3.one;

			effectID = IdGenerator.GenerateIntId( effectType.ToString() );

			isFinished = false;
			this.gameObject.SetActive( true );

			tweener = transform.DOScale( effectScale,0.2f );
			tweener.SetLoops( -1 ,LoopType.Yoyo );

			Invoke( "EffectFinish", lifeTime );
		}
	}
}
