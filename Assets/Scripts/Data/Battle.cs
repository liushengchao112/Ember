using UnityEngine;
using System.Collections.Generic;

namespace Data
{
    public class Battle
    {
        public long id;

        public bool simulateBattle;

        public BattleType battleType;

        public MatchSide side;

        public ForceMark forceMark;

        public List<Battler> battlers;

        public Dictionary<ForceMark , BattleNumData> dataList;

        public long seed;

        public long frame;

        public int killUnitCount;

        public float mvpValue;

        public int fatality;

        public int resources;

		public int institeLv;

        // TODO: Need to save on local cache
        public float cameraHeight;

        public int pveWaveNumber;

        public bool pveIsVictory;

        public System.DateTime startTime;

        public List<Frame> frames;

        public long battleDuration; // ms

        public Battle()
        {
            battlers = new List<Battler>();
            dataList = new Dictionary<ForceMark , BattleNumData>();

            battleType = BattleType.NoBattle;
            side = MatchSide.NoSide;
            forceMark = ForceMark.NoneForce;
            id = 0;
            frame = 0;
            cameraHeight = 16.5f;
			institeLv = 0;
            battleDuration = 0;
        }

        public void Reset()
        {
            battlers.Clear();
            dataList.Clear();

            if ( frames != null )
            {
                frames.Clear();
            }

            simulateBattle = false;

            battleType = BattleType.NoBattle;
            side = MatchSide.NoSide;
            forceMark = ForceMark.NoneForce;
            id = 0;
            frame = 0;

            killUnitCount = 0;
            fatality = 0;
            mvpValue = 0;
            resources = 0;
			institeLv = 0;

            pveWaveNumber = 0;
            pveIsVictory = false;

            simulateBattle = false;
            battleDuration = 0;
        }

        public class BattleNumData
        {
            public ForceMark mark;
            public int unitKillCount;
            public int unitFatality;
            public int unitResources;
        }

        public void AddUnitKillCount( ForceMark mark )
        {
            if ( dataList.ContainsKey( mark ) )
            {
                dataList[mark].unitKillCount++;
            }
            else
            {
                BattleNumData data = new BattleNumData();
                data.mark = mark;
                data.unitKillCount = 1;
                dataList.Add( mark , data );
            }
        }

        public int GetUnitKillCount( ForceMark mark )
        {
            if ( dataList.ContainsKey( mark ) )
            {
                return dataList[mark].unitKillCount;
            }
            return 0;
        }

        public void AddUnitFatality( ForceMark mark )
        {
            if ( dataList.ContainsKey( mark ) )
            {
                dataList[mark].unitFatality++;
            }
            else
            {
                BattleNumData data = new BattleNumData();
                data.mark = mark;
                data.unitFatality = 1;
                dataList.Add( mark , data );
            }
        }

        public int GetUnitFatality( ForceMark mark )
        {
            if ( dataList.ContainsKey( mark ) )
            {
                return dataList[mark].unitFatality;
            }
            return 0;
        }

        public void AddUnitResources( ForceMark mark , int emberCount )
        {
            if ( dataList.ContainsKey( mark ) )
            {
                dataList[mark].unitResources += emberCount;
            }
            else
            {
                BattleNumData data = new BattleNumData();
                data.mark = mark;
                data.unitResources = emberCount;
                dataList.Add( mark , data );
            }
        }

        public int GetUnitResources( ForceMark mark )
        {
            if ( dataList.ContainsKey( mark ) )
            {
                return dataList[mark].unitResources;
            }
            return 0;
        }
    }

    public class SquadData
    {
        public int index;
        public UnitsProto.Unit protoData;

        public SquadData( UnitsProto.Unit protoData, int index )
        {
            this.protoData = protoData;
            this.index = index;
        }
    }
}
