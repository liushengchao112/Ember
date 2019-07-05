using UnityEngine;
using DG.Tweening;
using Utils;

namespace UI
{
	//If we have more minimap effect,Add in here.Dwayne.
	public enum MiniMapEffectType
	{
		None,
		Warning,
	}

    public class MiniMapEffect : MonoBehaviour
    {
		protected Tweener tweener;

		protected Vector3 effectScale;
		protected float lifeTime ;

		protected bool isFinished = true;
		protected MiniMapEffectType effectType;

		protected int effectID = -1;
		protected long holderID = -1;
	
		//This is effct templates not recommended for direct use.Please use subclasses function.
		public virtual void EffectStart()
        {
			effectID = IdGenerator.GenerateIntId( effectType.ToString() );

			isFinished = false;
			this.gameObject.SetActive( true );

            tweener = transform.DOScale( effectScale, 0.2f );
            tweener.SetLoops( -1, LoopType.Yoyo );

			Invoke( "EffectFinish", lifeTime );

			DebugUtils.Log( DebugUtils.Type.MiniMap, string.Format( "The EffectStart holderID is {0}", this.holderID ));
        }
			
		//This function can use for reset.
		public virtual void EffectFinish()
        {
			MessageDispatcher.PostMessage( Constants.MessageType.MiniMapEffectDestroy, this.holderID );
            tweener.Kill( false );
			this.gameObject.SetActive( false );

			DebugUtils.Log( DebugUtils.Type.MiniMap, string.Format( "The EffectFinished, EffectID is {0} holderID is {1}", this.effectID, this.holderID ));

			isFinished = true;
			effectID = -1;
			holderID = -1;
        }

		public bool GetEffctItemStatus()
		{
			return isFinished;
		}

		public MiniMapEffectType GetEffectType()
		{
			return effectType;
		}
			
		public long GetHolderID()
		{
			return this.holderID;
		}

		public int GetEffectID()
		{
			return this.effectID;
		}

		public void SetHolderID( long holderID )
		{
			this.holderID = holderID;
		}
    }
}
