using System;
using System.IO;
using System.Collections;
using UnityEngine;

namespace Utils
{
    public class VersionUtil
    {
        private static VersionUtil instance;

        public static VersionUtil Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new VersionUtil();
                    instance.basicVersion = new Version( Application.version );
                    instance.Initialize();
                }
                return instance;
            }
        }

        public Version basicVersion;
        public Version curVersion;

        private static readonly string VERSION_CODE_KEY = "GameVersion";

        // Check the local bin
        void Initialize()
        {
            //if ( !PlayerPrefs.HasKey( VERSION_CODE_KEY ) )
            //{
                PlayerPrefs.SetString( VERSION_CODE_KEY, basicVersion.ToString() );
            //}

            this.curVersion = new Version( PlayerPrefs.GetString( VERSION_CODE_KEY ) );
        }

        public void SetResourceVersion( int resourceVersion )
        {
            this.curVersion = new Version( curVersion.Major, curVersion.Minor, curVersion.Build, resourceVersion );
        }
    }
}
