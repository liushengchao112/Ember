using UnityEngine;
using System.Collections;

namespace UI
{
    public class BuildingItem : MonoBehaviour
    {
		public enum BuildingItemType
		{
			None,
			Tower,
			Institute
		}

    	private MeshRenderer buildingSetAreaMeshRender; 
		private BoxCollider buildingItemCollider;
		private Vector3 towerItemSize = new Vector3( 1.5f, 2.5f, 1.5f );
		private Vector3 instituteItemSize = new Vector3( 3f, 1f, 2f );
		private BuildingItemType lastUsedBuildingType = BuildingItemType.None; 

    	public enum BuildCheckObjStatus
    	{
    		CanBuild,
    		CanNotBuild
    	}

    	public BuildCheckObjStatus selfStatus = BuildCheckObjStatus.CanBuild;
    	public bool isEnterTheCannotBuildObj = false;

    	void Awake () 
    	{
    		buildingSetAreaMeshRender = this.transform.Find( "BuildingSetArea" ).GetComponent<MeshRenderer>();
			buildingItemCollider = GetComponent<BoxCollider>();
    	}

    	void Update()
    	{
    		if( selfStatus == BuildCheckObjStatus.CanBuild && buildingSetAreaMeshRender.material.color == Color.red )
    		{
    			ShowCanBuildColor();
    		}
    		else if( selfStatus == BuildCheckObjStatus.CanNotBuild && buildingSetAreaMeshRender.material.color == Color.green )
    		{
    			ShowCannotBuildColor();
    		}
    		else
    		{
    			Utils.DebugUtils.Log( Utils.DebugUtils.Type.Building, "The check battlefield is can build Obj not need change." );
    		}
    	}

    	public void ShowCanBuildColor()
    	{
    		buildingSetAreaMeshRender.material.color = Color.green;
    	}

    	public void ShowCannotBuildColor()
    	{
    		buildingSetAreaMeshRender.material.color = Color.red;
    	}

    	void OnTriggerEnter( Collider collider )
    	{
    		if( collider.tag == "BuildingsObstacle" &&  isEnterTheCannotBuildObj == false )
    		{
    			isEnterTheCannotBuildObj = true;
    		}
    	}

    	void OnTriggerStay( Collider collider )
    	{
			if( collider.tag == "BuildingsObstacle" &&  isEnterTheCannotBuildObj == false )
    		{
    			isEnterTheCannotBuildObj = true;
    		}
    	}

    	void OnTriggerExit( Collider collider )
    	{
			if( collider.tag == "BuildingsObstacle" && isEnterTheCannotBuildObj == true )
    		{
    			isEnterTheCannotBuildObj = false;
    		}
    	}

    	void OnEnable()
    	{
    		ShowCanBuildColor();
    		isEnterTheCannotBuildObj = false;
    	}

		public void ChangeItemType( BuildingItemType type )
		{
			if( type != BuildingItemType.None && type != lastUsedBuildingType )
			{
				if( type == BuildingItemType.Tower )
				{
					this.transform.localScale = towerItemSize;
				}
				else if( type == BuildingItemType.Institute )
				{
					this.transform.localScale = instituteItemSize;	
				}
				else
				{
					Utils.DebugUtils.LogError( Utils.DebugUtils.Type.Building, "Can't find this buildingType :" + type );
				}
			}
			else if( type != BuildingItemType.None && type == lastUsedBuildingType )
			{
				//Not need change buildingitem size
			}
			else
			{
				Utils.DebugUtils.LogError( Utils.DebugUtils.Type.Building, "Can't find this buildingType :" + type );
			}
		}
    }
}
