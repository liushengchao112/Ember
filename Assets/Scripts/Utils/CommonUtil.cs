using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

using UnityEngine;
using Data;
using System.Globalization;
using System.IO;

namespace Utils
{
    public enum AssetBundleCompression{ chunk, clzf, lzma };

    public class CommonUtil
    {
        static string streamingAssetsPath { get { return Application.streamingAssetsPath + "/"; } }
        public static AssetBundleCompression compressionType = AssetBundleCompression.lzma;


        public static string GetPlatformString()
        {
#if UNITY_EDITOR
            switch ( UnityEditor.EditorUserBuildSettings.activeBuildTarget )
            {
                case UnityEditor.BuildTarget.Android:
                    return "android";
                case UnityEditor.BuildTarget.iOS:
                    return "ios";
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    return "windows";
                //case UnityEditor.BuildTarget.StandaloneOSXIntel64:
                //    return "osx";
                default: return "windows";
            }            
#else
            switch ( Application.platform)
            {
                case RuntimePlatform.Android:
                    return "android";
                case RuntimePlatform.IPhonePlayer:
                    return "ios";
                case RuntimePlatform.OSXPlayer:
                    return "osx";
                case RuntimePlatform.WindowsPlayer:
                    return "windows";
                default: return string.Empty;
            }
#endif
        }

        /// <summary>
        /// Encoding string to MD5 formate
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string EncodingToMd5( string data )
        {
            byte[] bytes = Encoding.Default.GetBytes( data );
            MD5 md5 = new MD5CryptoServiceProvider();
            bytes = md5.ComputeHash( bytes );
            return BitConverter.ToString( bytes ).Replace( "-", "" ); ;
        }

        /// <summary>
        /// Get the file MD5 information for comparison updates
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileMd5( string filePath )
        {
            try
            {
                FileStream fs = new FileStream( filePath, FileMode.Open );
                int len = ( int )fs.Length;
                byte[] data = new byte[len];
                fs.Read( data, 0, len );
                fs.Close();
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] result = md5.ComputeHash( data );
                StringBuilder fileMD5 = new StringBuilder();
                for ( var i = 0; i < result.Length; i++ )
                {
                    fileMD5.Append( Convert.ToString( result[i], 16 ) );
                }
                return fileMD5.ToString();
            }
            catch ( FileNotFoundException e )
            {
                DebugUtils.LogError( DebugUtils.Type.Special, "Get the file MD5 err, msg:" + e.Message );
                return "";
            }
        }

        public static string UnitNameToAssetName( string unitname )
        {
            return unitname.ToLower().Replace( " ", "_" );
        }

        /// <summary>
        /// Get the full resource path in local cache
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string getWWWURLFromTypeAndName( string name )
        {
#if UNITY_EDITOR
            return "file:// " + Application.dataPath + "/StreamingAssets/" + name;
#elif UNITY_ANDROID
			return "jar:file://" + Application.dataPath + "!/assets/" + name;
#elif UNITY_IOS
            return "file:// " + Application.dataPath + "/Raw/" + name;
#elif UNITY_STANDALONE_WIN
			return "file:// " + Application.dataPath + "/StreamingAssets/windows" + name;
#else
            return "file:// " + Application.dataPath + "/StreamingAssets/" + name;
#endif
        }


        /// Create item in the UI
        /// </summary>
        /// <typeparam name="T">The type of item </typeparam>
        /// <param name="obj"></param>
        /// <param name="parent">The parent of item</param>
        /// <returns></returns>
        public static T CreateItem<T>( UnityEngine.Object obj, Transform parent ) where T : MonoBehaviour
        {
            GameObject item = GameObject.Instantiate( obj ) as GameObject;
            T _item = item.AddComponent<T>();

            item.transform.SetParent( parent );
            item.transform.localScale = Vector3.one;
            item.transform.localPosition = Vector3.zero;

            return _item;
        }

        /// <summary>
        /// Clear item in the UI
        /// </summary>
        /// <typeparam name="T">The type of item</typeparam>
        /// <param name="list">The list of item</param>
        public static void ClearItemList<T>( List<T> list ) where T : MonoBehaviour
        {
            for ( int i = 0; i < list.Count; i++ )
            {
                list[i].gameObject.SetActive( false );
            }
        }

        private static void DecompressAssetBundleCLZF( string input, FileStream outStream )
        {
            byte[] infile = File.ReadAllBytes( input );
            byte[] outFile = CLZF.Decompress( infile );
            outStream.Write( outFile, 0, outFile.Length );
        }

