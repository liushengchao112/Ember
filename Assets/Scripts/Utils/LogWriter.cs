using UnityEngine;

using System;
using System.IO;
using System.Threading;
using Resource;

namespace Utils
{
    public class LogWriter
    {
        private FileStream fileStream;
        private static readonly object locker = new object();
        private string logFileName = "log_{0}.txt";
        private string logFilePath;
        private string logPath = ( Application.persistentDataPath + "/log/" );
        private Action<string> logWriter;
        private StreamWriter streamWriter;

        public LogWriter()
        {
            logPath = string.Concat( Application.persistentDataPath, "/", DownloadResource.SERVER_URL_BRANCK, "/GameResources/log/" );
            this.logFilePath = this.logPath + string.Format( this.logFileName, DateTime.Today.ToString( "yyyyMMdd" ) );

            string error;
            try
            {
                if ( !Directory.Exists( this.logPath ) )
                {
                    Directory.CreateDirectory( this.logPath );
                }

                this.logWriter = new Action<string>( this.Write );
                this.fileStream = new FileStream( this.logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite );
                this.streamWriter = new StreamWriter( this.fileStream, System.Text.Encoding.UTF8 );
                error = "success";
            }
            catch ( Exception exception )
            {
                Debug.LogError( exception.Message + ",data dir:" + Application.persistentDataPath + ",log path:" + logPath );
                error = exception.Message;
            }

            WriteLog( string.Format( "--------------------------------- start game : {0} ---------------------------------", error ) );
        }

        public void Release()
        {
            lock ( locker )
            {
                if ( this.streamWriter != null )
                {
                    this.streamWriter.Close();
                    this.streamWriter.Dispose();
                }
                if ( this.fileStream != null )
                {
                    this.fileStream.Close();
                    this.fileStream.Dispose();
                }
            }
        }

        private void Write( string msg )
        {
            object obj2;
            Monitor.Enter( obj2 = locker );
            try
            {
                if ( this.streamWriter != null )
                {
                    this.streamWriter.WriteLine( msg );
                    this.streamWriter.Flush();
                }
            }
            catch ( Exception exception )
            {
                Debug.LogError( exception.Message );
            }
            finally
            {
                Monitor.Exit( obj2 );
            }
        }

        public void WriteLog( string msg )
        {
            if ( logWriter == null ) return;

            if ( Application.platform == RuntimePlatform.IPhonePlayer )
            {
                this.logWriter( msg );
            }
            else
            {
                this.logWriter.BeginInvoke( msg, null, null );
            }
        }
    }
}
