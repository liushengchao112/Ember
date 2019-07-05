using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Data;
using Constants;

using TutorialStage = PVE.TutorialModeManager.TutorialModeStage;

public class MapData1V1: MonoBehaviour
{
	[HideInInspector]
	public Transform BlueBaseTransform;
	[HideInInspector]
	public Transform RedBaseTransform;

	[HideInInspector]
	public GameObject BlueDeployArea;
	[HideInInspector]
	public GameObject RedDeployArea;

	[HideInInspector]
	public List<Transform> smallCrystalsList;
    [HideInInspector]
    public List<Transform> bigCrystalsList;
    [HideInInspector]
    public List<Transform> wildMonsterList;
    [HideInInspector]
    public List<Transform> idolGuardList;
    [HideInInspector]
    public Transform idolBirthPosition;
	[HideInInspector]
	public Vector3 mapSize = new Vector3( 180f, 0, 100f );//This is new art 3D map size.

	#region TapBuildMode values

	public Dictionary<ForceMark, List<Vector3>> towerBase;
	public Dictionary<ForceMark, List<Vector3>> instituteBase;

	#endregion

    #region Camera set value

    private float cameraHeight = 16.5f;
	private Vector2 mapCameraLimitWidth = new Vector2( -79f, 79f );
	private Vector2 mapCameraLimitHeight = new Vector2( -54f, 30f );

	private Vector2 BlueBaseCameraPos = new Vector2( -79f, -54f );
	private Vector2 BlueBaseCameraLimitPos = new Vector2( 79f, 30f );

	private Vector2 RedBaseCameraPos = new Vector2( 79f, 30f );
	private Vector2 RedBaseCameraLimitPos = new Vector2( -79f, -54f );

	private Vector3 BlueBaseCameraAngle = new Vector3( 50f, 0, 0 );
	private Vector3 RedBaseCameraAngle = new Vector3( 50f, 0, 0 );

	private float highCameraViewValue = 50f;
	private float middleCameraViewValue = 45f;
	private float lowCameraViewValue = 40f;

	#endregion

	//TODO: If have time need create new mapdata move this value.Dwayne.
	#region TutorialValues

	//CameraPos
	private Vector2 normallyControlOperation_Stage_StartPos = new Vector2( -75f, -50f );
	private Vector2 normallyControlOperation_Stage_GenerateDollUnitPos = new Vector2( -64f, -43f );

	//Path point
	private List<Vector3> tutorialPathPointPos;

	#endregion

	#region FormationPoints

	private List<Vector3> redFormationPointList = new List<Vector3>();
	private List<Vector3> blueFormationPointList = new List<Vector3>();

	#endregion

	public void InitializeMapData1v1()
	{
		BlueBaseTransform = transform.Find( "Function/Base_blue/BottomBlueBase" ).transform;
		RedBaseTransform = transform.Find( "Function/Base_red/TopRedBase" ).transform;

		BlueDeployArea = transform.Find( "Path/2v2Map BottomBlueBornArea" ).gameObject;
		RedDeployArea = transform.Find( "Path/2v2Map TopRedBornArea" ).gameObject;

		SetCrystalsList();
        SetNPCPosition();
		SetInstitutePosition();
		SetTowerBasePosition();
		SetFormationPoint();

        MessageDispatcher.AddObserver( ShowDeployAreas, Constants.MessageType.ShowDeployAreas );
		MessageDispatcher.AddObserver( CloseDeployAreas, Constants.MessageType.CloseDeployAreas );
	}

	public void ShowDeployAreas( object s )
	{
		ForceMark mark = ( ForceMark )s;
		if( mark == ForceMark.TopBlueForce )
		{
			BlueDeployArea.SetActive( true );
		}
		else if( mark == ForceMark.TopRedForce )
		{
			RedDeployArea.SetActive( true );
		}
		else
		{
			DebugUtils.LogError( DebugUtils.Type.Map, "The operation's owner has error forceMark" + mark );
		}
	}

