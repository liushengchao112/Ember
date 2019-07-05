using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

using Data;
using Constants;
using Utils;
using Resource;
using Map;

namespace UI
{
	public class MiniMapView : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
		public enum MiniMapElementType
		{
			Soldier,
			Tower,
			Institute,
			Town,
			Rune,//Power up item
		}
			
        public MiniMapController controller;
        private Transform elementIconParent;

        public Dictionary<long, Transform> elementIconDic;
		public Dictionary<long, MiniMapEffect> effectElementDic;
		private Image miniMapBg;

        [HideInInspector]
        public Vector3 mapSize;//3D Map size

        private Vector2 mapOffsetPosition = new Vector2( 0, 3 );

        [HideInInspector]
        public Vector2 miniMapSize;//UI image size

		private Color32 selfForceColor = new Color32( 255, 255, 255, 255 );
        private Color32 friendForceColor = new Color32( 0, 0, 255, 255 );
        private Color32 enemyForceColor = new Color32( 255, 0, 0, 255 );
        private Color32 neutralForceColor = new Color32( 0, 255, 0, 255 );

		public DataManager datamanager;
		public BattleType battleType;
		public ForceMark selfMark;

		#region MiniMapValue

		private GameResourceLoadManager resourceLoad;

		private RectTransform miniMapRect;
		private GameObject point;
		private Vector3 cameraOffset;

		public float miniMapWidthRatio = 0;
		public float miniMapHeightRatio = 0;

		private bool isNeedFit;
		private float minimapRatio;
		private float realMapRatio;
		private float mapProportionalDifference;

		private Vector2 miniMapWidthLimit = Vector2.zero;
		private Vector2 miniMapHeightLimit = Vector2.zero;

		private uint soldierElementCount = 0;
		private uint towerElementCount = 0;
		private uint instituteElementCount = 0;
		private uint runeElementCount = 0;
		private uint effectItemCount = 0;

		#endregion

		#region ElementPool

		private List<Image> soldierIconPool;
		private List<Image> towerIconPool;
		private List<Image> instituteIconPool;
		private List<Image> runeIconPool;
		private List<MiniMapEffect> effectItemPool;

		#endregion

        void Awake()
        {
            MessageDispatcher.AddObserver( Init, MessageType.InitMiniMap );
        }

        void OnDestroy()
        {
            MessageDispatcher.RemoveObserver( Init, MessageType.InitMiniMap );

			if( controller != null )
			{
				controller.Destroy();
			}
        }

		#region Initialization

