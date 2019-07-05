using UnityEngine;
using System.Collections.Generic;
using Pathfinding;

namespace Pathfinding.RVO {
	/** Adds a navmesh as RVO obstacles.
	 * Add this to a scene in which has a navmesh based graph, when scanning (or loading from cache) the graph
	 * it will be added as RVO obstacles to the RVOSimulator (which must exist in the scene).
	 *
	 * \warning You should only have a single instance of this script in the scene, otherwise it will add duplicate
	 * obstacles and thereby increasing the CPU usage.
	 *
	 * \todo Support for grid based graphs will be added in future versions
	 *
	 * \astarpro
	 */
	[AddComponentMenu("Pathfinding/Local Avoidance/RVO Navmesh")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_r_v_o_1_1_r_v_o_navmesh.php")]
	public class RVONavmesh : GraphModifier {
		/** Height of the walls added for each obstacle edge.
		 * If a graph contains overlapping you should set this low enough so
		 * that edges on different levels do not interfere, but high enough so that
		 * agents cannot move over them by mistake.
		 */
		public float wallHeight = 5;

		/** Obstacles currently added to the simulator */
		readonly List<ObstacleVertex> obstacles = new List<ObstacleVertex>();

		/** Last simulator used */
		Simulator lastSim;

		public override void OnPostCacheLoad () {
			OnLatePostScan();
		}

		public override void OnLatePostScan () {
			if (!Application.isPlaying) return;

			RemoveObstacles();

			NavGraph[] graphs = AstarPath.active.graphs;

			RVOSimulator rvosim = FindObjectOfType<RVOSimulator>();
			if (rvosim == null) throw new System.NullReferenceException("No RVOSimulator could be found in the scene. Please add one to any GameObject");

			Pathfinding.RVO.Simulator sim = rvosim.GetSimulator();

			for (int i = 0; i < graphs.Length; i++) {
				var recast = graphs[i] as RecastGraph;
				if (recast != null) {
					foreach (var tile in recast.GetTiles()) {
						AddGraphObstacles(sim, tile);
					}
				} else {
					var navmesh = graphs[i] as INavmesh;
					if (navmesh != null) {
						AddGraphObstacles(sim, navmesh);
					}
				}
			}

			sim.UpdateObstacles();
		}

		/** Removes obstacles which were added with AddGraphObstacles */
		public void RemoveObstacles () {
			if (lastSim == null) return;

			Pathfinding.RVO.Simulator sim = lastSim;
			lastSim = null;

			for (int i = 0; i < obstacles.Count; i++) sim.RemoveObstacle(obstacles[i]);

			obstacles.Clear();
		}

		/** Adds obstacles for a graph */
		public void AddGraphObstacles (Pathfinding.RVO.Simulator sim, INavmesh ng) {
			if (obstacles.Count > 0 && lastSim != null && lastSim != sim) {
				Debug.LogError("Simulator has changed but some old obstacles are still added for the previous simulator. Deleting previous obstacles.");
				RemoveObstacles();
			}

			// Remember which simulator these obstacles were added to
			lastSim = sim;

			// Assume less than 20 vertices per node (actually assumes 3, but I will change that some day)
			var uses = new int[20];

			var outline = new Dictionary<int, int>();
			var vertexPositions = new Dictionary<int, Int3>();
			var hasInEdge = new HashSet<int>();

			ng.GetNodes(_node => {
				var node = _node as TriangleMeshNode;

				uses[0] = uses[1] = uses[2] = 0;

				if (node != null) {
				    // Find out which edges are shared with other nodes
					for (int j = 0; j < node.connections.Length; j++) {
						var other = node.connections[j] as TriangleMeshNode;

				        // Not necessarily a TriangleMeshNode
						if (other != null) {
							int a = node.SharedEdge(other);
							if (a != -1) uses[a] = 1;
						}
					}

				    // Loop through all edges on the node
					for (int j = 0; j < 3; j++) {
				        // The edge is not shared with any other node
				        // I.e it is an exterior edge on the mesh
						if (uses[j] == 0) {
							var i1 = j;
							var i2 = (j+1) % node.GetVertexCount();

							outline[node.GetVertexIndex(i1)] = node.GetVertexIndex(i2);
							hasInEdge.Add(node.GetVertexIndex(i2));
							vertexPositions[node.GetVertexIndex(i1)] = node.GetVertex(i1);
							vertexPositions[node.GetVertexIndex(i2)] = node.GetVertex(i2);
						}
					}
				}

				return true;
			});

			// Iterate through chains of the navmesh outline.
			// I.e segments of the outline that are not loops
			// we need to start these at the beginning of the chain.
			// Then iterate over all the loops of the outline.
			// Since they are loops, we can start at any point.
			for (int k = 0; k < 2; k++) {
				bool cycles = k == 1;

				foreach (int startIndex in new List<int>(outline.Keys)) {
					// Chains (not cycles) need to start at the start of the chain
					// Cycles can start at any point
					if (!cycles && hasInEdge.Contains(startIndex)) {
						continue;
					}

					var index = startIndex;
					var obstacleVertices = new List<Vector3>();

					obstacleVertices.Add((Vector3)vertexPositions[index]);

					while (outline.ContainsKey(index)) {
						var next = outline[index];
						outline.Remove(index);

						var v = (Vector3)vertexPositions[next];
						obstacleVertices.Add(v);

						if (next == startIndex) {
							break;
						}

						index = next;
					}

					if (obstacleVertices.Count > 1) {
						// TODO: Add layer
						sim.AddObstacle(obstacleVertices.ToArray(), wallHeight, cycles);
					}
				}
			}
		}
	}
}
