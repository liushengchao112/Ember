using UnityEngine;
using System.Collections;
using Utils;

using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

namespace UI
{
    [Flags]
    public enum UIEventState
    {
        Normal = 1,
        OnEnter = 2,
        OpenPage = 4,
        SetData = 8,
        WaitNetwork = 16,

    }

    [Flags]
    public enum UIEventGroup
    {
        None = 0,
        Top = 1,
        Left = 2,
        Middle = 4,
        Popup = 8,
    }

    public static class UILockManager
    {
        private static UIEventState canContinueState;

        private static Dictionary<UIEventGroup, UIEventState> groupStateDict;

        [RuntimeInitializeOnLoadMethod]
        static void OnRuntimeMethodLoad()
        {
            canContinueState = UIEventState.Normal;

            groupStateDict = new Dictionary<UIEventGroup, UIEventState>()
        {
            { UIEventGroup.None, UIEventState.Normal },
            { UIEventGroup.Top, UIEventState.Normal },
            { UIEventGroup.Left, UIEventState.Normal },
            { UIEventGroup.Middle, UIEventState.Normal },
            { UIEventGroup.Popup, UIEventState.Normal },
        };
        }


        public static UIEventState GetGroupState( UIEventGroup group )
        {
            return groupStateDict[group];
        }

        public static void SetGroupState( UIEventGroup group, UIEventState state )
        {
            groupStateDict[group] = state;
        }

        public static void ResetGroupState( UIEventGroup group )
        {
            groupStateDict[group] = UIEventState.Normal;
        }

        public static void AddListener( this Button button, UnityAction action, UIEventGroup thisGroup = UIEventGroup.None, UIEventGroup conflictGroup = UIEventGroup.None )
        {
            button.onClick.AddListener( delegate ()
            {
                SoundManager.Instance.PlaySound( 60120, true );
                OnButtonEvent( action, conflictGroup );
            } );

        }

        public static void AddListener( this Toggle toggle, UnityAction<bool> action, UIEventGroup thisGroup = UIEventGroup.None, UIEventGroup conflictGroup = UIEventGroup.None )
        {
            toggle.onValueChanged.AddListener( delegate ( bool isOn )
            {
                SoundManager.Instance.PlaySound( 60120, true );
                OnToggleEvent( action, isOn, conflictGroup );
            } );

        }

        private static void OnButtonEvent( UnityAction action, UIEventGroup conflictGroup )
        {
            if ( GetCanContinue( conflictGroup ) )
            {
                action();
            }
        }

        private static void OnToggleEvent( UnityAction<bool> action, bool isOn, UIEventGroup conflictGroup )
        {
            if ( isOn )
            {
                if ( GetCanContinue( conflictGroup ) )
                {
                    action( isOn );
                }
            }
            else
            {
                action( isOn );
            }
        }

        private static bool GetCanContinue( UIEventGroup conflictGroup )
        {
            bool canContinue = true;

            if ( conflictGroup != UIEventGroup.None )
            {
                foreach ( UIEventGroup item in Enum.GetValues( typeof( UIEventGroup ) ) )
                {
                    // Does it include the conflict of group 
                    // Does it include the state may continue to the state
                    if ( 0 != ( conflictGroup & item ) && 0 == ( canContinueState & groupStateDict[item] ) )
                    {
                        canContinue = false;
						DebugUtils.Log( DebugUtils.Type.UI, "Page: The click is blocked,Because the" + item + "status is " + groupStateDict[item] );
                        break;
                    }
                }
            }
            return canContinue;
        }
    }
}
