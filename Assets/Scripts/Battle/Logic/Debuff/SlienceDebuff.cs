using UnityEngine;
using System.Collections;

using Utils;

namespace Logic
{
    public class SlienceDebuff : Debuff
    {
        private Soldier soldier;

        public override void Attach( LogicUnit g, LogicUnit t )
        {
            base.Attach( g, t );

            // TODO: wait to add a trigger in soldier

            if ( t.type == LogicUnitType.Soldier )
            {
                soldier = (Soldier)t;

                if ( !soldier.GetFlag( SoldierAttributeFlags.BanSkill ) )
                {
                    soldier.SetFlag( SoldierAttributeFlags.BanSkill );
                }
                soldier.skillHandler.SetHandlerEnabled( false );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AI_AttributeEffect , string.Format( "{0} {1} can't handle {2} {3} now!" , t.type , t.id , type , attributeAffectType ) );
            }
        }

        public override void Detach()
        {
            base.Detach();

            if ( soldier.GetFlag( SoldierAttributeFlags.BanSkill ) )
            {
                soldier.RemoveFlag( SoldierAttributeFlags.BanSkill );
            }
            soldier.skillHandler.SetHandlerEnabled( true );
        }
    }
}
