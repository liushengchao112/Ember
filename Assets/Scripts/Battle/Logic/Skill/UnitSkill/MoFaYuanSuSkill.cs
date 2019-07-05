using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Data;
using BattleAgent;
using Utils;

namespace Logic
{
    // summon skill
    public class MoFaYuanSuSkill : Skill
    {
        private SummonedUnit summon;
        private int releaseTimer;
        private FixVector3 summonBrithPosition;
        private bool startGenerateSummon;

        public override void Initialize( long id, Soldier owner, UnitSkillsProto.UnitSkill skillProto, int skillIndex )
        {
            base.Initialize( id, owner, skillProto, skillIndex );

            summonBrithPosition = FixVector3.zero;

            startGenerateSummon = false;
            releaseTimer = 0;
        }

        public override void Fire()
        {
            base.Fire();

            if ( playerOwner )
            {
                // check brithPostion is suitable

                PathAgent agent = owner.pathAgent;
                FixVector3 destination = owner.position + owner.direction.normalized * 3; // 3 meter front of owner
                FixVector3 hitPosition;
                FixVector3 simpleOnMapPoint = FixVector3.zero;

                // mapping destination on navmesh
                bool sampleResult = agent.SamplePosition( destination, out simpleOnMapPoint );
                if ( sampleResult )
                {
                    destination = simpleOnMapPoint;
                }

                bool result = agent.Raycast( destination, out hitPosition );
                if ( result )
                {
                    // didn't encountered obstruction  
                    destination = hitPosition;
                }

                UpdateC2S message = new UpdateC2S();
                message.timestamp = DataManager.GetInstance().GetFrame();
                Operation op = new Operation();
                op.unitId = id;
                op.targetId = owner.id; // test 
                op.unitMetaId = metaId; // test 
                op.opType = OperationType.SyncSkillTargetPosition; // wrong type
                op.x = destination.vector3.x;
                op.y = destination.vector3.y;
                op.z = destination.vector3.z;
                message.operation = op;
                PostBattleMessage( MsgCode.UpdateMessage, message );

                DebugUtils.LogWarning( DebugUtils.Type.AI_Skill, string.Format( "skill {0} {1}，send sync position ", id, skillName ) );
            }

            DebugUtils.LogWarning( DebugUtils.Type.AI_Skill, string.Format( "Fire skill {0} {1}，waiting sync position ", id, skillName ) );
        }

        public override void SyncMessageHandler( FixVector3 position )
        {
            summonBrithPosition = position;

            RenderMessage rm1 = new RenderMessage();
            rm1.type = RenderMessage.Type.SoldierReleaseSkill;
            rm1.ownerId = owner.id;
            rm1.position = summonBrithPosition.vector3;
            rm1.arguments.Add( "metaId", metaId );
            rm1.arguments.Add( "index", index );
            PostRenderMessage( rm1 );

            startGenerateSummon = true;

            DebugUtils.LogWarning( DebugUtils.Type.AI_Skill, string.Format( "skill id = {0} {1}, recieve sync position ", id, skillName ) );
        }

        public override void LogicUpdate( int deltaTime )
        {
            base.LogicUpdate( deltaTime );

            if ( startGenerateSummon && skillEnable )
            {
                if ( releaseTimer >= skillActionHitTime && summon == null )
                {
                    summon = GenerateSummon( this, carrierId, summonBrithPosition, owner.direction );
                    summon.RegisterDestroyHandler( PostDestroy );
                    summon.RegisterRenderMessageHandler( PostRenderMessage );
                    summon.RegisterRandomMethod( GetRandomNumber );
                    summon.RegisterFindNeutralUnit( FindNeutralUnit );
                    summon.RegisterFindNeutralUnits( FindNeutralUnits );
                    summon.RegisterFindOpponentBuildings( FindOpponentBuildings );
                    summon.RegisterFindOpponentCrystalCar( FindOpponentCrystalCar );
                    summon.RegisterFindOpponentCrystalCars( FindOpponentCrystalCars );
                    summon.RegisterFindOpponentDemolisher( FindOpponentDemolisher );
                    summon.RegisterFindOpponentDemolishers( FindOpponentDemolishers );
                    summon.RegisterFindOpponentSoldier( FindOpponentSoldier );
                    summon.RegisterFindOpponentSoldiers( FindOpponentSoldiers );
                    summon.RegisterWithinFrontSectorAreaPredicate( WithinCircleAreaPredicate );

                    RenderMessage rm = new RenderMessage();
                    rm.type = RenderMessage.Type.SpawnSummonedUnit;
                    rm.ownerId = summon.id;
                    rm.position = summon.position.vector3;
                    rm.direction = summon.direction.vector3;
                    rm.arguments.Add( "mark", owner.mark );
                    rm.arguments.Add( "metaId", summon.metaId );
                    PostRenderMessage( rm );
                }

                releaseTimer += deltaTime;
            }
        }

        public override void Stop()
        {
            base.Stop();

            if ( summon != null )
            {
                summon.Release();
            }
        }

        public override void Reset()
        {
            base.Reset();

            summon = null;
            releaseTimer = 0;
        }

        public override bool DependOnSkillState()
        {
            return true;
        }
    }
}
