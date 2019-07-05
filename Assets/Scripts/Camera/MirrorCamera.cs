using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class MirrorCamera : MonoBehaviour
    {
        private Camera renderCamera;
        public bool flipHorizontal;

        void Awake()
        {
            renderCamera = GetComponent<Camera>();
        }

        void OnPreCull()
        {
            renderCamera.ResetWorldToCameraMatrix();
            renderCamera.ResetProjectionMatrix();
            Vector3 scale = new Vector3( flipHorizontal ? -1 : 1, 1, 1 );
            renderCamera.projectionMatrix = renderCamera.projectionMatrix * Matrix4x4.Scale( scale );
        }

        void OnPreRender()
        {
            GL.invertCulling = flipHorizontal;
        }

        void OnPostRender()
        {
            GL.invertCulling = false;
        }
    }
}
