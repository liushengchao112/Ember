using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Data;
using Utils;
using Logic;

//Use this detect unit is arrived the point.
public class TutorialModeBeacon : MonoBehaviour 
{
	public uint theBeaconID;//There need engineer input.
	private List<UnitType> Soldiers = new List<UnitType>();
		
	void OnTriggerEnter( Collider collider )
	{
		//Debug.Log( "Collider Enter Name is :" + collider.name );

		Soldier soldier = collider.GetComponent<Soldier>();

		if( Soldiers.Count == 0 || !Soldiers.Contains( soldier.unitType ) )
		{
			Soldiers.Add( soldier.unitType );	
			MessageDispatcher.PostMessage( Constants.MessageType.TutorialPathPointMessageSend, Soldiers.Count );
		}
	}

	//If need these functons open it;
	/*void OnTriggerStay( Collider collider )
	{
		Debug.Log( "Collider Stay Name is :" + collision.name );
	}

	void OnTriggerExit( Collider collider )
	{
		Debug.Log( "Collider Stay Name is :" + collision.name );
	}*/
}
