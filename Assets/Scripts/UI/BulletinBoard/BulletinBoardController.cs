using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Data;
using Network;
using Utils;

namespace UI
{
    public class BulletinBoardController : ControllerBase
    {
        public enum BulletinType
        {
            None,
            TextMode,
            ImageMode,
        }

        private static readonly string boardFileExtension = "txt";
        private static readonly string boardFileRemoteAddress = "http://ks3-cn-beijing.ksyun.com/ember2dev01/testPlacard/";

        private List<PlacardInfo> boardLeftDataList = new List<PlacardInfo>();

        private BulletinBoardView view;

        private List<string> boardDataList = new List<string>();

        public BulletinBoardController( BulletinBoardView view )
        {
            this.view = view;
            viewBase = view;
        }

        public override void OnResume()
        {
            base.OnResume();
            NetworkManager.RegisterServerMessageHandler( MsgCode.PlacardSendMessage, HandlePlacardFeedback );
        }

        public override void OnPause()
        {
            base.OnPause();
            NetworkManager.RemoveServerMessageHandler( MsgCode.PlacardSendMessage, HandlePlacardFeedback );
        }

        #region Send

        public void SendPlacardC2S()
        {
            PlacardC2S message = new PlacardC2S();

            byte[] data = ProtobufUtils.Serialize( message );
            NetworkManager.SendRequest( MsgCode.PlacardSendMessage, data );
        }

        #endregion

        #region Reponse Handle

        private void HandlePlacardFeedback( byte[] data )
        {
            PlacardS2C feedback = ProtobufUtils.Deserialize<PlacardS2C>( data );

            if ( feedback.placardInfo.Count > 0 )
            {
                boardDataList.Clear();
                boardLeftDataList = feedback.placardInfo;

                DownLoadBulletinBoardData( 0 );
            }
        }

        #endregion

        #region Data

        private string[] GetBulletinBoardData( int index )
        {
            if ( boardDataList.Count <= index )
            {
                DebugUtils.Log( DebugUtils.Type.UI, "Bulletin Board Data is null  " );
                return null;
            }
            return boardDataList[index].Split( '|' );
        }

        public int GetBulletinBoardCount()
        {
            return boardDataList.Count;
        }

        public string GetBulletinTitle( int index )
        {
            if ( boardDataList.Count <= index )
            {
                DebugUtils.Log( DebugUtils.Type.UI, "Bulletin Board Data is null  " );
                return "";
            }
            return GetBulletinBoardData( index )[0];
        }

        public BulletinType GetBulletinType( int index )
        {
            if ( boardDataList.Count <= index )
            {
                DebugUtils.Log( DebugUtils.Type.UI, "Bulletin Board Data is null  " );
                return BulletinType.None;
            }
            if ( GetBulletinBoardData( index )[1] == "img" )
                return BulletinType.ImageMode;
            else if ( GetBulletinBoardData( index )[1] == "content" )
                return BulletinType.TextMode;
            else
                return BulletinType.None;
        }

        public string GetBulletinContent( int index )
        {
            if ( boardDataList.Count <= index )
            {
                DebugUtils.Log( DebugUtils.Type.UI, "Bulletin Board Data is null  " );
                return "";
            }
            return GetBulletinBoardData( index )[2];
        }

        #endregion

        #region DownLoadData

        public void DownLoadBulletinBoardData( int index )
        {
            Resource.GameResourceLoadManager.GetInstance().StartCoroutine( DownLoadData( index ) );
        }

        IEnumerator DownLoadData( int index )
        {
            int bulletinId = boardLeftDataList[index].ID;

            string path = string.Format( "{0}{1}.{2}", boardFileRemoteAddress, bulletinId, boardFileExtension );

            WWW www = new WWW( path );

            yield return www;

            if ( !string.IsNullOrEmpty( www.error ) )
            {
                Debug.LogWarning( string.Format( "bulletin Id is {0},error is {1}", bulletinId, www.error ) );
            }

            string boardDataString = System.Text.Encoding.Default.GetString( www.bytes );

            boardDataList.Add( boardDataString );
            
            if ( ( index + 1 ) == boardLeftDataList.Count )
                view.RefreshBulletinBoardLeftItem();

            if ( ( index + 1 ) < boardLeftDataList.Count )
                DownLoadBulletinBoardData( ++index );
        }

        #endregion

    }
}
