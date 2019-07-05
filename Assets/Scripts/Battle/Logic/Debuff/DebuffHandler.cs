/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: Debuff.cs
// 
// created time：09/28/2016
//
//----------------------------------------------------------------*/

using UnityEngine;
using System.Collections.Generic;

using Utils;

namespace Logic
{
    public class DebuffHandler
    {
        // HP
        public int hpValue = 0;
        public List<Debuff> hpEffects;

        public FixFactor hpRateValue = FixFactor.zero;
        public List<Debuff> hpRateEffects;

        // Physical attack
        public int physicalAtkValue = 0;
        public List<Debuff> physicalAtkEffects;

        public FixFactor physicalAtkRateValue = FixFactor.zero;
        public List<Debuff> physicalAtkRateEffects;

        // Magic attack
        public int magicAtkValue = 0;
        public List<Debuff> magicAtkEffects;

        public FixFactor magicAtkRateValue = FixFactor.zero;
        public List<Debuff> magicAtkRateEffects;

        // Armor
        public int armorValue = 0;
        public List<Debuff> armorEffects;

        public FixFactor armorRateValue = FixFactor.zero;
        public List<Debuff> armorRateEffects;

        // Magic Resist
        public int magicResistValue = 0;
        public List<Debuff> magicResistEffects;

        public FixFactor magicResistRateValue = FixFactor.zero;
        public List<Debuff> magicResistRateEffects;

        // Critical Chance
        public int criticalChanceValue = 0;
        public List<Debuff> criticalChanceEffects;

        // Critical Damage
        public int criticalDamageValue = 0;
        public List<Debuff> criticalDamageEffects;

        // Speed 
        public int speedValue = 0;
        public List<Debuff> speedEffects;

        public FixFactor speedRateValue = FixFactor.zero;
        public List<Debuff> speedRateEffects;

        // Attack Speed
        public FixFactor attackSpeedRateValue = FixFactor.zero;
        public List<Debuff> attackSpeedRateEffects;

        // MaxHealth
        public int maxHealthValue = 0;
        public List<Debuff> maxHealthEffects;

        public FixFactor maxHealthRateValue = FixFactor.zero;
        public List<Debuff> maxHealthRateEffects;

        // Health Recover
        public int healthRecoverValue = 0;
        public List<Debuff> healthRecoverEffects;

        public FixFactor healthRecoverRateValue = FixFactor.zero;
        public List<Debuff> healthRecoverRateEffects;

        public int damageMitigationRateValue = 0;
        public List<Debuff> damageMitigationRateEffects;

        public FixFactor additionalDamageRateValue = FixFactor.zero;
        public List<Debuff> additionalDamageRateEffects;

        public int sneerTime = 0;
        public Debuff sneerEffect;

        public int stunTime = 0;
        public Debuff stunEffect;

        public int slienceTime = 0;
        public Debuff slienceEffect;

        public int forbiddenAttackTime = 0;
        public Debuff forbiddenAttackEffect;

        public int forbiddenMoveTime = 0;
        public Debuff forbiddenMoveEffect;

        public DebuffHandler()
        {
            hpEffects = new List<Debuff>();
            hpRateEffects = new List<Debuff>();

            physicalAtkEffects = new List<Debuff>();
            physicalAtkRateEffects = new List<Debuff>();

            magicAtkEffects = new List<Debuff>();
            magicAtkRateEffects = new List<Debuff>();

            armorEffects = new List<Debuff>();
            armorRateEffects = new List<Debuff>();

            magicResistEffects = new List<Debuff>();
            magicResistRateEffects = new List<Debuff>();

            criticalChanceEffects = new List<Debuff>();
            criticalDamageEffects = new List<Debuff>();

            speedEffects = new List<Debuff>();
            speedRateEffects = new List<Debuff>();

            attackSpeedRateEffects = new List<Debuff>();

            maxHealthEffects = new List<Debuff>();
            maxHealthRateEffects = new List<Debuff>();

            healthRecoverEffects = new List<Debuff>();

            damageMitigationRateEffects = new List<Debuff>();

            additionalDamageRateEffects = new List<Debuff>();
        }


        public void LogicUpdate( int delatTime )
        {
            for ( int i = 0; i < hpEffects.Count; i++ )
            {
                hpEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < hpRateEffects.Count; i++ )
            {
                hpRateEffects[i].LogicUpdate( delatTime );
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

            for ( int i = 0; i < additionalDamageRateEffects.Count; i++ )
            {
                additionalDamageRateEffects[i].LogicUpdate( delatTime );
            }

            for ( int i = 0; i < healthRecoverEffects.Count; i++ )
            {
                healthRecoverEffects[i].LogicUpdate( delatTime );
            }

            if ( sneerEffect != null )
            {
                sneerEffect.LogicUpdate( delatTime );
            }

            if ( stunEffect != null )
            {
                stunEffect.LogicUpdate( delatTime );
            }

            if ( slienceEffect != null )
            {
                slienceEffect.LogicUpdate( delatTime );
            }

            if ( forbiddenMoveEffect != null )
            {
                forbiddenMoveEffect.LogicUpdate( delatTime );
            }

            if ( forbiddenAttackEffect != null )
            {
                forbiddenAttackEffect.LogicUpdate( delatTime );
            }
        }

        public void Destory()
        {
            for ( int i = 0; i < hpEffects.Count; i++ )
            {
                hpEffects[i].Released();
            }

            for ( int i = 0; i < hpRateEffects.Count; i++ )
            {
                hpRateEffects[i].Released();
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

            for ( int i = 0; i < armorEffects.Count; i++ )
            {
                armorEffects[i].Released();
            }

            for ( int i = 0; i < armorRateEffects.Count; i++ )
            {
                armorRateEffects[i].Released();
            }

            for ( int i = 0; i < magicResistEffects.Count; i++ )
            {
                magicResistEffects[i].Released();
            }

            for ( int i = 0; i < magicResistRateEffects.Count; i++ )
            {
                magicResistRateEffects[i].Released();
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

            for ( int i = 0; i < additionalDamageRateEffects.Count; i++ )
            {
                additionalDamageRateEffects[i].Released();
            }

            if ( sneerEffect != null )
            {
                sneerEffect.Released();
            }

            for ( int i = 0; i < healthRecoverEffects.Count; i++ )
            {
                healthRecoverEffects[i].Released();
            }

            if ( stunEffect != null )
            {
                stunEffect.Released();
            }

            if ( slienceEffect != null )
            {
                slienceEffect.Released();
            }

            if ( forbiddenMoveEffect != null )
            {
                forbiddenMoveEffect.Released();
            }

            if ( forbiddenAttackEffect != null )
            {
                forbiddenAttackEffect.Released();
            }
        }
    }
}
