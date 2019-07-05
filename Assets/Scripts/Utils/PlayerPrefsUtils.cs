using UnityEngine;
using System;
using System.Collections;
using System.Reflection;

namespace Utils
{
    public class PlayerPrefsUtils
    {
        /// <summary>
        /// Save to PlayerPrefs
        /// </summary>
        /// <param name="saveKey">Key of Save</param>
        /// <param name="obj">the class of data</param>
        public static void SaveToPlayerPrefs( string saveKey, object obj )
        {
            Type t = obj.GetType();
            FieldInfo[] fiedls = t.GetFields();

            for ( int i = 0; i < fiedls.Length; i++ )
            {
                string saveName = saveKey + "." + fiedls[i].Name;

                switch ( fiedls[i].FieldType.Name )
                {
                    case "String":
                        PlayerPrefs.SetString( saveName, fiedls[i].GetValue( obj ).ToString() );
                        break;
                    case "Int32":
                    case "Int64":
                    case "Int":
                    case "uInt":
                        PlayerPrefs.SetInt( saveName, (int)fiedls[i].GetValue( obj ) );
                        break;
                    case "Single":
                    case "Float":
                        PlayerPrefs.SetFloat( saveName, (float)fiedls[i].GetValue( obj ) );
                        break;
                }
            }
        }

        /// <summary>
        /// Get the Value in PlayerPrefs
        /// </summary>
        /// <typeparam name="T">the type of data</typeparam>
        /// <param name="key">Key of GetValue</param>
        /// <returns></returns>
        public static T GetValueFromPlayerPrefs<T>( string key ) where T : new()
        {
            T newObj = new T();

            Type t = newObj.GetType();
            FieldInfo[] fiedls = t.GetFields();
            for ( int i = 0; i < fiedls.Length; i++ )
            {
                string saveName = key + "." + fiedls[i].Name;

                switch ( fiedls[i].FieldType.Name )
                {
                    case "String":
                        fiedls[i].SetValue( newObj, PlayerPrefs.GetString( saveName ) );
                        break;
                    case "Int32":
                    case "Int64":
                    case "Int":
                    case "uInt":
                        fiedls[i].SetValue( newObj, PlayerPrefs.GetInt( saveName ) );
                        break;
                    case "Single":
                    case "Float":
                        fiedls[i].SetValue( newObj, PlayerPrefs.GetFloat( saveName ) );
                        break;
                }
            }
            return newObj;
        }

        /// <summary>
        /// Delete key
        /// </summary>
        /// <typeparam name="T">the type of data</typeparam>
        /// <param name="deleteKey">Key of Delete</param>
        public static void DeleteValueFromPlayerPrefs<T>( string deleteKey ) where T : new()
        {
            T newObj = new T();

            Type t = newObj.GetType();
            FieldInfo[] fiedls = t.GetFields();
            for ( int i = 0; i < fiedls.Length; i++ )
            {
                string key = deleteKey + "." + fiedls[i].Name;

                if ( !PlayerPrefs.HasKey( key ) )
                {
                    return;
                }

                switch ( fiedls[i].FieldType.Name )
                {
                    case "String":
                        PlayerPrefs.DeleteKey( key );
                        break;
                    case "Int32":
                    case "Int64":
                    case "Int":
                    case "uInt":
                        PlayerPrefs.DeleteKey( key );
                        break;
                    case "Single":
                    case "Float":
                        PlayerPrefs.DeleteKey( key );
                        break;
                }
            }
        }
    }
}
