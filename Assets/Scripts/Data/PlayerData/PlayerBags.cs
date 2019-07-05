using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Network;
using Utils;

namespace Data
{
    public class PlayerBagInfo
    {
        public BagType type;//1-Complex  2-Box  3-Card  4-Rune
        public List<ItemInfo> itemList;

        public PlayerBagInfo( BagType type, List<ItemInfo> itemList )
        {
            this.type = type;
            this.itemList = itemList;
        }
    }

    public class PlayerBags
    {
        private List<PlayerBagInfo> bags;

        public PlayerBags()
        {
            bags = new List<PlayerBagInfo>();
            bags.Add( new PlayerBagInfo( BagType.BoxBag, new List<ItemInfo>() ) );
            bags.Add( new PlayerBagInfo( BagType.DebrisBag, new List<ItemInfo>() ) );
            bags.Add( new PlayerBagInfo( BagType.RuneBag, new List<ItemInfo>() ) );
            bags.Add( new PlayerBagInfo( BagType.ComplexBag, new List<ItemInfo>() ) );
        }

        public PlayerBagInfo GetBag( BagType type )
        {
            return bags.Find( p => p.type.Equals( type ) );
        }

        public void RegisterPlayerBagServerMessageHandler()
        {
            NetworkManager.RegisterServerMessageHandler( MsgCode.GainMessage, HandleGainItemFeedback );
            NetworkManager.RegisterServerMessageHandler( MsgCode.RefreshBagMessage, HandleRefreshBagFeedback );
        }

        public void RegisterPlayerBagSocialServerMessageHandler()
        {
            NetworkManager.RegisterServerMessageHandler( ServerType.SocialServer, MsgCode.GainMessage, HandleGainItemFeedback );
        }

        #region Handle bags data feedback
        private void HandleGainItemFeedback( byte[] data )
        {
            GainS2C feedback = ProtobufUtils.Deserialize<GainS2C>( data );

            if ( feedback == null )
            {
                DebugUtils.LogError( DebugUtils.Type.UI, "GainItem~~~~Feedback is null" );
                return;
            }
            if ( feedback.result )
            {
                for ( int i = 0; i < feedback.items.Count; i++ )
                {
                    PlayerBagItemType type = (PlayerBagItemType)feedback.items[i].itemType;

                    PlayerBagInfo bag_complex = GetBag( BagType.ComplexBag );
                    PlayerBagInfo bag_box = GetBag( BagType.BoxBag );
                    PlayerBagInfo bag_debris = GetBag( BagType.DebrisBag );
                    PlayerBagInfo bag_rune = GetBag( BagType.RuneBag );

                    switch ( type )
                    {
                        case PlayerBagItemType.RuneItem:
                            int id_rune = feedback.items[i].metaId;
                            ItemInfo info_rune = bag_rune.itemList.Find( p => p.metaId == id_rune );

                            if ( info_rune == null )
                                bag_rune.itemList.Add( feedback.items[i] );
                            else
                                info_rune.count += feedback.items[i].count;
                            break;
                        case PlayerBagItemType.BoxItem:
                            int id_box = feedback.items[i].metaId;
                            ItemInfo info_box = bag_box.itemList.Find( p => p.metaId == id_box );

                            if ( info_box == null )
                                bag_box.itemList.Add( feedback.items[i] );
                            else
                                info_box.count += feedback.items[i].count;
                            break;
                        case PlayerBagItemType.UnitDebrisItem:
                            int id_unitD = feedback.items[i].metaId;
                            ItemInfo info_unitD = bag_debris.itemList.Find( p => p.metaId == id_unitD );

                            if ( info_unitD == null )
                                bag_debris.itemList.Add( feedback.items[i] );
                            else
                                info_unitD.count += feedback.items[i].count;
                            break;
                        case PlayerBagItemType.SkinDebrisItem:
                            int id_skinD = feedback.items[i].metaId;
                            ItemInfo info_skinD = bag_debris.itemList.Find( p => p.metaId == id_skinD );

                            if ( info_skinD == null )
                                bag_debris.itemList.Add( feedback.items[i] );
                            else
                                info_skinD.count += feedback.items[i].count;
                            break;
                        case PlayerBagItemType.RuneDebrisItem:
                            int id_runeD = feedback.items[i].metaId;
                            ItemInfo info_runeD = bag_debris.itemList.Find( p => p.metaId == id_runeD );

                            if ( info_runeD == null )
                                bag_debris.itemList.Add( feedback.items[i] );
                            else
                                info_runeD.count += feedback.items[i].count;
                            break;
                        default:
                            int id_com = feedback.items[i].metaId;
                            ItemInfo info_com = bag_complex.itemList.Find( p => p.metaId == id_com );

                            if ( info_com == null )
                                bag_complex.itemList.Add( feedback.items[i] );
                            else
                                info_com.count += feedback.items[i].count;
                            break;
                    }
                }

                MessageDispatcher.PostMessage( Constants.MessageType.RefreshPlayerBagsData );

                if ( feedback.exps != 0 || feedback.currencies.Count > 0 || feedback.items.Count > 0 || feedback.soldiers.Count > 0 )
                    MessageDispatcher.PostMessage( Constants.MessageType.OpenGainItemWindow, feedback.exps, feedback.currencies, feedback.items, feedback.soldiers );
            }
        }

        private void HandleRefreshBagFeedback( byte[] data )
        {

            RefreshBagS2C feedback = ProtobufUtils.Deserialize<RefreshBagS2C>( data );

            if ( feedback.result )
            {
                foreach ( BagInfo bagInfo in feedback.bags )
                {
                    PlayerBagInfo bag = GetBag( (BagType)bagInfo.bagType );
                    bag.itemList = bagInfo.items;
                }

                MessageDispatcher.PostMessage( Constants.MessageType.RefreshPlayerBagsData );
            }
        }

        #endregion
    }
}