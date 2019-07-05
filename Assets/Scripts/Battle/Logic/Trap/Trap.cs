using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Data;

namespace Logic
{
    public enum TrapType
    {
        None,
        NormalTrap,
        DeadTrap,
        JingJiTrap,
        MagicTrap,
    }
    public class Trap : LogicUnit
    {
//<<<<<<< HEAD
//        private const float LIFE_TIME = 100.0f;
//        private int radius = ConvertUtils.ToLogicInt( 5f );
//=======
        protected int trapDuration;
        protected int attackRadius;
        protected int trrigerRadius;
        public int fireResId;
        public int explodeResId;
        public int hitResId;


        protected AttributeEffectGenerator GenerateAttributeEffect;

        protected WithinCircleAreaPredicate WithinCircleAreaPredicate;
        protected WithinFrontRectAreaPredicate WithinFrontRectAreaPredicate;

        // might be null
        protected FindOpponentSoldiersMethod FindOpponentSoldiers;
        protected FindNeutralUnitsMethod FindNeutralUnits;
        protected FindOpponentCrystalMethod FindOpponentCrystal;
        protected FindOpponentCrystalCarsMethod FindOpponentCrystalCars;
        protected FindOpponentDemolishersMethod FindOpponentDemolishers;
        protected FindOpponentBuildingsMethod FindOpponentBuildings;

        public LogicUnit owner;
        protected List<int> attributeEffects = new List<int>();
        public FixVector3 rotation;

        //--
        private int timer;
        private int workTime;


        public virtual void Initialize( LogicUnit owner, TrapProto.Trap proto, int id, FixVector3 position, FixVector3 rotation )
        {
            //--TODO
            type = LogicUnitType.Trap;
            attributeEffects.Clear();
            this.id = id;
            this.owner = owner;
            this.position = position;
            this.rotation = rotation;
            metaId = proto.ID;
            modelId = proto.TrapResID;            
            attackRadius = ConvertUtils.ToLogicInt( proto.AttackRadius);
            trrigerRadius = ConvertUtils.ToLogicInt( proto.TrrigerRadius);
            trapDuration = ConvertUtils.ToLogicInt( proto.TrapDuration );
            fireResId = proto.FireResID;
            explodeResId = proto.ExplodeResID;
            hitResId = proto.HitResID;
            AddAttributeEffect( proto.AttributeEffectID );

            DebugUtils.Log( DebugUtils.Type.AI_Trap , string.Format( "Trap has been initialize id = {0}" , id ) );
            workTime = 0;
        }

        public virtual void TrapDestroy()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Trap , string.Format( "Trap has been Release id = {0}" , id ) );

            RenderMessage rm = new RenderMessage();
            rm.ownerId = id;
            rm.type = RenderMessage.Type.TrapDestroy;
            PostRenderMessage( rm );

            PostDestroy( this );
        }

        private void AddAttributeEffect( string effects )
        {
            string[] effect = effects.Split( '|' );
            for ( int i = 0; i < effect.Length; i++ )
            {
                attributeEffects.Add( int.Parse( effect[i] ) );
            }            
        }

        public override void AddCoin( int coin )
        {
            
        }

        public override bool Alive()
        {
            DebugUtils.Assert( false , "Alive() in Trap is not implemented!" );
            return false;
        }

        public override void Hurt( int hurtValue , AttackPropertyType hurtType , bool isCrit, LogicUnit injurer )
        {
            DebugUtils.Assert( false , "Hurt() in Trap is not implemented!" );
        }

        public override void LogicUpdate( int deltaTime )
        {
            //--
            timer += deltaTime;
            if ( timer >= 500 )
            {
                timer = 0;
                Work();
            }

            if ( workTime > 0 )
            {
                workTime = 0;
                TrapDestroy();
            }

            if ( timer >= trapDuration )
            {
                TimeOut();
            }
        }

        //--
        private void Work()
        {
            List<Soldier> allSoldier = new List<Soldier>();

            allSoldier.AddRange( FindOpponentSoldiers( owner.mark , position , ( s ) =>
            {
                return WithinCircleAreaPredicate( position , attackRadius , s.position );
            } ) );

            for ( int i = 0; i < allSoldier.Count; i++ )
            {
                if ( allSoldier[i] != null && allSoldier[i].Alive() )
                {
                    for ( int j = 0; j < attributeEffects.Count; j++ )
                    {
                        AttributeEffect ae = GenerateAttributeEffect( attributeEffects[j] );
                        if ( hitResId != 0 )
                        {
                            ae.SetEffectInfo( hitResId , "FootEffectPoint" );
                        }
                        ae.Attach( (Soldier)owner , allSoldier[i] );
                        workTime++;
                    }
                }
            }
        }

        public void SetRadius( int radius )
        {
            this.attackRadius = radius;
        }

        public override void OnKillEnemy( int emberReward , LogicUnit killer , LogicUnit dead )
        {
            owner.OnKillEnemy( emberReward , owner , dead );
        }

        public override void Reset()
        {
            attributeEffects.Clear();
            workTime = 0;
        }

        public virtual void TimeOut()
        {
            DebugUtils.Log( DebugUtils.Type.AI_Trap , "the trap " + id + "  time out  " );
            TrapDestroy();
        }

        public void RegisterFindOpponentSoldiers( FindOpponentSoldiersMethod method )
        {
            FindOpponentSoldiers = method;
        }

        public void RegisterFindOpponentCrystal( FindOpponentCrystalMethod method )
        {
            FindOpponentCrystal = method;
        }

        public void RegisterFindOpponentBuildings( FindOpponentBuildingsMethod method )
        {
            FindOpponentBuildings = method;
        }

        public void RegisterFindOpponentCrystalCars( FindOpponentCrystalCarsMethod method )
        {
            FindOpponentCrystalCars = method;
        }

        public void RegisterFindOpponentDemolishers( FindOpponentDemolishersMethod method )
        {
            FindOpponentDemolishers = method;
        }

        public void RegisterWithinFrontRectangleAreaPredicate( WithinFrontRectAreaPredicate method )
        {
            WithinFrontRectAreaPredicate = method;
        }

        public void RegisterWithinCircleAreaPredicate( WithinCircleAreaPredicate method )
        {
            WithinCircleAreaPredicate = method;
        }

        public void RegisterGenerateAttributeEffect( AttributeEffectGenerator method )
        {
            GenerateAttributeEffect = method;
        }

        public void RegisterFindNeutralUnits( FindNeutralUnitsMethod method )
        {
            FindNeutralUnits = method;
        }
    }
}