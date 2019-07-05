using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

namespace Utils
{
    public class Loom : MonoBehaviour
    {
        public struct DelayedQueueItem
        {
            public float time;
            public Action action;
        }

        public static int maxThreads = 8;
        private static int numThreads;

        private List<DelayedQueueItem> delayed = new  List<DelayedQueueItem>();  
        private List<DelayedQueueItem> currentDelayed = new List<DelayedQueueItem>();

        private List<Action> actions = new List<Action>();
        private List<Action> currentActions = new List<Action>(); 

        private static System.Object delayedLock = new System.Object();
        private static System.Object actionLock = new System.Object();

        private static bool initialized;

        private static Loom current;

        public static Loom Current
        {
            get
            {
                Initialize();
                return current;
            }
        }

        void Awake()
        {
            current = this;
            initialized = true;
        }

        static void Initialize()
        {
            if (!initialized)
            {
                if (!Application.isPlaying)
                    return;
                initialized = true;
				GameObject g = new GameObject("Loom");
                current = g.AddComponent<Loom>();
            }
        }

        public static void QueueOnMainThread(Action action)  
        {  
            if( action != null )
            {
                QueueOnMainThread( action, 0f );
            }
        }

        public static void QueueOnMainThread( Action action, float time )  
        {
            if(time != 0)  
            {  
                lock( delayedLock )  
                {  
                    if( Current != null )
                        Current.delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });  
                }  
            }  
            else  
            {  
                lock (actionLock )  
                {  
                    if( Current != null )
                        Current.actions.Add(action);  
                }  
            }  
        }

        public static Thread QueueOnWorkThread(Action action)  
        {  
            Initialize();  
            while(numThreads >= maxThreads)  
            {  
                Thread.Sleep(1);  
            }  
            Interlocked.Increment(ref numThreads);  
            ThreadPool.QueueUserWorkItem(RunAction, action);  
            return null;  
        }  

        private static void RunAction(object action)  
        {  
            try  
            {  
                ((Action)action)();  
            }  
            catch( Exception e )
            {  
                DebugUtils.LogError( DebugUtils.Type.Important, "RunAction error: " + e.ToString() );
            }  
            finally  
            {  
                Interlocked.Decrement(ref numThreads);  
            }  

        }  

        void OnDisable()  
        {  
            lock( actionLock )
            {
                lock( delayedLock )
                {
                    current = null;
                }
            }
        }

        // Update is called once per frame  
        void Update()  
        {  
            lock (actionLock)  
            {  
                currentActions.Clear();  
                currentActions.AddRange(actions);  
                actions.Clear();  
            }

            foreach(var action in currentActions)  
            {  
                action();  
            }

            currentDelayed.Clear();
            lock(delayedLock)  
            {  
                currentDelayed.AddRange(delayed.Where(d => d.time <= Time.time));
                foreach(var item in currentDelayed)  
                    delayed.Remove(item);  
            }

            foreach(var delayed in currentDelayed)
            {  
                delayed.action();
            }

        }
    }
}
