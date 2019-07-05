using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;
using Data;
using Constants;

public class MapData2V2 : MonoBehaviour
{
	[HideInInspector]
	public Transform topBlueBaseTransform;
	[HideInInspector]
	public Transform topRedBaseTransform;
	[HideInInspector]
	public Transform bottomBlueBaseTransform;
	[HideInInspector]
	public Transform bottomRedBaseTransform;

	[HideInInspector]
	public GameObject topBlueDeployArea;
	[HideInInspector]
	public GameObject topRedDeployArea;
	[HideInInspector]
	public GameObject bottomBlueDeployArea;
	[HideInInspector]
	public GameObject bottomRedDeployArea;

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

    private Vector2 bottomBlueBaseCameraPos = new Vector2( -79f, -54f );
    private Vector2 bottomBlueBaseCameraLimitPos = new Vector2( 79f, 30f );

    private Vector2 bottomRedBaseCameraPos = new Vector2( 79f, -54f );
    private Vector2 bottomRedBaseCameraLimitPos = new Vector2( -79f, 30f );

    private Vector2 topRedBaseCameraPos = new Vector2( 79f, 30f );
    private Vector2 topRedBaseCameraLimitPos = new Vector2( -79f, -54f );

    private Vector2 topBlueBaseCameraPos = new Vector2( -79f, 30f );
    private Vector2 topBlueBaseCameraLimitPos = new Vector2( 79f, -54f );

	private Vector3 topBlueBaseCameraAngle = new Vector3( 50f, 0, 0 );
	private Vector3 bottomBlueBaseCameraAngle = new Vector3( 50f, 0, 0 );
	private Vector3 topRedBaseCameraAngle = new Vector3( 50f, 0, 0 );
	private Vector3 bottomRedBaseCameraAngle = new Vector3( 50f, 0, 0 );

	private float highCameraViewValue = 50f;
	private float middleCameraViewValue = 45f;
	private float lowCameraViewValue = 40f;

	#endregion

	#region FormationPoints

	private List<Vector3> topRedFormationPointList = new List<Vector3>();
	private List<Vector3> topBlueFormationPointList = new List<Vector3>();
	private List<Vector3> bottomRedFormationPointList = new List<Vector3>();
    private List<Vector3> bottomBlueFormationPointList = new List<Vector3>();

    #endregion

