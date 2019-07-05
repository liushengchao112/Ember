/*----------------------------------------------------------------
// Copyright (C) 2016 Jiawen(Kevin)
//
// file name: NetworkRequestOperation.cs
// description: 
// 
// created time：10/12/2016
//
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Threading;

using Operations;
using Constants;
using Utils;

namespace Network
{
    /// <summary>
    /// The operation of network request
    /// </summary>
    public class NetworkRequestOperation : Operation
    {
        public Action OperationSuccess;

        public Action OperationFailed;

        public byte[] data;

        public int dataLength;

        private ManualResetEvent timeoutEvent = new ManualResetEvent( false );

        public bool finished;

        public NetworkRequestOperation() : base()
        {
        }

        /////////////////////////// Override Functions //////////////////////////////////

        protected override void Bind()
        {
            throw new NotImplementedException();
        }

        protected override void Unbind()
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            finished = false;

			StartAsync();
            //Loom.QueueOnWorkThread( this.StartAsync );
        }


        ///////////////////////////// Async Operation /////////////////////////////////
     
        void StartAsync()
        {
            //AsyncSocketClient.SendRequest( data, dataLength, this.OnRequestSuccess, this.OnRequestFailed );

            // the async wait function doesn't need for now.
            //WaitAsync();
        }

        void ContinueWait()
        {
            //Loom.QueueOnWorkThread( WaitAsync );
        }

        void WaitAsync()
        {
            if( finished )
                return;

            timeoutEvent.Reset();

            bool signalled = timeoutEvent.WaitOne( NetworkConstants.REQUEST_TIMEOUT, false );
            if( !signalled )
            {
                Loom.QueueOnMainThread(() => {
                    // It just waits for now
                    ContinueWait();
                });
            }
        }

        /////////////////////////// Operation Callback //////////////////////////////////

        public void OnRequestSuccess()
        {
            finished = true;
            timeoutEvent.Set();

            Loom.QueueOnMainThread( () => {
				if (OperationSuccess != null)
                {
	                OperationSuccess();  
				}
            } );
        }

        public void OnRequestFailed()
        {
            finished = true;
            timeoutEvent.Set();
            Loom.QueueOnMainThread( () => {
				if (OperationFailed != null)
                {
	                OperationFailed();  
				}
            } );
        }
    }
}