using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Logic
{
    public class ChaoFengSkill : Skill
    {
        private int timer;

        public override bool DependOnSkillState()
        {
            return true;
        }

        public override void Fire()
        {
            base.Fire();

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SoldierReleaseSkill;
            rm.ownerId = owner.id;
            rm.direction = owner.direction.vector3;
            rm.arguments.Add( "index" , index );
            rm.arguments.Add( "metaId" , metaId );
            owner.PostRenderMessage( rm );

            timer = 0;
        }
        public override void LogicUpdate( int deltaTime )

        {
            base.LogicUpdate( deltaTime );

            if ( timer > skillActionHitTime )
            {
                Work();
                timer = 0;
            }

            timer += deltaTime;
        }

        private void Work()
        {
            for ( int i = 0; i < attributeEffects.Count; i++ )
            {
                if ( owner != null && owner.Alive() && owner.target.type == LogicUnitType.Soldier )
                {
                    AttributeEffect ae = GenerateAttributeEffect( attributeEffects[i] );
                    ae.Attach( (Soldier)owner , (Soldier)owner.target );
                }                
            }

            ReleaseEnd();
        }

    }
}