    public void InitializeMapData2v2()
	{
		topBlueBaseTransform = transform.Find( "Function/Base_blue/TopBlueBase" ).transform;
		bottomBlueBaseTransform = transform.Find( "Function/Base_blue/BottomBlueBase" ).transform;
		topRedBaseTransform = transform.Find( "Function/Base_red/TopRedBase" ).transform;
		bottomRedBaseTransform = transform.Find( "Function/Base_red/BottomRedBase" ).transform;

		topBlueDeployArea = transform.Find( "Path/2v2Map TopBlueBornArea" ).gameObject;
		topRedDeployArea = transform.Find( "Path/2v2Map TopRedBornArea" ).gameObject;
		bottomBlueDeployArea = transform.Find( "Path/2v2Map BottomBlueBornArea" ).gameObject;
		bottomRedDeployArea = transform.Find( "Path/2v2Map BottomRedBornArea" ).gameObject;

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
			topBlueDeployArea.SetActive( true );
		}
		else if( mark == ForceMark.TopRedForce )
		{
			topRedDeployArea.SetActive( true );
		}
		else if( mark == ForceMark.BottomBlueForce )
		{
			bottomBlueDeployArea.SetActive( true );
		}
		else if( mark == ForceMark.BottomRedForce )
		{
			bottomRedDeployArea.SetActive( true );
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
			topBlueDeployArea.SetActive( false );
		}
		else if( mark == ForceMark.TopRedForce )
		{
			topRedDeployArea.SetActive( false );
		}
		else if( mark == ForceMark.BottomBlueForce )
		{
			bottomBlueDeployArea.SetActive( false );
		}
		else if( mark == ForceMark.BottomRedForce )
		{
			bottomRedDeployArea.SetActive( false );
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
			return topBlueBaseTransform.position;
		}
		else if ( mark == ForceMark.TopRedForce )
		{
			return topRedBaseTransform.position;
		}
		else if ( mark == ForceMark.BottomBlueForce )
		{
			return bottomBlueBaseTransform.position;
		}
		else if ( mark == ForceMark.BottomRedForce )
		{
			return bottomRedBaseTransform.position;
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
            return topBlueBaseTransform.gameObject;
        }
        else if ( mark == ForceMark.TopRedForce )
        {
            return topRedBaseTransform.gameObject;
        }
        else if ( mark == ForceMark.BottomBlueForce )
        {
            return bottomBlueBaseTransform.gameObject;
        }
        else if ( mark == ForceMark.BottomRedForce )
        {
            return bottomRedBaseTransform.gameObject;
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
            return new Vector3( topBlueBaseCameraPos.x, cameraHeight, topBlueBaseCameraPos.y );
        }
        else if( mark == ForceMark.TopRedForce )
		{
            return new Vector3( topRedBaseCameraPos.x, cameraHeight, topRedBaseCameraPos.y );
		}
		else if( mark == ForceMark.BottomBlueForce )
		{
            return new Vector3( bottomBlueBaseCameraPos.x, cameraHeight, bottomBlueBaseCameraPos.y );
		}
		else if( mark == ForceMark.BottomRedForce )
		{
            return new Vector3( bottomRedBaseCameraPos.x, cameraHeight, bottomRedBaseCameraPos.y );
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
			return topBlueBaseCameraAngle; 
		}
		else if( mark == ForceMark.TopRedForce )
		{
			return topRedBaseCameraAngle;
		}
		else if( mark == ForceMark.BottomBlueForce )
		{
			return bottomBlueBaseCameraAngle;
		}
		else if( mark == ForceMark.BottomRedForce )
		{
			return bottomRedBaseCameraAngle;
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
            return new Vector2( topBlueBaseCameraPos.x, topBlueBaseCameraLimitPos.x );
        }
        else if ( mark == ForceMark.TopRedForce )
        {
            return new Vector2( topRedBaseCameraLimitPos.x, topRedBaseCameraPos.x );
        }
        else if ( mark == ForceMark.BottomBlueForce )
        {
            return new Vector2( bottomBlueBaseCameraPos.x, bottomBlueBaseCameraLimitPos.x );
        }
        else if ( mark == ForceMark.BottomRedForce )
        {
            return new Vector2( bottomRedBaseCameraLimitPos.x, bottomRedBaseCameraPos.x );
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
            return new Vector2( topBlueBaseCameraLimitPos.y, topBlueBaseCameraPos.y );
        }
        else if ( mark == ForceMark.TopRedForce )
        {
            return new Vector2( topRedBaseCameraLimitPos.y, topRedBaseCameraPos.y );
        }
        else if ( mark == ForceMark.BottomBlueForce )
        {
            return new Vector2( bottomBlueBaseCameraPos.y, bottomBlueBaseCameraLimitPos.y );
        }
        else if ( mark == ForceMark.BottomRedForce )
        {
            return new Vector2( bottomRedBaseCameraPos.y, bottomRedBaseCameraLimitPos.y );
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
            bigCrystalsList.Add( child );
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
        Transform wildMonsterTransRoot = transform.Find("Path/NpcBox");
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
		Transform topRedFormationPointRoot = transform.Find( "Function/Point/TopRedBase" );
		for ( int i = 0; i < topRedFormationPointRoot.childCount; i++ )
		{
			Transform child = topRedFormationPointRoot.GetChild( i );
			topRedFormationPointList.Add( child.position );
		}

		Transform bottomRedFormationPointRoot = transform.Find( "Function/Point/BottomRedBase" );
		for ( int i = 0; i < bottomRedFormationPointRoot.childCount; i++ )
		{
			Transform child = bottomRedFormationPointRoot.GetChild( i );
			bottomRedFormationPointList.Add( child.position );
		}

		Transform topBlueFormationPointRoot = transform.Find( "Function/Point/TopBlueBase" );
		for ( int i = 0; i < topBlueFormationPointRoot.childCount; i++ )
		{
			Transform child = topBlueFormationPointRoot.GetChild( i );
			topBlueFormationPointList.Add( child.position );
		}

		Transform bottomBlueFormationPointRoot = transform.Find( "Function/Point/BottomBlueBase" );
		for ( int i = 0; i < bottomBlueFormationPointRoot.childCount; i++ )
		{
			Transform child = bottomBlueFormationPointRoot.GetChild( i );
            bottomBlueFormationPointList.Add( child.position );
        }
    }

	public List<Vector3> GetFormationPoint( ForceMark mark )
	{
		if( mark == ForceMark.TopRedForce )
		{
			return topRedFormationPointList;
		}
		else if( mark == ForceMark.BottomRedForce )
		{
			return bottomRedFormationPointList;
		}
		else if( mark == ForceMark.TopBlueForce )
		{
			return topBlueFormationPointList;
		}
		else if( mark == ForceMark.BottomBlueForce )
		{
			return bottomBlueFormationPointList;
		}
		else
		{
			DebugUtils.LogError( DebugUtils.Type.Map, string.Format( "MapData2V2 can't find this mark {0} ", mark ) );
			return null;
		}

	}

