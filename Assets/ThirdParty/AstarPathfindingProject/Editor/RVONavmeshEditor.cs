using UnityEngine;
using System.Collections;
using UnityEditor;
using Pathfinding.RVO;

namespace Pathfinding {
	[CustomEditor(typeof(RVONavmesh))]
	public class RVONavmeshEditor : Editor {
		public override void OnInspectorGUI () {
			DrawDefaultInspector();
		}
	}
}
