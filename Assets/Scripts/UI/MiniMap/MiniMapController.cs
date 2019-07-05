using UnityEngine;
using Utils;
using Constants;
using Data;

namespace UI
{
	public class MiniMapController
    {
        private MiniMapView view;
				
		public ForceMark selfMark;
		public MatchSide selfSide;

        public MiniMapController( MiniMapView view )
        {
            this.view = view;

			selfMark = view.selfMark;
			selfSide = view.datamanager.GetMatchSide();
            
            MessageDispatcher.AddObserver( GenerateSoldierInMinimap, MessageType.GenerateSoldier );
            MessageDispatcher.AddObserver( BuildTowerInMinimap, MessageType.BuildTower );
            MessageDispatcher.AddObserver( BuildTownInMinimap, MessageType.BuildTown );
			MessageDispatcher.AddObserver( BuildInstituteMinimap, MessageType.BuildInstitute );
            MessageDispatcher.AddObserver( CreatePowerUp, MessageType.SpawnPowerUp );
			MessageDispatcher.AddObserver( GenerateSoldierInMinimap, MessageType.BuildTramCar );
			MessageDispatcher.AddObserver( GenerateSoldierInMinimap, MessageType.BuildDemolisher );

            MessageDispatcher.AddObserver( CreateEffectInMiniMap, MessageType.CreateMiniMapEffect );
			MessageDispatcher.AddObserver( DestroyEffect, MessageType.MiniMapEffectDestroy );

            MessageDispatcher.AddObserver( DestorySoldier, MessageType.SoldierDeath );
            MessageDispatcher.AddObserver( DestoryTower, MessageType.TowerDestroyed );
            MessageDispatcher.AddObserver( DestoryTown, MessageType.TownDestroy );
			MessageDispatcher.AddObserver( DestroyInstitute, MessageType.InstituteDestroyed );
            MessageDispatcher.AddObserver( DestroyPowerUp, MessageType.PowerUpDestroyed );
			MessageDispatcher.AddObserver( DestroyEngineeringVehicles, MessageType.DemolisherDestroyed );
			MessageDispatcher.AddObserver( DestroyEngineeringVehicles, MessageType.TramCarDestroyed );

            MessageDispatcher.AddObserver( SyncPositionInMinimap, MessageType.ChangeSoldierPosition );
        }

        public void Destroy()
        {
            MessageDispatcher.RemoveObserver( GenerateSoldierInMinimap, MessageType.GenerateSoldier );
            MessageDispatcher.RemoveObserver( BuildTowerInMinimap, MessageType.BuildTower );
            MessageDispatcher.RemoveObserver( BuildTownInMinimap, MessageType.BuildTown );
			MessageDispatcher.RemoveObserver( BuildInstituteMinimap, MessageType.BuildInstitute );
            MessageDispatcher.RemoveObserver( CreatePowerUp, MessageType.SpawnPowerUp );
			MessageDispatcher.RemoveObserver( GenerateSoldierInMinimap, MessageType.BuildTramCar );
			MessageDispatcher.RemoveObserver( GenerateSoldierInMinimap, MessageType.BuildDemolisher );

            MessageDispatcher.RemoveObserver( CreateEffectInMiniMap, MessageType.CreateMiniMapEffect );
			MessageDispatcher.RemoveObserver( DestroyEffect, MessageType.MiniMapEffectDestroy );

            MessageDispatcher.RemoveObserver( DestorySoldier, MessageType.SoldierDeath );
            MessageDispatcher.RemoveObserver( DestoryTower, MessageType.TowerDestroyed );
            MessageDispatcher.RemoveObserver( DestoryTown, MessageType.TownDestroy );
			MessageDispatcher.RemoveObserver( DestroyInstitute, MessageType.InstituteDestroyed );
			MessageDispatcher.RemoveObserver( DestroyPowerUp, MessageType.PowerUpDestroyed );
			MessageDispatcher.RemoveObserver( DestroyEngineeringVehicles, MessageType.DemolisherDestroyed );
			MessageDispatcher.RemoveObserver( DestroyEngineeringVehicles, MessageType.TramCarDestroyed );

            MessageDispatcher.RemoveObserver( SyncPositionInMinimap, MessageType.ChangeSoldierPosition );
        }

