using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Data;
using Utils;
using Constants;

namespace Render
{
    public class PathRender : MonoBehaviour
    {
        public class PathPoint
        {
            public Vector3 position;
            public Transform point;

            public PathPoint( Vector3 p, Transform t)
            {
                position = p;
                point = t;
            }
        }

        private ForceMark mark;

        private Transform pathParent;
        private Transform pathRenderOrgin;
        private Transform pathPointPoolParent;

        private UnityEngine.Object pathPointOrginPoint;

        private List<Transform> pathPointPool;
        private Dictionary<long, List<Vector3>> pathDic;
        private Dictionary<long, GameObject> pathRootDic;
        private Dictionary<long, List<PathPoint>> pathPointsDic;
        private Dictionary<long, float> ownerDistanceDic ;

        public void Awake()
        {
            pathDic = new Dictionary<long, List<Vector3>>();
            pathRootDic = new Dictionary<long, GameObject>();
            pathPointsDic = new Dictionary<long, List<PathPoint>>();
            pathPointPool = new List<Transform>();
            ownerDistanceDic = new Dictionary<long, float>();

            pathRenderOrgin = transform;
            pathRenderOrgin.position = Vector3.zero;
            pathRenderOrgin.localScale = Vector3.one;

            pathPointPoolParent = new GameObject( "PathPointPool" ).transform;
            pathPointPoolParent.parent = pathRenderOrgin;
            pathPointPoolParent.gameObject.SetActive( false );

            pathParent = new GameObject( "PathParent" ).transform;
            pathParent.parent = pathRenderOrgin;

            pathPointOrginPoint = Resources.Load( "Prefabs/Scene/Path/Quad" );
        }

        public void RenderStart()
        {
            mark = DataManager.GetInstance().GetForceMark();
            DebugUtils.Log( DebugUtils.Type.PathRender, string.Format( "Init path render, force = {0}", mark ) );
        }

        public void SyncPath( int s, long ownerId, int type, List<Vector3> p )
        {
            DebugUtils.Log( DebugUtils.Type.PathRender, string.Format( "Current path point count {0}", p.Count ) );

            List<Vector3> pathPoint = p;
            if ( mark != (ForceMark)s )
            {
                DebugUtils.Log( DebugUtils.Type.PathRender, string.Format( "path force = {0} not equal with force {1}", ( ForceMark )s ,mark ) );
                return;
            }

            if ( !pathRootDic.ContainsKey( ownerId ) )
            {
                GameObject ownerPathRoot = new GameObject( string.Format( "Path_{0}", ownerId.ToString() ) );
                ownerPathRoot.transform.parent = pathParent;

                pathRootDic.Add( ownerId, ownerPathRoot );
                pathDic.Add( ownerId, pathPoint );

                ShowPath( ownerId, ownerPathRoot, pathPoint );
            }
            else
            {
                pathDic[ownerId] = pathPoint;
                GameObject go = pathRootDic[ownerId];

                ClearPathPoint( ownerId );
                ShowPath( ownerId, go, pathPoint );
            }
        }

        public void AddUnitPathPoint( long ownerId, int type, Vector3 position )
        {
            if ( !pathRootDic.ContainsKey( ownerId ) )
            {
                GameObject ownerPathRoot = new GameObject( string.Format( "Path_{0}", ownerId.ToString() ) );
                ownerPathRoot.transform.parent = pathParent;

                pathRootDic.Add( ownerId, ownerPathRoot );
                pathDic.Add( ownerId, new List<Vector3>() );
                pathDic[ownerId].Add(position);

                ShowPath( ownerId, ownerPathRoot, pathDic[ownerId] );

                DebugUtils.Log( DebugUtils.Type.PathRender, string.Format( "Create id = {0} new path", ownerId ) );
            }
            else
            {
                pathDic[ownerId].Add( position );
                GameObject go = pathRootDic[ownerId];

                ClearPathPoint( ownerId );
                ShowPath( ownerId, go, pathDic[ownerId] );

                DebugUtils.Log( DebugUtils.Type.PathRender, string.Format( "Clear and Show id = {0} exist path", ownerId ) );
            }
        }

        public void SyncPosition( long ownerId, UnitRenderType type, Vector3 position, float deltaDistance )
        {
            if( pathDic.ContainsKey(ownerId) )
            {
                CheckPathPointCurrentStatus( ownerId, position, deltaDistance );
            }
            else
            {
                // current owner doesn't have path!
            }
        }

        public void UnitDead( long ownerId, int type )
        {
            ClearPath( ownerId, type );
        }

        public void ClearPath( long ownerId, int type )
        {
            if ( pathDic.ContainsKey( ownerId ) )
            {
                pathDic[ownerId].Clear();
                pathDic.Remove( ownerId );
            }

            if ( pathPointsDic.ContainsKey( ownerId ) )
            {
                for ( int i = 0; i < pathPointsDic[ownerId].Count; i++ )
                {
                    ReturnPathPoint( pathPointsDic[ownerId][i].point );
                }

                pathPointsDic[ownerId].Clear();
                pathPointsDic.Remove( ownerId );
            }


            if ( pathRootDic.ContainsKey( ownerId ) )
            {
                GameObject.DestroyImmediate( pathRootDic[ownerId] );
                pathRootDic.Remove( ownerId );
            }
        }

