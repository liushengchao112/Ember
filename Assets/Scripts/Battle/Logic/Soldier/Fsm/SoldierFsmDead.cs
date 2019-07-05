using Utils;

namespace Logic
{
    public class SoldierFsmDead : Fsm
    {
        protected Soldier owner;

        public SoldierFsmDead() { }

        public SoldierFsmDead( Soldier soldier )
        {
            owner = soldier;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            owner.skillHandler.TerminateAllSkill();
        }

        public override void Update( int deltaTime )
        {

        }
    }
}