        #region Create Object in Minimap

		private void GenerateSoldierInMinimap( object mark, object id, object pos )
        {
            Vector3 vec3 = (Vector3)pos;
            Vector2 vec2 = new Vector2( vec3.x, vec3.z );
            long itemId = (long)id;
			view.CreateElementIcon( itemId, MiniMapView.MiniMapElementType.Soldier, ( ForceMark )mark, vec2 );
        }

        private void BuildTowerInMinimap( object mark, object id, object pos )
        {
            Vector3 vec3 = (Vector3)pos;
            Vector2 vec2 = new Vector2( vec3.x, vec3.z );
            long itemId = (long)id;
            itemId += 100000;
			view.CreateElementIcon( itemId, MiniMapView.MiniMapElementType.Tower, ( ForceMark )mark, vec2 );
        }

		private void BuildInstituteMinimap( object mark, object id,object pos ,object instituteSkills )
		{
			Vector3 vec3 = (Vector3)pos;
			Vector2 vec2 = new Vector2( vec3.x, vec3.z );
			long itemId = (long)id;
			itemId += 110000;
			view.CreateElementIcon( itemId, MiniMapView.MiniMapElementType.Institute, ( ForceMark )mark, vec2 );
		}

        private void BuildTownInMinimap( object mark, object pos )
        {
            Vector3 vec3 = (Vector3)pos;
            Vector2 vec2 = new Vector2( vec3.x, vec3.z );
			view.CreateElementIcon( -1, MiniMapView.MiniMapElementType.Town, ( ForceMark )mark, vec2 );
        }

        private void CreatePowerUp( object id, object powerUpType, object pos )
        {
            Vector3 vec3 = (Vector3)pos;
            Vector2 vec2 = new Vector2( vec3.x, vec3.z );
            long itemId = (long)id;
            itemId += 200000;
			view.CreateElementIcon( itemId, MiniMapView.MiniMapElementType.Rune, ForceMark.NoneForce, vec2 );
        }

		private void CreateEffectInMiniMap( object effectType, object id, object mark, object pos )
        {
            Vector3 vec3 = (Vector3)pos;
            Vector2 vec2 = new Vector2( vec3.x, vec3.z );
            ForceMark forceMark = (ForceMark)mark;

			DebugUtils.Log( DebugUtils.Type.MiniMap, string.Format( "Receive createEffectInMiniMap, Type is {0} ", ( MiniMapEffectType )effectType ) );

			if ( forceMark == selfMark )
            {
				view.CreateEffectIcon( ( MiniMapEffectType )effectType, ( long )id, vec2 );
            }
        }

        #endregion

        #region Destory Object in Minimap

        private void DestorySoldier( object mark, object id )
        {
            long itemId = (long)id;
            view.DestroyElementIcon( itemId );
        }

		private void DestroyEngineeringVehicles( object mark, object id, object direction )
		{
			DestorySoldier( mark, id );
		}

        private void DestoryTower( object mark, object id )
        {
            long itemId = (long)id;
            itemId += 100000;
            view.DestroyElementIcon( itemId );
        }

		private void DestroyInstitute( object mark, object id, object pos )
		{
			long itemId = (long)id;
			itemId += 110000;
			view.DestroyElementIcon( itemId );
		}

        private void DestoryTown( object id )
        {
            long itemId = (long)id;
            view.DestroyElementIcon( itemId );
        }

        private void DestroyPowerUp( object id, object powerUpType, object pos )
        {
            long itemId = (long)id;
            itemId += 200000;
            view.DestroyElementIcon( itemId );
        }

		private void DestroyEffect( object id )
		{
			view.EffectDestroy( ( long )id );
		}

        #endregion

        private void SyncPositionInMinimap( object id, object pos )
        {
            Vector3 vec3 = (Vector3)pos;
            Vector2 vec2 = new Vector2( vec3.x, vec3.z );
            view.MoveElementIcon( (long)id, vec2 );
        }
			
    }
}
