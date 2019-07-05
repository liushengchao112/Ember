using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildAssetBundleConfig : ScriptableObject
{
    public List<BuildAssetBundleData> list = new List<BuildAssetBundleData>();

    public bool Check( string bundleName, string hashName )
    {
        bool change = true;
        foreach ( var bundle in list )
        {
            if ( bundle.bundleName.Equals( bundleName ) )
            {
                change = bundle.hashName != hashName;
                break;
            }
        }
        return change;
    }

    public BuildAssetBundleData GetBundleData( string bundleName )
    {
        foreach ( var bundle in list )
        {
            if ( bundle.bundleName.Equals( bundleName ) )
            {
                return bundle;
            }
        }
        return null;
    }

    public void Add( string bundleName, string hashName, long size )
    {
        BuildAssetBundleData data = null;
        foreach ( var bundle in list )
        {
            if ( bundle.bundleName.Equals( bundleName ) )
            {
                data = bundle;
                data.hashName = hashName;
                data.size = size;
            }
        }

        if(data == null)
        {
            data = new BuildAssetBundleData( bundleName, hashName, size );
            list.Add( data );
        }
    }
}

[System.Serializable]
public class BuildAssetBundleData
{
    public string bundleName;
    public string hashName;
    public long size;

    public BuildAssetBundleData( string bundleName, string hashName, long size )
    {
        this.bundleName = bundleName;
        this.hashName = hashName;
        this.size = size;
    }

}
