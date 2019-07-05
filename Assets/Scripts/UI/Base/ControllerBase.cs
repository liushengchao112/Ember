using System;
using System.Collections;
using System.Collections.Generic;

namespace UI
{
    public class ControllerBase
    {
        protected ViewBase viewBase;

        // will be invoke when view enter at first time.
        public virtual void OnCreate()
        {

        }

        // will be invoke when view enter everytime.
        public virtual void OnResume()
        {

        }

        // will be invoke when view's gameobject be exit and view be destroyed
        public virtual void OnPause()
        {

        }

        // will be invoke when view gameobject destroyed
        public virtual void OnDestroy()
        {

        }
    }
}
