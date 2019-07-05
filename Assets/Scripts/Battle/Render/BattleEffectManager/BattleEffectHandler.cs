using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Render
{
    public class BattleEffectHandler : MonoBehaviour
    {
        private ParticleSystem[] particleSystems;
        private Animator[] animators;

        public void Awake()
        {
            particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>();
            animators = gameObject.GetComponentsInChildren<Animator>();
        }

        public void Play( float speed = 1 )
        {
            if ( particleSystems != null )
            {
                for ( int i = 0; i < particleSystems.Length; i++ )
                {
                    ParticleSystem.MainModule main = particleSystems[i].main;
                    main.simulationSpeed = speed;
                }
            }

            if ( animators != null )
            {
                for ( int i = 0; i < animators.Length; i++ )
                {
                    animators[i].speed = speed;
                }
            }

            gameObject.SetActive( false );
            gameObject.SetActive( true );
        }

        public void Stop()
        {
            gameObject.SetActive( false );
        }

        public void OnDestroy()
        {
            particleSystems = null;
            animators = null;
        }
    }

}
