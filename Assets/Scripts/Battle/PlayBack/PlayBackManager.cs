using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pathfinding.Ionic.Zip;

using Data;
using Utils;
using Constants;

public class PlayBackManager
{
    public static string localCacheDirectory;
    private static readonly string localCacheFileExtension = "pb";
    private static readonly string playbackFileRemoteAddress = "http://ks3-cn-beijing.ksyun.com/ember2dev01/playback/";

    public static PlayBackManager instance;

    public static PlayBackManager GetInstance()
    {
        if ( instance == null )
        {
            instance = new PlayBackManager();

            localCacheDirectory = string.Format( "{0}/{1}", Application.persistentDataPath, "localCache" );

            DebugUtils.Log( DebugUtils.Type.Playback, string.Format( "Local cache directory:{0}", localCacheDirectory ) );
        }

        if ( !Directory.Exists( localCacheDirectory ) )
        {
            Directory.CreateDirectory( localCacheDirectory );

            DebugUtils.Log( DebugUtils.Type.Playback, string.Format( "Creat local cache directory:{0}", localCacheDirectory ) );
        }

        return instance;
    }

    public void DownloadPlaybackData( long battleId )
    {
        Resource.GameResourceLoadManager.GetInstance().StartCoroutine( DownloadData( battleId ) );
    }

    IEnumerator DownloadData( long battleId )
    {
        string path = String.Format( "{0}{1}", playbackFileRemoteAddress, battleId );
        DebugUtils.Log( DebugUtils.Type.Playback, string.Format( " start download play back files, path = {0}", path ) );

        WWW www = new WWW( path );

        yield return www;

        if ( !string.IsNullOrEmpty( www.error ) )
        {
            DebugUtils.LogError( DebugUtils.Type.Playback, string.Format( "battle id {0}, error {1}", battleId, www.error ) );
        }

        DebugUtils.Log( DebugUtils.Type.Playback, string.Format( " download play back files complete, length = {0}", www.bytes.Length ) );

        ReceivePlayBackData( www.bytes, battleId );
    }

    public void ReceivePlayBackData( byte[] data, long battleId )
    {
        WriteToLocalCache( data, battleId );
    }

    public long[] GetBattleIdsFromPlaybackLocalCache()
    {
        string[] files = Directory.GetFiles( localCacheDirectory );
        DebugUtils.Log( DebugUtils.Type.Playback, string.Format( "Total local playback file count:{0}", files.Length ) );

        
        // Check localcache deadline
        for ( int i = 0; i < files.Length; i++ )
        {
            string path = files[i];
            DateTime createTime = File.GetCreationTime( path );
            if ( DateDiff( createTime, DateTime.Now ) > GameConstants.PLAYBACK_LOCALCACHE_TIMELIMIT )
            {
                if ( File.Exists( path ) )
                {
                    File.Delete( path );
                    DebugUtils.Log( DebugUtils.Type.Playback, string.Format( "local playback file:{0} already out of date, will be delected", path ) );
                }
            }
        }

        files = Directory.GetFiles( localCacheDirectory );
        long[] battleIds = new long[files.Length];

        for ( int i = 0; i < battleIds.Length; i++ )
        {
            battleIds[i] = Convert.ToInt64( Path.GetFileNameWithoutExtension( files[i] ) );

            DebugUtils.Log( DebugUtils.Type.Playback, string.Format( "local playback file:{0}", files[i] ) );
        }

        return battleIds;
    }

    public void PlayBattleBack( long battleId )
    {
        string path = string.Format( "{0}/{1}.{2}", localCacheDirectory, battleId.ToString(), localCacheFileExtension );
        if ( File.Exists( path ) )
        {
            DebugUtils.Log( DebugUtils.Type.Playback, string.Format( " begin to read play back local cache, path = {0}", path ) );

            ReadLocalCache( path );
        }
        else
        {
            DebugUtils.LogError( DebugUtils.Type.Playback, string.Format( " Can't play battle back, because find play back cache in local path, path = {0}", path ) );
        }
    }

    public void QuitPlaybattleBack()
    {
        MessageDispatcher.PostMessage( MessageType.QuitBattleRequest, false );
    }

