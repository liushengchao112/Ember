using Math = System.Math;

using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding {
	using Pathfinding.Voxels;
	using Pathfinding.Serialization;
	using Pathfinding.Recast;
	using Pathfinding.Util;
	using System.Threading;

	[System.Serializable]
	[JsonOptIn]
	/** Automatically generates navmesh graphs based on world geometry.
	 * The recast graph is based on Recast (http://code.google.com/p/recastnavigation/).\n
	 * I have translated a good portion of it to C# to run it natively in Unity. The Recast process is described as follows:
	 * - The voxel mold is build from the input triangle mesh by rasterizing the triangles into a multi-layer heightfield.
	 * Some simple filters are then applied to the mold to prune out locations where the character would not be able to move.
	 * - The walkable areas described by the mold are divided into simple overlayed 2D regions.
	 * The resulting regions have only one non-overlapping contour, which simplifies the final step of the process tremendously.
	 * - The navigation polygons are peeled off from the regions by first tracing the boundaries and then simplifying them.
	 * The resulting polygons are finally converted to convex polygons which makes them perfect for pathfinding and spatial reasoning about the level.
	 *
	 * It works exactly like that in the C# version as well, except that everything is triangulated to triangles instead of n-gons.
	 * The recast generation process usually works directly on the visiable geometry in the world, this is usually a good thing, because world geometry is usually more detailed than the colliders.
	 * You can however specify that colliders should be rasterized, if you have very detailed world geometry, this can speed up the scan.
	 *
	 * Check out the second part of the Get Started Tutorial which discusses recast graphs.
	 *
	 * \section export Exporting for manual editing
	 * In the editor there is a button for exporting the generated graph to a .obj file.
	 * Usually the generation process is good enough for the game directly, but in some cases you might want to edit some minor details.
	 * So you can export the graph to a .obj file, open it in your favourite 3D application, edit it, and export it to a mesh which Unity can import.
	 * You can then use that mesh in a navmesh graph.
	 *
	 * Since many 3D modelling programs use different axis systems (unity uses X=right, Y=up, Z=forward), it can be a bit tricky to get the rotation and scaling right.
	 * For blender for example, what you have to do is to first import the mesh using the .obj importer. Don't change anything related to axes in the settings.
	 * Then select the mesh, open the transform tab (usually the thin toolbar to the right of the 3D view) and set Scale -> Z to -1.
	 * If you transform it using the S (scale) hotkey, it seems to set both Z and Y to -1 for some reason.
	 * Then make the edits you need and export it as an .obj file to somewhere in the Unity project.
	 * But this time, edit the setting named "Forward" to "Z forward" (not -Z as it is per default).
	 *
	 * \shadowimage{recastgraph_graph.png}
	 * \shadowimage{recastgraph_inspector.png}
	 *
	 *
	 * \ingroup graphs
	 *
	 * \astarpro
	 */
	public class RecastGraph : NavGraph, INavmesh, IRaycastableGraph, IUpdatableGraph, INavmeshHolder {
		/** Enables graph updating.
		 * Uses more memory if enabled.
		 */
		public bool dynamic = true;


		[JsonMember]
		/** Radius of the agent which will traverse the navmesh.
		 * The navmesh will be eroded with this radius.
		 * \shadowimage{recast/character_radius.gif}
		 */
		public float characterRadius = 1.5F;

		/** Max distance from simplified edge to real edge.
		 * \shadowimage{recast/max_edge_error.gif}
		 */
		[JsonMember]
		public float contourMaxError = 2F;

		/** Voxel sample size (x,z).
		 * Lower values will yield higher quality navmeshes, however the graph will be slower to scan.
		 *
		 * \shadowimage{recast/cell_size.gif}
		 */
		[JsonMember]
		public float cellSize = 0.5F;

		/** Character height.
		 * \shadowimage{recast/walkable_height.gif}
		 */
		[JsonMember]
		public float walkableHeight = 2F;

		/** Height the character can climb.
		 * \shadowimage{recast/walkable_climb.gif}
		 */
		[JsonMember]
		public float walkableClimb = 0.5F;

		/** Max slope in degrees the character can traverse.
		 * \shadowimage{recast/max_slope.gif}
		 */
		[JsonMember]
		public float maxSlope = 30;

		/** Longer edges will be subdivided.
		 * Reducing this value can improve path quality since similarly sized polygons
		 * yield better paths than really large and really small next to each other
		 */
		[JsonMember]
		public float maxEdgeLength = 20;

		/** Minumum region size.
		 * Small regions will be removed from the navmesh.
		 * Measured in square world units (square meters in most games).
		 *
		 * \shadowimage{recast/min_region_size.gif}
		 *
		 * If a region is adjacent to a tile border, it will not be removed
		 * even though it is small since the adjacent tile might join it
		 * to form a larger region.
		 *
		 * \shadowimage{recast_minRegionSize_1.png}
		 * \shadowimage{recast_minRegionSize_2.png}
		 */
		[JsonMember]
		public float minRegionSize = 3;

		/** Size in voxels of a single tile.
		 * This is the width of the tile.
		 *
		 * A large tile size can be faster to initially scan (but beware of out of memory issues if you try with a too large tile size in a large world)
		 * smaller tile sizes are (much) faster to update.
		 *
		 * Different tile sizes can affect the quality of paths. It is often good to split up huge open areas into several tiles for
		 * better quality paths, but too small tiles can lead to effects looking like invisible obstacles.
		 */
		[JsonMember]
		public int editorTileSize = 128;

		/** Size of a tile along the X axis in voxels.
		 * \warning Do not modify, it is set from #editorTileSize at Scan
		 */
		[JsonMember]
		public int tileSizeX = 128;

		/** Size of a tile along the Z axis in voxels.
		 * \warning Do not modify, it is set from #editorTileSize at Scan
		 */
		[JsonMember]
		public int tileSizeZ = 128;

		/** Perform nearest node searches in XZ space only.
		 */
		[JsonMember]
		public bool nearestSearchOnlyXZ;


		/** If true, divide the graph into tiles, otherwise use a single tile covering the whole graph */
		[JsonMember]
		public bool useTiles;

		/** If true, scanning the graph will yield a completely empty graph.
		 * Useful if you want to replace the graph with a custom navmesh for example
		 */
		public bool scanEmptyGraph;

		public enum RelevantGraphSurfaceMode {
			DoNotRequire,
			OnlyForCompletelyInsideTile,
			RequireForAll
		}

		/** Require every region to have a RelevantGraphSurface component inside it.
		 * A RelevantGraphSurface component placed in the scene specifies that
		 * the navmesh region it is inside should be included in the navmesh.
		 *
		 * If this is set to OnlyForCompletelyInsideTile
		 * a navmesh region is included in the navmesh if it
		 * has a RelevantGraphSurface inside it, or if it
		 * is adjacent to a tile border. This can leave some small regions
		 * which you didn't want to have included because they are adjacent
		 * to tile borders, but it removes the need to place a component
		 * in every single tile, which can be tedious (see below).
		 *
		 * If this is set to RequireForAll
		 * a navmesh region is included only if it has a RelevantGraphSurface
		 * inside it. Note that even though the navmesh
		 * looks continous between tiles, the tiles are computed individually
		 * and therefore you need a RelevantGraphSurface component for each
		 * region and for each tile.
		 *
		 *
		 *
		 * \shadowimage{relevantgraphsurface/dontreq.png}
		 * In the above image, the mode OnlyForCompletelyInsideTile was used. Tile borders
		 * are highlighted in black. Note that since all regions are adjacent to a tile border,
		 * this mode didn't remove anything in this case and would give the same result as DoNotRequire.
		 * The RelevantGraphSurface component is shown using the green gizmo in the top-right of the blue plane.
		 *
		 * \shadowimage{relevantgraphsurface/no_tiles.png}
		 * In the above image, the mode RequireForAll was used. No tiles were used.
		 * Note that the small region at the top of the orange cube is now gone, since it was not the in the same
		 * region as the relevant graph surface component.
		 * The result would have been identical with OnlyForCompletelyInsideTile since there are no tiles (or a single tile, depending on how you look at it).
		 *
		 * \shadowimage{relevantgraphsurface/req_all.png}
		 * The mode RequireForAll was used here. Since there is only a single RelevantGraphSurface component, only the region
		 * it was in, in the tile it is placed in, will be enabled. If there would have been several RelevantGraphSurface in other tiles,
		 * those regions could have been enabled as well.
		 *
		 * \shadowimage{relevantgraphsurface/tiled_uneven.png}
		 * Here another tile size was used along with the OnlyForCompletelyInsideTile.
		 * Note that the region on top of the orange cube is gone now since the region borders do not intersect that region (and there is no
		 * RelevantGraphSurface component inside it).
		 *
		 * \note When not using tiles. OnlyForCompletelyInsideTile is equivalent to RequireForAll.
		 */
		[JsonMember]
		public RelevantGraphSurfaceMode relevantGraphSurfaceMode = RelevantGraphSurfaceMode.DoNotRequire;

		[JsonMember]
		/** Use colliders to calculate the navmesh */
		public bool rasterizeColliders;

		[JsonMember]
		/** Use scene meshes to calculate the navmesh */
		public bool rasterizeMeshes = true;

		/** Include the Terrain in the scene. */
		[JsonMember]
		public bool rasterizeTerrain = true;

		/** Rasterize tree colliders on terrains.
		 *
		 * If the tree prefab has a collider, that collider will be rasterized.
		 * Otherwise a simple box collider will be used and the script will
		 * try to adjust it to the tree's scale, it might not do a very good job though so
		 * an attached collider is preferable.
		 *
		 * \see rasterizeTerrain
		 * \see colliderRasterizeDetail
		 */
		[JsonMember]
		public bool rasterizeTrees = true;

		/** Controls detail on rasterization of sphere and capsule colliders.
		 * This controls the number of rows and columns on the generated meshes.
		 * A higher value does not necessarily increase quality of the mesh, but a lower
		 * value will often speed it up.
		 *
		 * You should try to keep this value as low as possible without affecting the mesh quality since
		 * that will yield the fastest scan times.
		 *
		 * \see rasterizeColliders
		 */
		[JsonMember]
		public float colliderRasterizeDetail = 10;

		/** Center of the bounding box.
		 * Scanning will only be done inside the bounding box */
		[JsonMember]
		public Vector3 forcedBoundsCenter;

		/** Size of the bounding box. */
		[JsonMember]
		public Vector3 forcedBoundsSize = new Vector3(100, 40, 100);

		/** Layer mask which filters which objects to include.
		 * \see tagMask
		 */
		[JsonMember]
		public LayerMask mask = -1;

		/** Objects tagged with any of these tags will be rasterized.
		 * Note that this extends the layer mask, so if you only want to use tags, set #mask to 'Nothing'.
		 *
		 * \see mask
		 */
		[JsonMember]
		public List<string> tagMask = new List<string>();

		/** Show an outline of the polygons in the Unity Editor */
		[JsonMember]
		public bool showMeshOutline = true;

		/** Show the connections between the polygons in the Unity Editor */
		[JsonMember]
		public bool showNodeConnections;

		/** Show the surface of the navmesh */
		[JsonMember]
		public bool showMeshSurface;

		/** Controls how large the sample size for the terrain is.
		 * A higher value is faster to scan but less accurate
		 */
		[JsonMember]
		public int terrainSampleSize = 3;

		/** Called when tiles have been completely recalculated.
		 * This is called after scanning the graph and after
		 * performing graph updates that completely recalculate tiles
		 * (not ones that simply modify e.g penalties).
		 * It is not called after NavmeshCut updates.
		 */
		public event System.Action<NavmeshTile[]> OnRecalculatedTiles;

		private Voxelize globalVox;

		/** World bounds for the graph.
		 * Defined as a bounds object with size #forcedBoundsSize and centered at #forcedBoundsCenter
		 */
		public Bounds forcedBounds {
			get {
				return new Bounds(forcedBoundsCenter, forcedBoundsSize);
			}
		}

		/** Number of tiles along the X-axis */
		public int tileXCount;
		/** Number of tiles along the Z-axis */
		public int tileZCount;

		/** All tiles.
		 * A tile can be got from a tile coordinate as tiles[x + z*tileXCount]
		 */
		NavmeshTile[] tiles;

		/** Currently updating tiles in a batch */
		bool batchTileUpdate;

		/** List of tiles updating during batch */
		List<int> batchUpdatedTiles = new List<int>();

		/** List of tiles that have been calculated in a graph update, but have not yet been added to the graph.
		 * When updating the graph in a separate thread, large changes cannot be made directly to the graph
		 * as other scripts might use the graph data structures at the same time in another thread.
		 * So the tiles are calculated, but they are not yet connected to the existing tiles
		 * that will be done in UpdateAreaPost which runs in the Unity thread.
		 */
		List<NavmeshTile> stagingTiles = new List<NavmeshTile>();

#if ASTAR_RECAST_LARGER_TILES
		// Larger tiles
		public const int VertexIndexMask = 0xFFFFF;

		public const int TileIndexMask = 0x7FF;
		public const int TileIndexOffset = 20;
#else
		// Larger worlds
		public const int VertexIndexMask = 0xFFF;

		public const int TileIndexMask = 0x7FFFF;
		public const int TileIndexOffset = 12;
#endif

		public const int BorderVertexMask = 1;
		public const int BorderVertexOffset = 31;

		public class NavmeshTile : INavmeshHolder, INavmesh {
			/** Tile triangles */
			public int[] tris;

			/** Tile vertices */
			public Int3[] verts;

			/** Tile X Coordinate */
			public int x;

			/** Tile Z Coordinate */
			public int z;

			/** Width, in tile coordinates.
			 * \warning Widths other than 1 are not supported. This is mainly here for possible future features.
			 */
			public int w;

			/** Depth, in tile coordinates.
			 * \warning Depths other than 1 are not supported. This is mainly here for possible future features.
			 */
			public int d;

			/** All nodes in the tile */
			public TriangleMeshNode[] nodes;

			/** Bounding Box Tree for node lookups */
			public BBTree bbTree;

			/** Temporary flag used for batching */
			public bool flag;

			public void GetTileCoordinates (int tileIndex, out int x, out int z) {
				x = this.x;
				z = this.z;
			}

			public int GetVertexArrayIndex (int index) {
				return index & VertexIndexMask;
			}

			/** Get a specific vertex in the tile */
			public Int3 GetVertex (int index) {
				int idx = index & VertexIndexMask;

				return verts[idx];
			}

			public void GetNodes (GraphNodeDelegateCancelable del) {
				if (nodes == null) return;
				for (int i = 0; i < nodes.Length && del(nodes[i]); i++) {}
			}
		}

		/** Tile at the specified x, z coordinate pair.
		 * The first tile is at (0,0), the last tile at (tileXCount-1, tileZCount-1).
		 */
		public NavmeshTile GetTile (int x, int z) {
			return tiles[x + z * tileXCount];
		}

		/** Gets the vertex coordinate for the specified index.
		 *
		 * \throws IndexOutOfRangeException if the vertex index is invalid.
		 * \throws NullReferenceException if the tile the vertex is in is not calculated.
		 *
		 * \see NavmeshTile.GetVertex
		 */
		public Int3 GetVertex (int index) {
			int tileIndex = (index >> TileIndexOffset) & TileIndexMask;

			return tiles[tileIndex].GetVertex(index);
		}

		/** Returns a tile index from a vertex index */
		public int GetTileIndex (int index) {
			return (index >> TileIndexOffset) & TileIndexMask;
		}

		public int GetVertexArrayIndex (int index) {
			return index & VertexIndexMask;
		}

		/** Returns tile coordinates from a tile index */
		public void GetTileCoordinates (int tileIndex, out int x, out int z) {
			//z = System.Math.DivRem (tileIndex, tileXCount, out x);
			z = tileIndex/tileXCount;
			x = tileIndex - z*tileXCount;
		}

		/** Get all tiles.
		 * \warning Do not modify this array
		 */
		public NavmeshTile[] GetTiles () {
			return tiles;
		}

		/** Returns an XZ bounds object with the bounds of a group of tiles.
		 * The bounds object is defined in world units.
		 */
		public Bounds GetTileBounds (IntRect rect) {
			return GetTileBounds(rect.xmin, rect.ymin, rect.Width, rect.Height);
		}

		/** Returns an XZ bounds object with the bounds of a group of tiles.
		 * The bounds object is defined in world units.
		 */
		public Bounds GetTileBounds (int x, int z, int width = 1, int depth = 1) {
			var b = new Bounds();

			b.SetMinMax(
				new Vector3(x*tileSizeX*cellSize, 0, z*tileSizeZ*cellSize) + forcedBounds.min,
				new Vector3((x+width)*tileSizeX*cellSize, forcedBounds.size.y, (z+depth)*tileSizeZ*cellSize) + forcedBounds.min
				);
			return b;
		}

		/** Returns the tile coordinate which contains the point \a p.
		 * Is not necessarily a valid tile (i.e it could be out of bounds).
		 */
		public Int2 GetTileCoordinates (Vector3 p) {
			p -= forcedBounds.min;
			p.x /= cellSize*tileSizeX;
			p.z /= cellSize*tileSizeZ;
			return new Int2((int)p.x, (int)p.z);
		}

		public override void OnDestroy () {
			base.OnDestroy();

			// Cleanup
			TriangleMeshNode.SetNavmeshHolder(active.astarData.GetGraphIndex(this), null);
		}

		/** Relocates the nodes in this graph.
		 * Assumes the nodes are already transformed using the "oldMatrix", then transforms them
		 * such that it will look like they have only been transformed using the "newMatrix".
		 *
		 * The matrix the graph is transformed with is typically stored in the #matrix field, so the typical usage for this method is
		 * \code
		 * var myNewMatrix = Matrix4x4.TRS (...);
		 * myGraph.RelocateNodes (myGraph.matrix, myNewMatrix);
		 * \endcode
		 *
		 * So for example if you want to move all your nodes in e.g a recast graph 10 units along the X axis from the initial position
		 * \code
		 * var graph = AstarPath.astarData.recastGraph;
		 * var m = Matrix4x4.TRS (new Vector3(10,0,0), Quaternion.identity, Vector3.one);
		 * graph.RelocateNodes (graph.matrix, m);
		 * \endcode
		 *
		 * \warning This method is lossy, so calling it many times may cause node positions to lose precision.
		 * For example if you set the scale to 0 in one call, and then to 1 in the next call, it will not be able to
		 * recover the correct positions since when the scale was 0, all nodes were scaled/moved to the same point.
		 * The same thing happens for other - less extreme - values as well, but to a lesser degree.
		 *
		 * \version Prior to version 3.6.1 the oldMatrix and newMatrix parameters were reversed by mistake.
		 */
		public override void RelocateNodes (Matrix4x4 oldMatrix, Matrix4x4 newMatrix) {
			// Move all the vertices in each tile
			if (tiles != null) {
				Matrix4x4 inv = oldMatrix.inverse;
				Matrix4x4 m = newMatrix * inv;

				if (tiles.Length > 1) {
					throw new System.Exception("RelocateNodes cannot be used on tiled recast graphs");
				}

				for (int tileIndex = 0; tileIndex < tiles.Length; tileIndex++) {
					var tile = tiles[tileIndex];
					if (tile != null) {
						var tileVerts = tile.verts;
						for (int vertexIndex = 0; vertexIndex < tileVerts.Length; vertexIndex++) {
							tileVerts[vertexIndex] = ((Int3)m.MultiplyPoint((Vector3)tileVerts[vertexIndex]));
						}

						for (int nodeIndex = 0; nodeIndex < tile.nodes.Length; nodeIndex++) {
							var node = tile.nodes[nodeIndex];
							node.UpdatePositionFromVertices();
						}
						tile.bbTree.RebuildFrom(tile.nodes);
					}
				}
			}

			SetMatrix(newMatrix);
		}

		/** Creates a single new empty tile */
		static NavmeshTile NewEmptyTile (int x, int z) {
			return new NavmeshTile {
					   x = x,
					   z = z,
					   w = 1,
					   d = 1,
					   verts = new Int3[0],
					   tris = new int[0],
					   nodes = new TriangleMeshNode[0],
					   bbTree = ObjectPool<BBTree>.Claim()
			};
		}

		public override void GetNodes (GraphNodeDelegateCancelable del) {
			if (tiles == null) return;

			for (int i = 0; i < tiles.Length; i++) {
				if (tiles[i] == null || tiles[i].x+tiles[i].z*tileXCount != i) continue;
				TriangleMeshNode[] nodes = tiles[i].nodes;

				if (nodes == null) continue;

				for (int j = 0; j < nodes.Length && del(nodes[j]); j++) {}
			}
		}

		/** Returns the closest point of the node */
		[System.Obsolete("Use node.ClosestPointOnNode instead")]
		public Vector3 ClosestPointOnNode (TriangleMeshNode node, Vector3 pos) {
			return Polygon.ClosestPointOnTriangle((Vector3)GetVertex(node.v0), (Vector3)GetVertex(node.v1), (Vector3)GetVertex(node.v2), pos);
		}

		/** Returns if the point is inside the node in XZ space */
		[System.Obsolete("Use node.ContainsPoint instead")]
		public bool ContainsPoint (TriangleMeshNode node, Vector3 pos) {
			if (VectorMath.IsClockwiseXZ((Vector3)GetVertex(node.v0), (Vector3)GetVertex(node.v1), pos)
				&& VectorMath.IsClockwiseXZ((Vector3)GetVertex(node.v1), (Vector3)GetVertex(node.v2), pos)
				&& VectorMath.IsClockwiseXZ((Vector3)GetVertex(node.v2), (Vector3)GetVertex(node.v0), pos)) {
				return true;
			}
			return false;
		}

		/** Changes the bounds of the graph to precisely encapsulate all objects in the scene that can be included in the scanning process based on the settings.
		 * Which objects are used depends on the settings. If an object would have affected the graph with the current settings if it would have
		 * been inside the bounds of the graph, it will be detected and the bounds will be expanded to contain that object.
		 *
		 * This method corresponds to the 'Snap bounds to scene' button in the inspector.
		 *
		 * \see rasterizeMeshes
		 * \see rasterizeTerrain
		 * \see rasterizeColliders
		 * \see mask
		 * \see tagMask
		 *
		 * \see forcedBoundsCenter
		 * \see forcedBoundsSize
		 */
		public void SnapForceBoundsToScene () {
			var meshes = CollectMeshes(new Bounds(Vector3.zero, new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity)));

			if (meshes.Count == 0) {
				return;
			}

			var bounds = meshes[0].bounds;

			for (int i = 1; i < meshes.Count; i++) {
				bounds.Encapsulate(meshes[i].bounds);
			}

			forcedBoundsCenter = bounds.center;
			forcedBoundsSize = bounds.size;
		}

		/** Returns a rect containing the indices of all tiles touching the specified bounds */
		public IntRect GetTouchingTiles (Bounds b) {
			b.center -= forcedBounds.min;

			//Calculate world bounds of all affected tiles
			var r = new IntRect(Mathf.FloorToInt(b.min.x / (tileSizeX*cellSize)), Mathf.FloorToInt(b.min.z / (tileSizeZ*cellSize)), Mathf.FloorToInt(b.max.x / (tileSizeX*cellSize)), Mathf.FloorToInt(b.max.z / (tileSizeZ*cellSize)));
			//Clamp to bounds
			r = IntRect.Intersection(r, new IntRect(0, 0, tileXCount-1, tileZCount-1));
			return r;
		}

		/** Returns a rect containing the indices of all tiles by rounding the specified bounds to tile borders.
		 * This is different from GetTouchingTiles in that the tiles inside the rectangle returned from this method
		 * may not contain the whole bounds, while that is guaranteed for GetTouchingTiles.
		 */
		public IntRect GetTouchingTilesRound (Bounds b) {
			b.center -= forcedBounds.min;

			//Calculate world bounds of all affected tiles
			var r = new IntRect(Mathf.RoundToInt(b.min.x / (tileSizeX*cellSize)), Mathf.RoundToInt(b.min.z / (tileSizeZ*cellSize)), Mathf.RoundToInt(b.max.x / (tileSizeX*cellSize))-1, Mathf.RoundToInt(b.max.z / (tileSizeZ*cellSize))-1);
			//Clamp to bounds
			r = IntRect.Intersection(r, new IntRect(0, 0, tileXCount-1, tileZCount-1));
			return r;
		}

		GraphUpdateThreading IUpdatableGraph.CanUpdateAsync (GraphUpdateObject o) {
			return o.updatePhysics ? GraphUpdateThreading.UnityInit | GraphUpdateThreading.SeparateThread | GraphUpdateThreading.UnityPost : GraphUpdateThreading.SeparateThread;
		}

		void IUpdatableGraph.UpdateAreaInit (GraphUpdateObject o) {
			if (!o.updatePhysics) {
				return;
			}

			if (!dynamic) {
				throw new System.Exception("Recast graph must be marked as dynamic to enable graph updates");
			}

			AstarProfiler.Reset();
			AstarProfiler.StartProfile("UpdateAreaInit");
			AstarProfiler.StartProfile("CollectMeshes");

			RelevantGraphSurface.UpdateAllPositions();

			//Calculate world bounds of all affected tiles
			IntRect touchingTiles = GetTouchingTiles(o.bounds);
			Bounds tileBounds = GetTileBounds(touchingTiles);

			// Expand TileBorderSizeInWorldUnits voxels on each side
			tileBounds.Expand(new Vector3(1, 0, 1)*TileBorderSizeInWorldUnits*2);

			var meshes = CollectMeshes(tileBounds);

			if (globalVox == null) {
				// Create the voxelizer and set all settings
				globalVox = new Voxelize(CellHeight, cellSize, walkableClimb, walkableHeight, maxSlope);
				globalVox.maxEdgeLength = maxEdgeLength;
			}

			globalVox.inputMeshes = meshes;

			AstarProfiler.EndProfile("CollectMeshes");
			AstarProfiler.EndProfile("UpdateAreaInit");
		}

		void IUpdatableGraph.UpdateArea (GraphUpdateObject guo) {
			// Figure out which tiles are affected
			var r = GetTouchingTiles(guo.bounds);

			if (!guo.updatePhysics) {
				for (int z = r.ymin; z <= r.ymax; z++) {
					for (int x = r.xmin; x <= r.xmax; x++) {
						NavmeshTile tile = tiles[z*tileXCount + x];
						NavMeshGraph.UpdateArea(guo, tile);
					}
				}
				return;
			}

			if (!dynamic) {
				throw new System.Exception("Recast graph must be marked as dynamic to enable graph updates with updatePhysics = true");
			}

			Voxelize vox = globalVox;

			if (vox == null) {
				throw new System.InvalidOperationException("No Voxelizer object. UpdateAreaInit should have been called before this function.");
			}

			AstarProfiler.StartProfile("Build Tiles");

			// Build the new tiles
			for (int x = r.xmin; x <= r.xmax; x++) {
				for (int z = r.ymin; z <= r.ymax; z++) {
					stagingTiles.Add(BuildTileMesh(vox, x, z));
				}
			}

			uint graphIndex = (uint)AstarPath.active.astarData.GetGraphIndex(this);

			// Set the correct graph index
			for (int i = 0; i < stagingTiles.Count; i++) {
				NavmeshTile tile = stagingTiles[i];
				GraphNode[] nodes = tile.nodes;

				for (int j = 0; j < nodes.Length; j++) nodes[j].GraphIndex = graphIndex;
			}

			AstarProfiler.EndProfile("Build Tiles");
		}

		/** Called on the Unity thread to complete a graph update */
		void IUpdatableGraph.UpdateAreaPost (GraphUpdateObject guo) {
			UnityEngine.Profiling.Profiler.BeginSample("RemoveConnections");
			// Remove connections from existing tiles destroy the nodes
			// Replace the old tile by the new tile
			for (int i = 0; i < stagingTiles.Count; i++) {
				var tile = stagingTiles[i];
				int index = tile.x + tile.z * tileXCount;
				var oldTile = tiles[index];

				// Destroy the previous nodes
				for (int j = 0; j < oldTile.nodes.Length; j++) {
					oldTile.nodes[j].Destroy();
				}

				tiles[index] = tile;
			}

			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("Connect With Neighbours");
			// Connect the new tiles with their neighbours
			for (int i = 0; i < stagingTiles.Count; i++) {
				var tile = stagingTiles[i];
				ConnectTileWithNeighbours(tile, false);
			}

			// This may be used to update the tile again to take into
			// account NavmeshCut components.
			// It is not the super efficient, but it works.
			// Usually you only use either normal graph updates OR navmesh
			// cutting, not both.
			if (OnRecalculatedTiles != null) {
				OnRecalculatedTiles(stagingTiles.ToArray());
			}

			stagingTiles.Clear();
			UnityEngine.Profiling.Profiler.EndSample();
		}

		void ConnectTileWithNeighbours (NavmeshTile tile, bool onlyUnflagged = false) {
			if (tile.w != 1 || tile.d != 1) {
				throw new System.ArgumentException("Tile widths or depths other than 1 are not supported. The fields exist mainly for possible future expansions.");
			}

			// Loop through z and x offsets to adjacent tiles
			// _ x _
			// x _ x
			// _ x _
			for (int zo = -1; zo <= 1; zo++) {
				var z = tile.z + zo;
				if (z < 0 || z >= tileZCount) continue;

				for (int xo = -1; xo <= 1; xo++) {
					var x = tile.x + xo;
					if (x < 0 || x >= tileXCount) continue;

					// Ignore diagonals and the tile itself
					if ((xo == 0) == (zo == 0)) continue;

					var otherTile = tiles[x + z*tileXCount];
					if (!onlyUnflagged || !otherTile.flag) {
						ConnectTiles(otherTile, tile);
					}
				}
			}
		}

		void RemoveConnectionsFromTile (NavmeshTile tile) {
			if (tile.x > 0) {
				int x = tile.x-1;
				for (int z = tile.z; z < tile.z+tile.d; z++) RemoveConnectionsFromTo(tiles[x + z*tileXCount], tile);
			}
			if (tile.x+tile.w < tileXCount) {
				int x = tile.x+tile.w;
				for (int z = tile.z; z < tile.z+tile.d; z++) RemoveConnectionsFromTo(tiles[x + z*tileXCount], tile);
			}
			if (tile.z > 0) {
				int z = tile.z-1;
				for (int x = tile.x; x < tile.x+tile.w; x++) RemoveConnectionsFromTo(tiles[x + z*tileXCount], tile);
			}
			if (tile.z+tile.d < tileZCount) {
				int z = tile.z+tile.d;
				for (int x = tile.x; x < tile.x+tile.w; x++) RemoveConnectionsFromTo(tiles[x + z*tileXCount], tile);
			}
		}

		void RemoveConnectionsFromTo (NavmeshTile a, NavmeshTile b) {
			if (a == null || b == null) return;
			//Same tile, possibly from a large tile (one spanning several x,z tile coordinates)
			if (a == b) return;

			int tileIdx = b.x + b.z*tileXCount;

			for (int i = 0; i < a.nodes.Length; i++) {
				TriangleMeshNode node = a.nodes[i];
				if (node.connections == null) continue;
				for (int j = 0;; j++) {
					//Length will not be constant if connections are removed
					if (j >= node.connections.Length) break;

					var other = node.connections[j] as TriangleMeshNode;

					//Only evaluate TriangleMeshNodes
					if (other == null) continue;

					int tileIdx2 = other.GetVertexIndex(0);
					tileIdx2 = (tileIdx2 >> TileIndexOffset) & TileIndexMask;

					if (tileIdx2 == tileIdx) {
						node.RemoveConnection(node.connections[j]);
						j--;
					}
				}
			}
		}

		public override NNInfoInternal GetNearest (Vector3 position, NNConstraint constraint, GraphNode hint) {
			return GetNearestForce(position, null);
		}

		public override NNInfoInternal GetNearestForce (Vector3 position, NNConstraint constraint) {
			if (tiles == null) return new NNInfoInternal();

			Vector3 localPosition = position - forcedBounds.min;
			int tx = Mathf.FloorToInt(localPosition.x / (cellSize*tileSizeX));
			int tz = Mathf.FloorToInt(localPosition.z / (cellSize*tileSizeZ));

			// Clamp to graph borders
			tx = Mathf.Clamp(tx, 0, tileXCount-1);
			tz = Mathf.Clamp(tz, 0, tileZCount-1);

			int wmax = Math.Max(tileXCount, tileZCount);

			var best = new NNInfoInternal();
			float bestDistance = float.PositiveInfinity;

			bool xzSearch = nearestSearchOnlyXZ || (constraint != null && constraint.distanceXZ);

			// Search outwards in a diamond pattern from the closest tile
			for (int w = 0; w < wmax; w++) {
				if (!xzSearch && bestDistance < (w-1)*cellSize*Math.Max(tileSizeX, tileSizeZ)) break;

				int zmax = Math.Min(w+tz +1, tileZCount);
				for (int z = Math.Max(-w+tz, 0); z < zmax; z++) {
					// Solve for z such that abs(x-tx) + abs(z-tx) == w
					// Delta X coordinate
					int dx = Math.Abs(w - Math.Abs(z-tz));
					// Solution is dx + tx and -dx + tx

					// First solution negative delta x
					if (-dx + tx >= 0) {
						// Absolute x coordinate
						int x = -dx + tx;
						NavmeshTile tile = tiles[x + z*tileXCount];

						if (tile != null) {
							if (xzSearch) {
								best = tile.bbTree.QueryClosestXZ(position, constraint, ref bestDistance, best);
								if (bestDistance < float.PositiveInfinity) break;
							} else {
								best = tile.bbTree.QueryClosest(position, constraint, ref bestDistance, best);
							}
						}
					}

					// Other solution, make sure it is not the same solution by checking x != 0
					if (dx != 0 && dx + tx < tileXCount) {
						// Absolute x coordinate
						int x = dx + tx;
						NavmeshTile tile = tiles[x + z*tileXCount];
						if (tile != null) {
							if (xzSearch) {
								best = tile.bbTree.QueryClosestXZ(position, constraint, ref bestDistance, best);
								if (bestDistance < float.PositiveInfinity) break;
							} else {
								best = tile.bbTree.QueryClosest(position, constraint, ref bestDistance, best);
							}
						}
					}
				}
			}

			best.node = best.constrainedNode;
			best.constrainedNode = null;
			best.clampedPosition = best.constClampedPosition;

			return best;
		}

		/** Finds the first node which contains \a position.
		 * "Contains" is defined as \a position is inside the triangle node when seen from above. So only XZ space matters.
		 * In case of a multilayered environment, which node of the possibly several nodes
		 * containing the point is undefined.
		 *
		 * Returns null if there was no node containing the point. This serves as a quick
		 * check for "is this point on the navmesh or not".
		 *
		 * Note that the behaviour of this method is distinct from the GetNearest method.
		 * The GetNearest method will return the closest node to a point,
		 * which is not necessarily the one which contains it in XZ space.
		 *
		 * \see GetNearest
		 */
		public GraphNode PointOnNavmesh (Vector3 position, NNConstraint constraint) {
			if (tiles == null) return null;

			Vector3 localPosition = position - forcedBounds.min;
			int tx = Mathf.FloorToInt(localPosition.x / (cellSize*tileSizeX));
			int tz = Mathf.FloorToInt(localPosition.z / (cellSize*tileSizeZ));

			// Graph borders
			if (tx < 0 || tz < 0 || tx >= tileXCount || tz >= tileZCount) return null;

			NavmeshTile tile = tiles[tx + tz*tileXCount];

			if (tile != null) {
				GraphNode node = tile.bbTree.QueryInside(position, constraint);
				return node;
			}

			return null;
		}

		public override IEnumerable<Progress> ScanInternal () {
			AstarProfiler.Reset();
			AstarProfiler.StartProfile("Base Scan");
			//AstarProfiler.InitializeFastProfile (new string[] {"Rasterize", "Rasterize Inner 1", "Rasterize Inner 2", "Rasterize Inner 3"});

			TriangleMeshNode.SetNavmeshHolder(AstarPath.active.astarData.GetGraphIndex(this), this);


			foreach (var p in ScanAllTiles()) {
				yield return p;
			}


#if DEBUG_REPLAY
			DebugReplay.WriteToFile();
#endif
			AstarProfiler.PrintFastResults();
			AstarProfiler.PrintResults();
		}

		void InitializeTileInfo () {
			// Voxel grid size
			int totalVoxelWidth = Mathf.Max((int)(forcedBounds.size.x/cellSize + 0.5f), 1);
			int totalVoxelDepth = Mathf.Max((int)(forcedBounds.size.z/cellSize + 0.5f), 1);

			if (!useTiles) {
				tileSizeX = totalVoxelWidth;
				tileSizeZ = totalVoxelDepth;
			} else {
				tileSizeX = editorTileSize;
				tileSizeZ = editorTileSize;
			}

			// Number of tiles
			tileXCount = (totalVoxelWidth + tileSizeX-1) / tileSizeX;
			tileZCount = (totalVoxelDepth + tileSizeZ-1) / tileSizeZ;

			if (tileXCount * tileZCount > TileIndexMask+1) {
				throw new System.Exception("Too many tiles ("+(tileXCount * tileZCount)+") maximum is "+(TileIndexMask+1)+
					"\nTry disabling ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* inspector.");
			}

			tiles = new NavmeshTile[tileXCount*tileZCount];
		}

		void BuildTiles (Queue<Int2> tileQueue, List<RasterizationMesh>[] meshBuckets, ManualResetEvent doneEvent, int threadIndex) {
			try {
				// Create the voxelizer and set all settings
				var vox = new Voxelize(CellHeight, cellSize, walkableClimb, walkableHeight, maxSlope);
				vox.maxEdgeLength = maxEdgeLength;

				while (true) {
					Int2 tile;
					lock (tileQueue) {
						if (tileQueue.Count == 0) {
							return;
						}

						tile = tileQueue.Dequeue();
					}

					vox.inputMeshes = meshBuckets[tile.x + tile.y*tileXCount];
					tiles[tile.x + tile.y*tileXCount] = BuildTileMesh(vox, tile.x, tile.y, threadIndex);
				}
			} catch (System.Exception e) {
				Debug.LogException(e);
			} finally {
				if (doneEvent != null) doneEvent.Set();
			}
		}

		void ConnectTiles (Queue<Int2> tileQueue, ManualResetEvent doneEvent) {
			try {
				while (true) {
					Int2 tile;
					lock (tileQueue) {
						if (tileQueue.Count == 0) {
							return;
						}

						tile = tileQueue.Dequeue();
					}

					// Connect with tile at (x+1,z) and (x,z+1)
					if (tile.x < tileXCount - 1)
						ConnectTiles(tiles[tile.x + tile.y * tileXCount], tiles[tile.x + 1 + tile.y * tileXCount]);
					if (tile.y < tileZCount - 1)
						ConnectTiles(tiles[tile.x + tile.y * tileXCount], tiles[tile.x + (tile.y + 1) * tileXCount]);
				}
			} catch (System.Exception e) {
				Debug.LogException(e);
			} finally {
				// See BuildTiles
				if (doneEvent != null) doneEvent.Set();
			}
		}

		/** Creates a list for every tile and adds every mesh that touches a tile to the corresponding list */
		List<RasterizationMesh>[] PutMeshesIntoTileBuckets (List<RasterizationMesh> meshes) {
			var result = new List<RasterizationMesh>[tiles.Length];
			var borderExpansion = new Vector3(1, 0, 1)*TileBorderSizeInWorldUnits*2;

			for (int i = 0; i < result.Length; i++) {
				result[i] = new List<RasterizationMesh>();
			}

			for (int i = 0; i < meshes.Count; i++) {
				var mesh = meshes[i];
				var bounds = mesh.bounds;
				// Expand borderSize voxels on each side
				bounds.Expand(borderExpansion);

				var rect = GetTouchingTiles(bounds);
				for (int z = rect.ymin; z <= rect.ymax; z++) {
					for (int x = rect.xmin; x <= rect.xmax; x++) {
						result[x + z*tileXCount].Add(mesh);
					}
				}
			}

			return result;
		}

		protected IEnumerable<Progress> ScanAllTiles () {
			InitializeTileInfo();

			// If this is true, just fill the graph with empty tiles
			if (scanEmptyGraph) {
				FillWithEmptyTiles();
				yield break;
			}

			yield return new Progress(0, "Finding Meshes");
			var meshes = CollectMeshes(forcedBounds);

			// A walkableClimb higher than walkableHeight can cause issues when generating the navmesh since then it can in some cases
			// Both be valid for a character to walk under an obstacle and climb up on top of it (and that cannot be handled with navmesh without links)
			// The editor scripts also enforce this but we enforce it here too just to be sure
			walkableClimb = Mathf.Min(walkableClimb, walkableHeight);

			var buckets = PutMeshesIntoTileBuckets(meshes);

			Queue<Int2> tileQueue = new Queue<Int2>();

			// Put all tiles in the queue
			for (int z = 0; z < tileZCount; z++) {
				for (int x = 0; x < tileXCount; x++) {
					tileQueue.Enqueue(new Int2(x, z));
				}
			}

#if UNITY_WEBGL && !UNITY_EDITOR
			// WebGL does not support multithreading so we will do everything synchronously instead
			BuildTiles(tileQueue, buckets, null, 0);
#else
			// Fire up a bunch of threads to scan the graph in parallel
			int threadCount = Mathf.Min(tileQueue.Count, Mathf.Max(1, AstarPath.CalculateThreadCount(ThreadCount.AutomaticHighLoad)));
			var waitEvents = new ManualResetEvent[threadCount];

			for (int i = 0; i < waitEvents.Length; i++) {
				waitEvents[i] = new ManualResetEvent(false);
				ThreadPool.QueueUserWorkItem(state => BuildTiles(tileQueue, buckets, waitEvents[(int)state], (int)state), i);
			}

			// Prioritize responsiveness while playing
			// but when not playing prioritize throughput
			// (the Unity progress bar is also pretty slow to update)
			int timeoutMillis = Application.isPlaying ? 1 : 200;

			while (!WaitHandle.WaitAll(waitEvents, timeoutMillis)) {
				int count;
				lock (tileQueue) {
					count = tileQueue.Count;
				}

				yield return new Progress(Mathf.Lerp(0.1f, 0.9f, (tiles.Length - count + 1) / (float)tiles.Length), "Generating Tile " + (tiles.Length - count + 1) + "/" + tiles.Length);
			}
#endif

			yield return new Progress(0.9f, "Assigning Graph Indices");

			// Assign graph index to nodes
			uint graphIndex = (uint)AstarPath.active.astarData.GetGraphIndex(this);

			GetNodes(node => {
				node.GraphIndex = graphIndex;
				return true;
			});

#if UNITY_WEBGL && !UNITY_EDITOR
			// Put all tiles in the queue to be connected
			for (int i = 0; i < tiles.Length; i++) tileQueue.Enqueue(new Int2(tiles[i].x, tiles[i].z));

			// Calculate synchronously
			ConnectTiles(tileQueue, null);
#else
			// First connect all tiles with an EVEN coordinate sum
			// This would be the white squares on a chess board.
			// Then connect all tiles with an ODD coordinate sum (which would be all black squares).
			// This will prevent the different threads that do all
			// this in parallel from conflicting with each other
			// Looping over 0 and then 1
			for (int coordinateSum = 0; coordinateSum <= 1; coordinateSum++) {
				for (int i = 0; i < tiles.Length; i++) {
					if ((tiles[i].x + tiles[i].z) % 2 == coordinateSum) {
						tileQueue.Enqueue(new Int2(tiles[i].x, tiles[i].z));
					}
				}

				for (int i = 0; i < waitEvents.Length; i++) {
					waitEvents[i].Reset();
					ThreadPool.QueueUserWorkItem(state => ConnectTiles(tileQueue, state as ManualResetEvent), waitEvents[i]);
				}

				while (!WaitHandle.WaitAll(waitEvents, timeoutMillis)) {
					int count;
					lock (tileQueue) {
						count = tileQueue.Count;
					}

					yield return new Progress(Mathf.Lerp(0.9f, 1.0f, (tiles.Length - count + 1) / (float)tiles.Length), "Connecting Tile " + (tiles.Length - count + 1) + "/" + tiles.Length + " (Phase " + (coordinateSum+1) + ")");
				}
			}
#endif

			// This may be used by the TileHandlerHelper script to update the tiles
			// while taking NavmeshCuts into account after the graph has been completely recalculated.
			if (OnRecalculatedTiles != null) {
				OnRecalculatedTiles(tiles.Clone() as NavmeshTile[]);
			}
		}

		List<RasterizationMesh> CollectMeshes (Bounds bounds) {
			var result = new List<RasterizationMesh>();

			var meshGatherer = new RecastMeshGatherer(bounds, terrainSampleSize, mask, tagMask, colliderRasterizeDetail);

			if (rasterizeMeshes) {
				meshGatherer.CollectSceneMeshes(result);
			}

			meshGatherer.CollectRecastMeshObjs(result);

			if (rasterizeTerrain) {
				// Split terrains up into meshes approximately the size of a single chunk
				var desiredTerrainChunkSize = cellSize*Math.Max(tileSizeX, tileSizeZ);
				meshGatherer.CollectTerrainMeshes(rasterizeTrees, desiredTerrainChunkSize, result);
			}

			if (rasterizeColliders) {
				meshGatherer.CollectColliderMeshes(result);
			}

			if (result.Count == 0) {
				Debug.LogWarning("No MeshFilters were found contained in the layers specified by the 'mask' variables");
			}

			return result;
		}

		/** Fills graph with tiles created by NewEmptyTile */
		void FillWithEmptyTiles () {
			for (int z = 0; z < tileZCount; z++) {
				for (int x = 0; x < tileXCount; x++) {
					tiles[z*tileXCount + x] = NewEmptyTile(x, z);
				}
			}
		}

		float CellHeight {
			get {
				// Voxel y coordinates will be stored as ushorts which have 65536 values
				// Leave a margin to make sure things do not overflow
				return Mathf.Max(forcedBounds.size.y / 64000, 0.001f);
			}
		}

		/** Convert character radius to a number of voxels */
		int CharacterRadiusInVoxels {
			get {
				// Round it up most of the time, but round it down
				// if it is very close to the result when rounded down
				return Mathf.CeilToInt((characterRadius / cellSize) - 0.1f);
			}
		}

		/** Number of extra voxels on each side of a tile to ensure accurate navmeshes near the tile border.
		 * The width of a tile is expanded by 2 times this value (1x to the left and 1x to the right)
		 */
		int TileBorderSizeInVoxels {
			get {
				return CharacterRadiusInVoxels + 3;
			}
		}

		float TileBorderSizeInWorldUnits {
			get {
				return TileBorderSizeInVoxels*cellSize;
			}
		}

		Bounds CalculateTileBoundsWithBorder (int x, int z) {
			// World size of tile
			float tcsx = tileSizeX*cellSize;
			float tcsz = tileSizeZ*cellSize;

			Vector3 forcedBoundsMin = forcedBounds.min;
			Vector3 forcedBoundsMax = forcedBounds.max;

			var bounds = new Bounds();

			bounds.SetMinMax(new Vector3(x*tcsx, 0, z*tcsz) + forcedBoundsMin,
				new Vector3((x+1)*tcsx + forcedBoundsMin.x, forcedBoundsMax.y, (z+1)*tcsz + forcedBoundsMin.z)
				);

			// Expand borderSize voxels on each side
			bounds.Expand(new Vector3(1, 0, 1)*TileBorderSizeInWorldUnits*2);
			return bounds;
		}

		protected NavmeshTile BuildTileMesh (Voxelize vox, int x, int z, int threadIndex = 0) {
			AstarProfiler.StartProfile("Build Tile");
			AstarProfiler.StartProfile("Init");

			vox.borderSize = TileBorderSizeInVoxels;
			vox.forcedBounds = CalculateTileBoundsWithBorder(x, z);
			vox.width = tileSizeX + vox.borderSize*2;
			vox.depth = tileSizeZ + vox.borderSize*2;

			if (!useTiles && relevantGraphSurfaceMode == RelevantGraphSurfaceMode.OnlyForCompletelyInsideTile) {
				// This best reflects what the user would actually want
				vox.relevantGraphSurfaceMode = RelevantGraphSurfaceMode.RequireForAll;
			} else {
				vox.relevantGraphSurfaceMode = relevantGraphSurfaceMode;
			}

			vox.minRegionSize = Mathf.RoundToInt(minRegionSize / (cellSize*cellSize));

			AstarProfiler.EndProfile("Init");


			// Init voxelizer
			vox.Init();
			vox.VoxelizeInput();

			AstarProfiler.StartProfile("Filter Ledges");


			vox.FilterLedges(vox.voxelWalkableHeight, vox.voxelWalkableClimb, vox.cellSize, vox.cellHeight, vox.forcedBounds.min);

			AstarProfiler.EndProfile("Filter Ledges");

			AstarProfiler.StartProfile("Filter Low Height Spans");
			vox.FilterLowHeightSpans(vox.voxelWalkableHeight, vox.cellSize, vox.cellHeight, vox.forcedBounds.min);
			AstarProfiler.EndProfile("Filter Low Height Spans");

			vox.BuildCompactField();
			vox.BuildVoxelConnections();
			vox.ErodeWalkableArea(CharacterRadiusInVoxels);
			vox.BuildDistanceField();
			vox.BuildRegions();

			var cset = new VoxelContourSet();
			vox.BuildContours(contourMaxError, 1, cset, Voxelize.RC_CONTOUR_TESS_WALL_EDGES);

			VoxelMesh mesh;
			vox.BuildPolyMesh(cset, 3, out mesh);

			AstarProfiler.StartProfile("Build Nodes");

			// Position the vertices correctly in the world
			for (int i = 0; i < mesh.verts.Length; i++) {
				mesh.verts[i] = vox.VoxelToWorldInt3(mesh.verts[i]);
			}

			NavmeshTile tile = CreateTile(vox, mesh, x, z, threadIndex);

			AstarProfiler.EndProfile("Build Nodes");

			AstarProfiler.EndProfile("Build Tile");
			return tile;
		}

		/** Create a tile at tile index \a x, \a z from the mesh.
		 * \version Since version 3.7.6 the implementation is thread safe
		 */
		NavmeshTile CreateTile (Voxelize vox, VoxelMesh mesh, int x, int z, int threadIndex = 0) {
			if (mesh.tris == null) throw new System.ArgumentNullException("mesh.tris");
			if (mesh.verts == null) throw new System.ArgumentNullException("mesh.verts");

			// Create a new navmesh tile and assign its settings
			var tile = new NavmeshTile {
				x = x,
				z = z,
				w = 1,
				d = 1,
				tris = mesh.tris,
				verts = mesh.verts,
				bbTree = new BBTree()
			};

			if (tile.tris.Length % 3 != 0) throw new System.ArgumentException("Indices array's length must be a multiple of 3 (mesh.tris)");

			if (tile.verts.Length >= VertexIndexMask) {
				if (tileXCount*tileZCount == 1) {
					throw new System.ArgumentException("Too many vertices per tile (more than " + VertexIndexMask + ")." +
						"\n<b>Try enabling tiling in the recast graph settings.</b>\n");
				} else {
					throw new System.ArgumentException("Too many vertices per tile (more than " + VertexIndexMask + ")." +
						"\n<b>Try reducing tile size or enabling ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* Inspector</b>");
				}
			}

			tile.verts = Utility.RemoveDuplicateVertices(tile.verts, tile.tris);

			var nodes = new TriangleMeshNode[tile.tris.Length/3];
			tile.nodes = nodes;

			// Here we are faking a new graph
			// The tile is not added to any graphs yet, but to get the position queries from the nodes
			// to work correctly (not throw exceptions because the tile is not calculated) we fake a new graph
			// and direct the position queries directly to the tile
			// The thread index is added to make sure that if multiple threads are calculating tiles at the same time
			// they will not use the same temporary graph index
			uint temporaryGraphIndex = (uint)(AstarPath.active.astarData.graphs.Length + threadIndex);

			if (temporaryGraphIndex > GraphNode.MaxGraphIndex) {
				// Multithreaded tile calculations use fake graph indices, see above.
				throw new System.Exception("Graph limit reached. Multithreaded recast calculations cannot be done because a few scratch graph indices are required.");
			}

			// This index will be ORed to the triangle indices
			int tileIndex = x + z*tileXCount;
			tileIndex <<= TileIndexOffset;

			TriangleMeshNode.SetNavmeshHolder((int)temporaryGraphIndex, tile);
			// We need to lock here because creating nodes is not thread safe
			// and we may be doing this from multiple threads at the same time
			lock (AstarPath.active) {
				// Create nodes and assign vertex indices
				for (int i = 0; i < nodes.Length; i++) {
					var node = new TriangleMeshNode(active);
					nodes[i] = node;
					node.GraphIndex = temporaryGraphIndex;
					// The vertices stored on the node are composed
					// out of the triangle index and the tile index
					node.v0 = tile.tris[i*3+0] | tileIndex;
					node.v1 = tile.tris[i*3+1] | tileIndex;
					node.v2 = tile.tris[i*3+2] | tileIndex;

					//Degenerate triangles might occur, but they will not cause any large troubles anymore
					//if (Polygon.IsColinear (node.GetVertex(0), node.GetVertex(1), node.GetVertex(2))) {
					//	Debug.Log ("COLINEAR!!!!!!");
					//}

					//Make sure the triangle is clockwise
					if (!VectorMath.IsClockwiseXZ(node.GetVertex(0), node.GetVertex(1), node.GetVertex(2))) {
						int tmp = node.v0;
						node.v0 = node.v2;
						node.v2 = tmp;
					}

					node.Walkable = true;
					node.Penalty = initialPenalty;
					node.UpdatePositionFromVertices();
				}
			}

			tile.bbTree.RebuildFrom(nodes);
			CreateNodeConnections(tile.nodes);
			// Remove the fake graph
			TriangleMeshNode.SetNavmeshHolder((int)temporaryGraphIndex, null);

			return tile;
		}

		/** Create connections between all nodes.
		 * \version Since 3.7.6 the implementation is thread safe
		 */
		void CreateNodeConnections (TriangleMeshNode[] nodes) {
			List<MeshNode> connections = ListPool<MeshNode>.Claim();
			List<uint> connectionCosts = ListPool<uint>.Claim();

			var nodeRefs = ObjectPoolSimple<Dictionary<Int2, int> >.Claim();
			nodeRefs.Clear();

			// Build node neighbours
			for (int i = 0; i < nodes.Length; i++) {
				TriangleMeshNode node = nodes[i];

				int av = node.GetVertexCount();

				for (int a = 0; a < av; a++) {
					// Recast can in some very special cases generate degenerate triangles which are simply lines
					// In that case, duplicate keys might be added and thus an exception will be thrown
					// It is safe to ignore the second edge though... I think (only found one case where this happens)
					var key = new Int2(node.GetVertexIndex(a), node.GetVertexIndex((a+1) % av));
					if (!nodeRefs.ContainsKey(key)) {
						nodeRefs.Add(key, i);
					}
				}
			}

			for (int i = 0; i < nodes.Length; i++) {
				TriangleMeshNode node = nodes[i];

				connections.Clear();
				connectionCosts.Clear();

				int av = node.GetVertexCount();

				for (int a = 0; a < av; a++) {
					int first = node.GetVertexIndex(a);
					int second = node.GetVertexIndex((a+1) % av);
					int connNode;

					if (nodeRefs.TryGetValue(new Int2(second, first), out connNode)) {
						TriangleMeshNode other = nodes[connNode];

						int bv = other.GetVertexCount();

						for (int b = 0; b < bv; b++) {
							/** \todo This will fail on edges which are only partially shared */
							if (other.GetVertexIndex(b) == second && other.GetVertexIndex((b+1) % bv) == first) {
								uint cost = (uint)(node.position - other.position).costMagnitude;
								connections.Add(other);
								connectionCosts.Add(cost);
								break;
							}
						}
					}
				}

				node.connections = connections.ToArray();
				node.connectionCosts = connectionCosts.ToArray();
			}

			nodeRefs.Clear();
			ObjectPoolSimple<Dictionary<Int2, int> >.Release(ref nodeRefs);
			ListPool<MeshNode>.Release(connections);
			ListPool<uint>.Release(connectionCosts);
		}

		/** Generate connections between the two tiles.
		 * The tiles must be adjacent.
		 */
		void ConnectTiles (NavmeshTile tile1, NavmeshTile tile2) {
			if (tile1 == null || tile2 == null) return;

			if (tile1.nodes == null) throw new System.ArgumentException("tile1 does not contain any nodes");
			if (tile2.nodes == null) throw new System.ArgumentException("tile2 does not contain any nodes");

			int t1x = Mathf.Clamp(tile2.x, tile1.x, tile1.x+tile1.w-1);
			int t2x = Mathf.Clamp(tile1.x, tile2.x, tile2.x+tile2.w-1);
			int t1z = Mathf.Clamp(tile2.z, tile1.z, tile1.z+tile1.d-1);
			int t2z = Mathf.Clamp(tile1.z, tile2.z, tile2.z+tile2.d-1);

			int coord, altcoord;
			int t1coord, t2coord;

			float tcs;

			// Figure out which side that is shared between the two tiles
			// and what coordinate index is fixed along that edge (x or z)
			if (t1x == t2x) {
				coord = 2;
				altcoord = 0;
				t1coord = t1z;
				t2coord = t2z;
				tcs = tileSizeZ*cellSize;
			} else if (t1z == t2z) {
				coord = 0;
				altcoord = 2;
				t1coord = t1x;
				t2coord = t2x;
				tcs = tileSizeX*cellSize;
			} else {
				throw new System.ArgumentException("Tiles are not adjacent (neither x or z coordinates match)");
			}

			if (Math.Abs(t1coord-t2coord) != 1) {
				Debug.Log(tile1.x + " " + tile1.z + " " + tile1.w + " " + tile1.d + "\n"+
					tile2.x + " " + tile2.z + " " + tile2.w + " " + tile2.d+"\n"+
					t1x + " " + t1z + " " + t2x + " " + t2z);
				throw new System.ArgumentException("Tiles are not adjacent (tile coordinates must differ by exactly 1. Got '" + t1coord + "' and '" + t2coord + "')");
			}

			//Midpoint between the two tiles
			int midpoint = (int)Math.Round((Math.Max(t1coord, t2coord) * tcs + forcedBounds.min[coord]) * Int3.Precision);

#if ASTARDEBUG
			Vector3 v1 = new Vector3(-100, 0, -100);
			Vector3 v2 = new Vector3(100, 0, 100);
			v1[coord] = midpoint*Int3.PrecisionFactor;
			v2[coord] = midpoint*Int3.PrecisionFactor;

			Debug.DrawLine(v1, v2, Color.magenta);
#endif

			TriangleMeshNode[] nodes1 = tile1.nodes;
			TriangleMeshNode[] nodes2 = tile2.nodes;

			// Find adjacent nodes on the border between the tiles
			for (int i = 0; i < nodes1.Length; i++) {
				TriangleMeshNode nodeA = nodes1[i];
				int aVertexCount = nodeA.GetVertexCount();

				// Loop through all *sides* of the node
				for (int a = 0; a < aVertexCount; a++) {
					// Vertices that the segment consists of
					Int3 aVertex1 = nodeA.GetVertex(a);
					Int3 aVertex2 = nodeA.GetVertex((a+1) % aVertexCount);

					// Check if it is really close to the tile border
					if (Math.Abs(aVertex1[coord] - midpoint) < 2 && Math.Abs(aVertex2[coord] - midpoint) < 2) {
#if ASTARDEBUG
						Debug.DrawLine((Vector3)ap1, (Vector3)ap2, Color.red);
#endif

						int minalt = Math.Min(aVertex1[altcoord], aVertex2[altcoord]);
						int maxalt = Math.Max(aVertex1[altcoord], aVertex2[altcoord]);

						// Degenerate edge
						if (minalt == maxalt) continue;

						for (int j = 0; j < nodes2.Length; j++) {
							TriangleMeshNode nodeB = nodes2[j];
							int bVertexCount = nodeB.GetVertexCount();
							for (int b = 0; b < bVertexCount; b++) {
								Int3 bVertex1 = nodeB.GetVertex(b);
								Int3 bVertex2 = nodeB.GetVertex((b+1) % aVertexCount);
								if (Math.Abs(bVertex1[coord] - midpoint) < 2 && Math.Abs(bVertex2[coord] - midpoint) < 2) {
									int minalt2 = Math.Min(bVertex1[altcoord], bVertex2[altcoord]);
									int maxalt2 = Math.Max(bVertex1[altcoord], bVertex2[altcoord]);

									// Degenerate edge
									if (minalt2 == maxalt2) continue;

									if (maxalt > minalt2 && minalt < maxalt2) {
										// The two nodes seem to be adjacent

										// Test shortest distance between the segments (first test if they are equal since that is much faster)
										if ((aVertex1 == bVertex1 && aVertex2 == bVertex2) || (aVertex1 == bVertex2 && aVertex2 == bVertex1) ||
											VectorMath.SqrDistanceSegmentSegment((Vector3)aVertex1, (Vector3)aVertex2, (Vector3)bVertex1, (Vector3)bVertex2) < walkableClimb*walkableClimb) {
											uint cost = (uint)(nodeA.position - nodeB.position).costMagnitude;

											nodeA.AddConnection(nodeB, cost);
											nodeB.AddConnection(nodeA, cost);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		/** Start batch updating of tiles.
		 * During batch updating, tiles will not be connected if they are updating with ReplaceTile.
		 * When ending batching, all affected tiles will be connected.
		 * This is faster than not using batching.
		 */
		public void StartBatchTileUpdate () {
			if (batchTileUpdate) throw new System.InvalidOperationException("Calling StartBatchLoad when batching is already enabled");
			batchTileUpdate = true;
		}

		/** End batch updating of tiles.
		 * During batch updating, tiles will not be connected if they are updating with ReplaceTile.
		 * When ending batching, all affected tiles will be connected.
		 * This is faster than not using batching.
		 */
		public void EndBatchTileUpdate () {
			if (!batchTileUpdate) throw new System.InvalidOperationException("Calling EndBatchLoad when batching not enabled");

			batchTileUpdate = false;

			int tw = tileXCount;
			int td = tileZCount;

			//Clear all flags
			for (int z = 0; z < td; z++) {
				for (int x = 0; x < tw; x++) {
					tiles[x + z*tileXCount].flag = false;
				}
			}

			for (int i = 0; i < batchUpdatedTiles.Count; i++) tiles[batchUpdatedTiles[i]].flag = true;

			for (int z = 0; z < td; z++) {
				for (int x = 0; x < tw; x++) {
					if (x < tw-1
						&& (tiles[x + z*tileXCount].flag || tiles[x+1 + z*tileXCount].flag)
						&& tiles[x + z*tileXCount] != tiles[x+1 + z*tileXCount]) {
						ConnectTiles(tiles[x + z*tileXCount], tiles[x+1 + z*tileXCount]);
					}

					if (z < td-1
						&& (tiles[x + z*tileXCount].flag || tiles[x + (z+1)*tileXCount].flag)
						&& tiles[x + z*tileXCount] != tiles[x + (z+1)*tileXCount]) {
						ConnectTiles(tiles[x + z*tileXCount], tiles[x + (z+1)*tileXCount]);
					}
				}
			}

			batchUpdatedTiles.Clear();
		}

		/** Clear all tiles within the rectangle with one corner at (x,z), width w and depth d */
		void ClearTiles (int x, int z, int w, int d) {
			for (int cz = z; cz < z+d; cz++) {
				for (int cx = x; cx < x+w; cx++) {
					NavmeshTile otile = tiles[cx + cz*tileXCount];
					if (otile == null) continue;

					// Remove old tile connections
					RemoveConnectionsFromTile(otile);

					for (int i = 0; i < otile.nodes.Length; i++) {
						otile.nodes[i].Destroy();
					}

					for (int qz = otile.z; qz < otile.z+otile.d; qz++) {
						for (int qx = otile.x; qx < otile.x+otile.w; qx++) {
							NavmeshTile qtile = tiles[qx + qz*tileXCount];
							if (qtile == null || qtile != otile) throw new System.Exception("This should not happen");

							if (qz < z || qz >= z+d || qx < x || qx >= x+w) {
								// if out of this tile's bounds, replace with empty tile
								tiles[qx + qz*tileXCount] = NewEmptyTile(qx, qz);

								if (batchTileUpdate) {
									batchUpdatedTiles.Add(qx + qz*tileXCount);
								}
							} else {
								//Will be replaced by the new tile
								tiles[qx + qz*tileXCount] = null;
							}
						}
					}

					ObjectPool<BBTree>.Release(ref otile.bbTree);
				}
			}
		}

		/** Replace tile at index with nodes created from specified navmesh.
		 * \see StartBatchTileUpdating
		 */
		public void ReplaceTile (int x, int z, Int3[] verts, int[] tris, bool worldSpace) {
			ReplaceTile(x, z, 1, 1, verts, tris, worldSpace);
		}

		public void ReplaceTile (int x, int z, int w, int d, Int3[] verts, int[] tris, bool worldSpace) {
			if (x + w > tileXCount || z+d > tileZCount || x < 0 || z < 0) {
				throw new System.ArgumentException("Tile is placed at an out of bounds position or extends out of the graph bounds ("+x+", " + z + " [" + w + ", " + d+ "] " + tileXCount + " " + tileZCount + ")");
			}

			if (w < 1 || d < 1) throw new System.ArgumentException("width and depth must be greater or equal to 1. Was " + w + ", " + d);

			UnityEngine.Profiling.Profiler.BeginSample("Replace Tile Init");

			// Remove previous tiles
			ClearTiles(x, z, w, d);

			//Create a new navmesh tile and assign its settings
			var tile = new NavmeshTile {
				x = x,
				z = z,
				w = w,
				d = d,
				tris = tris,
				verts = verts,
				bbTree = ObjectPool<BBTree>.Claim()
			};

			if (tile.tris.Length % 3 != 0) throw new System.ArgumentException("Triangle array's length must be a multiple of 3 (tris)");

			if (tile.verts.Length > 0xFFFF) throw new System.ArgumentException("Too many vertices per tile (more than 65535)");

			if (!worldSpace) {
				if (!Mathf.Approximately(x*tileSizeX*cellSize*Int3.FloatPrecision, (float)Math.Round(x*tileSizeX*cellSize*Int3.FloatPrecision))) Debug.LogWarning("Possible numerical imprecision. Consider adjusting tileSize and/or cellSize");
				if (!Mathf.Approximately(z*tileSizeZ*cellSize*Int3.FloatPrecision, (float)Math.Round(z*tileSizeZ*cellSize*Int3.FloatPrecision))) Debug.LogWarning("Possible numerical imprecision. Consider adjusting tileSize and/or cellSize");

				var offset = (Int3)(new Vector3((x * tileSizeX * cellSize), 0, (z * tileSizeZ * cellSize)) + forcedBounds.min);

				for (int i = 0; i < verts.Length; i++) {
					verts[i] += offset;
				}
			}

			var nodes = new TriangleMeshNode[tile.tris.Length/3];
			tile.nodes = nodes;

			//Here we are faking a new graph
			//The tile is not added to any graphs yet, but to get the position querys from the nodes
			//to work correctly (not throw exceptions because the tile is not calculated) we fake a new graph
			//and direct the position queries directly to the tile
			int graphIndex = AstarPath.active.astarData.graphs.Length;

			TriangleMeshNode.SetNavmeshHolder(graphIndex, tile);

			//This index will be ORed to the triangle indices
			int tileIndex = x + z*tileXCount;
			tileIndex <<= TileIndexOffset;

			if (tile.verts.Length > VertexIndexMask) {
				Debug.LogError("Too many vertices in the tile (" + tile.verts.Length + " > " + VertexIndexMask +")\nYou can enable ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* Inspector to raise this limit.");
				tiles[tileIndex] = NewEmptyTile(x, z);
				return;
			}

			//Create nodes and assign triangle indices
			for (int i = 0; i < nodes.Length; i++) {
				var node = new TriangleMeshNode(active);
				nodes[i] = node;
				node.GraphIndex = (uint)graphIndex;
				node.v0 = tile.tris[i*3+0] | tileIndex;
				node.v1 = tile.tris[i*3+1] | tileIndex;
				node.v2 = tile.tris[i*3+2] | tileIndex;

				//Degenerate triangles might occur, but they will not cause any large troubles anymore
				//if (Polygon.IsColinear (node.GetVertex(0), node.GetVertex(1), node.GetVertex(2))) {
				//	Debug.Log ("COLINEAR!!!!!!");
				//}

				//Make sure the triangle is clockwise
				if (!VectorMath.IsClockwiseXZ(node.GetVertex(0), node.GetVertex(1), node.GetVertex(2))) {
					int tmp = node.v0;
					node.v0 = node.v2;
					node.v2 = tmp;
				}

				node.Walkable = true;
				node.Penalty = initialPenalty;
				node.UpdatePositionFromVertices();
			}

			UnityEngine.Profiling.Profiler.EndSample();
			UnityEngine.Profiling.Profiler.BeginSample("AABBTree Rebuild");
			tile.bbTree.RebuildFrom(nodes);
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("Create Node Connections");
			CreateNodeConnections(tile.nodes);
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("Connect With Neighbours");

			//Set tile
			for (int cz = z; cz < z+d; cz++) {
				for (int cx = x; cx < x+w; cx++) {
					tiles[cx + cz*tileXCount] = tile;
				}
			}

			if (batchTileUpdate) {
				batchUpdatedTiles.Add(x + z*tileXCount);
			} else {
				ConnectTileWithNeighbours(tile);
				/*if (x > 0) ConnectTiles (tiles[(x-1) + z*tileXCount], tile);
				 * if (z > 0) ConnectTiles (tiles[x + (z-1)*tileXCount], tile);
				 * if (x < tileXCount-1) ConnectTiles (tiles[(x+1) + z*tileXCount], tile);
				 * if (z < tileZCount-1) ConnectTiles (tiles[x + (z+1)*tileXCount], tile);*/
			}

			//Remove the fake graph
			TriangleMeshNode.SetNavmeshHolder(graphIndex, null);
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("Set graph index");

			//Real graph index
			//TODO, could this step be changed for this function, is a fake index required?
			graphIndex = AstarPath.active.astarData.GetGraphIndex(this);

			for (int i = 0; i < nodes.Length; i++) nodes[i].GraphIndex = (uint)graphIndex;

			UnityEngine.Profiling.Profiler.EndSample();
		}


		public bool Linecast (Vector3 origin, Vector3 end) {
			return Linecast(origin, end, GetNearest(origin, NNConstraint.None).node);
		}

		public bool Linecast (Vector3 origin, Vector3 end, GraphNode hint, out GraphHitInfo hit) {
			return NavMeshGraph.Linecast(this as INavmesh, origin, end, hint, out hit, null);
		}

		public bool Linecast (Vector3 origin, Vector3 end, GraphNode hint) {
			GraphHitInfo hit;

			return NavMeshGraph.Linecast(this as INavmesh, origin, end, hint, out hit, null);
		}

		/** Returns if there is an obstacle between \a origin and \a end on the graph.
		 * \param [in] tmp_origin Point to start from
		 * \param [in] tmp_end Point to linecast to
		 * \param [out] hit Contains info on what was hit, see GraphHitInfo
		 * \param [in] hint You need to pass the node closest to the start point, if null, a search for the closest node will be done
		 * \param trace If a list is passed, then it will be filled with all nodes the linecast traverses
		 * This is not the same as Physics.Linecast, this function traverses the \b graph and looks for collisions instead of checking for collider intersection.
		 * \astarpro */
		public bool Linecast (Vector3 tmp_origin, Vector3 tmp_end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace) {
			return NavMeshGraph.Linecast(this, tmp_origin, tmp_end, hint, out hit, trace);
		}

		public override void OnDrawGizmos (bool drawNodes) {
			if (!drawNodes) {
				return;
			}

			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(forcedBounds.center, forcedBounds.size);

			PathHandler debugData = AstarPath.active.debugPathData;

			GraphNodeDelegateCancelable del = delegate(GraphNode _node) {
				var node = _node as TriangleMeshNode;

				if (AstarPath.active.showSearchTree && debugData != null) {
					bool v = InSearchTree(node, AstarPath.active.debugPath);
					//debugData.GetPathNode(node).parent != null && debugData.GetPathNode(node).parent.node != null;
					if (v && showNodeConnections) {
						//Gizmos.color = new Color (0,1,0,0.7F);
						var pnode = debugData.GetPathNode(node);
						if (pnode.parent != null) {
							Gizmos.color = NodeColor(node, debugData);
							Gizmos.DrawLine((Vector3)node.position, (Vector3)debugData.GetPathNode(node).parent.node.position);
						}
					}

					if (showMeshOutline) {
						Gizmos.color = node.Walkable ? NodeColor(node, debugData) : AstarColor.UnwalkableNode;
						if (!v) Gizmos.color = Gizmos.color * new Color(1, 1, 1, 0.1f);

						Gizmos.DrawLine((Vector3)node.GetVertex(0), (Vector3)node.GetVertex(1));
						Gizmos.DrawLine((Vector3)node.GetVertex(1), (Vector3)node.GetVertex(2));
						Gizmos.DrawLine((Vector3)node.GetVertex(2), (Vector3)node.GetVertex(0));
					}
				} else {
					if (showNodeConnections) {
						Gizmos.color = NodeColor(node, null);

						for (int q = 0; q < node.connections.Length; q++) {
							//Gizmos.color = Color.Lerp (Color.green,Color.red,node.connectionCosts[q]/8000F);
							Gizmos.DrawLine((Vector3)node.position, Vector3.Lerp((Vector3)node.connections[q].position, (Vector3)node.position, 0.4f));
						}
					}

					if (showMeshOutline) {
						Gizmos.color = node.Walkable ? NodeColor(node, debugData) : AstarColor.UnwalkableNode;


						Gizmos.DrawLine((Vector3)node.GetVertex(0), (Vector3)node.GetVertex(1));
						Gizmos.DrawLine((Vector3)node.GetVertex(1), (Vector3)node.GetVertex(2));
						Gizmos.DrawLine((Vector3)node.GetVertex(2), (Vector3)node.GetVertex(0));
					}
				}

				//Gizmos.color.a = 0.2F;

				return true;
			};

			GetNodes(del);
		}

		public override void DeserializeSettingsCompatibility (GraphSerializationContext ctx) {
			base.DeserializeSettingsCompatibility(ctx);

			characterRadius = ctx.reader.ReadSingle();
			contourMaxError = ctx.reader.ReadSingle();
			cellSize = ctx.reader.ReadSingle();
			ctx.reader.ReadSingle(); // Backwards compatibility, cellHeight was previously read here
			walkableHeight = ctx.reader.ReadSingle();
			maxSlope = ctx.reader.ReadSingle();
			maxEdgeLength = ctx.reader.ReadSingle();
			editorTileSize = ctx.reader.ReadInt32();
			tileSizeX = ctx.reader.ReadInt32();
			nearestSearchOnlyXZ = ctx.reader.ReadBoolean();
			useTiles = ctx.reader.ReadBoolean();
			relevantGraphSurfaceMode = (RelevantGraphSurfaceMode)ctx.reader.ReadInt32();
			rasterizeColliders = ctx.reader.ReadBoolean();
			rasterizeMeshes = ctx.reader.ReadBoolean();
			rasterizeTerrain = ctx.reader.ReadBoolean();
			rasterizeTrees = ctx.reader.ReadBoolean();
			colliderRasterizeDetail = ctx.reader.ReadSingle();
			forcedBoundsCenter = ctx.DeserializeVector3();
			forcedBoundsSize = ctx.DeserializeVector3();
			mask = ctx.reader.ReadInt32();

			int count = ctx.reader.ReadInt32();
			tagMask = new List<string>(count);
			for (int i = 0; i < count; i++) {
				tagMask.Add(ctx.reader.ReadString());
			}

			showMeshOutline = ctx.reader.ReadBoolean();
			showNodeConnections = ctx.reader.ReadBoolean();
			terrainSampleSize = ctx.reader.ReadInt32();

			// These were originally forgotten but added in an upgrade
			// To keep backwards compatibility, they are only deserialized
			// If they exist in the streamed data
			walkableClimb = ctx.DeserializeFloat(walkableClimb);
			minRegionSize = ctx.DeserializeFloat(minRegionSize);

			// Make the world square if this value is not in the stream
			tileSizeZ = ctx.DeserializeInt(tileSizeX);

			showMeshSurface = ctx.reader.ReadBoolean();
		}

		/** Serializes Node Info.
		 * Should serialize:
		 * - Base
		 *    - Node Flags
		 *    - Node Penalties
		 *    - Node
		 * - Node Positions (if applicable)
		 * - Any other information necessary to load the graph in-game
		 * All settings marked with json attributes (e.g JsonMember) have already been
		 * saved as graph settings and do not need to be handled here.
		 *
		 * It is not necessary for this implementation to be forward or backwards compatible.
		 *
		 * \see
		 */
		public override void SerializeExtraInfo (GraphSerializationContext ctx) {
			BinaryWriter writer = ctx.writer;

			if (tiles == null) {
				writer.Write(-1);
				return;
			}
			writer.Write(tileXCount);
			writer.Write(tileZCount);

			for (int z = 0; z < tileZCount; z++) {
				for (int x = 0; x < tileXCount; x++) {
					NavmeshTile tile = tiles[x + z*tileXCount];

					if (tile == null) {
						throw new System.Exception("NULL Tile");
						//writer.Write (-1);
						//continue;
					}

					writer.Write(tile.x);
					writer.Write(tile.z);

					if (tile.x != x || tile.z != z) continue;

					writer.Write(tile.w);
					writer.Write(tile.d);

					writer.Write(tile.tris.Length);

					for (int i = 0; i < tile.tris.Length; i++) writer.Write(tile.tris[i]);

					writer.Write(tile.verts.Length);
					for (int i = 0; i < tile.verts.Length; i++) {
						ctx.SerializeInt3(tile.verts[i]);
					}

					writer.Write(tile.nodes.Length);
					for (int i = 0; i < tile.nodes.Length; i++) {
						tile.nodes[i].SerializeNode(ctx);
					}
				}
			}
		}

		public override void DeserializeExtraInfo (GraphSerializationContext ctx) {
			BinaryReader reader = ctx.reader;

			tileXCount = reader.ReadInt32();

			if (tileXCount < 0) return;

			tileZCount = reader.ReadInt32();

			tiles = new NavmeshTile[tileXCount * tileZCount];

			//Make sure mesh nodes can reference this graph
			TriangleMeshNode.SetNavmeshHolder((int)ctx.graphIndex, this);

			for (int z = 0; z < tileZCount; z++) {
				for (int x = 0; x < tileXCount; x++) {
					int tileIndex = x + z*tileXCount;
					int tx = reader.ReadInt32();
					if (tx < 0) throw new System.Exception("Invalid tile coordinates (x < 0)");

					int tz = reader.ReadInt32();
					if (tz < 0) throw new System.Exception("Invalid tile coordinates (z < 0)");

					// This is not the origin of a large tile. Refer back to that tile.
					if (tx != x || tz != z) {
						tiles[tileIndex] = tiles[tz*tileXCount + tx];
						continue;
					}

					var tile = new NavmeshTile();

					tile.x = tx;
					tile.z = tz;
					tile.w = reader.ReadInt32();
					tile.d = reader.ReadInt32();
					tile.bbTree = ObjectPool<BBTree>.Claim();

					tiles[tileIndex] = tile;

					int trisCount = reader.ReadInt32();

					if (trisCount % 3 != 0) throw new System.Exception("Corrupt data. Triangle indices count must be divisable by 3. Got " + trisCount);

					tile.tris = new int[trisCount];
					for (int i = 0; i < tile.tris.Length; i++) tile.tris[i] = reader.ReadInt32();

					tile.verts = new Int3[reader.ReadInt32()];
					for (int i = 0; i < tile.verts.Length; i++) {
						tile.verts[i] = ctx.DeserializeInt3();
					}

					int nodeCount = reader.ReadInt32();
					tile.nodes = new TriangleMeshNode[nodeCount];

					//Prepare for storing in vertex indices
					tileIndex <<= TileIndexOffset;

					for (int i = 0; i < tile.nodes.Length; i++) {
						var node = new TriangleMeshNode(active);
						tile.nodes[i] = node;

						node.DeserializeNode(ctx);

						node.v0 = tile.tris[i*3+0] | tileIndex;
						node.v1 = tile.tris[i*3+1] | tileIndex;
						node.v2 = tile.tris[i*3+2] | tileIndex;
						node.UpdatePositionFromVertices();
					}

					tile.bbTree.RebuildFrom(tile.nodes);
				}
			}
		}
	}
}