	public void CloseDeployAreas( object s )
	{
		ForceMark mark = ( ForceMark )s;

		if ( mark == ForceMark.TopBlueForce )
		{
			BlueDeployArea.SetActive( false );
		}
		else if( mark == ForceMark.TopRedForce )
		{
			RedDeployArea.SetActive( false );
		}
		else
		{
			DebugUtils.LogError( DebugUtils.Type.Map, "The operation's owner has error forceMark" + mark );
		}
	}

	public Vector3 GetTownPosition( ForceMark mark )
	{
		if ( mark == ForceMark.TopBlueForce )
		{
			return BlueBaseTransform.position;
		}
		else if ( mark == ForceMark.TopRedForce )
		{
			return RedBaseTransform.position;
		}
		else
		{
			DebugUtils.LogError( DebugUtils.Type.Map, string.Format( "The mark doesn't has town! please check input forceMark. mark = {0}", mark ) );
			return Vector3.zero;
		}
	}

    public GameObject GetTownBaseObject( ForceMark mark )
    {
        if ( mark == ForceMark.TopBlueForce )
        {
            return BlueBaseTransform.gameObject;
        }
        else if ( mark == ForceMark.TopRedForce )
        {
            return RedBaseTransform.gameObject;
        }
        else
        {
            DebugUtils.LogError( DebugUtils.Type.Map, string.Format( "The mark doesn't has town! please check input forceMark. mark = {0}", mark ) );
            return null;
        }
    }

    public Vector3 GetCameraOriginalPosition( ForceMark mark )
	{
		if( mark == ForceMark.TopBlueForce )
		{
			return new Vector3( BlueBaseCameraPos.x, cameraHeight, BlueBaseCameraPos.y );
        }
        else if( mark == ForceMark.TopRedForce )
		{
            return new Vector3( RedBaseCameraPos.x, cameraHeight, RedBaseCameraPos.y );
		}
		else
		{
			DebugUtils.LogError( DebugUtils.Type.Map, "Can't find this ForceMark :" + mark );
			return Vector3.zero;
		}
	}

	public Vector3 GetCameraOriginalAngle( ForceMark mark )
	{
		if( mark == ForceMark.TopBlueForce )
		{
			return BlueBaseCameraAngle;
		}
		else if( mark == ForceMark.TopRedForce )
		{
			return RedBaseCameraAngle;
		}
		else
		{
			DebugUtils.LogError( DebugUtils.Type.Map, "Can't find this ForceMark :" + mark );
			return Vector3.zero;
		}
	}

	public float GetViewField( int inputChoose )
	{
		if( inputChoose == ( int )UI.CameraViewType.High )
		{
			return highCameraViewValue;
		}
		else if( inputChoose == ( int )UI.CameraViewType.Middle )
		{
			return middleCameraViewValue;
		}
		else if( inputChoose == ( int )UI.CameraViewType.Low )
        {
            return lowCameraViewValue;
		}
		else
		{
			DebugUtils.LogError( DebugUtils.Type.Map, "Can't know this CameraViewType :" + inputChoose );
			return -1;
		}
	}

	public Vector2 GetCameraWidthRange()
	{
		return mapCameraLimitWidth;
	}

	public Vector2 GetCameraWidthRange( ForceMark mark )
	{
        if ( mark == ForceMark.TopBlueForce )
        {
			return new Vector2( BlueBaseCameraPos.x, BlueBaseCameraLimitPos.x );
        }
        else if ( mark == ForceMark.TopRedForce )
        {
            return new Vector2( RedBaseCameraLimitPos.x, RedBaseCameraPos.x );
        }
        else
        {
            DebugUtils.LogError( DebugUtils.Type.Map, string.Format( "Can't know this ForceMark : {0}", mark ) );
            return Vector2.zero;
        }
    }

	public Vector2 GetCameraHeightRange()
	{
		return mapCameraLimitHeight;
	}

    public Vector2 GetCameraHeightRange( ForceMark mark )
	{
        if ( mark == ForceMark.TopBlueForce )
        {
			return new Vector2( BlueBaseCameraPos.y, BlueBaseCameraLimitPos.y );
        }
        else if ( mark == ForceMark.TopRedForce )
        {
            return new Vector2( RedBaseCameraLimitPos.y, RedBaseCameraPos.y );
        }
        else
        {
            DebugUtils.LogError( DebugUtils.Type.Map, string.Format( "Can't know this ForceMark : {0}", mark ) );
            return Vector2.zero;
        }
    }

