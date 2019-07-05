using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class SneerDebuff : Debuff
    {
        private Soldier soldier;

        public override void Attach( LogicUnit g , LogicUnit t )
        {
            base.Attach( g , t );

            if ( t.type == LogicUnitType.Soldier )
            {
                soldier = (Soldier)t;

                if ( ownerDebuff.sneerEffect == null && soldier!= null && soldier.Alive() )
                {
                    if ( !soldier.GetFlag( SoldierAttributeFlags.BanSkill ) )
                    {
                        soldier.SetFlag( SoldierAttributeFlags.BanSkill );
                    }
                    soldier.skillHandler.SetHandlerEnabled( false );

                    if ( !soldier.GetFlag( SoldierAttributeFlags.BanCommand ) )
                    {
                        soldier.SetFlag( SoldierAttributeFlags.BanCommand );
                    }

                    ownerDebuff.sneerEffect = this;
                    soldier.ClearPathRender();
                    soldier.Chase( g );
                }                
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} can't handle debuff now!" , t.type , t.id ) );
            }
        }

        public override void Detach()
        {
            base.Detach();

            DebugUtils.Log( DebugUtils.Type.AI_AttributeEffect , string.Format( " {0} {1} has been detached" , type , attributeAffectType ) );

            if ( soldier.GetFlag( SoldierAttributeFlags.BanSkill ) )
            {
                soldier.RemoveFlag( SoldierAttributeFlags.BanSkill );
            }
            soldier.skillHandler.SetHandlerEnabled( true );

            if ( soldier.GetFlag( SoldierAttributeFlags.BanCommand ) )
            {
                soldier.RemoveFlag( SoldierAttributeFlags.BanCommand );
            }

            soldier.target = null;
            soldier.ChangeState( 1 , soldier.fsmIdle );
            ownerDebuff.sneerEffect = null;
        }
    }
}
