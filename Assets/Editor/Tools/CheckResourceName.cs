using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CheckResourceName : MonoBehaviour {

    [MenuItem( "Tools/CheckPrefabName" )]
    static void CheckPrefabName()
    {
        string[] guids = AssetDatabase.FindAssets( "t:Prefab" );

        for ( int i = 0; i < guids.Length; i++ )
        {
            string path = AssetDatabase.GUIDToAssetPath( guids[i] );
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>( path );
            CheckName( go, path );
        }
        Debug.Log( "check done!" );
    }

    static void CheckName( Object obj , string path)
    {
        if ( obj.name.Contains( " " )
            || obj.name.Contains( "  " ) )
        {
            Debug.LogError( "There is space in the name, info : " + path );
        }
    }
}
