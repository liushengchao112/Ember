namespace Logic
{
    public class SoldierFsmPlaceholder : Fsm
    {
        protected Soldier owner;

        public SoldierFsmPlaceholder() { }

        public SoldierFsmPlaceholder( Soldier soldier )
        {
            owner = soldier;
        }
    }
}

