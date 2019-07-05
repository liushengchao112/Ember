using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Logic;

[CustomEditor(typeof(Soldier), true)]
public class SoldierInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Soldier soldier = target as Soldier;
        Fsm currentFsm = soldier.currentFsm;

        System.Type type = currentFsm.GetType();

        //EditorGUILayout.PropertyField( soldier.currentFsm );

        if( type == typeof( SoldierFsmWalk ) )
        {
            SoldierFsmWalk fsm = currentFsm as SoldierFsmWalk;
            fsm.subState = EditorGUILayout.IntField( "sub state:", fsm.subState );
            fsm.avoidSpeed = new Utils.FixVector3( EditorGUILayout.Vector3Field( "avoid speed:", fsm.avoidSpeed.vector3 ) );
        }
        else if( type == typeof( SoldierFsmChase ) )
        {
            SoldierFsmChase fsm = currentFsm as SoldierFsmChase;
            fsm.subState = EditorGUILayout.IntField( "sub state:", fsm.subState );
            fsm.avoidSpeed = new Utils.FixVector3( EditorGUILayout.Vector3Field( "avoid speed:", fsm.avoidSpeed.vector3 ) );
        }
    }
	
    void OnSceneGUI()
    {
        Soldier soldier = target as Soldier;

        string stateName = soldier.currentFsm.ToString().Substring( 16 );
        Handles.Label( soldier.transform.position + Vector3.up * 2, stateName );

        Handles.BeginGUI();

        GUILayout.BeginArea( new Rect( 50, 100, 200, 200 ) );
        
        GUILayout.Label( soldier.name );
        Vector3 position = soldier.position.vector3;
        GUILayout.Label( string.Format( "Position = ({0}, {1}, {2})", position.x, position.y, position.z ) );
        GUILayout.Label( "current fsm = " + stateName );
        
        GUILayout.EndArea();

        Handles.EndGUI();
    }

    /*
    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
    static void DrawGameObjectName(Transform transform, GizmoType gizmoType)
    {   
        Handles.Label(transform.position, transform.gameObject.name);
    }
    */
}
