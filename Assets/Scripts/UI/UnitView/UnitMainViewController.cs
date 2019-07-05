using UnityEngine;
using System.Collections;
using Data;
using System.Collections.Generic;


namespace UI
{
	public class UnitMainViewController : ControllerBase
	{
		private UnitMainView view;

		private List<UnitsProto.Unit> unitsProtos;

		private  PlayerUnits playerUnits;

		public UnitMainViewController( UnitMainView v )
		{
			view = v;
			viewBase = v;
			OnCreate ();
		}

		public override void OnCreate()
		{
			playerUnits =  DataManager.GetInstance ().GetPlayerUnits ();
			unitsProtos = DataManager.GetInstance ().unitsProtoData;
		}

		public override void OnResume()
		{
			
		}

		public override void OnPause()
		{
			
		}

		#region Send

		#endregion

		#region Handle

		#endregion

		#region  Data UI

		public int GetUnitTag(int unitId)
		{
			return unitsProtos.Find ( p => p.ID == unitId ).ProfessionType;
		}

		public UnitsProto.Unit GetUnitData(int unitId)
		{
            return unitsProtos.Find( p => p.ID == unitId );
		}

		public Dictionary<int, int> GetUnitList( int tagld, bool isHave )
		{
			Dictionary<int, int> units = new Dictionary<int, int> ();

			List<SoldierInfo> soldierInfoList =  DataManager.GetInstance ().GetPlayerUnits ().soldiers;

			for ( int i = 0; i < soldierInfoList.Count; i++ )
			{
				int unitId = soldierInfoList[ i ].metaId;
				int number = soldierInfoList[ i ].count;

				if( tagld == -1  )
				{
					units.Add ( unitId , number );
				}
				else if( tagld == 6 )
				{
                    if ( ( GetUnitTag( unitId ) ) >= 6 )
					{
						units.Add ( unitId , number );
					}
				}
				else if( GetUnitTag( unitId ) == tagld )
				{
					units.Add ( unitId , number );
				}
			}

			if( !isHave )
			{
				for ( int i = 0; i < unitsProtos.Count; i++ )
				{
					if( !units.ContainsKey( unitsProtos[i].ID ) )
					{
						if( tagld == -1 )
						{
							units.Add ( unitsProtos[ i ].ID , 0 );
						}
						else if( tagld == 6)
						{
							if ( unitsProtos[i].ProfessionType >= 6) {
								units.Add ( unitsProtos[ i ].ID , 0 );
							}
						}
						else if( tagld == unitsProtos[i].ProfessionType )
						{
							units.Add ( unitsProtos[ i ].ID , 0 );
						}
					}
				}
			}

			return units;
		}

		#endregion

	}
}