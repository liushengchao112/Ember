using System;
using UnityEngine;

using Utils;
using Constants;

namespace Data
{
    /**
     * Unified formula management class
     */
    public class Formula
    {
        /**
         *  Fight MVP formula
         *  resourcesScore：The player gets the number of resources
         *  killTotal：The total number of players killed
         *  playerFatality：Player number of deaths
         *  
         *  the document is located ： 战斗系统.docx  MVP公式
         */
        public static float BattleMVPValue( int resources, int killTotal, int playerFatality )
        {
            int resourceScore = (int)( resources * 0.01f );
            return resourceScore + resourceScore * ( killTotal * 0.01f - playerFatality * 0.01f );
        }

        // Get actually damage value after calculate armor
        public static int ActuallyPhysicalDamage( int hurtValue, int armor )
        {
            return new FixFactor( hurtValue * 100, armor + 100 ).integer;
        }

        // Get actually damage value after calculate magicResist
        public static int ActuallyMagicDamage( int hurtValue, int magicResist )
        {
            return new FixFactor( hurtValue * 100, magicResist + 100 ).integer;
        }

        // Get floating damage value with the given range.
        public static int GetAttackFloatingValue( int attackAttribute, int randomNumber1, int randomNumber2  )
        {
            bool isPlus = randomNumber1 % 2 == 0;
            int v = attackAttribute;

            if ( isPlus )
            {
                v = ( v + randomNumber2 );
            }
            else
            {
                v = ( v - randomNumber2 );
            }

            return v;
        }

        // Get damage value after calculate critical damage coefficient
        public static int GetCriticalDamage( int damage, int criticalDamage )
        {
            return ( damage * new FixFactor( criticalDamage, (long)GameConstants.LOGIC_FIXPOINT_PRECISION ) );
        }

        // Use this formula to know current attack whether trigger the critical hit
        public static bool TriggerCritical( int randomValue, int CriticalChance )
        {
            bool result = randomValue < CriticalChance;
            DebugUtils.Log( DebugUtils.Type.Soldier_Properties, string.Format( "{0} to trigger critical roll value: {1}, critcal value {2} ", result, randomValue, CriticalChance ) );
            return result;
        }

        public static float DistanceSqr( Vector3 v1, Vector3 v2 )
        {
            float x = v1.x - v2.x;
            float y = v1.y - v2.y;
            float z = v1.z - v2.z;

            return x * x + y * y + z * z;
        }
    }
}
