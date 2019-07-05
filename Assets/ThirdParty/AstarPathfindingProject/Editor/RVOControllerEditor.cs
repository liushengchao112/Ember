using UnityEngine;
using UnityEditor;
using Pathfinding;
using Pathfinding.RVO;

namespace Pathfinding {
	[CustomEditor(typeof(RVOController))]
	[CanEditMultipleObjects]
	public class RVOControllerEditor : Editor {
		public override void OnInspectorGUI () {
			DrawDefaultInspector();

			bool maxNeighboursLimit = false;
			bool debugAndMultithreading = false;

			for (int i = 0; i < targets.Length; i++) {
				var controller = targets[i] as RVOController;
				if (controller.rvoAgent != null) {
					if (controller.rvoAgent.NeighbourCount >= controller.rvoAgent.MaxNeighbours) {
						maxNeighboursLimit = true;
					}
				}

				if (controller.simulator != null && controller.simulator.Multithreading && controller.debug) {
					debugAndMultithreading = true;
				}
			}

			if (maxNeighboursLimit) {
				EditorGUILayout.HelpBox("Limit of how many neighbours to consider (Max Neighbours) has been reached. Some agents within a distance of 'Neighbour Dist' may have been ignored. " +
					"To ensure all agents are taken into account you can raise the 'Max Neighbours' value at a cost to performance.", MessageType.Warning);
			}

			if (debugAndMultithreading) {
				EditorGUILayout.HelpBox("Debug mode can only be used when no multithreading is used. Set the 'Worker Threads' field on the RVOSimulator to 'None'", MessageType.Error);
			}
		}
	}
}