        private void ShowPath( long ownerId, GameObject root, List<Vector3> corners )
        {
            List<PathPoint> points;
            if ( pathPointsDic.ContainsKey( ownerId ) )
            {
                points = pathPointsDic[ownerId];
            }
            else
            {
                points = new List<PathPoint>();
            }

            for ( int i = 0; i < corners.Count - 1; i++ )
            {
                float distance = Vector3.Distance( corners[i], corners[i + 1] );
                Vector3 direct = ( corners[i + 1] - corners[i] ).normalized;
                RenderPath( points, root, corners[i], direct, distance );
            }

            if ( !pathPointsDic.ContainsKey( ownerId ) )
            {
                pathPointsDic.Add( ownerId, points );
            }
            else
            {
                pathPointsDic[ownerId] = points;
            }
        }

        UnityEngine.AI.NavMeshHit hit;
        private void RenderPath( List<PathPoint> points, GameObject pathRoot, Vector3 orgin, Vector3 direct, float distance )
        {
            for ( int j = 0; j < ( distance - 1 ); j++ )
            {
                Vector3 targetPos = orgin + ( j + GameConstants.PATHPOINT_GAPDISTANCE ) * direct;

                GameObject go = GetNewPathPoint();
                go.transform.parent = pathRoot.transform;
                go.transform.rotation = Quaternion.FromToRotation( Vector3.forward, direct );
                go.transform.rotation = Quaternion.Euler( go.transform.rotation.eulerAngles.x, go.transform.rotation.eulerAngles.y, 0 );

                if ( UnityEngine.AI.NavMesh.SamplePosition( targetPos, out hit, 4, UnityEngine.AI.NavMesh.AllAreas ) )
                {
                    go.transform.position = hit.position + new Vector3( 0, 0.3f, 0 );
                    //Debug.Log("Hit distance = " + hit.distance + " isHit " + hit.hit );
                    go.name = points.Count.ToString();
                    points.Add( new PathPoint( hit.position, go.transform ) );
                }
            }
        }

        private void ClearPathPoint( long ownerId )
        {
            if( pathPointsDic.ContainsKey(ownerId) )
            {
                List<PathPoint> points = pathPointsDic[ownerId];

                for ( int i = points.Count - 1; i >= 0; i-- )
                {
                    ReturnPathPoint( points[i].point );
                    points.RemoveAt( i );
                }
            }

            if ( ownerDistanceDic.ContainsKey(ownerId) )
            {
                ownerDistanceDic.Remove(ownerId);
            }
        }

        private void CheckPathPointCurrentStatus( long ownerId, Vector3 ownerPosition, float deltaMoveDistance )
        {
            if( pathPointsDic.ContainsKey( ownerId ) )
            {
                if( pathPointsDic[ownerId].Count == 0 )
                {
                    return;
                }

                if ( !ownerDistanceDic.ContainsKey(ownerId) )
                {
                    ownerDistanceDic.Add( ownerId, 0 );
                }

                ownerDistanceDic[ownerId] += deltaMoveDistance;
                //DebugUtils.Log( Utils.DebugUtils.Type.PathRender, string.Format( "-----------------Begin check path! id = {0}", ownerId ) );
                List<PathPoint> points = pathPointsDic[ownerId];

                if ( ownerDistanceDic[ownerId] >= GameConstants.PATHPOINT_GAPDISTANCE )
                {
                    ReturnPathPoint( points[0].point );
                    pathDic[ownerId].Remove( points[0].position );

                    //DebugUtils.Log( Utils.DebugUtils.Type.PathRender, string.Format( "Remove path point: i = {0}, position = {1}", i, points[i].position ) );
                    points.RemoveAt( 0 );
                    ownerDistanceDic[ownerId] = 0;
                }

                //DebugUtils.Log( Utils.DebugUtils.Type.PathRender, string.Format( "-----------------End check path! id = {0}", ownerId ) );
            }
        }

        private GameObject GetNewPathPoint()
        {
            if ( pathPointPool.Count == 0 )
            {
                return GameObject.Instantiate( pathPointOrginPoint ) as GameObject; 
            }
            else
            {
                GameObject go = pathPointPool[0].gameObject;
                pathPointPool.RemoveAt(0);
                return go;
            }
        }

        private void ReturnPathPoint( Transform t )
        {
            if ( t == null )
            {
                Utils.DebugUtils.LogError( Utils.DebugUtils.Type.PathRender, "Return a null path point" );
            }

            pathPointPool.Add( t );
            t.parent = pathPointPoolParent;
        }

        protected List<Vector3> GetVector3ListFromPositions( List<Position> posList )
        {
            List<Vector3> v = new List<Vector3>();
            for ( int i = 0; i < posList.Count; i++ )
            {
                Vector3 pos = new Vector3();
                pos.x = posList[i].x;
                pos.y = posList[i].y;
                pos.z = posList[i].z;
                v.Add( pos );
            }
            return v;
        }

#if UNITY_EDITOR
        //void Update()
        //{
        //    foreach ( var item in pathPointsDic )
        //    {
        //        for ( int i = 0; i < item.Value.Count; i++ )
        //        {
        //            if ( ( item.Value.Count + 1 ) != i )
        //            {
        //                if ( i % 2 == 0 )
        //                {
        //                    Debug.DrawLine( item.Value[i].position, item.Value[i + 1].position, Color.red );
        //                }
        //                else
        //                {
        //                    Debug.DrawLine( item.Value[i].position, item.Value[i + 1].position, Color.blue);
        //                }
        //            }
        //        }
        //    }
        //}
#endif
    }
}
