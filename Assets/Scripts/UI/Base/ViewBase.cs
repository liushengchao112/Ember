using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Utils;

namespace UI
{
    public class ViewBase : MonoBehaviour
    {
        // Enter a new view will make all already existed view out, except the view registered for blockViewList
        // And when exit current view will go back to last one.
        public static List<UIType> blockViewlist = new List<UIType>();
        private static Stack<ViewBase> _viewStack = new Stack<ViewBase>();

        public UIType uiType = UIType.None;
        public UIMenuDepth uiMenuDepth = UIMenuDepth.Background;
        public bool openState = false;
        public UITransitionMode uiTransitionMode = UITransitionMode.None;

        protected ControllerBase _controller;
        protected Animation _falledInAnimation;
        protected Animation _falledOutAnimation;

        private bool _inited = false;
        private UIType _lastViewType = UIType.None;

        public virtual void OnInit()
        {
            _inited = true;
            if ( _controller != null )
            {
                _controller.OnCreate();
            }

            DebugUtils.Log( DebugUtils.Type.UI, string.Format( " {0} has been initialized ", uiType.ToString() ) );
        }

        public virtual void OnEnter()
        {
            if (_inited == false)
            {
                OnInit();
            }

            if ( _controller != null )
            {
                _controller.OnResume();
            }

            _lastViewType = UIType.None;

            // save last menu type
            if ( _viewStack.Count > 0 )
            {
                _lastViewType = _viewStack.Peek().uiType;
            }

            EnterTransition( uiTransitionMode );

            gameObject.SetActive( true );

            if ( _falledInAnimation != null )
            {
                _falledInAnimation.Play();
            }

            openState = true;
        }

        /// <summary>
        /// Exit current view
        /// </summary>
        /// <param name="isGoBack"> true: Exit current view and return to top view, false, Just exit current view </param>
        public virtual void OnExit( bool isGoBack )
        {
            if ( _viewStack != null )
            {
                _viewStack.Pop();
                UIManager.Instance.RecycleUI( this );

                if ( isGoBack )
                {
                    ExitTransition( uiTransitionMode );
                }

                if ( this._controller != null )
                {
                    this._controller.OnPause();
                }

                openState = false;
                UILockManager.ResetGroupState( UIEventGroup.Middle );
            }
        }

        public virtual void OnDestroy()
        {
            if ( this._controller != null )
            {
                if ( openState )
                {
                    this._controller.OnPause();
                }

                this._controller.OnDestroy();
            }
        }

        // just clear the viewstack cache
        // every view will invok OnExit
        public static void ExitAllActiveView()
        {
            int count = _viewStack.Count;
            for ( int i = 0; i < count; i++ )
            {
                ViewBase v = _viewStack.Peek();
                v.OnExit( false );
            }
        }

        // just clear the viewstack cache, when UIManager enter a new scene
        public static void ClearViewStack()
        {
            int count = _viewStack.Count;
            for ( int i = 0; i < count; i++ )
            {
                ViewBase v = _viewStack.Pop();
                UIManager.Instance.RecycleUI( v );
            }            
        }

        private void EnterTransition( UITransitionMode mode )
        {
            if ( mode != UITransitionMode.None )
            {
                // Analysis the filter view need to keep
                List<UIType> filter = new List<UIType>();

                for ( int i = 0; i < blockViewlist.Count; i++ )
                {
                    if ( i == ( blockViewlist.Count - (int)mode ) )
                    {
                        break;
                    }

                    filter.Add( blockViewlist[i] );
                }

                // Exit all view that we don't need
                if ( _viewStack.Count > 0 )
                {
                    int viewStackCount = _viewStack.Count;
                    for ( int i = 0; i < viewStackCount; i++ )
                    {
                        //Debug.Log( uiType + " Exit UI type : " + _viewStack.Peek().uiType );
                        if ( !filter.Contains( _viewStack.Peek().uiType ) )
                        {
                            //Debug.Log( uiType + " Exit UI type : " + _viewStack.Peek().uiType );
                            ViewBase v = _viewStack.Peek();
                            v.OnExit( false );
                        }
                        else
                        {
                            //Debug.Log( uiType + " Didn't Exit UI type : " + _viewStack.Peek().uiType );
                            break;
                        }
                    }
                }

                // if lost some filter ui, we need to get it back
                if ( _viewStack.Count < filter.Count )
                {
                    int index = blockViewlist.IndexOf( _viewStack.Peek().uiType );
                    
                    for( int i = blockViewlist.Count - 1; i >= 0; i-- )
                    {
                        if ( i > index )
                        {
							UIManager.Instance.GetUIByType( blockViewlist[i], ( v, p ) => { v.OnEnter(); DebugUtils.Log( DebugUtils.Type.UI, string.Format( "Add  " + blockViewlist[i] )); } );
                        }
                    }
                }
            }

            if ( !_viewStack.Contains( this ) )
            {
                _viewStack.Push( this );
            }
            else
            {
                DebugUtils.LogError( DebugUtils.Type.UI, string.Format( "Already has the same ui in viewStack!! UIType = {0}", uiType ) );
            }
        }

        private void ExitTransition( UITransitionMode mode )
        {
            // Analysis the filter view need to show
            List<UIType> curfilter = new List<UIType>();

            for ( int i = blockViewlist.Count - 1; i >= 0; i-- )
            {
                curfilter.Add( blockViewlist[i] );

                if ( i == ( blockViewlist.Count - (int)mode ) )
                {
                    break;
                }
            }

            //return to filter view
            for ( int i = curfilter.Count; i > 0; i-- )
            {
                if ( curfilter[i - 1] != _viewStack.Peek().uiType )
                {
                    UIManager.Instance.GetUIByType( curfilter[i - 1], ( v, p ) => { v.OnEnter(); } );
                }
            }

            // return to the view that come from
            if ( !curfilter.Contains( _lastViewType ) && !_lastViewType.Equals( UIType.None ) )
            {
                UIManager.Instance.GetUIByType( _lastViewType, ( v, p ) => { v.OnEnter(); } );
            }
        }
    }
}