using UnityEngine;
using System.Collections;
using System;

using Constants;
using Utils;
using Data;

namespace Logic
{
    public class Crystal : LogicUnit
    {
        public ForceMark ownerMark;

        public BuildingState state;
        public bool crystalPlus;

        private int removeOwnerInterval;
        private int removeOwnerTimer;
        private int rebornTime;
        private int rebornTimer;

        public void Initialize( long id, FixVector3 pos, FixVector3 rot, bool isPlusCrystal )
        {
            this.id = id;
            type = LogicUnitType.Crystal;
            state = BuildingState.IDLE;

            ownerMark = ForceMark.NoneForce;
            transform.position = pos.vector3;
            transform.eulerAngles = rot.vector3;
            position = pos;
            crystalPlus = isPlusCrystal;
            modelRadius = ConvertUtils.ToLogicInt( GameConstants.CRYSTAL_MODELRADIUS );

            if ( isPlusCrystal )
            {
                modelId = GameConstants.CRYSTAL_BIG_RESOURCEID;
                maxHp = GameConstants.CRYSTAL_BIG_RESERVES;
                rebornTime = GameConstants.CRYSTAL_BIG_RECOVERTIME;
            }
            else
            {
                modelId = GameConstants.CRYSTAL_SMALL_RESOURCEID;
                maxHp = GameConstants.CRYSTAL_SMALL_RESERVES;
                rebornTime = GameConstants.CRYSTAL_SMALL_RECOVERTIME;
            }

            removeOwnerInterval = GameConstants.CRYSTAL_REMOVEOWNER_INTERVAL;

            hp = maxHp;
        }

        public override void LogicUpdate( int deltaTime )
        {
            if( state == BuildingState.IDLE )
            {
                if( hp <= 0 )
                {
                    Destroy();
                }
            }
            else if( state == BuildingState.DESTROY )
            {
                rebornTimer += deltaTime;
                if ( rebornTimer >= rebornTime )
                {
                    Recover();
                }
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.AI_Crystal, "why the crystal " + id + " gets here? state = " + state );
            }

            removeOwnerTimer += deltaTime;
            if ( removeOwnerTimer >= removeOwnerInterval )
            {
                mark = ForceMark.NoneForce;
                DebugUtils.Log( DebugUtils.Type.AI_Crystal, "Crystal go back to owner less state" );
            }
        }

        public void Destroy()
        {
            state = BuildingState.DESTROY;

            RenderMessage message = new RenderMessage();
            message.type = RenderMessage.Type.CrystalDestroy;
            message.ownerId = id;
            PostRenderMessage( message );
        }

        public bool OwnerLess()
        {
            return mark == ForceMark.NoneForce;
        }

        public bool SameOwner( ForceMark m )
        {
            return mark == m;
        }

        public bool CanBeMined( ForceMark m )
        {
            return mark == ForceMark.NoneForce || mark == m;
        }

        //TODO:how to reborn crystal
        public void Recover()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Crystal, "crystal " + id + " enter recover! " + state );

            // clear runtime data
            hp = 0;
            ownerMark = ForceMark.NoneForce;
            removeOwnerTimer = 0;
            rebornTimer = 0;

            Initialize( id, position, (FixVector3)transform.eulerAngles, crystalPlus );

            RenderMessage rm = new RenderMessage();
            rm.type = RenderMessage.Type.SpawnCrystal;
            rm.ownerId = id;
            rm.position = position.vector3;
            rm.direction = transform.eulerAngles;
            rm.arguments.Add( "hp", maxHp );
            rm.arguments.Add( "modelId", modelId );
            rm.arguments.Add( "plus", crystalPlus );
            PostRenderMessage( rm );
        }

        public int Mined( int h, CrystalCar c )
        {
            int harvest = 0;
            if ( Alive() )
            {
                if ( OwnerLess() || SameOwner( c.mark ) )
                {
                    DebugUtils.Log( DebugUtils.Type.AI_Crystal, "Crystal has been occupied by" + c.mark );
                    ownerMark = c.mark;
                }
                else
                {
                    return harvest;
                }

                if ( hp - h >= 0 )
                {
                    hp -= h;
                    harvest = h;
                }
                else
                {
                    hp = 0;
                    harvest = (int)hp;
                }

                RenderMessage message = new RenderMessage();
                message.type = RenderMessage.Type.CrystalMined;
                message.ownerId = id;
                message.arguments.Add( "value", harvest );
                PostRenderMessage( message );

                // Timing Hurt Gap
                removeOwnerTimer = 0;
            }

            return harvest;
        }

        public override void AddCoin( int coin )
        {
            DebugUtils.Assert( false, "AddCoin() in Npc is not implemented!" );
        }

        public override bool Alive()
        {
            return state != BuildingState.DESTROY;
        }

        public override void Hurt( int hurtValue, AttackPropertyType hurtType, bool isCrit, LogicUnit injurer )
        {
            DebugUtils.Assert( false, "Hurt() in Npc is not implemented!" );
        }

        public override void Reset()
        {
            id = -1;
            rebornTimer = 0;
            state = BuildingState.NONE;
            ownerMark = ForceMark.NoneForce;
            removeOwnerTimer = 0;
            maxHp = 0;
            hp = maxHp;
            crystalPlus = false;
            PostRenderMessage = null;
        }

        public override void OnKillEnemy( int emberReward, LogicUnit killer, LogicUnit dead )
        {
            DebugUtils.Assert( false, "OnKillEnemy() in Crystal is not implemented!" );
        }

    }
}