		private void Init( object mapData )
        {
			datamanager = DataManager.GetInstance();
			battleType = datamanager.GetBattleType();
			selfMark = datamanager.GetForceMark();

            miniMapBg = transform.Find( "Mask/Bg" ).GetComponent<Image>();
			point = transform.Find( "Mask/Bg/Point" ).gameObject;

			if ( battleType == BattleType.BattleP2vsP2)
            {
				GameResourceLoadManager.GetInstance().LoadAtlasSprite( GameConstants.MINIMAP_2V2ID, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
                {
                    miniMapBg.SetSprite( atlasSprite );
                }, true );

                MapData2V2 map2v2 = ( MapData2V2 )mapData;
				mapSize = map2v2.mapSize;

				miniMapWidthLimit = map2v2.GetCameraWidthRange();
				miniMapHeightLimit = map2v2.GetCameraHeightRange();
            }
			else if( battleType == BattleType.BattleP1vsP1 || battleType == BattleType.Tutorial )
			{
				GameResourceLoadManager.GetInstance().LoadAtlasSprite( GameConstants.MINIMAP_1V1ID, delegate ( string name, AtlasSprite atlasSprite, System.Object param )
				{
					miniMapBg.SetSprite( atlasSprite );
				}, true );

				MapData1V1 map1v1 = ( MapData1V1 )mapData;
				mapSize = map1v1.mapSize;

				miniMapWidthLimit = map1v1.GetCameraWidthRange();
				miniMapHeightLimit = map1v1.GetCameraHeightRange();
			}
            else
            {
                GameResourceLoadManager.GetInstance().LoadAtlasSprite( "MapPVE", delegate ( string name, AtlasSprite atlasSprite, System.Object param )
                {
                    miniMapBg.SetSprite( atlasSprite );
                }, true );

                //When we have more map need change.
                MapDataPVE mapPvE = ( MapDataPVE )mapData;
				mapSize = mapPvE.mapSize;

				miniMapWidthLimit = mapPvE.GetCameraWidthRange();
				miniMapHeightLimit = mapPvE.GetCameraHeightRange();
            }

			miniMapRect = miniMapBg.GetComponent<RectTransform>();

			miniMapSize = miniMapRect.sizeDelta;
			minimapRatio = miniMapSize.x / miniMapSize.y;
			realMapRatio = mapSize.x / mapSize.z;

			if( realMapRatio > minimapRatio )
			{
				isNeedFit = true;
				mapProportionalDifference = realMapRatio - minimapRatio;
				
			}

			miniMapWidthRatio = ( miniMapSize.x / mapSize.x );
			miniMapHeightRatio = ( miniMapSize.y / mapSize.z );

			elementIconDic = new Dictionary<long, Transform>();
			elementIconParent = transform.Find( "Mask/ElementParent" ).transform;

			cameraOffset = Camera.main.transform.position;

			InitMiniMapElement( battleType );

			controller = new MiniMapController( this );

			DebugUtils.Log( DebugUtils.Type.MiniMap, "The MiniMapView initialization Complete." );
        }

		#endregion

		#region ElementIcon functions

		public void CreateElementIcon( long id, MiniMapElementType type, ForceMark mark, Vector2 pos )
        {
			if( this.gameObject.activeInHierarchy )
			{
				Image icon = TakeElementFromPool( type );

				SetIconColor( icon, mark );
				SetElementIconPosition( icon.transform, pos );
				icon.gameObject.SetActive( true );

				if( id != -1 )
				{
					elementIconDic.Add( id, icon.transform );
				}
			}
        }

		public void CreateEffectIcon( MiniMapEffectType type, long id, Vector2 pos )
		{
			if( this.gameObject.activeInHierarchy )
			{
				MiniMapEffect effect = TakeEffectItemFromPool( type, id );
				if( effect != null )
				{
					SetElementIconPosition( effect.transform, pos );

					if( type == MiniMapEffectType.Warning )
					{
						MiniMapEffect_Warning warning = ( MiniMapEffect_Warning )effect;
						warning.EffectStart();
					}
					//TODO:If have more minimapEffectType add here.
					//else if( type == MiniMapEffectType.None )
					//{
						
					//}
				}
				else
				{
					DebugUtils.Log( DebugUtils.Type.MiniMap, "CreateEffectIcon is null" );
				}
			}
		}
			
		//here recycling elementIcon pool.  
        public void DestroyElementIcon( long id )
        {
            if ( elementIconDic.ContainsKey( id ) )
            {
				elementIconDic[id].gameObject.SetActive( false );
                elementIconDic.Remove( id );
            }
        }
			
        public void MoveElementIcon( long id, Vector2 vec )
        {
            if ( elementIconDic.ContainsKey( id ) )
            {
                SetElementIconPosition( elementIconDic[id], vec );
            }
        }

        private void SetElementIconPosition( Transform elementIcon, Vector2 vec )
        {
			if( IsNeedMirror() )
			{
				vec.x = -vec.x;
				miniMapBg.rectTransform.rotation = Quaternion.Euler( 0, 180, 0 );
			}
				
			if( isNeedFit || battleType == BattleType.BattleP1vsP1 || battleType == BattleType.BattleP2vsP2 )
			{
				//2v2 1v1 Not need offesetPosition, all data is confirm;
				elementIcon.localPosition = new Vector2 ( ( vec.x * miniMapWidthRatio ) * ( 1 - mapProportionalDifference ), vec.y * miniMapHeightRatio );
			}
			else
			{
				//Just for PvE
				vec = vec + mapOffsetPosition;
				elementIcon.localPosition = new Vector2( vec.x * miniMapWidthRatio, vec.y * miniMapHeightRatio );
			}
        }
			