    public CameraInvertType GetCameraInvertType( ForceMark mark )
    {
        // Only flip horizontal now
        CameraInvertType type = CameraInvertType.None;

        switch ( mark )
        {
            case ForceMark.TopRedForce:
            {
                type = CameraInvertType.Horizontal;
                break;
            }
            case ForceMark.BottomRedForce:
            {
                type = CameraInvertType.Horizontal;
                break;
            }
            case ForceMark.TopBlueForce:
            {
                type = CameraInvertType.None;
                break;
            }
            case ForceMark.BottomBlueForce:
            {
                type = CameraInvertType.None;
                break;
            }
        }
        
        return type;
    }

    private void SetCrystalsList()
	{
        Transform crystalRoot = transform.Find( "Function/Building_mine" );
        bigCrystalsList = new List<Transform>();

        for ( int i = 0; i < crystalRoot.childCount; i++ )
        {
            Transform child = crystalRoot.transform.GetChild( i );
            bigCrystalsList.Add(child);
        }
	}

    public GameObject GetCrystalObject( Vector3 p )
    {
		Transform trans = bigCrystalsList.Find( b => b.position == p );
        return trans.gameObject;
    }

    public List<Transform> GetSmallCrystalsList()
	{
		return smallCrystalsList;
	}

    public List<Transform> GetBigCrystalsList()
	{
		return bigCrystalsList;
	}

    private void SetNPCPosition()
    {
        // wild monster;
        Transform wildMonsterTransRoot = transform.Find( "Path/NpcBox" );
        wildMonsterList = new List<Transform>();

        for ( int i = 0; i < wildMonsterTransRoot.childCount; i++ )
        {
            wildMonsterList.Add( wildMonsterTransRoot.GetChild( i ) );
        }

        //idol Gurad;
        Transform idolGuradTransRoot = transform.Find( "Path/IdolGuard" );
        idolGuardList = new List<Transform>();
        if ( idolGuradTransRoot != null )
        {
            for ( int i = 0; i < idolGuradTransRoot.childCount; i++ )
            {
                idolGuardList.Add( idolGuradTransRoot.GetChild( i ) );
            }
        }

        //idol
        idolBirthPosition = transform.Find( "Path/Idol" );
    }

    public List<Transform> GetWildMonsterPosition()
    {
        return wildMonsterList;
    }

    public List<Transform> GetIdolGuardPosition()
    {
        return idolGuardList;
    }

    public Transform GetIdolPosition()
    {
        return idolBirthPosition;
    }

	#region FormationPoint functions

	private void SetFormationPoint()
	{
		Transform redFormationPointRoot = transform.Find( "Function/Point/TopRedBase" );

		for ( int i = 0; i < redFormationPointRoot.childCount; i++ )
		{
			Transform child = redFormationPointRoot.GetChild( i );
			redFormationPointList.Add( child.position );
		}

		Transform blueFormationPointRoot = transform.Find( "Function/Point/BottomBlueBase" );

		for ( int i = 0; i < redFormationPointRoot.childCount; i++ )
		{
			Transform child = blueFormationPointRoot.GetChild( i );
			blueFormationPointList.Add( child.position );
		}
	}

	public List<Vector3> GetFormationPoint( ForceMark mark )
	{
		if( mark == ForceMark.TopRedForce )
		{
			return redFormationPointList;
		}
		else
		{
			return blueFormationPointList;
		}
	}

	#endregion

	#region TapBuildMode buildingBase

	public void SetTowerBasePosition()
	{
		towerBase = new Dictionary<ForceMark, List<Vector3>>();

		List<Vector3> blueBasePos = new List<Vector3>();
		List<Vector3> redBasePos = new List<Vector3>();

		Transform blueBaseRoot = transform.Find( "Function/Tower_blue/BottomBlueBase" );

		for( int i = 0; i < blueBaseRoot.childCount; i++ )
		{
			blueBasePos.Add( blueBaseRoot.GetChild( i ).transform.position );
		}

		Transform redBaseRoot = transform.Find( "Function/Tower_red/TopRedBase" );

		for( int i = 0; i < redBaseRoot.childCount; i++ )
		{
			redBasePos.Add( redBaseRoot.GetChild( i ).transform.position );
		}

		towerBase.Add( ForceMark.TopBlueForce, blueBasePos );
		towerBase.Add( ForceMark.TopRedForce, redBasePos );
	}

