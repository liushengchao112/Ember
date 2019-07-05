using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Logic;
using Constants;

namespace BattleAgent
{
    public class UnityPhysicsAgent : MonoBehaviour
    {
        private CollisionStartMethod CollisionStart;

        private CollisionEndMethod CollisionEnd;

        public LogicUnit owner;

        public void Initialize( LogicUnit owner )
        {
            this.owner = owner;

            BoxCollider boxCollider = owner.gameObject.GetComponent<BoxCollider>();
            if ( boxCollider == null )
            {
                boxCollider = owner.gameObject.AddComponent<BoxCollider>();
            }
            boxCollider.isTrigger = true;


            Rigidbody rigidbody = owner.gameObject.GetComponent<Rigidbody>();
            if ( rigidbody == null )
            {
                rigidbody = owner.gameObject.AddComponent<Rigidbody>();
            }
            rigidbody.useGravity = false;
            rigidbody.isKinematic = false;
        }

        public void LogicUpdate()
        {
            //transform.position = owner.transform.position;
        }

        void OnTriggerEnter( Collider other )
        {
            if ( other.CompareTag( TagName.TAG_GRASS ) )
            {
                CollisionStart( CollisionType.Grass );
            }
            else if ( other.CompareTag( TagName.TAG_SHOALWATERS ) )
            {
                CollisionStart( CollisionType.ShallowWater );
            }
        }

        void OnTriggerStay( Collider other )
        {
            
        }

        void OnTriggerExit( Collider other )
        {
            if ( other.CompareTag( TagName.TAG_GRASS ) )
            {
                CollisionEnd( CollisionType.Grass );
            }
            else if ( other.CompareTag( TagName.TAG_SHOALWATERS ) )
            {
                CollisionEnd( CollisionType.ShallowWater );
            }
        }

        public void RegisterCollisionStartMethod( CollisionStartMethod method )
        {
            CollisionStart = method;
        }

        public void RegisterCollisionEndMethod( CollisionEndMethod method )
        {
            CollisionEnd = method;
        }
    }
}
