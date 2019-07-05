using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ScrollViewBase : MonoBehaviour
    {
        protected ScrollRect rect;
        private RectTransform content;

        private UnityEngine.Object cellPrefab;
        private int NUMBER_OF_COLUMNS = 1;//numberOfPerLine
        private float cellWidth = 30.0f;
        private float cellHeight = 25.0f;
        private float cellPath = 0;
		private float maskHeight;
		private float maskWidth;

        private int visibleCellsTotalCount = 0;
        private int visibleCellsRowCount = 0;
        public int VisibleCellsRowCount
        {
            get
            {
                return visibleCellsRowCount;
            }
        }
        private LinkedList<GameObject> localCellsPool = new LinkedList<GameObject>();
        private LinkedList<GameObject> cellsInUse = new LinkedList<GameObject>();

        private IList allCellsData;
        private int previousInitialIndex = 0;
        private int initialIndex = 0;
        private float initpostion = 0;
        private Vector3 contentPostion;
        private float adaptationScale = 1.0f;

        private bool horizontal
        {
            get
            {
                return rect.horizontal;
            }
        }

        private Vector3 firstCellPostion;

        private Vector3 FirstCellPosition
        {
            get
            {
                return firstCellPostion;
            }
            set
            {
                firstCellPostion = value;
            }
        }

        // must 
        public virtual void InitDataBase( ScrollRect rect , UnityEngine.Object pre , int numberOfPerLine , float cellWidth , float cellHeight , float cellPath , Vector3 firstCellPos )
        {
            if (rect==null)
            {
                return;
            }
            this.rect = rect;
            rect.onValueChanged.AddListener( UpdateScrollView );
            content = rect.content;
            cellPrefab = pre;
            NUMBER_OF_COLUMNS = numberOfPerLine;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            this.cellPath = cellPath;
            FirstCellPosition = firstCellPos;
			maskWidth =  rect.viewport.GetComponent<RectTransform>().rect.width;
			maskHeight = rect.viewport.GetComponent<RectTransform> ().rect.height;
            if ( horizontal )
            {
				visibleCellsRowCount = Mathf.CeilToInt( maskWidth / (cellWidth + cellPath ) );
            }
            else
            {
				visibleCellsRowCount = Mathf.CeilToInt( maskHeight / (cellHeight + cellPath ) );
            }

            visibleCellsTotalCount =  visibleCellsRowCount + 1;
            visibleCellsTotalCount *= NUMBER_OF_COLUMNS;
            contentPostion = content.localPosition;

            CreateCellPool();
        }

        private void CreateCellPool()
        {
            GameObject tempCell = null;
            for ( int i = 0; i < visibleCellsTotalCount; i++ )
            {
                tempCell = InstantiateCell();
                localCellsPool.AddLast( tempCell );
            }
            content.gameObject.SetActive( false );
        }

        public virtual GameObject InstantiateCell()
        {
            GameObject cellTempObject = Instantiate( cellPrefab ) as GameObject;
            cellTempObject.layer = this.gameObject.layer;
            cellTempObject.transform.SetParent( content.transform , false );
            cellTempObject.transform.localScale = Vector3.one * adaptationScale;
            cellTempObject.transform.localPosition = Vector3.zero;
            cellTempObject.transform.localRotation = Quaternion.identity;
            cellTempObject.SetActive( false );
            return cellTempObject;
        }


        void setContentSize()
        {
            int cellOneWayCount = (int)Math.Ceiling( (float)allCellsData.Count / NUMBER_OF_COLUMNS );
            if ( horizontal )
            {
                content.sizeDelta = new Vector2( cellOneWayCount * (cellWidth + cellPath ) + Mathf.Abs( Mathf.Abs( FirstCellPosition.x ) - cellWidth / 2 ) , content.sizeDelta.y );
            }
            else
            {
                content.sizeDelta = new Vector2( content.sizeDelta.x , cellOneWayCount * (cellHeight + cellPath ) + Mathf.Abs( Mathf.Abs( FirstCellPosition.y ) - cellHeight / 2 ) );
            }

        }

        private LinkedListNode<GameObject> GetCellFromPool( bool scrollingPositive )
        {
            if ( localCellsPool.Count == 0 )
            {
                return null;
            }

            LinkedListNode<GameObject> cell = localCellsPool.First;
            localCellsPool.RemoveFirst();

            if ( scrollingPositive )
            {
                cellsInUse.AddLast( cell );
            }
            else
	        {
                cellsInUse.AddFirst( cell );
            }                
            return cell;
        }

        private void PositionCell( GameObject go , int index )
        {
            int rowMod = index % NUMBER_OF_COLUMNS;

            if ( !horizontal )
            {
                go.transform.localPosition = firstCellPostion + new Vector3( cellWidth * ( rowMod ) , -( index / NUMBER_OF_COLUMNS ) * (cellHeight + cellPath ) , 0 );
            }
            else
            {
                go.transform.localPosition = firstCellPostion + new Vector3( ( index / NUMBER_OF_COLUMNS ) * (cellWidth + cellPath ) , -cellHeight * ( rowMod ) , 0 );
            }
        }
        
        /// <summary>
        /// when data changed use this
        /// </summary>
        /// <param name="cellDataList"></param>
        public void InitializeWithData( IList cellDataList )
        {
            if (this.rect==null)
            {
                return;
            }
            if ( cellsInUse.Count > 0 )
            {
                foreach ( var cell in cellsInUse )
                {
                    localCellsPool.AddLast( cell );
                }
                cellsInUse.Clear();
            }
            else
            {
                if ( horizontal )
                {
                    initpostion = content.localPosition.x;
                }
                else
                {
                    initpostion = content.localPosition.y;
                }
            }

            previousInitialIndex = 0;
            initialIndex = 0;
            content.gameObject.SetActive( true );
            LinkedListNode<GameObject> tempCell = null;
            allCellsData = cellDataList;

            setContentSize();
            firstCellPostion = FirstCellPosition;

            int currentDataIndex = 0;

            for ( int i = 0; i < visibleCellsTotalCount; i++ )
            {
                tempCell = GetCellFromPool( true );
                if ( tempCell == null || tempCell.Value == null )
                {
                    continue;
                }  
                currentDataIndex = i + initialIndex;

                PositionCell( tempCell.Value , currentDataIndex );

                tempCell.Value.SetActive( true );

                ScrollViewItemBase scrollableCell = tempCell.Value.GetComponent<ScrollViewItemBase>();
                if ( currentDataIndex < cellDataList.Count )
                {
                    scrollableCell.UpdateItemData( cellDataList[i] );
                }
                else
                {
                    scrollableCell.UpdateItemData( null );
                }
            }

            UpdateScrollView( Vector2.zero );
        }

        public virtual void UpdateScrollView(Vector2 vt)
        {
            if ( allCellsData == null )
            {
                return;
            }

            previousInitialIndex = initialIndex;

            CalculateCurrentIndex();

            InternalCellsUpdate();
        }

        public void ShowItemByDataIndex( int index )
        {
            if ( allCellsData == null || allCellsData.Count < index )
            {
                return;
            }
            if ( visibleCellsTotalCount > index )
            {
                GoTop();
                return;
            }
            if ( allCellsData.Count - index < visibleCellsTotalCount )
            {
                GoDown();
                return;
            }
            rect.StopMovement();            
            if ( !horizontal )
            {                
                content.localPosition = new Vector3( content.localPosition.x , ( cellHeight + cellPath ) * ( index / NUMBER_OF_COLUMNS - visibleCellsRowCount / 2 ) , content.localPosition.z );
            }
            else
            {
                content.localPosition = new Vector3( ( cellWidth + cellPath ) * ( index / NUMBER_OF_COLUMNS + visibleCellsRowCount / 2 ) , content.localPosition.y , content.localPosition.z );
            }
            UpdateScrollView( Vector2.zero );
        }

        public virtual void GoTop()
        {
            rect.StopMovement();
            if ( !horizontal )
            {
                content.localPosition = new Vector3( content.localPosition.x , 0 , content.localPosition.z );
            }
            else
            {
                content.localPosition = new Vector3( 0 , content.localPosition.y , content.localPosition.z );
            }
            UpdateScrollView( Vector2.zero );
        }

        public virtual void GoDown()
        {
            rect.StopMovement();
            if ( !horizontal )
            {
                content.localPosition = new Vector3( content.localPosition.x , content.sizeDelta.y , content.localPosition.z );
            }
            else
            {
                content.localPosition = new Vector3( content.sizeDelta.x , content.localPosition.y , content.localPosition.z );
            }
            UpdateScrollView( Vector2.zero );
        }

        private void CalculateCurrentIndex()
        {
            if ( !horizontal )
            {
                initialIndex = Mathf.FloorToInt( ( content.localPosition.y - initpostion ) / (cellHeight + cellPath ) );
            }
            else
            {
                initialIndex = -(int)( ( content.localPosition.x - initpostion ) / (cellWidth + cellPath ) );
            }

            int limit = Mathf.CeilToInt( (float)allCellsData.Count / (float)NUMBER_OF_COLUMNS ) - visibleCellsRowCount;

            if ( initialIndex < 0 )
            {
                initialIndex = 0;
            }                

            if ( initialIndex >= limit )
            {
                initialIndex = limit - 1;
            }                
        }

        private void InternalCellsUpdate()
        {
            if ( previousInitialIndex != initialIndex )
            {
                bool scrollingPositive = previousInitialIndex < initialIndex;
                int indexDelta = Mathf.Abs( previousInitialIndex - initialIndex );

                int deltaSign = scrollingPositive ? +1 : -1;

                for ( int i = 1; i <= indexDelta; i++ )
                {
                    this.UpdateContent( previousInitialIndex + i * deltaSign , scrollingPositive );
                }                    
            }
        }

        private void UpdateContent( int cellIndex , bool scrollingPositive )
        {
            int index = scrollingPositive ? ( ( cellIndex - 1 ) * NUMBER_OF_COLUMNS ) + ( visibleCellsTotalCount ) : ( cellIndex * NUMBER_OF_COLUMNS );            

            LinkedListNode<GameObject> tempCell = null;

            int currentDataIndex = 0;
            for ( int i = 0; i < NUMBER_OF_COLUMNS; i++ )
            {
                this.FreeCell( scrollingPositive );
                tempCell = GetCellFromPool( scrollingPositive );
                currentDataIndex = index + i;

                PositionCell( tempCell.Value , index + i );

                ScrollViewItemBase scrollableCell = tempCell.Value.GetComponent<ScrollViewItemBase>();
                if ( currentDataIndex >= 0 && currentDataIndex < allCellsData.Count )
                {
                    scrollableCell.UpdateItemData( allCellsData[currentDataIndex] );
                }
                else
                {
                    scrollableCell.UpdateItemData( null );
                }
            }
        }

        private void FreeCell( bool scrollingPositive )
        {
            LinkedListNode<GameObject> cell = null;
            // Add this GameObject to the end of the list
            if ( scrollingPositive )
            {
                cell = cellsInUse.First;
                cellsInUse.RemoveFirst();
                localCellsPool.AddLast( cell );
            }
            else
            {
                cell = cellsInUse.Last;
                cellsInUse.RemoveLast();
                localCellsPool.AddFirst( cell );
            }
        }

        public void SetContentMinimumPostion()
        {
            Vector3 pos = contentPostion;
			pos.y = pos.y + allCellsData.Count * ( (int)cellHeight  + cellPath ) - maskHeight;
			SetContentLocalPostion( pos );
        }

        public void SetContentLocalPostion( Vector3 postion )
        {
            content.localPosition = postion;
        }
    }
}