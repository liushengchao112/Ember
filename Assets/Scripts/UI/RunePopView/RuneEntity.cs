using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class SlotInfo
    {
        public int pageid;
        public int slotid;
        public int buycost;
        public RuneSlotState state;
        public int type;
    }

    public class RuneInfo
    {
        public int runeid;
        public string nane;
        public int count;
        public int iconid;
        public int level;
        public string describer;
        public string itemattribute;
        public int sellprice_gold;
        public List<KeyValuePair<RuneCostType, int>> buyruneandprice = new List<KeyValuePair<RuneCostType, int>>();
        public int boughtNumber;
        public int boughtNumberLimit;
    }
   
}
