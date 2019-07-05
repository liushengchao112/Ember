using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class FloatingHealthHandler : MonoBehaviour
    {

        private float floatingHealthShowTime;
        private Queue<FloatingHealth> floatingHealthQueue;
        private float deltaTime;
        private bool canShow;
        private object lockObj = new object();

        public bool CanShow
        {
            set
            {
                canShow = value;
            }
            get
            {
                return canShow;
            }
        }

        public void Init()
        {
            floatingHealthQueue = new Queue<FloatingHealth>();
            deltaTime = Time.deltaTime;
            floatingHealthShowTime = 0;
            CanShow = false;
        }

        public void Show(int dmg, bool isCrit, Transform tf, Vector3 offset, bool hasbloodBar)
        {
            lock(lockObj)
            {
                CanShow = true;
                FloatingHealth fh = HealthbarControl.Instance.GetFloatingHealth();
                fh.SetValue( dmg, isCrit, tf, offset, hasbloodBar, delegate () { HealthbarControl.Instance.RemoveFloatingHealth( fh ); } );
                floatingHealthQueue.Enqueue( fh );
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!CanShow) return;

            floatingHealthShowTime += deltaTime;

            if (floatingHealthShowTime >= 0.1f)
            {
                floatingHealthShowTime = 0;

                if (floatingHealthQueue.Count > 0)
                {
                    floatingHealthQueue.Dequeue().Show();
                }
            }
        }

        public void Dispose()
        {
            if (floatingHealthQueue != null)
            {
                for (int i = 0; i < floatingHealthQueue.Count; i++)
                {
                    FloatingHealth fh = floatingHealthQueue.Dequeue();
                    fh.Dispose();
                    HealthbarControl.Instance.RemoveFloatingHealth( fh );
                }
                floatingHealthQueue.Clear();
            }
        }
    }
}
