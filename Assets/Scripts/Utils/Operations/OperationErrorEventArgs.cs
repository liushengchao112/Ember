using System;
using UnityEngine;

namespace Operations
{
    public class OperationErrorEventArgs : EventArgs
    {
        public short errorCode { get; set; }

        public string Message
        {
            get { return this.errorCode.ToString(); }
        }

        public OperationErrorEventArgs() : base()
        {
        }

        public OperationErrorEventArgs(short errorCode) : base()
        {
            this.errorCode = errorCode;
        } 

    }
}