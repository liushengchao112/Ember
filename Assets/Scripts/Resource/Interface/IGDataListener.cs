using UnityEngine;
using System.Collections;

namespace Resource
{
    public interface IGDataListener
    {
        void dataLoaded( string name );
        void dataError( string name );
        void DataProgress( string name );
    }
}
