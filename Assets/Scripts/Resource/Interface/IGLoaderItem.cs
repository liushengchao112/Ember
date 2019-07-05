using UnityEngine;
using System.Collections;

namespace Resource
{
    public interface IGLoaderItem
    {
        /// <summary>
        /// Item�����ص�ַ
        /// </summary>
        string URL
        {
            get;
        }
	
        /// <summary>
        /// ��ɼ���
        /// </summary>
		void LoadCompleteHandler(System.Object content);

        // <summary>
        // ��ɼ��أ����汾��HTTP��Ϣ
        // </summary>
        void GetLoadingDataLength( string param );

        void LoadErrorHandler();

        void GetLoadingProgress( float progress );
    }
}