	public void SetInstitutePosition()
	{
		instituteBase = new Dictionary<ForceMark, List<Vector3>>();

		List<Vector3> blueBasePos = new List<Vector3>();
		List<Vector3> redBasePos = new List<Vector3>();

		Transform blueBaseRoot = transform.Find( "Function/Lab_blue/BottomBlueBase" );

		for( int i = 0; i < blueBaseRoot.childCount; i++ )
		{
			blueBasePos.Add( blueBaseRoot.GetChild( i ).transform.position );
		}

		Transform redBaseRoot = transform.Find( "Function/Lab_red/TopRedBase" );

		for( int i = 0; i < redBaseRoot.childCount; i++ )
		{
			redBasePos.Add( redBaseRoot.GetChild( i ).transform.position );
		}

		instituteBase.Add( ForceMark.TopBlueForce, blueBasePos );
		instituteBase.Add( ForceMark.TopRedForce, redBasePos );
	}

	public bool IsSelfTowerBase( ForceMark mark, Vector3 pos )
	{
		DebugUtils.Log( DebugUtils.Type.Map, string.Format( "IsSelfTowerBase is active, mark is {0}, pos is {1}", mark , pos ));
		List<Vector3> towerList = new List<Vector3>(); 
		towerBase.TryGetValue( mark, out towerList );

		if( towerList != null && towerList.Count > 0 )
		{
			return towerList.Contains( pos );
		}
		else
		{
			DebugUtils.LogError( DebugUtils.Type.Map, string.Format( "Can't find this mark: {0} towerList is null", mark ) );
			return false;
		}
	}

	public bool IsSelfInstituteBase( ForceMark mark, Vector3 pos )
	{
		List<Vector3> instituteList = new List<Vector3>();
		instituteBase.TryGetValue( mark, out instituteList );

		if( instituteList != null && instituteList.Count > 0 )
		{
			return instituteList.Contains( pos );
		}
		else
		{
			DebugUtils.LogError( DebugUtils.Type.Map, string.Format( "Can't find this mark: {0} instituteList is null", mark ) );
			return true;
		}
	}
		
	#endregion

	//TODO:If have time need create new map data, move these value. Dwayne.
	#region Tutorial Functions

	public Vector2 GetTutorialCameraPos( TutorialStage stage, int index )
	{
		if( stage == TutorialStage.NormallyControlOperation_Stage )
		{
			if( index == 1 )
			{
				return normallyControlOperation_Stage_StartPos;
			}
			else if( index == 2 )
			{
				return normallyControlOperation_Stage_GenerateDollUnitPos;
			}
			else
			{
				DebugUtils.LogError( DebugUtils.Type.Tutorial, string.Format( "Can't find this index {0}", index ) );
				return Vector2.zero;
			}
		}
		else if( true )
		{
			DebugUtils.Log( DebugUtils.Type.Tutorial, "Now just have normallyControlOperation_Stage camera pos." );
			return Vector2.zero;
		}
	}
	//TODO;This is temp function, waiting Tutorial mapdata finished move this.Dwayne.
	public void SetTutorialPathPointPosition()
	{
		tutorialPathPointPos = new List<Vector3>();
		Transform tutorialPathPointRoot = transform.Find( "Path/TutorialPathPoint" );

		for ( int i = 0; i < tutorialPathPointRoot.childCount; i++ )
		{
			tutorialPathPointPos.Add( tutorialPathPointRoot.GetChild( i ).position );
		}
	}

	public List<Vector3> GetTutorialPathPointPosition()
	{
		return tutorialPathPointPos;
	}
		
	#endregion

    public void OnDestroy()
	{
		MessageDispatcher.RemoveObserver( ShowDeployAreas, Constants.MessageType.ShowDeployAreas );
		MessageDispatcher.RemoveObserver( CloseDeployAreas, Constants.MessageType.CloseDeployAreas );
	}
}
