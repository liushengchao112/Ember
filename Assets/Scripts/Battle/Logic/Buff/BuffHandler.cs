/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: Buff.cs
// 
// created time：09/28/2016
//
//----------------------------------------------------------------*/

using UnityEngine;
using System.Collections.Generic;

using Utils;

namespace Logic
{
    public class BuffHandler
    {
        // HP
        public int physicalDmgValue = 0;
        public List<Buff> physicalDmgEffects;

        public FixFactor physicalDmgRateValue = FixFactor.zero;
        public List<Buff> physicalDmgRateEffects;

        // Physical attack
        public int physicalAtkValue = 0;
        public List<Buff> physicalAtkEffects;

        public FixFactor physicalAtkRateValue = FixFactor.zero;
        public List<Buff> physicalAtkRateEffects;

        // Magic attack
        public int magicAtkValue = 0;
        public List<Buff> magicAtkEffects;

        public FixFactor magicAtkRateValue = FixFactor.zero;
        public List<Buff> magicAtkRateEffects;

        // Armor
        public int armorValue = 0;
        public List<Buff> armorEffects;

        public FixFactor armorRateValue = FixFactor.zero;
        public List<Buff> armorRateEffects;

        // Magic Resist
        public int magicResistValue = 0;
        public List<Buff> magicResistEffects;

        public FixFactor magicResistRateValue = FixFactor.zero;
        public List<Buff> magicResistRateEffects;

        // Critical Chance
        public int criticalChanceValue = 0;
        public List<Buff> criticalChanceEffects;

        // Critical Damage
        public int criticalDamageValue = 0;
        public List<Buff> criticalDamageEffects;

        // Speed 
        public int speedValue = 0;
        public List<Buff> speedEffects;

        public FixFactor speedRateValue = FixFactor.zero;
        public List<Buff> speedRateEffects;

        // Attack Speed
        public FixFactor attackSpeedRateValue = FixFactor.zero;
        public List<Buff> attackSpeedRateEffects;

        // MaxHealth
        public int maxHealthValue = 0;
        public List<Buff> maxHealthEffects;

        public FixFactor maxHealthRateValue = FixFactor.zero;
        public List<Buff> maxHealthRateEffects;

        // Health Recover
        public int healthRecoverValue = 0;
        public List<Buff> healthRecoverEffects;

        public FixFactor healthRecoverRateValue = FixFactor.zero;
        public List<Buff> healthRecoverRateEffects;

        // Cloaking
        public int cloakingTime = 0;
        public Buff cloakingEffect;

        public List<Buff> damageMitigationRateEffects;

        public BuffHandler()
        {
            physicalDmgEffects = new List<Buff>();
            physicalDmgRateEffects = new List<Buff>();

            physicalAtkEffects = new List<Buff>();
            physicalAtkRateEffects = new List<Buff>();

            magicAtkEffects = new List<Buff>();
            magicAtkRateEffects = new List<Buff>();

            armorEffects = new List<Buff>();
            armorRateEffects = new List<Buff>();

            magicResistEffects = new List<Buff>();
            magicResistRateEffects = new List<Buff>();

            criticalChanceEffects = new List<Buff>();
            criticalDamageEffects = new List<Buff>();

            speedEffects = new List<Buff>();
            speedRateEffects = new List<Buff>();

            attackSpeedRateEffects = new List<Buff>();

            maxHealthEffects = new List<Buff>();
            maxHealthRateEffects = new List<Buff>();

            healthRecoverEffects = new List<Buff>();
            healthRecoverRateEffects = new List<Buff>();

            damageMitigationRateEffects = new List<Buff>();
        }

