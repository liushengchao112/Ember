using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;

using UnitSkillData = Data.UnitSkillsProto.UnitSkill;

namespace Logic
{
    public class JiShuGengXinSkill : Skill
    {
        public List<AttributeEffect> attributeEffect;

        public override void Initialize( long id, Soldier owner, UnitSkillData skillProto, int index )
        {
            base.Initialize( id, owner, skillProto, index );

            attributeEffect = new List<AttributeEffect>();
        }

        public override void Fire()
        {
            base.Fire();

            for ( int i = 0; i < attributeEffects.Count; i++ )
            {
                AttributeEffect ae = GenerateAttributeEffect( attributeEffects[i] );
                ae.Attach( owner, owner );
                attributeEffect.Add( ae );
            }
        }

        public override void ReleaseEnd()
        {
            base.ReleaseEnd();

            for ( int i = 0; i < attributeEffect.Count; i++ )
            {
                attributeEffect[i].Detach();
            }

            attributeEffect.Clear();
            attributeEffect = null;
        }
    }
}
