using UnityEngine;
using System.Collections.Generic;
using Data;
using Network;

namespace Utils
{


    public class GmUtilsInputValue
    {
        public string commandHead;
        public int inputCount;

        public string id;
        public string count;

        public GmUtilsInputValue( string commandHead, int inputCount, string id, string count )
        {
            this.commandHead = commandHead;
            this.inputCount = inputCount;

            this.id = id;
            this.count = count;
        }
    }

    public class GMUtils : MonoBehaviour
    {
        enum GmUtilsType
        {
            Ruby,
            Gold,
            Exp,
            ExpUnit,
            Item,
            Gear,
            Unit,
            ClearMetaData,
        }

        bool isShow = false;

        bool isBusy = false;

        GmUtilsType busyType = GmUtilsType.Exp;

        Dictionary<GmUtilsType, GmUtilsInputValue> typeInputValueDict;

        string empty = "Empty";

        void Awake()
        {
            typeInputValueDict = new Dictionary<GmUtilsType, GmUtilsInputValue>()
            {
                {GmUtilsType.Ruby, new GmUtilsInputValue( "#RUBY", 1, empty, "count" ) },
                {GmUtilsType.Gold, new GmUtilsInputValue( "#GOLD", 1, empty, "count" ) },
                {GmUtilsType.Exp, new GmUtilsInputValue( "#EXP", 1, empty, "count" ) },
                {GmUtilsType.ExpUnit, new GmUtilsInputValue( "#EXPUNIT", 2, "unitid", "count" ) },
                {GmUtilsType.Item, new GmUtilsInputValue( "#ITEM", 2, "metadataId", "count" )},
                {GmUtilsType.Gear, new GmUtilsInputValue( "#GEAR", 2, "metadataId", "count" ) },
                {GmUtilsType.Unit,  new GmUtilsInputValue( "#UNIT", 2, "unitId", "count" ) },
                {GmUtilsType.ClearMetaData,  new GmUtilsInputValue( "#CLEARMETADATA", 1, empty, "string" ) },
            };
        }

        private void Start()
        {
            MessageDispatcher.AddObserver( ConnectToGameServer, Constants.MessageType.ConnectGameServer_GM );
        }

        private void ConnectToGameServer()
        {
            Network.NetworkManager.RegisterServerMessageHandler( MsgCode.GMCommandMessage, HandleGMCommandFeedback );
        }
        
        void SendGMCommand( GmUtilsType type )
        {
            busyType = type;
            isBusy = true;

            UseGMCommondC2S useGMCommandC2s = new UseGMCommondC2S();

            string str = typeInputValueDict[type].commandHead;
            if ( typeInputValueDict[type].id != empty )
            {
                str += " " + typeInputValueDict[type].id;
            }
            if ( typeInputValueDict[type].count != empty )
            {
                str += " " + typeInputValueDict[type].count;
            }

            DebugUtils.Log( DebugUtils.Type.Data, "Gm Send:" + str );

            useGMCommandC2s.gmStr = str;
            byte[] data = ProtobufUtils.Serialize( useGMCommandC2s );
            NetworkManager.SendRequest( MsgCode.GMCommandMessage, data );
        }


        void HandleGMCommandFeedback( byte[] data )
        {
            isBusy = false;

            UseGMCommondS2C message = ProtobufUtils.Deserialize<UseGMCommondS2C>( data );

            DebugUtils.Log( DebugUtils.Type.Data, "Gm Back:" + busyType.ToString() + " " + message.result );
        }

        void OnGUI()
        {
            float inputWidth = 200;
            float buttonWidth = 140;
            float height = 70;

            float xInputDistance = 200;
            float xButtonDistance = 155;
            float yDistance = 75;

            float xStartPos = 5;
            float yStartPos = 105;

            float xCurrentPos;
            float yCurrentPos;

            if ( isShow )
            {
                GUI.Box( new Rect( 0, 0, xInputDistance * 2 + buttonWidth + xStartPos * 2, 840 ), "" );
            }

            GUIStyle gmMenuStyle = new GUIStyle();
            gmMenuStyle.normal.background = null;
            gmMenuStyle.normal.textColor = new Color( 1, 0, 0 );
            gmMenuStyle.fontSize = 40;

            if ( GUILayout.Button( "GM Menu" ) )
            {
                isShow = !isShow;
            }

            if ( isBusy )
            {
                GUI.Box( new Rect( xStartPos + buttonWidth + 2, 2, buttonWidth * 1.5f, height ), "Networking:" + busyType.ToString() );
            }

            if ( !isShow )
            {
                return;
            }


            int index = 0;
            foreach ( KeyValuePair<GmUtilsType, GmUtilsInputValue> item in typeInputValueDict )
            {
                xCurrentPos = xStartPos;
                yCurrentPos = ( yDistance * index ) + yStartPos;

                int indexX = 0;

                if ( item.Value.inputCount == 1 )
                {
                    if ( GUI.Button( new Rect( xCurrentPos, yCurrentPos, buttonWidth, height ), item.Key.ToString() ) )
                    {
                        if ( !isBusy )
                        {
                            SendGMCommand( item.Key );
                        }
                    }

                    xCurrentPos = ( xButtonDistance * indexX + buttonWidth + xStartPos + 2 );

                    if ( item.Value.id != empty )
                    {
                        item.Value.id = GUI.TextField( new Rect( xCurrentPos, yCurrentPos, inputWidth, height ), item.Value.id, 15 );

                        indexX++;
                    }

                    xCurrentPos = ( xButtonDistance * indexX + buttonWidth + xStartPos + 2 );

                    if ( item.Value.count != empty )
                    {
                        item.Value.count = GUI.TextField( new Rect( xCurrentPos, yCurrentPos, inputWidth, height ), item.Value.count, 15 );

                        indexX++;
                    }
                }
                else if ( item.Value.inputCount == 2 )
                {
                    if ( GUI.Button( new Rect( xCurrentPos, yCurrentPos, buttonWidth, height ), item.Key.ToString() ) )
                    {
                        if ( !isBusy )
                        {
                            SendGMCommand( item.Key );
                        }
                    }

                    xCurrentPos = ( xInputDistance * indexX + buttonWidth + xStartPos + 2 );

                    if ( item.Value.id != empty )
                    {
                        item.Value.id = GUI.TextField( new Rect( xCurrentPos, yCurrentPos, inputWidth, height ), item.Value.id, 15 );

                        indexX++;
                    }

                    xCurrentPos = ( xInputDistance * indexX + buttonWidth + xStartPos + 2 );

                    if ( item.Value.count != empty )
                    {
                        item.Value.count = GUI.TextField( new Rect( xCurrentPos, yCurrentPos, inputWidth, height ), item.Value.count, 15 );

                        indexX++;
                    }
                }

                index++;
            }
        }

    }
}
