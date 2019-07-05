using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace Render
{
    public class CullingGroupManager
    {
        public static bool disableCullAnimation = false;
        public static bool disableCullMesh = false;
        public static bool disableCullSkin = false;
        public static bool disableCullPraticle = false;
        
        private CullingGroup cullingGroup;
        private BoundingSphere[] boundingSpheres;

        private Dictionary<int, UnitRender> boundingSpheresDic;
        private List<int> usedIndexPool;
        private List<int> unusedIndexPool;

        private int groupCount;
        private int boundingSpheresIndex;
        private float modelRadius = 0.4f; // Temp data

        public CullingGroupManager()
        {
            // Temp data
            groupCount = 500;

            boundingSpheresIndex = 0;
            boundingSpheres = new BoundingSphere[groupCount]; // Temp count

            cullingGroup = new CullingGroup();
            cullingGroup.SetBoundingSpheres( boundingSpheres );
            cullingGroup.SetBoundingSphereCount( groupCount );
            cullingGroup.onStateChanged = HandleCullingStateChange;
            cullingGroup.targetCamera = Camera.main;

            boundingSpheresDic = new Dictionary<int, UnitRender>();
            usedIndexPool = new List<int>();
            unusedIndexPool = new List<int>();

            for ( int i = 0; i < groupCount; i++ )
            {
                unusedIndexPool.Add( i );
            }

            SetCullMode();
        }

        private void SetCullMode()
        {
            cullingGroup.enabled = true;
            disableCullAnimation = false;
            disableCullMesh = false;
            disableCullSkin = false;
            disableCullPraticle = false;
        }

        public void AddUnitRender( UnitRender render )
        {
            if ( unusedIndexPool.Count > 0 )
            {
                int index = unusedIndexPool[0];

                DebugUtils.Assert( boundingSpheres.Length > index, "Culling Group: boundSpheresIndex out of length " + render.ToString() );

                BoundingSphere sphere = boundingSpheres[index];
                sphere.position = render.transform.position;
                sphere.radius = modelRadius;
                boundingSpheres[index] = sphere;

                bool unitIsVisible = cullingGroup.IsVisible( index );
                render.boundSpheresIndex = index;
                render.OnCullingStateChange( unitIsVisible );

                boundingSpheresDic.Add( index, render );
                usedIndexPool.Add( index );
                unusedIndexPool.RemoveAt( 0 );
            }
            else
            {
                DebugUtils.Assert( false, "Culling Group: unusedIndexPool count is not enough, count = " + groupCount );
            }
        }

        public void SyncPosition( UnitRender render, Vector3 position )
        {
            int index = render.boundSpheresIndex;

            DebugUtils.Assert( boundingSpheres.Length > index, "Culling Group: boundSpheresIndex out of length " + render.ToString() );

            boundingSpheres[index].position = position;
        }

        public void RemoveUnitRender( UnitRender ur )
        {
            if ( usedIndexPool.Remove( ur.boundSpheresIndex ) )
            {
                unusedIndexPool.Add( ur.boundSpheresIndex );
                boundingSpheresDic.Remove( ur.boundSpheresIndex );
            }
            else
            {
                DebugUtils.Assert( false, "Culling Group: can't recycle a unit render " + ur.ToString() );
            }
        }

        public void HandleCullingStateChange( CullingGroupEvent evt )
        {
            int index = evt.index;
            if ( boundingSpheresDic.ContainsKey( index ) )
            {
                boundingSpheresDic[index].OnCullingStateChange( evt.isVisible );
            }
        }

        public void Release()
        {
            cullingGroup.Dispose();
            cullingGroup = null;

            boundingSpheres = null;
            boundingSpheresDic.Clear();
            usedIndexPool.Clear();
            unusedIndexPool.Clear();
        }
    }
}