        public void LogicUpdate( int delatTime )
        {
            for ( int i = 0; i < physicalDmgEffects.Count; i++ )
            {
                physicalDmgEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < physicalDmgRateEffects.Count; i++ )
            {
                physicalDmgRateEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < physicalAtkEffects.Count; i++ )
            {
                physicalAtkEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < physicalAtkRateEffects.Count; i++ )
            {
                physicalAtkRateEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < magicAtkEffects.Count; i++ )
            {
                magicAtkEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < magicAtkRateEffects.Count; i++ )
            {
                magicAtkRateEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < armorEffects.Count; i++ )
            {
                armorEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < armorRateEffects.Count; i++ )
            {
                armorRateEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < magicResistEffects.Count; i++ )
            {
                magicResistEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < magicResistRateEffects.Count; i++ )
            {
                magicResistRateEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < criticalChanceEffects.Count; i++ )
            {
                criticalChanceEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < criticalDamageEffects.Count; i++ )
            {
                criticalDamageEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < speedEffects.Count; i++ )
            {
                speedEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < speedRateEffects.Count; i++ )
            {
                speedRateEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < attackSpeedRateEffects.Count; i++ )
            {
                attackSpeedRateEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < maxHealthEffects.Count; i++ )
            {
                maxHealthEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < maxHealthRateEffects.Count; i++ )
            {
                maxHealthRateEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < damageMitigationRateEffects.Count; i++ )
            {
                damageMitigationRateEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < healthRecoverEffects.Count; i++ )
            {
                healthRecoverEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < healthRecoverRateEffects.Count; i++ )
            {
                healthRecoverRateEffects[i].LogicUpdate( delatTime );
            }

            if ( cloakingEffect != null )
            {
                cloakingEffect.LogicUpdate( delatTime );
            }
        }

        public void Destory()
        {
            for ( int i = 0; i < physicalDmgEffects.Count; i++ )
            {
                physicalDmgEffects[i].Released();
            }

            for ( int i = 0; i < physicalDmgRateEffects.Count; i++ )
            {
                physicalDmgRateEffects[i].Released();
            }

            for ( int i = 0; i < physicalAtkEffects.Count; i++ )
            {
                physicalAtkEffects[i].Released();
            }

            for ( int i = 0; i < physicalAtkRateEffects.Count; i++ )
            {
                physicalAtkRateEffects[i].Released();
            }

            for ( int i = 0; i < magicAtkEffects.Count; i++ )
            {
                magicAtkEffects[i].Released();
            }

            for ( int i = 0; i < magicAtkRateEffects.Count; i++ )
            {
                magicAtkRateEffects[i].Released();
            }

            for ( int i = 0; i < magicResistEffects.Count; i++ )
            {
                magicResistEffects[i].Released();
            }

            for ( int i = 0; i < magicResistRateEffects.Count; i++ )
            {
                magicResistRateEffects[i].Released();
            }

            for ( int i = 0; i < armorEffects.Count; i++ )
            {
                armorEffects[i].Released();
            }

            for ( int i = 0; i < armorRateEffects.Count; i++ )
            {
                armorRateEffects[i].Released();
            }

            for ( int i = 0; i < criticalChanceEffects.Count; i++ )
            {
                criticalChanceEffects[i].Released();
            }

            for ( int i = 0; i < criticalDamageEffects.Count; i++ )
            {
                criticalDamageEffects[i].Released();
            }

            for ( int i = 0; i < speedEffects.Count; i++ )
            {
                speedEffects[i].Released();
            }

            for ( int i = 0; i < speedRateEffects.Count; i++ )
            {
                speedRateEffects[i].Released();
            }

            for ( int i = 0; i < attackSpeedRateEffects.Count; i++ )
            {
                attackSpeedRateEffects[i].Released();
            }

            for ( int i = 0; i < maxHealthEffects.Count; i++ )
            {
                maxHealthEffects[i].Released();
            }

            for ( int i = 0; i < maxHealthRateEffects.Count; i++ )
            {
                maxHealthRateEffects[i].Released();
            }

            for ( int i = 0; i < damageMitigationRateEffects.Count; i++ )
            {
                damageMitigationRateEffects[i].Released();
            }

            for ( int i = 0; i < healthRecoverEffects.Count; i++ )
            {
               healthRecoverEffects[i].Released();
            }

            for ( int i = 0; i < healthRecoverRateEffects.Count; i++ )
            {
                healthRecoverRateEffects[i].Released();
            }

            if ( cloakingEffect != null)
            {
                cloakingEffect.Released();
            }
        }
    }
}



