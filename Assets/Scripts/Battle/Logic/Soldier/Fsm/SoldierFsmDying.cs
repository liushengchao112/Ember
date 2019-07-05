using Utils;

namespace Logic
{
    public class SoldierFsmDying : Fsm
    {
        protected Soldier owner;
        protected UnitBehaviorListener stateListener;

        public SoldierFsmDying() { }

        public SoldierFsmDying( Soldier soldier ) 
        {
            owner = soldier;
            stateListener = owner.stateListener;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            stateListener.PostAliveStateChangedEvent( false );
            stateListener.PostDeathEvent();

            owner.ClearPathRender();
        }

        public override void Update( int deltaTime )
        {
            owner.Dead();
        }
    }
}
