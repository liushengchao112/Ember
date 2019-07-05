using UnityEngine;
using System.Collections;

namespace Data
{
    public class BagItem
    {
        private int ItemId;

        private int MetaId;

        public int ItemType;//1-Gear  2-Item

        public int Count = 0;

        public int GetItemType()
        {
            return ItemType;
        }
    }
}
