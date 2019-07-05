using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Util;

namespace Pathfinding {
	/** Helper for navmesh cut objects.
	 * Adding an instance of this component into the scene makes
	 * sure that NavmeshCut components update the recast graph correctly when they move around.
	 *
	 * \astarpro
	 */
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_tile_handler_helper.php")]
	public class TileHandlerHelper : MonoBehaviour {
		TileHandler handler;

		/** How often to check if an update needs to be done (real seconds between checks).
		 * For very large worlds with lots of NavmeshCut objects, it might be a performance penalty to do this check every frame.
		 * If you think this is a performance penalty, increase this number to check less often.
		 *
		 * For almost all games, this can be kept at 0.
		 *
		 * If negative, no updates will be done. They must be manually triggered using #ForceUpdate
		 */
		public float updateInterval;

		float lastUpdateTime = -999;

		readonly List<Bounds> forcedReloadBounds = new List<Bounds>();

		/** Use the specified handler, will create one at start if not called */
		public void UseSpecifiedHandler (TileHandler handler) {
			this.handler = handler;
		}

		void OnEnable () {
			NavmeshCut.OnDestroyCallback += HandleOnDestroyCallback;
			if (handler != null) {
				handler.graph.OnRecalculatedTiles += OnRecalculatedTiles;
			}
		}

		void OnDisable () {
			NavmeshCut.OnDestroyCallback -= HandleOnDestroyCallback;
			if (handler != null) {
				handler.graph.OnRecalculatedTiles -= OnRecalculatedTiles;
			}
		}

		/** Discards all pending updates caused by moved or modified navmesh cuts */
		public void DiscardPending () {
			List<NavmeshCut> cuts = NavmeshCut.GetAll();
			for (int i = 0; i < cuts.Count; i++) {
				if (cuts[i].RequiresUpdate()) {
					cuts[i].NotifyUpdated();
				}
			}
		}

		void Start () {
			if (FindObjectsOfType(typeof(TileHandlerHelper)).Length > 1) {
				Debug.LogError("There should only be one TileHandlerHelper per scene. Destroying.");
				Destroy(this);
				return;
			}

			if (handler == null) {
				if (AstarPath.active == null || AstarPath.active.astarData.recastGraph == null) {
					Debug.LogWarning("No AstarPath object in the scene or no RecastGraph on that AstarPath object");
				}

				var graph = AstarPath.active.astarData.recastGraph;
				handler = new TileHandler(graph);
				graph.OnRecalculatedTiles += OnRecalculatedTiles;
				handler.CreateTileTypesFromGraph();
			}
		}

		/** Called when some recast graph tiles have been completely recalculated */
		void OnRecalculatedTiles (RecastGraph.NavmeshTile[] tiles) {
			if (handler != null) {
				if (!handler.isValid) {
					handler = new TileHandler(handler.graph);
				}

				handler.OnRecalculatedTiles(tiles);
			}
		}

		/** Called when a NavmeshCut is destroyed */
		void HandleOnDestroyCallback (NavmeshCut obj) {
			forcedReloadBounds.Add(obj.LastBounds);
			lastUpdateTime = -999;
		}

		/** Update is called once per frame */
		void Update () {
			if (handler == null || ((updateInterval == -1 || Time.realtimeSinceStartup - lastUpdateTime < updateInterval) && handler.isValid) || AstarPath.active.isScanning) {
				return;
			}

			ForceUpdate();
		}

		/** Checks all NavmeshCut instances and updates graphs if needed.
		 * \note This schedules updates for all necessary tiles to happen as soon as possible.
		 * The pathfinding threads will continue to calculate the paths that they were calculating when this function
		 * was called and then they will be paused and the graph updates will be carried out (this may be several frames into the
		 * future and the graph updates themselves may take several frames to complete).
		 * If you want to force all navmesh cutting to be completed in a single frame call this method
		 * and immediately after call AstarPath.FlushWorkItems.
		 */
		public void ForceUpdate () {
			if (handler == null) {
				throw new System.Exception("Cannot update graphs. No TileHandler. Do not call this method in Awake.");
			}

			lastUpdateTime = Time.realtimeSinceStartup;

			// Get all navmesh cuts in the scene
			List<NavmeshCut> cuts = NavmeshCut.GetAll();

			if (!handler.isValid) {
				Debug.Log("TileHandler no longer matched the underlaying RecastGraph (possibly because of a graph scan). Recreating TileHandler...");
				handler = new TileHandler(handler.graph);
				handler.CreateTileTypesFromGraph();

				// Reload in huge bounds. Cannot use infinity because that will not convert well to integers
				// This will cause all tiles to be updated
				forcedReloadBounds.Add(new Bounds(Vector3.zero, new Vector3(10000000, 10000000, 10000000)));
			}

			if (forcedReloadBounds.Count == 0) {
				int any = 0;

				// Check if any navmesh cuts need updating
				for (int i = 0; i < cuts.Count; i++) {
					if (cuts[i].RequiresUpdate()) {
						any++;
						break;
					}
				}

				// Nothing needs to be done for now
				if (any == 0) return;
			}

			// Start batching tile updates which is good for performance
			// if we are updating a lot of them
			bool end = handler.StartBatchLoad();

			// Reload all tiles which touch the bounds in the forcedReloadBounds list
			for (int i = 0; i < forcedReloadBounds.Count; i++) {
				handler.ReloadInBounds(forcedReloadBounds[i]);
			}
			forcedReloadBounds.Clear();

			// Reload all bounds touching the previous bounds and current bounds
			// of navmesh cuts that have moved or changed in some other way
			for (int i = 0; i < cuts.Count; i++) {
				if (cuts[i].enabled) {
					if (cuts[i].RequiresUpdate()) {
						handler.ReloadInBounds(cuts[i].LastBounds);
						handler.ReloadInBounds(cuts[i].GetBounds());
					}
				} else if (cuts[i].RequiresUpdate()) {
					// The navmesh cut has been disabled
					// Make sure the tile where it was is updated
					handler.ReloadInBounds(cuts[i].LastBounds);
				}
			}

			// Notify navmesh cuts that they have been updated
			// This will cause RequiresUpdate to return false
			// until it is changed again
			for (int i = 0; i < cuts.Count; i++) {
				if (cuts[i].RequiresUpdate()) {
					cuts[i].NotifyUpdated();
				}
			}

			if (end) handler.EndBatchLoad();
		}
	}
}