		//Abuot player forceMark change icon color.
        private void SetIconColor( Image image, ForceMark mark )
        {
            Color32 color = new Color32();

			if( controller.selfSide == MatchSide.Red )
			{
				if ( mark == controller.selfMark )
				{
					color = selfForceColor;
				}
				else if( mark == ForceMark.TopRedForce || mark == ForceMark.BottomRedForce )
				{
					color = friendForceColor;
				}
				else if( mark == ForceMark.NoneForce )
				{
					color = neutralForceColor;
				}
				else
				{
					color = enemyForceColor;
				}
			}
			else if( controller.selfSide == MatchSide.Blue )
			{
				if ( mark == controller.selfMark )
				{
					color = selfForceColor;
				}
				else if( mark == ForceMark.TopBlueForce || mark == ForceMark.BottomBlueForce )
				{
					color = friendForceColor;
				}
				else if( mark == ForceMark.NoneForce )
				{
					color = neutralForceColor;
				}
				else
				{
					color = enemyForceColor;
				}
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.MiniMap, "Can't know player MatchSide!" );
			}
				
            image.color = color;
        }

		#endregion

		#region CameraControl functions

		//Move camera abuot player touched mimimap image point.
		//Careful,This eventData.position is input point abuot canvas of UGUI.Not world position.
		public void OnPointerDown( PointerEventData eventData )
		{
			DebugUtils.Log( DebugUtils.Type.MiniMap, string.Format( "EventData pos is X {0} Y {1}", eventData.position.x, eventData.position.y ));

			point.transform.position = eventData.position;

			SetCameraPosition( point.transform.localPosition );
		}

		public void OnDrag( PointerEventData eventData )
		{
			point.transform.position = eventData.position;

			SetCameraPosition( point.transform.localPosition );
		}

		Tweener tweener;
		private void SetCameraPosition( Vector2 vec )
		{
			float posX = vec.x / miniMapWidthRatio;
			float posZ = vec.y / miniMapHeightRatio;

			if( isNeedFit )
			{
				posX = posX * ( 1 + mapProportionalDifference );
			}

			if( posX < miniMapWidthLimit.x )
			{
				posX = miniMapWidthLimit.x;
			}

			if( posX > miniMapWidthLimit.y )
			{
				posX = miniMapWidthLimit.y;
			}

			//Just for player visual effect.
			if( battleType == BattleType.BattleP2vsP2 || battleType == BattleType.BattleP1vsP1 )
			{
				posZ -= 7;
			}
			else
			{
				posZ -= 5;
			}

			if( posZ < miniMapHeightLimit.x )
			{
				posZ = miniMapHeightLimit.x;
			}

			if( posZ > miniMapHeightLimit.y )
			{
				posZ = miniMapHeightLimit.y;
			}

			Vector3 targetPosition = new Vector3 ( posX, cameraOffset.y, posZ );

			if ( tweener != null && tweener.IsPlaying() )
			{
				tweener.Kill( false );
			}

			tweener = DOTween.To( () => Camera.main.transform.position, value => Camera.main.transform.position = value, targetPosition, 0.5f ).SetEase( Ease.OutQuart );
		}

		//Check is need mirror about ForceMark.
		private bool IsNeedMirror()
		{
			if( selfMark == ForceMark.TopRedForce || selfMark == ForceMark.BottomRedForce )
			{
				return true;
			}

			return false;
		}

		#endregion

		#region ResourcePool functions

		//Default minimap icon element pool,If need more chache can modify numbers.
		private void InitMiniMapElement( BattleType type )
		{
			resourceLoad = GameResourceLoadManager.GetInstance();

			if( type == BattleType.BattleP2vsP2 )
			{
				FillSoldierElementIconPool( 32 );
				FillTowerElementIconPool( 20 );
				FillInstituteElementIconPool( 4 );
				FillEffectItemPool( 8 );
				FillRuneElementIconPool( 2 );
			}
			else if( type == BattleType.BattleP1vsP1 || type == BattleType.Tranining || type == BattleType.Tutorial )
			{
				FillSoldierElementIconPool( 16 );
				FillTowerElementIconPool( 10 );
				FillInstituteElementIconPool( 2 );
				FillEffectItemPool( 4 );
				FillRuneElementIconPool( 2 );
			}
			else if( type == BattleType.Survival )
			{
				FillSoldierElementIconPool( 20 );
				FillTowerElementIconPool( 5 );
				FillInstituteElementIconPool( 1 );
				FillEffectItemPool( 4 );
				FillRuneElementIconPool( 2 );
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.MiniMap, string.Format( "Can't find this BattleType : {0}", type ));
			}
		}

		private void FillSoldierElementIconPool( uint value )
		{
			if( soldierIconPool == null )
			{
				soldierIconPool = new List<Image>();
			}

			for( int i = 0; i < value; i++ )
			{
                //When finished the prefab id, use id.
                GameObject temp = GameObject.Instantiate( resourceLoad.LoadAsset<GameObject>( "Soldier" ) );
				temp.SetActive( false );
				soldierElementCount++;
				temp.name = string.Format( "SoldierElementIcon{0}", soldierElementCount );
				temp.transform.SetParent( elementIconParent, false );
				soldierIconPool.Add( temp.GetComponent<Image>() );
			}
		}

		private void FillTowerElementIconPool( uint value )
		{
			if( towerIconPool == null )
			{
				towerIconPool = new List<Image>();
			}
	
			for( int i = 0; i < value; i++ )
			{
                //TODO:When finished the prefab id, use id.
                GameObject temp = GameObject.Instantiate( resourceLoad.LoadAsset<GameObject>( "Tower" ) );
				temp.SetActive( false );
				towerElementCount++;
				temp.name = string.Format( "TowerElementIcon{0}", towerElementCount );
				temp.transform.SetParent( elementIconParent, false );
				towerIconPool.Add( temp.GetComponent<Image>() );
			}
		}

		private void FillInstituteElementIconPool( uint value )
		{
			if( instituteIconPool == null )
			{
				instituteIconPool = new List<Image>();	
			}

			for( int i = 0; i < value; i++ )
			{
                //TODO:When finished the prefab id, use id.
                GameObject temp = GameObject.Instantiate( resourceLoad.LoadAsset<GameObject>( "Institute" ) );
				temp.SetActive( false );
				instituteElementCount++;
				temp.name = string.Format( "InstituteElementIcon{0}", instituteElementCount );
				temp.transform.SetParent( elementIconParent, false );
				instituteIconPool.Add( temp.GetComponent<Image>() );
			}
		}

		private void FillRuneElementIconPool( uint value )
		{
			if( runeIconPool == null )
			{
				runeIconPool = new List<Image>();	
			}

			for( int i = 0; i < value; i++ )
			{
                //TODO:When finished the prefab id, use id.rune not have self icon, used institute.
                GameObject temp = GameObject.Instantiate( resourceLoad.LoadAsset<GameObject>( "Institute" ) );
				temp.SetActive( false );
				runeElementCount++;
				temp.name = string.Format( "RuneElementIcon{0}", runeElementCount );
				temp.transform.SetParent( elementIconParent, false );
				runeIconPool.Add( temp.GetComponent<Image>() );
			}
		}

		private void FillEffectItemPool( uint value )
		{
			if( effectItemPool == null )
			{
				effectItemPool = new List<MiniMapEffect>();
			}
				
			for( int i = 0; i < value; i++ )
			{
                //TODO:When finished the prefab id, use id.
                GameObject temp = GameObject.Instantiate( resourceLoad.LoadAsset<GameObject>( "Warning" ) );
				temp.GetComponent<MiniMapEffect_Warning>().Init();
				temp.SetActive( false );
				effectItemCount++;
				temp.name = string.Format( "EffectItem{0}", effectItemCount );
				temp.transform.SetParent( elementIconParent, false );
				effectItemPool.Add( temp.GetComponent<MiniMapEffect>() );
			}
		}
			
		#endregion

		#region TakeElement functions

		private Image TakeElementFromPool( MiniMapElementType type )
		{
			Image temp = null;

			if( type == MiniMapElementType.Soldier )
			{
				while( temp == null )
				{
					for( int i = 0; i < soldierIconPool.Count; i++ )
					{
						if( !soldierIconPool[i].gameObject.activeInHierarchy )
						{
							temp = soldierIconPool[i];
							return temp;
						}
					}

					FillSoldierElementIconPool( 6 );
				}
			}
			else if( type == MiniMapElementType.Tower )
			{
				while( temp == null )
				{
					for( int i = 0; i < towerIconPool.Count; i++ )
					{
						if( !towerIconPool[i].gameObject.activeInHierarchy )
						{
							temp = towerIconPool[i];
							return temp;
						}
					}

					FillTowerElementIconPool( 4 );
				}
			}
			else if( type == MiniMapElementType.Institute )
			{
				while( temp == null )
				{
					for( int i = 0; i < instituteIconPool.Count; i++ )
					{
						if( !instituteIconPool[i].gameObject.activeInHierarchy )
						{
							temp = instituteIconPool[i];
							return temp;
						}
					}

					//I do't think this situation can happen
					FillInstituteElementIconPool( 2 );
				}
			}
			else if( type == MiniMapElementType.Rune )
			{
				while( temp == null )
				{
					for( int i = 0; i < runeIconPool.Count; i++ )
					{
						if( !runeIconPool[i].gameObject.activeInHierarchy )
						{
							temp = runeIconPool[i];
							return temp;
						}
					}

					FillRuneElementIconPool( 2 );
				}
			}
			else if( type == MiniMapElementType.Town )
			{
                //When town destroy will lose the play, not need cache more number.
                GameObject obj = GameObject.Instantiate( resourceLoad.LoadAsset<GameObject>( "Town" ) );
				obj.transform.SetParent( elementIconParent, false );
				obj.transform.SetAsFirstSibling();
				obj.gameObject.SetActive( true );
				temp = obj.GetComponent <Image>();
				return temp;
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.MiniMap, string.Format( "Can't find this type element{0}", type ));
				return null;
			}

			return null;
		}

		private MiniMapEffect TakeEffectItemFromPool( MiniMapEffectType type, long id )
		{
			DebugUtils.Log( DebugUtils.Type.MiniMap, string.Format( "TakeEffect, Type is {0} ", type ) );

			if( effectElementDic == null )
			{
				effectElementDic = new Dictionary<long, MiniMapEffect>();
			}

			MiniMapEffect temp = null;
			//Not need too more warning in one place.
			if( type == MiniMapEffectType.Warning )
			{
				effectElementDic.TryGetValue( id, out temp );

				if( temp != null )
				{
					DebugUtils.Log( DebugUtils.Type.MiniMap, string.Format( "This waring effect is playing. EffectID is {0} effect HolderID is {1}", temp.GetEffectID(), temp.GetHolderID() ));
					return null;
				}
			}
				
			while( temp == null )
			{
				for( int i = 0; i < effectItemPool.Count; i++ )
				{
					if( effectItemPool[i].GetEffectType() == type && effectItemPool[i].GetEffctItemStatus() )
					{
						DebugUtils.Log( DebugUtils.Type.MiniMap, string.Format( "Find can use effect, Type is {0} .", type ) );
						effectItemPool[i].SetHolderID( id );
						effectElementDic.Add( id ,effectItemPool[i] );
						temp = effectItemPool[i];
						return temp;
					}
				}

				FillEffectItemPool( 5 );
			}		

			return null;
		}

		public void EffectDestroy( long id )
		{
			effectElementDic.Remove( id );
		}
			
		#endregion
    }
}