        private static void DecompressAssetBundleLZMA( FileStream inStream, FileStream outStream )
        {
            SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();

            byte[] properties = new byte[5];
            inStream.Read( properties, 0, properties.Length );

            byte[] fileLengthBytes = new byte[8];
            inStream.Read( fileLengthBytes, 0, fileLengthBytes.Length );
            long fileLength = BitConverter.ToInt64( fileLengthBytes, 0 );

            decoder.SetDecoderProperties( properties );

            decoder.Code( inStream, outStream, inStream.Length, fileLength, null );            
        }

        public static void DecompressAssetBundle( string input )
        {
            if ( !( compressionType == AssetBundleCompression.clzf ) && !( compressionType == AssetBundleCompression.lzma ) ) return;

            //Utils.DebugUtils.Log( Utils.DebugUtils.Type.Resource, "Decompress " + input );

            string output = "";
            FileStream inStream = null;

            switch ( compressionType )
            {                
                case AssetBundleCompression.clzf:
                    output = input + ".c";
                    break;
                case AssetBundleCompression.lzma:
                    output = input + ".z";
                    inStream = new FileStream( input, FileMode.Open );
                    break;
            }            
            
            FileStream outStream = new FileStream( output, FileMode.Create );

            try
            {
                float time = Time.realtimeSinceStartup;

                switch ( compressionType )
                {
                    case AssetBundleCompression.clzf:
                        DecompressAssetBundleCLZF( input, outStream );
                        break;
                    case AssetBundleCompression.lzma:
                        DecompressAssetBundleLZMA( inStream, outStream );
                        break;
                }

                outStream.Flush();
                outStream.Close();
                if (inStream != null)
                    inStream.Close();

                System.IO.File.Delete( input );
                System.IO.File.Move( output, input );

                Utils.DebugUtils.Log( DebugUtils.Type.Resource, "Decomp: " + input + " " + (Time.realtimeSinceStartup - time ) );                
            }
            catch ( System.Exception e )
            {
                outStream.Close();
                if (inStream != null)
                    inStream.Close();
                Utils.DebugUtils.Log( DebugUtils.Type.Resource, e.Message );
                System.IO.File.Delete( input );
                System.IO.File.Delete( output );
            }
        }

        /// <summary>
        /// check vector3 value is valid
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool CheckVector3Value(Vector3 v)
        {
            return CheckFloatValue( v.x ) && CheckFloatValue( v.y ) && CheckFloatValue( v.z );
        }

        /// <summary>
        /// check float value is valid
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static bool CheckFloatValue(float f)
        {
            return f > float.MinValue && f < float.MaxValue;
        }

        /// <summary>
        /// Used for string encrypt
        /// </summary>
        /// <param name="content"></param>
        /// <param name="key">base64 16</param>
        /// <returns></returns>
        public static byte[] EncryptStringToBytes( string content, string key )
        {
            byte[] keyBytes = Convert.FromBase64String( key );
            RijndaelManaged rm = new RijndaelManaged();
            rm.Key = keyBytes;
            rm.Mode = CipherMode.ECB;
            rm.Padding = PaddingMode.PKCS7;
            ICryptoTransform ict = rm.CreateEncryptor();
            byte[] contentBytes = UTF8Encoding.UTF8.GetBytes( content );
            byte[] resultBytes = ict.TransformFinalBlock( contentBytes, 0, contentBytes.Length );
            return resultBytes;
        }

        /// <summary>
        /// Used for string decrypt
        /// </summary>
        /// <param name="content"></param>
        /// <param name="key">base64 16</param>
        /// <returns></returns>
        public static string DecryptStringFromBytes( string content, string key )
        {
            byte[] keyBytes = Convert.FromBase64String( key );
            RijndaelManaged rm = new RijndaelManaged();
            rm.Key = keyBytes;
            rm.Mode = CipherMode.ECB;
            rm.Padding = PaddingMode.PKCS7;
            ICryptoTransform ict = rm.CreateDecryptor();
            byte[] contentBytes = Convert.FromBase64String( content );
            byte[] resultBytes = ict.TransformFinalBlock( contentBytes, 0, contentBytes.Length );
            return UTF8Encoding.UTF8.GetString( resultBytes );
        }

        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime( 1970, 1, 1, 0, 0, 0, 0 );
            return Convert.ToInt64( ts.TotalMilliseconds ).ToString();
        }
    }
}