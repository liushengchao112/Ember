using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using UI;

namespace Data
{
    public class PlayerGuide
    {
        private int currentGuideIndex = 0;

        public void SetCurrentGuideIndex( int index )
        {
            currentGuideIndex = index;
        }

        public int GetCurrentGuideIndex()
        {
            return currentGuideIndex;
        }
    }
}