	#endregion 

	#region TapBuildMode buildingBase

	public void SetTowerBasePosition()
	{
		towerBase = new Dictionary<ForceMark, List<Vector3>>();

		List<Vector3> topBlueBasePos = new List<Vector3>();
		List<Vector3> bottomBlueBasePos = new List<Vector3>();
		List<Vector3> topRedBasePos = new List<Vector3>();
		List<Vector3> bottomRedBasePos = new List<Vector3>();

		Transform topBlueBaseRoot = transform.Find( "Function/Tower_blue/TopBlueBase" );

		for( int i = 0; i < topBlueBaseRoot.childCount; i++ )
		{
			topBlueBasePos.Add( topBlueBaseRoot.GetChild( i ).transform.position );
		}

		Transform bottomBlueBaseRoot = transform.Find( "Function/Tower_blue/BottomBlueBase" );

		for( int i = 0; i < bottomBlueBaseRoot.childCount; i++ )
		{
			bottomBlueBasePos.Add( bottomBlueBaseRoot.GetChild( i ).transform.position );
		}

		Transform topRedBaseRoot = transform.Find( "Function/Tower_red/TopRedBase" );

		for( int i = 0; i < topRedBaseRoot.childCount; i++ )
		{
			topRedBasePos.Add( topRedBaseRoot.GetChild( i ).transform.position );
		}

		Transform bottomRedBaseRoot = transform.Find( "Function/Tower_red/BottomRedBase" );

		for( int i = 0; i < bottomRedBaseRoot.childCount; i++ )
		{
			bottomRedBasePos.Add( bottomRedBaseRoot.GetChild( i ).transform.position );
		}

		towerBase.Add( ForceMark.TopBlueForce, topBlueBasePos );
		towerBase.Add( ForceMark.BottomBlueForce, bottomBlueBasePos );
		towerBase.Add( ForceMark.TopRedForce, topRedBasePos );
		towerBase.Add( ForceMark.BottomRedForce, bottomRedBasePos );
	}

	public void SetInstitutePosition()
	{
		instituteBase = new Dictionary<ForceMark, List<Vector3>>();

		List<Vector3> topBlueBasePos = new List<Vector3>();
		List<Vector3> bottomBlueBasePos = new List<Vector3>();
		List<Vector3> topRedBasePos = new List<Vector3>();
		List<Vector3> bottomRedBasePos = new List<Vector3>();

		Transform topBlueBaseRoot = transform.Find( "Function/Lab_blue/TopBlueBase" );

		for( int i = 0; i < topBlueBaseRoot.childCount; i++ )
		{
			topBlueBasePos.Add( topBlueBaseRoot.GetChild( i ).transform.position );
		}

		Transform bottomBlueBaseRoot = transform.Find( "Function/Lab_blue/BottomBlueBase" );

		for( int i = 0; i < bottomBlueBaseRoot.childCount; i++ )
		{
			bottomBlueBasePos.Add( bottomBlueBaseRoot.GetChild( i ).transform.position );
		}

		Transform topRedBaseRoot = transform.Find( "Function/Lab_red/TopRedBase" );

		for( int i = 0; i < topRedBaseRoot.childCount; i++ )
		{
			topRedBasePos.Add( topRedBaseRoot.GetChild( i ).transform.position );
		}

		Transform bottomRedBaseRoot = transform.Find( "Function/Lab_red/BottomRedBase" );

		for( int i = 0; i < bottomRedBaseRoot.childCount; i++ )
		{
			bottomRedBasePos.Add( bottomRedBaseRoot.GetChild( i ).transform.position );
		}

		instituteBase.Add( ForceMark.TopBlueForce, topBlueBasePos );
		instituteBase.Add( ForceMark.BottomBlueForce, bottomBlueBasePos );
		instituteBase.Add( ForceMark.TopRedForce, topRedBasePos );
		instituteBase.Add( ForceMark.BottomRedForce, bottomRedBasePos );
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

    public void OnDestroy()
	{
		MessageDispatcher.RemoveObserver( ShowDeployAreas, Constants.MessageType.ShowDeployAreas );
		MessageDispatcher.RemoveObserver( CloseDeployAreas, Constants.MessageType.CloseDeployAreas );
	}
}
