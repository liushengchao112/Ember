using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class CrystalCarFsmMining : Fsm
    {
        private CrystalCar owner;
        private int miningTimer = 0;
        private int miningInterval;

        public CrystalCarFsmMining( CrystalCar crystalCar )
        {
            owner = crystalCar;
        }

        public override void Update( int deltaTime )
        {
            base.Update( deltaTime );

            miningTimer += deltaTime;
            if ( miningTimer > owner.miningInterval )
            {
                miningTimer = 0;
                if ( owner.target != null && owner.target.Alive() && owner.target.CanBeMined( owner.mark ) )
                {
                    int harvest = owner.target.Mined( owner.emberHarvest, owner );
                    owner.AddCoin( harvest );
                }
                else
                {
                    owner.ChangeState( CrystalCarState.IDLE, owner.fsmIdle );
                }
            }
        }
    }

}
