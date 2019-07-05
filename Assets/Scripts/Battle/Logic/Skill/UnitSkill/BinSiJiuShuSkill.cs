using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    // Heal friendly force unit when owner death
    public class BinSiJiuShuSkill : Skill
    {
        public override void Fire()
        {
            base.Fire();

            List<Soldier> s = FindFriendlySoldiers( mark, position, radius );

            for ( int i = 0; i < s.Count; i++ )
            {
                for ( int j = 0; j < attributeEffects.Count; j++ )
                {
                    AttributeEffect ae = GenerateAttributeEffect( attributeEffects[j] );
                    ae.Attach( owner, s[i] );
                }
            }
        }
    }
}
