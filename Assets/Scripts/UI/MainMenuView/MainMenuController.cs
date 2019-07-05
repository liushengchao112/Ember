using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network;
using Data;
using System;
using Utils;

namespace UI
{
    public class MainMenuController : ControllerBase
    {
        private MainMenuView view;

        public MainMenuController( MainMenuView v )
        {
            viewBase = v;
            view = v;
        }

        public void RegisterHorseRaceLampMessage()
        {
            NetworkManager.RegisterServerMessageHandler(ServerType.GameServer, MsgCode.HorseRaceMessage, HandleHorseRaceMessage);
            NetworkManager.RegisterServerMessageHandler(ServerType.SocialServer, MsgCode.ForwardChatsMessage, ForwardChatsMessage);
        }             

        public void RemoveHorseRaceLampMessage()
        {
            NetworkManager.RemoveServerMessageHandler(ServerType.GameServer, MsgCode.HorseRaceMessage, HandleHorseRaceMessage);
            NetworkManager.RemoveServerMessageHandler(ServerType.SocialServer, MsgCode.ForwardChatsMessage, ForwardChatsMessage);
        }


        public void HandleHorseRaceMessage(byte[] obj)
        {    
           
            HorseLampS2C feedback = ProtobufUtils.Deserialize<HorseLampS2C>(obj);
            if (feedback!=null)
            {
                view.ShowHorseRaceLamp(new LampMessage() { count = 3, text = feedback.content, type=HorseRaceLampType.ApplictionMessage });
            }            
        }


        public void ForwardChatsMessage(byte[] obj)
        {
            ForwardChatS2C feedback = ProtobufUtils.Deserialize<ForwardChatS2C>(obj);
            if (feedback!=null)
            {
                if (feedback.chatConsumptionType==ChatConsumptionType.ChatCurrency|| feedback.chatConsumptionType == ChatConsumptionType.ChatItem)
                {
                    view.ShowHorseRaceLamp(new LampMessage() { count = 3, text = feedback.chatContent, type = HorseRaceLampType.PlayerMessage });
                }              
            }
        }


    }
}