using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace UI
{
    public class UnRegularButton : EventTrigger
    {
        public Action OnUnRegularButtonClick;
        /// <summary>
        /// 多边形碰撞器
        /// </summary>
        PolygonCollider2D polygonCollider;
        //Image image;//--need read and write

        void Start()
        {
            //获取多边形碰撞器
            polygonCollider = transform.GetComponent<PolygonCollider2D>();
            //image = GetComponent<Image>();
            //image.alphaHitTestMinimumThreshold = 0.5f;
        }


        public override void OnPointerClick( PointerEventData eventData )
        {
            //对2D屏幕坐标系进行转换
            Vector2 local;
            local.x = eventData.position.x - (float)Screen.width / 2.0f;
            local.y = eventData.position.y - (float)Screen.height / 2.0f;
            if ( ContainsPoint( polygonCollider.points , local ) )
            {
                //---OnUnRegularButtonClick
                Debug.Log( "这是一个正五边形!" );
                if ( OnUnRegularButtonClick != null )
                {
                    OnUnRegularButtonClick();
                }
            }

        }

        /// <summary>
        /// 判断指定点是否在给定的任意多边形内
        /// </summary>
        bool ContainsPoint( Vector2[] polyPoints , Vector2 p )
        {
            //统计射线和多边形交叉次数
            int cn = 0;

            //遍历多边形顶点数组中的每条边
            for ( int i = 0; i < polyPoints.Length - 1; i++ )
            {
                //正常情况下这一步骤可以忽略这里是为了统一坐标系
                //polyPoints[i].x += transform.GetComponent<RectTransform>().position.x;
                //polyPoints[i].y += transform.GetComponent<RectTransform>().position.y;

                //从当前位置发射向上向下两条射线
                if ( ( ( polyPoints[i].y <= p.y ) && ( polyPoints[i + 1].y > p.y ) )
                   || ( ( polyPoints[i].y > p.y ) && ( polyPoints[i + 1].y <= p.y ) ) )
                {
                    //compute the actual edge-ray intersect x-coordinate
                    float vt = (float)( p.y - polyPoints[i].y ) / ( polyPoints[i + 1].y - polyPoints[i].y );

                    //p.x < intersect
                    if ( p.x < polyPoints[i].x + vt * ( polyPoints[i + 1].x - polyPoints[i].x ) )
                        ++cn;
                }

            }

            //实际测试发现cn为0的情况即为宣雨松算法中存在的问题
            //所以在这里进行屏蔽直接返回false这样就可以让透明区域不再响应
            if ( cn == 0 )
                return false;

            //返回true表示在多边形外部否则表示在多边形内部
            return cn % 2 != 0;
        }
    }
}