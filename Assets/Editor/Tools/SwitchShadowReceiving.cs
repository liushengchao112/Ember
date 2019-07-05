using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SwitchShadowReceiving
{
    [MenuItem( "Tools/SwitchShadowReceiving" )]
    static void SwitchShadowReceivingInTerrain()
    {
        GameObject obj = GameObject.Find( "Static" );
        Debug.AssertFormat( obj != null, "There is no GameObject named \"Static\" in the scene!" );

        foreach( MeshRenderer render in obj.GetComponentsInChildren<MeshRenderer>() )
        {
            render.receiveShadows = !render.receiveShadows;
        }

        obj = GameObject.Find( "Function" );
        Debug.AssertFormat( obj != null, "There is no GameObject named \"Function\" in the scene!" );

        foreach( MeshRenderer render in obj.GetComponentsInChildren<MeshRenderer>() )
        {
            render.receiveShadows = !render.receiveShadows;
        }
    }
}