    public void ClearPlaybackLocalCache()
    {
        string[] files = Directory.GetFiles( localCacheDirectory );
        for ( int i = 0; i < files.Length; i++ )
        {
            string filePath = files[i];
            if ( File.Exists( filePath ) && Path.GetExtension( filePath ).Equals( localCacheFileExtension ) )
            {
                File.Delete( filePath );
            }
        }
    }

    private void WriteToLocalCache( byte[] data, long battleId )
    {       
        // PlaybackS2C message = ProtobufUtils.Deserialize<PlaybackS2C>( data );

        string filePath = string.Format( "{0}/{1}.{2}", localCacheDirectory, battleId, localCacheFileExtension );
        using ( FileStream fs = File.Create( filePath ) )
        {
            fs.Write( data, 0, data.Length );
            fs.Flush();
            fs.Close();
        }

        DebugUtils.Log( DebugUtils.Type.Playback, string.Format( "Creat new playback cache files:{0}", filePath ) );
    }

    private void ReadLocalCache( string path )
    {
        // read files
        byte[] data = File.ReadAllBytes( path );

        DebugUtils.Log( DebugUtils.Type.Playback, string.Format( " Read play battle back done, zipcache length = {0}", data.Length ) );

        // unzip 
        MemoryStream memoryStream = new MemoryStream();

        using ( MemoryStream zipstream = new MemoryStream( data, 0, data.Length ) )
        {
            using ( ZipFile zip = ZipFile.Read( zipstream ) )
            {
                ICollection<ZipEntry> collection = zip.Entries;

                foreach ( var zipEntry in collection )
                {
                    memoryStream = new MemoryStream();
                    zipEntry.Extract( memoryStream );
                    memoryStream.Position = 0;
                }
            }
        }

        byte[] unzipData = memoryStream.ToArray();
        DebugUtils.Log( DebugUtils.Type.Playback, string.Format( " Unzip play battle back done, zipcache length = {0}", unzipData.Length ) );

        PlaybackS2C playbackData = ProtobufUtils.Deserialize<PlaybackS2C>( unzipData );

        DataManager clientData = DataManager.GetInstance();

        // Set Matchers
        clientData.ResetMatchers();
        List<Battler> battlers = playbackData.battlers;
        for ( int i = 0; i < battlers.Count; i++ )
        {
            Battler battler = battlers[i];

            Matcher matcher = new Matcher();
            matcher.mark = battler.forceMark;
            matcher.name = battler.name;
            matcher.playerId = battler.playerId;
            matcher.rating = battler.rating;
            matcher.side = battler.side;
            matcher.portrait = battler.portrait; // head icon

            clientData.SetMatcher( matcher );
        }

        // Set battle info
        DebugUtils.Log( DebugUtils.Type.Playback, string.Format( "playback cache data: battle id = {0}", playbackData.id ) );
        DebugUtils.Log( DebugUtils.Type.Playback, string.Format( "playback cache data: battleType = {0}", playbackData.battleType ) );
        DebugUtils.Log( DebugUtils.Type.Playback, string.Format( "playback cache data: seed = {0}", playbackData.seed ) );
        DebugUtils.Log( DebugUtils.Type.Playback, string.Format( "playback cache data: battlers count = {0}", playbackData.battlers.Count ) );
        DebugUtils.Log( DebugUtils.Type.Playback, string.Format( "playback cache data: frames count = {0}", playbackData.frames.Count ) );
        DebugUtils.Log( DebugUtils.Type.Playback, string.Format( "playback cache data: battleDuration = {0}", playbackData.battleDuration ) );

        clientData.SetBattleType( playbackData.battleType, true );
        clientData.SetSeed( playbackData.seed );
        clientData.SetBattlers( playbackData.battlers );
        clientData.SetBattleId( playbackData.id );
        clientData.SetSimulateBattleData( playbackData.frames );
        clientData.SetSimBattleDuration( playbackData.battleDuration );

        SceneManager.LoadScene( "Loading" );
    }

    private int DateDiff( DateTime DateTime1, DateTime DateTime2 )
    {
        TimeSpan ts1 = new TimeSpan( DateTime1.Ticks );
        TimeSpan ts2 = new TimeSpan( DateTime2.Ticks );

        TimeSpan ts = ts1.Subtract( ts2 ).Duration();
        return ts.Days;
    }
}
