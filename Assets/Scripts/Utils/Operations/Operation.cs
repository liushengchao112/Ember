using System;

namespace Operations
{
    public abstract class Operation
    {

        public Operation()
        {
        }

        protected abstract void Bind();

        protected abstract void Unbind();

        public abstract void Start();

    }
}