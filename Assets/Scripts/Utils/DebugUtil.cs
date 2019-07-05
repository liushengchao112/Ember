/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: DebugUtils.cs
// description: the assistant debug class
// 
// created time：09/26/2016
//
//----------------------------------------------------------------*/

//#define DEBUG
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class DebugUtils
    {
        public static bool DebugMode = true;
        private static LogWriter logWriter;

        public enum Type
        {
            Data,
            AsyncSocket,
            Network,
            Protocol,
            UI,
			UI_SocialScreen,
            Bag,
            Map,
			MiniMap,
			Chat,

            AI_LogicWorld,
            AI_Town,
            AI_Barrack,
            AI_Tower,
            AI_Soldier,
            AI_Hero,
            AI_Projectile,
            AI_Crystal,
            AI_CrystalCar,
            AI_Demolisher,
            AI_Npc,
            AI_SkillTrigger,
            AI_Skill,
            AI_AttributeEffect,
            AI_MovableUnit,
            AI_Summon,
			AI_Trap,

            Battle,
            Match,
            PathFinding,
            InputPathPoint,
            Avoidance,
            Physics,
            Resource,
            AssetBundle,
            NetAlert,
            DownloadResource,
            Sound,
            Login,
            PathRender,
            Gesture,
            Effect,
            LocalBattleMessage,
            SimulateBattleMessage,
            Playback,
            MessageAnalyze,

            Soldier_Properties,

            Training,
			Endless,
			Tutorial,
            HeartBeatPing,
			Building,
			BuildingLevelUp,
			InstitutesSkill,

            LoadingScene,

            Special,
            Important,
        }

        private static Dictionary<Type, string> type2string = new Dictionary<Type, string>()
        {
            //{ Type.Data, "Data : " },
            //{ Type.AsyncSocket, "AsyncSocket : " },
            { Type.Network, "Network : " },
            { Type.Protocol, "Protocol : " },
            //{ Type.UI, "UI : " },
            //{ Type.UI_SocialScreen, "UI : " },
            //{ Type.Bag,"Bag：" },
            //{ Type.Map, "Map : " },
            //{ Type.MiniMap, "MiniMap : " },
            //{ Type.Chat, "Chat : " },
            //{ Type.AI_LogicWorld, "AI-logicworld : " },
            //{ Type.AI_Town, "AI-town : " },
            //{ Type.AI_Barrack, "AI-barrack : " },
            //{ Type.AI_Tower, "AI-tower : " },
            //{ Type.AI_Soldier, "AI-soldier : " },
            //{ Type.AI_Hero, "AI_Hero : " },
            //{ Type.AI_Npc, "AI-Npc : " },
            //{ Type.AI_Projectile, "AI-projectile : " },
            //{ Type.AI_Crystal, "AI_Crystal : " },
            //{ Type.AI_CrystalCar, "AI_CrystalCar : " },
            //{ Type.AI_Demolisher, "AI_Demolisher : " },
            //{ Type.AI_SkillTrigger, "AI-spell : " },
            //{ Type.AI_Skill, "AI-skill : " },
            //{ Type.AI_Buff, "Buff : " },
            //{ Type.AI_Debuff, "Debuff : " },
            //{ Type.AI_AttributeEffect, "AI_AttributeEffect : "},
            //{ Type.AI_MovableUnit, "Moveable : "},
            //{ Type.AI_Summon, "AI_Summon : "},
            //{ Type.AI_Trap, "AI_Trap : "},
            //{ Type.Battle, "Battle : " },
            //{ Type.Match, "Match : " },
            //{ Type.PathFinding, "PathFinding : " },
            //{ Type.Avoidance, "Avoidance : " },
            //{ Type.Physics, "Physics : " },
            //{ Type.Resource, "Resource : " },
            //{ Type.NetAlert, "NetAlert : " },
            //{ Type.DownloadResource, "DownloadResource : " },
            //{ Type.Sound, "Sound : " },
            //{ Type.Login, "Login: "},
            //{ Type.Gesture, "Gesture : " },
            //{ Type.Effect, "Effect : " },
            //{ Type.InputPathPoint, "InputPathPoint : "},
            //{ Type.PathRender, "PathRender" }
            //{ Type.Endless, "EndLess : " },
			{Type.Tutorial, "Tutorial : " }
            //{ Type.Training, "Training : " },
            //{ Type.HeartBeatPing, "HeartBeatPing : " },
            //{ Type.Building, "Building : "},
            //{ Type.BuildingLevelUp, "BuildingLevelUp : " },
            //{ Type.LocalBattleMessage, "LocalBattleMessage : " },
            //{ Type.InstitutesSkill, "InstitutesSkill : " },
            //{ Type.LoadingScene, "LoadingScene : " },
            //{ Type.Playback, "Playback : " },
            //{ Type.AssetBundle, "AssetBundle : " },
            //{ Type.SimulateBattleMessage, "SimulateBattle : " },
            //{Type.Soldier_Properties,"Unit properties:" },
            //{ Type.MessageAnalyze, "MessageAnalyze : " },
            //{ Type.Special, "Special : " },
            //{ Type.Important, "Important : " },
        };

        public static void Assert( bool cond, string message = "" )
        {
            if ( DebugMode && !cond )
            {
                //Logger.instance.log(message);
                // Debug.LogError( message );
                throw new Exception( message );
            }
        }

        public static void Log( Type type, string message )
        {
            if ( DebugMode && type2string.ContainsKey( type ) )
            {
                Debug.Log( string.Concat( type2string[type], message ) );
            }
        }

        public static void TestLog( bool cond, Type type, string message )
        {
            if ( DebugMode && cond )
            {
                Log( type, message );
            }
        }

        public static void JsonLog( Type type, object jsonableObject )
        {
            if ( DebugMode && type2string.ContainsKey( type ) )
            {
                Debug.Log( string.Concat( type2string[type], UnityEngine.JsonUtility.ToJson( jsonableObject, true ) ) );
            }
        }

        public static void LogWarning( Type type, string message )
        {
            if ( DebugMode && type2string.ContainsKey( type ) )
            {
                Debug.LogWarning( string.Concat( type2string[type], message ) );
            }
        }

        public static void TestLogWarning( bool cond, Type type, string message )
        {
            if ( DebugMode && cond )
            {
                LogWarning( type, message );
            }
        }

        public static void LogError( Type type, string message )
        {
            /*
            if( DebugMode && type2string.ContainsKey( type ) )
            {
                Debug.LogError( string.Concat(type2string[type] , message ));
            }
            */
            Debug.LogError( message );
            //LogOnScreen( message );
        }

        public static void TestLogError( bool cond, Type type, string message )
        {
            if ( DebugMode && cond )
            {
                LogError( type, message );
            }
        }

        /// <summary>
        /// Push the log to screen and send it to server
        /// </summary>
        /// <param name="message"></param>
        public static void LogOnScreen( string message )
        {
            if ( string.IsNullOrEmpty( message ) )
            {
                return;
            }

            DebugToScreen.PostException( message );
        }
        
        public static void Init( GameObject go )
        {
#if DEBUG
            Debug.Log( "Unity DEBUG mode is on!" );
#else
		    Debug.Log( "Unity DEBUG mode is off!" );
#endif
            if ( DebugMode )
            {
                Debug.Log( "DebugUtils' debug mode is on!" );
                DebugToScreen.Init( go );
                DebugToScreen.RegisterHandler();

                logWriter = new LogWriter();
                Application.logMessageReceived += new Application.LogCallback( ProcessExceptionReport );

                logWriter.WriteLog( "The current app version number is : " + Application.version );
                logWriter.WriteLog( "The current resource version number : " + PlayerPrefs.GetInt( Resource.DownloadResource.VERSION_CODE_KEY, 0 ) );
                logWriter.WriteLog( "The model of the device : " + SystemInfo.deviceName );
                logWriter.WriteLog( "The user defined name of the device : " + SystemInfo.deviceModel );
                logWriter.WriteLog( "Amount of video memory present : " + SystemInfo.graphicsMemorySize );
                logWriter.WriteLog( "Graphics device shader capability level : " + SystemInfo.graphicsShaderLevel );
                logWriter.WriteLog( "Amount of system memory present : " + SystemInfo.systemMemorySize );
            }
            else
            {
                Debug.Log( "DebugUtils' debug mode is off!" );
            }
        }

        public static void Release()
        {
            if ( logWriter != null )
            {
                logWriter.Release();
                logWriter = null;
            }
        }
        private static void ProcessExceptionReport( string message, string stackTrace, LogType type )
        {
            if ( !string.IsNullOrEmpty( stackTrace ) )
            {
                SystemLog( message, type, stackTrace );
            }
            else
            {
                SystemLog( message, type, Environment.StackTrace );
            }
        }

        private static void SystemLog( string message, LogType logType, string stackTrace )
        {
            message = string.Format( "[{3}]:{0}:{1}'\n'{2}", DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss,fff" ), message, stackTrace, logType );         
            logWriter.WriteLog( message );
            if ( logType == LogType.Error || logType == LogType.Exception )
            {
                LogOnScreen( message );
            }
        }

    }
}