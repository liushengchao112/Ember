/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: MessageDispatcher.cs
// description:
// 
// created time： 03/24/2016
//
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;

using Constants;

namespace Utils
{
    public class MessageDispatcher
    {
        private readonly static Dictionary<MessageType, Action> messages = new Dictionary<MessageType, Action>();
        private readonly static Dictionary<MessageType, Action<object>> messages1 = new Dictionary<MessageType, Action<object>>();
        private readonly static Dictionary<MessageType, Action<object, object>> messages2 = new Dictionary<MessageType, Action<object, object>>();
        private readonly static Dictionary<MessageType, Action<object, object, object>> messages3 = new Dictionary<MessageType, Action<object, object, object>>();
        private readonly static Dictionary<MessageType, Action<object, object, object, object>> messages4 = new Dictionary<MessageType, Action<object, object, object, object>>();

        public static void AddObserver( Action observer, MessageType name )         
        {
            if ( !messages.ContainsKey(name) ) 
            {
                messages[name] = observer;
            }
            else
            {
                if ( messages[name] != null )
                {
                    Delegate[] dels = messages[name].GetInvocationList();
                    foreach ( Delegate del in dels )
                    {
                        if ( del.Equals( observer ) )
                            return;
                    }
                }

                messages[name] += observer;
            }
        }

        public static void AddObserver( Action<object> observer, MessageType name )         
        {   
            if ( !messages1.ContainsKey(name) ) 
            {
                messages1[name] = observer;
            }
            else
            {
                if ( messages1[name] != null )
                {
                    Delegate[] dels = messages1[name].GetInvocationList();
                    foreach ( Delegate del in dels )
                    {
                        if ( del.Equals( observer ) )
                            return;
                    }
                }

                messages1[name] += observer;
            }
        }

        public static void AddObserver( Action<object, object> observer, MessageType name )         
        {   
            if ( !messages2.ContainsKey(name) ) 
            {
                messages2[name] = observer;
            }
            else
            {
                if ( messages2[name] != null )
                {
                    Delegate[] dels = messages2[name].GetInvocationList();
                    foreach ( Delegate del in dels )
                    {
                        if ( del.Equals( observer ) )
                            return;
                    }
                }

                messages2[name] += observer;
            }
        }

        public static void AddObserver( Action<object, object, object> observer, MessageType name )         
        {   
            if ( !messages3.ContainsKey(name) ) 
            {
                messages3[name] = observer;
            }
            else
            {
                if ( messages3[name] != null )
                {
                    Delegate[] dels = messages3[name].GetInvocationList();
                    foreach ( Delegate del in dels )
                    {
                        if ( del.Equals( observer ) )
                            return;
                    }
                }

                messages3[name] += observer;
            }
        }

        public static void AddObserver( Action<object, object, object, object> observer, MessageType name )         
        {   
            if ( !messages4.ContainsKey(name) ) 
            {
                messages4[name] = observer;
            }
            else
            {
                if ( messages4[name] != null )
                {
                    Delegate[] dels = messages4[name].GetInvocationList();
                    foreach ( Delegate del in dels )
                    {
                        if ( del.Equals( observer ) )
                            return;
                    }
                }

                messages4[name] += observer;
            }
        }

        public static void RemoveObserver( Action observer, MessageType name ) 
        {
            if ( messages.ContainsKey( name ) )
            {
                messages[name] -= observer;

                if ( messages[name] == null )
                {
                    messages.Remove( name );
                }
            }
        }

        public static void RemoveObserver( Action<object> observer, MessageType name ) 
        {
            if ( messages1.ContainsKey( name ) )
            {
                messages1[name] -= observer;

                if ( messages1[name] == null )
                {
                    messages1.Remove( name );
                }
            }
        }

        public static void RemoveObserver( Action<object, object> observer, MessageType name ) 
        {
            if ( messages2.ContainsKey( name ) )
            {
                messages2[name] -= observer;

                if ( messages2[name] == null )
                {
                    messages2.Remove( name );
                }
            }
        }

        public static void RemoveObserver( Action<object, object, object> observer, MessageType name ) 
        {
            if ( messages3.ContainsKey( name ) )
            {
                messages3[name] -= observer;

                if ( messages3[name] == null )
                {
                    messages3.Remove( name );
                }
            }
        }

        public static void RemoveObserver( Action<object, object, object, object> observer, MessageType name ) 
        {
            if ( messages4.ContainsKey( name ) )
            {
                messages4[name] -= observer;

                if ( messages4[name] == null )
                {
                    messages4.Remove( name );
                }
            }
        }

        public static void PostMessage ( MessageType name )
        {
            if ( messages.ContainsKey( name ) )
            {
                messages[name]();
            }
        }

        public static void PostMessage ( MessageType name, object obj )
        {
            if ( messages1.ContainsKey( name ) )
            {
                messages1[name]( obj );
            }
        }

        public static void PostMessage ( MessageType name, object obj1, object obj2 )
        {
            if ( messages2.ContainsKey( name ) )
            {
                messages2[name]( obj1, obj2 );
            }
        }

        public static void PostMessage ( MessageType name, object obj1, object obj2, object obj3 )
        {
            if ( messages3.ContainsKey( name ) )
            {
                messages3[name]( obj1, obj2, obj3 );
            }
        }

        public static void PostMessage ( MessageType name, object obj1, object obj2, object obj3, object obj4 )
        {
            if ( messages4.ContainsKey( name ) )
            {
                messages4[name]( obj1, obj2, obj3, obj4 );
            }
        }

        public static void Clear()
        {
            // TODO: clear the messagex OnApplicationQuit.
        }
    }

}
