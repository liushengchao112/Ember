using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UI
{
    [CustomEditor( typeof( ShakeCamera ) )]
    public class ShakeCameraEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            var script = ( ShakeCamera )target;
            if ( GUILayout.Button( "SHAKE" ) )
            {
                script.ShowShake();
            }
        }
    }
}