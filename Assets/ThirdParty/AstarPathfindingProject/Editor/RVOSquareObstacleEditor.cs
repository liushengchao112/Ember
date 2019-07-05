using UnityEngine;
using System.Collections;
using UnityEditor;
using Pathfinding.RVO;

namespace Pathfinding {
	[CustomEditor(typeof(RVOSquareObstacle))]
	public class RVOSquareObstacleEditor : Editor {
		public override void OnInspectorGUI () {
			DrawDefaultInspector();
		}
	}
}
