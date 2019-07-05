using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;

using ProtoBuf;
using Data;
using Constants;

namespace Utils
{
    public class ProtobufUtils
    {

    	public string protoPath = "/GameResources/";

    	private static ProtobufSerializer serializer = new ProtobufSerializer ();

        /*
    	public string RootPath
        {
    		get{
    			#if UNITY_EDITOR
    			string rootPath = Application.dataPath ;
    			#else
    			string rootPath = Application.persistentDataPath;
    			#endif
    			string folder = rootPath + protoPath;
    			return folder;
    		}
    	}

    	public T LoadProtoData<T> (string name)
    	{
    		//string md5Name = CommonUtils.GenerateMD5 (Encoding.UTF8.GetBytes (name));

    		string path = RootPath + name + ".bytes";

    		Debug.Assert(File.Exists(path),"No file at path :" + path);

    		DebugUtils.Log (DebugUtils.Type.Data, "load proto file " + name + ", path = " + path);
    		byte[] data = File.ReadAllBytes (path);
    		return Deserialize<T> (data);
    	}

    	public void SavaProtoData (byte[] data, string fileName)
    	{
    		Stream sw;

    		//string md5Name = CommonUtils.GenerateMD5 (Encoding.UTF8.GetBytes (fileName));

    		string folder = RootPath ;
    		if(!Directory.Exists(folder)){
    			Directory.CreateDirectory(folder);
    		}

    		string path = folder + fileName + ".bytes";
    		DebugUtils.Log (DebugUtils.Type.Data, "path: " + path + " originName : " + fileName);
    		FileInfo f = new FileInfo (path);
    		if (f.Exists) {
    			f.Delete ();
    			f = new FileInfo (path);
    			sw = f.Create ();
    		} else {
    			sw = f.Create ();
    		}
    		sw.Write (data, 0, data.Length);
    		sw.Close ();
    		sw.Dispose ();
    	}
        */   

        /*
    	public static byte[] Serialize<T>( T dataObject )
    	{
    		DebugUtils.Assert( dataObject != null );
    		byte[] buffer = null;
            using( MemoryStream m = new MemoryStream() ) // use a cached memory stream instead?
            { 
				Serializer.Serialize<T>( m, dataObject );
    			m.Position = 0;
    			int length = (int)m.Length;
    			buffer = new byte[length];
    			m.Read( buffer, 0, length );
    		}
    		return buffer;
    	}

    	public static T Deserialize<T>( byte[] data )
    	{
    		DebugUtils.Assert( data != null && data.Length > 0 );
    		T dataObject;
    		using( MemoryStream m = new MemoryStream( data ) )
            {
				dataObject = Serializer.Deserialize<T>( m );
    		}
    		return dataObject;
    	}
        */

        public static byte[] Serialize (object dataObject)
        {
            DebugUtils.Assert (dataObject != null);
            byte[] buffer = null;
            using (MemoryStream m = new MemoryStream ()) { // use a cached memory stream instead?
                serializer.Serialize (m, dataObject);
                m.Position = 0;
                int length = (int)m.Length;
                buffer = new byte[length];
                m.Read (buffer, 0, length);
            }
            return buffer;
        }

        public static T Deserialize<T> (byte[] data)
        {
            DebugUtils.Assert (data != null && data.Length > 0);
            T dataObject;
            using (MemoryStream m = new MemoryStream (data)) {
                dataObject = (T)serializer.Deserialize (m, null, typeof(T));
            }
            return dataObject;
        }
    }
}
