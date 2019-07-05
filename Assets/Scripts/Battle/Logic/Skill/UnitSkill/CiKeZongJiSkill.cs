using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

using Utils;

namespace Logic
{
    public class CiKeZongJiSkill : Skill
    {
        private int timer;
        private int workTime = 0;
        private List<AttributeEffect> tempBuffList = new List<AttributeEffect>();

        public override void Initialize( long id , Soldier owner , UnitSkillsProto.UnitSkill skillProto , int skillIndex )
        {
            base.Initialize( id , owner , skillProto , skillIndex );
            owner.stateListener.RegisterAfterUnitFight( ReleaseEnd );
        }

        public override bool DependOnSkillState()
        {
            return false;
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
            tempBuffList.Clear();
            DebugUtils.Log( DebugUtils.Type.AI_Skill , "CiKeZongJiSkill + fire" );
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            if ( timer > skillActionHitTime && workTime == 0 )
            {
                Work();
                timer = 0;
            }

            timer += deltaTime;
        }

        private void Work()
        {
            if ( owner != null && owner.Alive() )
            {
                for ( int i = 0; i < attributeEffects.Count; i++ )
                {
                    AttributeEffect ae = GenerateAttributeEffect( attributeEffects[i] );
                    ae.Attach( (Soldier)owner , (Soldier)owner );
                    tempBuffList.Add( ae );
                }

                workTime++;
            }
        }

        public override void ReleaseEnd()
        {
            base.ReleaseEnd();

            for ( int i = 0; i < tempBuffList.Count; i++ )
            {
                tempBuffList[i].Detach();
            }
            tempBuffList.Clear();
        }

        public override void Reset()
        {
            base.Reset();
            workTime = 0;
        }
    }
}