using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Utils;

public static class ExtensionMethodUtils
{
    #region INT Extension Methods

    /// <summary>
    /// Use id to get the localized string from LocalizationProto
    /// </summary>
    /// <param name="id">Main Key</param>
    /// <returns></returns>
    public static string Localize( this int id )
    {
        LocalizationProto.Localization l = DataManager.GetInstance().localizationProtoData.Find( p => p.ID == id );
        if ( l == null )
        {
            DebugUtils.LogWarning( DebugUtils.Type.Data, string.Format( "Can't get the localize string by id {0}", id ) );
            return id.ToString();
        }

        // TODO: Add language type
        return l.Chinese;
    }

    #endregion
}
