using Utils;

namespace Logic
{
    public class SoldierFsmIdle : Fsm
    {
        protected Soldier owner;
        protected UnitBehaviorListener stateListener;

        public SoldierFsmIdle() { }

        public SoldierFsmIdle( Soldier soldier ) 
        {
            owner = soldier;
            stateListener = owner.stateListener;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            
            stateListener.PostIdleStateChanged( true );

            owner.ClearPathRender();
        }

        public override void Update( int deltaTime )
        {
            LogicUnit target = owner.target;

            // Check target
            if ( target != null )
            {
                if ( !target.Alive() )
                {
                    owner.target = null;
                    owner.targetId = 0;
                    target = null;
                }
                else
                {
                    // Target Alive
                }
            }

            if ( target != null )
            {
                owner.Attack( target );
            }
            else
            {
                owner.FindOpponent();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            stateListener.PostIdleStateChanged( false );
        }
    }
}
