using System;

namespace Operations
{
    public class OperationSuccessEventArgs : EventArgs
    {
        public Object Result { get; set; }

        public OperationSuccessEventArgs() : base()
        {
        }

        public OperationSuccessEventArgs(Object result) : base()
        {
            this.Result = result;
        } 
    }
}
