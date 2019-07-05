using UnityEngine;
using System.Collections;

namespace Resource
{
    public interface IGLoaderItem
    {
        /// <summary>
        /// Item的下载地址
        /// </summary>
        string URL
        {
            get;
        }
	
        /// <summary>
        /// 完成加载
        /// </summary>
		void LoadCompleteHandler(System.Object content);

        // <summary>
        // 完成加载，保存本次HTTP信息
        // </summary>
        void GetLoadingDataLength( string param );

        void LoadErrorHandler();

        void GetLoadingProgress( float progress );
    }
}

