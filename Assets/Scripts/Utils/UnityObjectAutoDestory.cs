using UnityEngine;
using System.Collections;

namespace Utils
{
    public class UnityObjectAutoDestory : MonoBehaviour
    {
        public float LifeTime = 0;

        private float lifeTimer = 0;
    	
    	// Update is called once per frame
    	void Update ()
        {
            if ( lifeTimer <= LifeTime )
            {
                lifeTimer += Time.deltaTime;
            }
            else
            {
                GameObject.DestroyImmediate( gameObject );
            }
        }
    }
}
