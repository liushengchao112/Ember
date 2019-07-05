/*----------------------------------------------------------------
// Copyright (C) 2017 Jiawen(Kevin)
//
// file name: Fsm.cs
// description: 
// 
// created time：06/06/2017
//
//----------------------------------------------------------------*/

using Utils;

namespace Logic
{
    public abstract class Fsm
    {
        public virtual void OnEnter() {}

        public virtual void Update( int deltaTime ) {}

        public virtual void OnExit() {}
    }
}
