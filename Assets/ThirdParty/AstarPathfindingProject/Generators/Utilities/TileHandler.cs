
using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Pathfinding.ClipperLib;
using Pathfinding.Poly2Tri;
using Pathfinding.Util;

namespace Pathfinding.Util {
	/** Utility class for updating tiles of recast graphs.
	 * Primarily used by the TileHandlerHelper for navmesh cutting.
	 *
	 * Most operations that this class does are asynchronous.
	 * They will be added as work items to the AstarPath class
	 * and executed when the pathfinding threads have finished
	 * calculating their current paths.
	 *
	 * \see TileHandlerHelper
	 */
	public class TileHandler {
		/** Graph which is handled by this instance */
		readonly RecastGraph _graph;

		/** Number of tiles along the x axis */
		readonly int tileXCount;

		/** Number of tiles along the z axis */
		readonly int tileZCount;

		/** Handles polygon clipping operations */
		readonly Clipper clipper = new Clipper();

		/** Cached int array to avoid excessive allocations */
		int[] cached_int_array = new int[32];

		/** Cached dictionary to avoid excessive allocations */
		readonly Dictionary<Int3, int> cached_Int3_int_dict = new Dictionary<Int3, int>();

		/** Cached dictionary to avoid excessive allocations */
		readonly Dictionary<Int2, int> cached_Int2_int_dict = new Dictionary<Int2, int>();

		/** Which tile type is active on each tile index.
		 * This array will be tileXCount*tileZCount elements long.
		 */
		readonly TileType[] activeTileTypes;

		/** Rotations of the active tiles */
		readonly int[] activeTileRotations;

		/** Offsets along the Y axis of the active tiles */
		readonly int[] activeTileOffsets;

		/** A flag for each tile that is set to true if it has been reloaded while batching is in progress */
		readonly bool[] reloadedInBatch;

		/** True while batching tile updates.
		 * Batching tile updates has a positive effect on performance
		 */
		bool isBatching;

		/** Utility for clipping polygons to rectangles.
		 * Implemented as a struct and not a bunch of static methods
		 * because it needs some buffer arrays that are best cached
		 * to avoid excessive allocations
		 */
		readonly Pathfinding.Voxels.VoxelPolygonClipper simpleClipper;

		/** The underlaying graph which this object handles */
		public RecastGraph graph {
			get {
				return _graph;
			}
		}

		/** True if the tile handler still has the same number of tiles and tile layout as the graph.
		 * If the graph is rescanned the tile handler will get out of sync and needs to be recreated.
		 */
		public bool isValid {
			get {
				return _graph != null && tileXCount == _graph.tileXCount && tileZCount == _graph.tileZCount;
			}
		}

		public TileHandler (RecastGraph graph) {
			if (graph == null) throw new ArgumentNullException("graph");
			if (graph.GetTiles() == null) Debug.LogWarning("Creating a TileHandler for a graph with no tiles. Please scan the graph before creating a TileHandler");
			tileXCount = graph.tileXCount;
			tileZCount = graph.tileZCount;
			activeTileTypes = new TileType[tileXCount*tileZCount];
			activeTileRotations = new int[activeTileTypes.Length];
			activeTileOffsets = new int[activeTileTypes.Length];
			reloadedInBatch = new bool[activeTileTypes.Length];
			this._graph = graph;
		}

		/** Call to update the specified tiles with new information based on the recast graph.
		 * This is usually called right after a recast graph has recalculated some tiles
		 * and thus some calculations need to be done to take navmesh cutting into account
		 * as well.
		 *
		 * Will reload all tiles in the list.
		 */
		public void OnRecalculatedTiles (RecastGraph.NavmeshTile[] recalculatedTiles) {
			for (int i = 0; i < recalculatedTiles.Length; i++) {
				UpdateTileType(recalculatedTiles[i]);
			}

			bool batchStarted = StartBatchLoad();

			for (int i = 0; i < recalculatedTiles.Length; i++) {
				ReloadTile(recalculatedTiles[i].x, recalculatedTiles[i].z);
			}

			if (batchStarted) EndBatchLoad();
		}

		public int GetActiveRotation (Int2 p) {
			return activeTileRotations[p.x + p.y*_graph.tileXCount];
		}

		/** \deprecated */
		[System.Obsolete("Use the result from RegisterTileType instead")]
		public TileType GetTileType (int index) {
			throw new System.Exception("This method has been deprecated. Use the result from RegisterTileType instead.");
		}

		/** \deprecated */
		[System.Obsolete("Use the result from RegisterTileType instead")]
		public int GetTileTypeCount () {
			throw new System.Exception("This method has been deprecated. Use the result from RegisterTileType instead.");
		}

		/** A template for a single tile in a recast graph */
		public class TileType {
			Int3[] verts;
			int[] tris;
			Int3 offset;
			int lastYOffset;
			int lastRotation;
			int width;
			int depth;

			public int Width {
				get {
					return width;
				}
			}

			public int Depth {
				get {
					return depth;
				}
			}

			/** Matrices for rotation.
			 * Each group of 4 elements is a 2x2 matrix.
			 * The XZ position is multiplied by this.
			 * So
			 * \code
			 * //A rotation by 90 degrees clockwise, second matrix in the array
			 * (5,2) * ((0, 1), (-1, 0)) = (2,-5)
			 * \endcode
			 */
			private static readonly int[] Rotations = {
				1, 0,  //Identity matrix
				0, 1,

				0, 1,
				-1, 0,

				-1, 0,
				0, -1,

				0, -1,
				1, 0
			};

			public TileType (Int3[] sourceVerts, int[] sourceTris, Int3 tileSize, Int3 centerOffset, int width = 1, int depth = 1) {
				if (sourceVerts == null) throw new ArgumentNullException("sourceVerts");
				if (sourceTris == null) throw new ArgumentNullException("sourceTris");

				tris = new int[sourceTris.Length];
				for (int i = 0; i < tris.Length; i++) tris[i] = sourceTris[i];

				verts = new Int3[sourceVerts.Length];

				for (int i = 0; i < sourceVerts.Length; i++) {
					verts[i] = sourceVerts[i] + centerOffset;
				}

				offset = tileSize/2;
				offset.x *= width;
				offset.z *= depth;
				offset.y = 0;

				for (int i = 0; i < sourceVerts.Length; i++) {
					verts[i] = verts[i] + offset;
				}

				lastRotation = 0;
				lastYOffset = 0;

				this.width = width;
				this.depth = depth;
			}

			/** Create a new TileType.
			 * First all vertices of the source mesh are offseted by the \a centerOffset.
			 * The source mesh is assumed to be centered (after offsetting). Corners of the tile should be at tileSize*0.5 along all axes.
			 * When width or depth is not 1, the tileSize param should not change, but corners of the tile are assumed to lie further out.
			 *
			 * \param source The navmesh as a unity Mesh
			 * \param width The number of base tiles this tile type occupies on the x-axis
			 * \param depth The number of base tiles this tile type occupies on the z-axis
			 * \param tileSize Size of a single tile, the y-coordinate will be ignored.
			 */
			public TileType (Mesh source, Int3 tileSize, Int3 centerOffset, int width = 1, int depth = 1) {
				if (source == null) throw new ArgumentNullException("source");

				Vector3[] vectorVerts = source.vertices;
				tris = source.triangles;
				verts = new Int3[vectorVerts.Length];

				for (int i = 0; i < vectorVerts.Length; i++) {
					verts[i] = (Int3)vectorVerts[i] + centerOffset;
				}

				offset = tileSize/2;
				offset.x *= width;
				offset.z *= depth;
				offset.y = 0;

				for (int i = 0; i < vectorVerts.Length; i++) {
					verts[i] = verts[i] + offset;
				}

				lastRotation = 0;
				lastYOffset = 0;

				this.width = width;
				this.depth = depth;
			}

			/** Load a tile, result given by the vert and tris array.
			 * \warning For performance and memory reasons, the returned arrays are internal arrays, so they must not be modified in any way or
			 * subsequent calls to Load may give corrupt output. The contents of the verts array is only valid until the next call to Load since
			 * different rotations and y offsets can be applied.
			 * If you need persistent arrays, please copy the returned ones.
			 */
			public void Load (out Int3[] verts, out int[] tris, int rotation, int yoffset) {
				//Make sure it is a number 0 <= x < 4
				rotation = ((rotation % 4) + 4) % 4;

				//Figure out relative rotation (relative to previous rotation that is, since that is still applied to the verts array)
				int tmp = rotation;
				rotation = (rotation - (lastRotation % 4) + 4) % 4;
				lastRotation = tmp;

				verts = this.verts;

				int relYOffset = yoffset - lastYOffset;
				lastYOffset = yoffset;

				if (rotation != 0 || relYOffset != 0) {
					for (int i = 0; i < verts.Length; i++) {
						Int3 op = verts[i] - offset;
						Int3 p = op;
						p.y += relYOffset;
						p.x = op.x * Rotations[rotation*4 + 0] + op.z * Rotations[rotation*4 + 1];
						p.z = op.x * Rotations[rotation*4 + 2] + op.z * Rotations[rotation*4 + 3];
						verts[i] = p + offset;
					}
				}

				tris = this.tris;
			}
		}

		/** Register that a tile can be loaded from \a source.
		 * \param centerOffset Assumes that the mesh has its pivot point at the center of the tile.
		 * If it has not, you can supply a non-zero \a centerOffset to offset all vertices.
		 *
		 * \param width width of the tile. In base tiles, not world units.
		 * \param depth depth of the tile. In base tiles, not world units.
		 * \param source Source mesh, must be readable.
		 *
		 * \returns Identifier for loading that tile type
		 */
		public TileType RegisterTileType (Mesh source, Int3 centerOffset, int width = 1, int depth = 1) {
			return new TileType(source, new Int3(graph.tileSizeX, 1, graph.tileSizeZ)*(Int3.Precision*graph.cellSize), centerOffset, width, depth);
		}

		public void CreateTileTypesFromGraph () {
			RecastGraph.NavmeshTile[] tiles = graph.GetTiles();
			if (tiles == null)
				return;

			if (!isValid) {
				throw new InvalidOperationException("Graph tiles are invalid (number of tiles is not equal to width*depth of the graph). You need to create a new tile handler if you have changed the graph.");
			}

			for (int z = 0; z < graph.tileZCount; z++) {
				for (int x = 0; x < graph.tileXCount; x++) {
					RecastGraph.NavmeshTile tile = tiles[x + z*graph.tileXCount];
					UpdateTileType(tile);
				}
			}
		}

		void UpdateTileType (RecastGraph.NavmeshTile tile) {
			int x = tile.x;
			int z = tile.z;

			Bounds b = graph.GetTileBounds(x, z);
			var min = (Int3)b.min;
			Int3 size = new Int3(graph.tileSizeX, 1, graph.tileSizeZ)*(Int3.Precision*graph.cellSize);

			min += new Int3(size.x*tile.w/2, 0, size.z*tile.d/2);
			min = -min;

			var tp = new TileType(tile.verts, tile.tris, size, min, tile.w, tile.d);

			int index = x + z*graph.tileXCount;
			activeTileTypes[index] = tp;
			activeTileRotations[index] = 0;
			activeTileOffsets[index] = 0;
		}

		/** Start batch loading.
		 * \returns True if batching wasn't started yet, and thus EndBatchLoad should be called,
		 * False if batching was already started by some other part of the code and you should not call EndBatchLoad
		 */
		public bool StartBatchLoad () {
			//if (isBatching) throw new Exception ("Starting batching when batching has already been started");
			if (isBatching) return false;

			isBatching = true;

			AstarPath.active.AddWorkItem(new AstarWorkItem(force => {
				graph.StartBatchTileUpdate();
				return true;
			}));

			return true;
		}

		public void EndBatchLoad () {
			if (!isBatching) throw new Exception("Ending batching when batching has not been started");

			for (int i = 0; i < reloadedInBatch.Length; i++) reloadedInBatch[i] = false;
			isBatching = false;

			AstarPath.active.AddWorkItem(new AstarWorkItem(force => {
				graph.EndBatchTileUpdate();
				return true;
			}));
		}

		[Flags]
		public enum CutMode {
			CutAll = 1,
			CutDual = 2,
			CutExtra = 4
		}

		/** Internal class describing a single NavmeshCut */
		class Cut {
			/** Bounds in XZ space */
			public IntRect bounds;

			/** X is the lower bound on the y axis, Y is the upper bounds on the Y axis */
			public Int2 boundsY;
			public bool isDual;
			public bool cutsAddedGeom;
			public List<IntPoint> contour;
		}

		/** Cuts a piece of navmesh using navmesh cuts.
		 *
		 * \note I am sorry for the really messy code in this method.
		 * It really needs to be refactored.
		 */
		void CutPoly (Int3[] verts, int[] tris, ref Int3[] outVertsArr, ref int[] outTrisArr, out int outVCount, out int outTCount, Int3[] extraShape, Int3 cuttingOffset, Bounds realBounds, CutMode mode = CutMode.CutAll | CutMode.CutDual, int perturbate = -1) {
			// Nothing to do here
			if (verts.Length == 0 || tris.Length == 0) {
				outVCount = 0;
				outTCount = 0;
				outTrisArr = new int[0];
				outVertsArr = new Int3[0];
				return;
			}

			if (perturbate > 10) {
				Debug.LogError("Too many perturbations aborting.\n" +
					"This may cause a tile in the navmesh to become empty. " +
					"Try to see see if any of your NavmeshCut or NavmeshAdd components use invalid custom meshes.");
				outVCount = verts.Length;
				outTCount = tris.Length;
				outTrisArr = tris;
				outVertsArr = verts;
				return;
			}

			List<IntPoint> extraClipShape = null;

			// Do not cut with extra shape if there is no extra shape
			if (extraShape == null && (mode & CutMode.CutExtra) != 0) {
				throw new Exception("extraShape is null and the CutMode specifies that it should be used. Cannot use null shape.");
			}

			if ((mode & CutMode.CutExtra) != 0) {
				extraClipShape = ListPool<IntPoint>.Claim(extraShape.Length);
				for (int i = 0; i < extraShape.Length; i++) {
					extraClipShape.Add(new IntPoint(extraShape[i].x + cuttingOffset.x, extraShape[i].z + cuttingOffset.z));
				}
			}

			var bounds = new IntRect(verts[0].x, verts[0].z, verts[0].x, verts[0].z);

			// Expand bounds to contain all vertices
			for (int i = 0; i < verts.Length; i++) {
				bounds = bounds.ExpandToContain(verts[i].x, verts[i].z);
			}

			// Find all NavmeshCut components that could be inside these bounds
			List<NavmeshCut> navmeshCuts;
			if (mode == CutMode.CutExtra) {
				// Not needed when only cutting extra
				navmeshCuts = ListPool<NavmeshCut>.Claim();
			} else {
				navmeshCuts = NavmeshCut.GetAllInRange(realBounds);
			}

			// Find all NavmeshAdd components that could be inside the bounds
			List<NavmeshAdd> navmeshAdds = NavmeshAdd.GetAllInRange(realBounds);
			var intersectingCuts = ListPool<int>.Claim();

			var cuts = PrepareNavmeshCutsForCutting(navmeshCuts, cuttingOffset, bounds, perturbate, navmeshAdds.Count > 0);

			var outverts = ListPool<Int3>.Claim(verts.Length*2);
			var outtris = ListPool<int>.Claim(tris.Length);

			// Current list of vertices and triangles that are being processed
			Int3[] cverts = verts;
			int[] ctris = tris;

			if (navmeshCuts.Count == 0 && navmeshAdds.Count == 0 && (mode & ~(CutMode.CutAll | CutMode.CutDual)) == 0 && (mode & CutMode.CutAll) != 0) {
				// Fast path for the common case, no cuts or adds to the navmesh, so we just copy the vertices
				CopyMesh(cverts, ctris, outverts, outtris);
			} else {
				var poly = ListPool<IntPoint>.Claim();
				var point2Index = new Dictionary<TriangulationPoint, int>();
				var polypoints = ListPool<Poly2Tri.PolygonPoint>.Claim();

				var clipResult = new Pathfinding.ClipperLib.PolyTree();
				var intermediateClipResult = new List<List<IntPoint> >();
				var polyCache = new Stack<Poly2Tri.Polygon>();

				// If we failed the previous iteration
				// use a higher quality cutting
				// this is heavier on the CPU, so only use it in special cases
				clipper.StrictlySimple = perturbate > -1;
				clipper.ReverseSolution = true;

				Int3[] clipIn = null;
				Int3[] clipOut = null;
				Int2 clipExtents = new Int2();

				if (navmeshAdds.Count > 0) {
					clipIn = new Int3[7];
					clipOut = new Int3[7];
					// TODO: What if the size is odd?
					clipExtents = new Int2(((Int3)realBounds.extents).x, ((Int3)realBounds.extents).z);
				}

				// This is an interesting loop
				// It loops over the ctris array
				// but when it reaches the end of the array
				// it will switch out the ctris array for the next array (from a navmesh add)
				// and restart the loop
				int addIndex = -1;
				int tri = -3;
				while (true) {
					tri += 3;
					while (tri >= ctris.Length) {
						addIndex++;
						tri = 0;

						if (addIndex >= navmeshAdds.Count) {
							cverts = null;
							break;
						}

						// This array must not be modified
						if (cverts == verts)
							cverts = null;

						navmeshAdds[addIndex].GetMesh(cuttingOffset, ref cverts, out ctris);
					}

					// Inner loop above decided that we should break the while(true) loop
					if (cverts == null)
						break;

					Int3 tp1 = cverts[ctris[tri + 0]];
					Int3 tp2 = cverts[ctris[tri + 1]];
					Int3 tp3 = cverts[ctris[tri + 2]];

					if (VectorMath.IsColinearXZ(tp1, tp2, tp3)) {
						Debug.LogWarning("Skipping degenerate triangle.");
						continue;
					}

					var triBounds = new IntRect(tp1.x, tp1.z, tp1.x, tp1.z);
					triBounds = triBounds.ExpandToContain(tp2.x, tp2.z);
					triBounds = triBounds.ExpandToContain(tp3.x, tp3.z);

					// Upper and lower bound on the Y-axis, the above bounds do not have Y axis information
					int tpYMin = Math.Min(tp1.y, Math.Min(tp2.y, tp3.y));
					int tpYMax = Math.Max(tp1.y, Math.Max(tp2.y, tp3.y));

					intersectingCuts.Clear();
					bool hasDual = false;

					for (int i = 0; i < cuts.Count; i++) {
						int ymin = cuts[i].boundsY.x;
						int ymax = cuts[i].boundsY.y;

						if (IntRect.Intersects(triBounds, cuts[i].bounds) && !(ymax< tpYMin || ymin > tpYMax) && (cuts[i].cutsAddedGeom || addIndex == -1)) {
							Int3 p1 = tp1;
							p1.y = ymin;
							Int3 p2 = tp1;
							p2.y = ymax;

							intersectingCuts.Add(i);
							hasDual |= cuts[i].isDual;
						}
					}

					// Check if this is just a simple triangle which no navmesh cuts intersect and
					// there are no other special things that should be done
					if (intersectingCuts.Count == 0 && (mode & CutMode.CutExtra) == 0 && (mode & CutMode.CutAll) != 0 && addIndex == -1) {
						// Just add the triangle and be done with it

						// Refers to vertices to be added a few lines below
						outtris.Add(outverts.Count + 0);
						outtris.Add(outverts.Count + 1);
						outtris.Add(outverts.Count + 2);

						outverts.Add(tp1);
						outverts.Add(tp2);
						outverts.Add(tp3);
						continue;
					}

					// Add current triangle as subject polygon for cutting
					poly.Clear();
					if (addIndex == -1) {
						// Geometry from a tile mesh is assumed to be completely inside the tile
						poly.Add(new IntPoint(tp1.x, tp1.z));
						poly.Add(new IntPoint(tp2.x, tp2.z));
						poly.Add(new IntPoint(tp3.x, tp3.z));
					} else {
						// Added geometry must be clipped against the tile bounds
						clipIn[0] = tp1;
						clipIn[1] = tp2;
						clipIn[2] = tp3;

						// TODO: Use size instead to avoid multiplication by 2
						int ct = ClipAgainstRectangle(clipIn, clipOut, new Int2(clipExtents.x*2, clipExtents.y*2));

						// Check if triangle was completely outside the tile
						if (ct == 0) {
							continue;
						}

						for (int q = 0; q < ct; q++)
							poly.Add(new IntPoint(clipIn[q].x, clipIn[q].z));
					}

					point2Index.Clear();

					// Loop through all possible modes (just 4 at the moment, so < 4 could be used actually)
					for (int cmode = 0; cmode < 16; cmode++) {
						// Ignore modes which are not active
						if ((((int)mode >> cmode) & 0x1) == 0)
							continue;

						if (1 << cmode == (int)CutMode.CutAll) {
							CutAll(poly, intersectingCuts, cuts, clipResult);
						} else if (1 << cmode == (int)CutMode.CutDual) {
							// No duals, don't bother processing this
							if (!hasDual)
								continue;

							CutDual(poly, intersectingCuts, cuts, hasDual, intermediateClipResult, clipResult);
						} else if (1 << cmode == (int)CutMode.CutExtra) {
							CutExtra(poly, extraClipShape, clipResult);
						}

						for (int exp = 0; exp < clipResult.ChildCount; exp++) {
							PolyNode node = clipResult.Childs[exp];
							List<IntPoint> outer = node.Contour;
							List<PolyNode> holes = node.Childs;

							if (holes.Count == 0 && outer.Count == 3 && addIndex == -1) {
								for (int i = 0; i < 3; i++) {
									var p = new Int3((int)outer[i].X, 0, (int)outer[i].Y);
									p.y = SampleYCoordinateInTriangle(tp1, tp2, tp3, p);

									outtris.Add(outverts.Count);
									outverts.Add(p);
								}
							} else {
								Poly2Tri.Polygon polygonToTriangulate = null;
								// Loop over outer and all holes
								int hole = -1;
								List<IntPoint> contour = outer;
								while (contour != null) {
									polypoints.Clear();
									for (int i = 0; i < contour.Count; i++) {
										// Create a new point
										var pp = new PolygonPoint(contour[i].X, contour[i].Y);

										// Add the point to the polygon
										polypoints.Add(pp);

										var p = new Int3((int)contour[i].X, 0, (int)contour[i].Y);
										p.y = SampleYCoordinateInTriangle(tp1, tp2, tp3, p);

										// Prepare a lookup table for pp -> vertex index
										point2Index[pp] = outverts.Count;

										// Add to resulting vertex list
										outverts.Add(p);
									}

									Poly2Tri.Polygon contourPolygon = null;
									if (polyCache.Count > 0) {
										contourPolygon = polyCache.Pop();
										contourPolygon.AddPoints(polypoints);
									} else {
										contourPolygon = new Poly2Tri.Polygon(polypoints);
									}

									// Since the outer contour is the first to be processed, polygonToTriangle will be null
									// Holes are processed later, when polygonToTriangle is not null
									if (hole == -1) {
										polygonToTriangulate = contourPolygon;
									} else {
										polygonToTriangulate.AddHole(contourPolygon);
									}

									hole++;
									contour = hole < holes.Count ? holes[hole].Contour : null;
								}

								// Triangulate the polygon with holes
								try {
									P2T.Triangulate(polygonToTriangulate);
								} catch (Poly2Tri.PointOnEdgeException) {
									Debug.LogWarning("PointOnEdgeException, perturbating vertices slightly.\nThis is usually fine. It happens sometimes because of rounding errors. Cutting will be retried a few more times.");
									CutPoly(verts, tris, ref outVertsArr, ref outTrisArr, out outVCount, out outTCount, extraShape, cuttingOffset, realBounds, mode, perturbate + 1);
									return;
								}

								try {
									for (int i = 0; i < polygonToTriangulate.Triangles.Count; i++) {
										Poly2Tri.DelaunayTriangle t = polygonToTriangulate.Triangles[i];

										// Add the triangle with the correct indices (using the previously built lookup table)
										outtris.Add(point2Index[t.Points._0]);
										outtris.Add(point2Index[t.Points._1]);
										outtris.Add(point2Index[t.Points._2]);
									}
								} catch (System.Collections.Generic.KeyNotFoundException) {
									Debug.LogWarning("KeyNotFoundException, perturbating vertices slightly.\nThis is usually fine. It happens sometimes because of rounding errors. Cutting will be retried a few more times.");
									CutPoly(verts, tris, ref outVertsArr, ref outTrisArr, out outVCount, out outTCount, extraShape, cuttingOffset, realBounds, mode, perturbate + 1);
									return;
								}

								PoolPolygon(polygonToTriangulate, polyCache);
							}
						}
					}
				}

				ListPool<IntPoint>.Release(poly);
				ListPool<Poly2Tri.PolygonPoint>.Release(polypoints);
			}

			// This next step will remove all duplicate vertices in the data (of which there are quite a few)
			// and output the final vertex and triangle arrays to the outVertsArr and outTrisArr variables
			CompressMesh(outverts, outtris, ref outVertsArr, ref outTrisArr, out outVCount, out outTCount);

			// Notify the navmesh cuts that they were used
			for (int i = 0; i < navmeshCuts.Count; i++) {
				navmeshCuts[i].UsedForCut();
			}

			// Release back to pools
			ListPool<Int3>.Release(outverts);
			ListPool<int>.Release(outtris);
			ListPool<int>.Release(intersectingCuts);

			for (int i = 0; i < cuts.Count; i++) {
				ListPool<IntPoint>.Release(cuts[i].contour);
			}

			ListPool<Cut>.Release(cuts);
			ListPool<NavmeshCut>.Release(navmeshCuts);
		}

		/** Generates a list of cuts from the navmesh cut components.
		 * Each cut has a single contour (NavmeshCut components may contain multiple).
		 */
		static List<Cut> PrepareNavmeshCutsForCutting (List<NavmeshCut> navmeshCuts, Int3 cuttingOffset, IntRect bounds, int perturbate, bool anyNavmeshAdds) {
			System.Random rnd = null;
			if (perturbate > 0) {
				rnd = new System.Random();
			}

			var contourBuffer = ListPool<List<IntPoint> >.Claim();
			var result = ListPool<Cut>.Claim();

			for (int i = 0; i < navmeshCuts.Count; i++) {
				Bounds worldBounds = navmeshCuts[i].GetBounds();
				Int3 mn = (Int3)worldBounds.min + cuttingOffset;
				Int3 mx = (Int3)worldBounds.max + cuttingOffset;
				IntRect boundsRect = new IntRect(mn.x, mn.z, mx.x, mx.z);

				// Use the navmesh cut if the bounds of it intersects the bounds of the
				// existing vertices or if there are any NavmeshAdd components in the tile
				// in which case they might intersect the cut, but the bounds parameter
				// does not take those into account (maybe TODO?)
				if (IntRect.Intersects(boundsRect, bounds) || anyNavmeshAdds) {
					// Generate random perturbation for this obstacle if required
					Int2 perturbation = new Int2(0, 0);
					if (perturbate > 0) {
						// Create a perturbation vector, choose a point with coordinates in the set [-3*perturbate,3*perturbate] \ 0
						// makes sure the coordinates are not zero

						perturbation.x = (rnd.Next() % 6*perturbate) - 3*perturbate;
						if (perturbation.x >= 0) perturbation.x++;

						perturbation.y = (rnd.Next() % 6*perturbate) - 3*perturbate;
						if (perturbation.y >= 0) perturbation.y++;
					}

					contourBuffer.Clear();
					navmeshCuts[i].GetContour(contourBuffer);

					for (int j = 0; j < contourBuffer.Count; j++) {
						List<IntPoint> contour = contourBuffer[j];

						if (contour.Count == 0) {
							Debug.LogError("Zero Length Contour");
							continue;
						}

						IntRect contourBounds = new IntRect((int)contour[0].X+cuttingOffset.x, (int)contour[0].Y+cuttingOffset.z, (int)contour[0].X+cuttingOffset.x, (int)contour[0].Y+cuttingOffset.z);

						for (int q = 0; q < contour.Count; q++) {
							IntPoint p = contour[q];
							p.X += cuttingOffset.x;
							p.Y += cuttingOffset.z;
							if (perturbate > 0) {
								p.X += perturbation.x;
								p.Y += perturbation.y;
							}

							contourBounds = contourBounds.ExpandToContain((int)p.X, (int)p.Y);
							contour[q] = new IntPoint(p.X, p.Y);
						}

						Cut cut = new Cut();
						cut.boundsY = new Int2(mn.y, mx.y);
						cut.bounds = contourBounds;
						cut.isDual = navmeshCuts[i].isDual;
						cut.cutsAddedGeom = navmeshCuts[i].cutsAddedGeom;
						cut.contour = contour;
						result.Add(cut);
					}
				}
			}

			ListPool<List<IntPoint> >.Release(contourBuffer);
			return result;
		}

		static void PoolPolygon (Poly2Tri.Polygon polygon, Stack<Poly2Tri.Polygon> pool) {
			if (polygon.Holes != null)
				for (int i = 0; i < polygon.Holes.Count; i++) {
					polygon.Holes[i].Points.Clear();
					polygon.Holes[i].ClearTriangles();

					if (polygon.Holes[i].Holes != null)
						polygon.Holes[i].Holes.Clear();

					pool.Push(polygon.Holes[i]);
				}
			polygon.ClearTriangles();
			if (polygon.Holes != null)
				polygon.Holes.Clear();
			polygon.Points.Clear();
			pool.Push(polygon);
		}

		/** Sample Y coordinate of the triangle (p1, p2, p3) at the point p in XZ space.
		 * The y coordinate of \a p is ignored.
		 *
		 * \returns The interpolated y coordinate unless the triangle is degenerate in which case a DivisionByZeroException will be thrown
		 *
		 * \see https://en.wikipedia.org/wiki/Barycentric_coordinate_system
		 */
		static int SampleYCoordinateInTriangle (Int3 p1, Int3 p2, Int3 p3, Int3 p) {
			double det = ((double)(p2.z - p3.z)) * (p1.x - p3.x) + ((double)(p3.x - p2.x)) * (p1.z - p3.z);

			double lambda1 = ((((double)(p2.z - p3.z)) * (p.x - p3.x) + ((double)(p3.x - p2.x)) * (p.z - p3.z)) / det);
			double lambda2 = ((((double)(p3.z - p1.z)) * (p.x - p3.x) + ((double)(p1.x - p3.x)) * (p.z - p3.z)) / det);

			return (int)Math.Round(lambda1 * p1.y + lambda2 * p2.y + (1 - lambda1 - lambda2) * p3.y);
		}

		void CutAll (List<IntPoint> poly, List<int> intersectingCutIndices, List<Cut> cuts, Pathfinding.ClipperLib.PolyTree result) {
			clipper.Clear();
			clipper.AddPolygon(poly, PolyType.ptSubject);

			// Add all holes (cuts) as clip polygons
			// TODO: AddPolygon allocates quite a lot, modify ClipperLib to use object pooling
			for (int i = 0; i < intersectingCutIndices.Count; i++) {
				clipper.AddPolygon(cuts[intersectingCutIndices[i]].contour, PolyType.ptClip);
			}

			result.Clear();
			clipper.Execute(ClipType.ctDifference, result, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
		}

		void CutDual (List<IntPoint> poly, List<int> tmpIntersectingCuts, List<Cut> cuts, bool hasDual, List<List<IntPoint> > intermediateResult, Pathfinding.ClipperLib.PolyTree result) {
			// First calculate
			// a = original intersection dualCuts
			// then
			// b = a difference normalCuts
			// then process b as normal
			clipper.Clear();
			clipper.AddPolygon(poly, PolyType.ptSubject);

			// Add all holes (cuts) as clip polygons
			for (int i = 0; i < tmpIntersectingCuts.Count; i++) {
				if (cuts[tmpIntersectingCuts[i]].isDual) {
					clipper.AddPolygon(cuts[tmpIntersectingCuts[i]].contour, PolyType.ptClip);
				}
			}

			clipper.Execute(ClipType.ctIntersection, intermediateResult, PolyFillType.pftEvenOdd, PolyFillType.pftNonZero);
			clipper.Clear();

			if (intermediateResult != null) {
				for (int i = 0; i < intermediateResult.Count; i++) {
					clipper.AddPolygon(intermediateResult[i], Pathfinding.ClipperLib.Clipper.Orientation(intermediateResult[i]) ? PolyType.ptClip : PolyType.ptSubject);
				}
			}

			for (int i = 0; i < tmpIntersectingCuts.Count; i++) {
				if (!cuts[tmpIntersectingCuts[i]].isDual) {
					clipper.AddPolygon(cuts[tmpIntersectingCuts[i]].contour, PolyType.ptClip);
				}
			}

			result.Clear();
			clipper.Execute(ClipType.ctDifference, result, PolyFillType.pftEvenOdd, PolyFillType.pftNonZero);
		}

		void CutExtra (List<IntPoint> poly, List<IntPoint> extraClipShape, Pathfinding.ClipperLib.PolyTree result) {
			clipper.Clear();
			clipper.AddPolygon(poly, PolyType.ptSubject);
			clipper.AddPolygon(extraClipShape, PolyType.ptClip);

			result.Clear();
			clipper.Execute(ClipType.ctIntersection, result, PolyFillType.pftEvenOdd, PolyFillType.pftNonZero);
		}

		/** Clips the input polygon against a rectangle with one corner at the origin and one at size in XZ space.
		 * \param clipIn Input vertices
		 * \param clipOut Output vertices. This buffer must be large enough to contain all output vertices.
		 * \param size The clipping rectangle has one corner at the origin and one at this position in XZ space.
		 *
		 * \returns Number of output vertices
		 */
		int ClipAgainstRectangle (Int3[] clipIn, Int3[] clipOut, Int2 size) {
			int ct;

			ct = simpleClipper.ClipPolygon(clipIn, 3, clipOut, 1, 0, 0);
			if (ct == 0)
				return ct;

			ct = simpleClipper.ClipPolygon(clipOut, ct, clipIn, -1, size.x, 0);
			if (ct == 0)
				return ct;

			ct = simpleClipper.ClipPolygon(clipIn, ct, clipOut, 1, 0, 2);
			if (ct == 0)
				return ct;

			ct = simpleClipper.ClipPolygon(clipOut, ct, clipIn, -1, size.y, 2);
			return ct;
		}

		/** Copy mesh from (vertices, triangles) to (outVertices, outTriangles) */
		void CopyMesh (Int3[] vertices, int[] triangles, List<Int3> outVertices, List<int> outTriangles) {
			outTriangles.Capacity = Math.Max(outTriangles.Capacity, triangles.Length);
			outVertices.Capacity = Math.Max(outVertices.Capacity, triangles.Length);

			for (int i = 0; i < vertices.Length; i++) {
				outVertices.Add(vertices[i]);
			}

			for (int i = 0; i < triangles.Length; i++) {
				outTriangles.Add(triangles[i]);
			}
		}

		/** Compress the mesh by removing duplicate vertices.
		 *
		 * \param vertices Vertices of the input mesh
		 * \param triangles Triangles of the input mesh
		 * \param outVertices Vertices of the output mesh.
		 *      If non-null, this buffer will be reused instead of allocating a new one if it is large enough.
		 *      The indices up to (but not including) outVertexCount are the final vertices.
		 * \param outTriangles Triangles of the output mesh.
		 *      If non-null, this buffer will be reused instead of allocating a new one if it is large enough.
		 *      The indices up to (but not including) outVertexCount are the final triangles.
		 * \param outVertexCount The number of vertices in the output mesh.
		 * \param outTriangleCount The number of used indices in the outTriangles array.
		 *      This will be the number of triangles multiplied by 3.
		 *
		 * \see CutPoly
		 */
		void CompressMesh (List<Int3> vertices, List<int> triangles, ref Int3[] outVertices, ref int[] outTriangles, out int outVertexCount, out int outTriangleCount) {
			// TODO: Structs as keys in a Dictionary may allocate when using Mono
			Dictionary<Int3, int> firstVerts = cached_Int3_int_dict;
			firstVerts.Clear();

			// Use cached array to reduce memory allocations
			if (cached_int_array.Length < vertices.Count)
				cached_int_array = new int[Math.Max(cached_int_array.Length * 2, vertices.Count)];
			int[] compressedPointers = cached_int_array;

			// Map positions to the first index they were encountered at
			int count = 0;
			for (int i = 0; i < vertices.Count; i++) {
				// Check if the vertex position has already been added
				// Also check one position up and one down because rounding errors can cause vertices
				// that should end up in the same position to be offset 1 unit from each other
				// TODO: Check along X and Z axes as well?
				int ind;
				if (!firstVerts.TryGetValue(vertices[i], out ind) && !firstVerts.TryGetValue(vertices[i] + new Int3(0, 1, 0), out ind) && !firstVerts.TryGetValue(vertices[i] + new Int3(0, -1, 0), out ind)) {
					firstVerts.Add(vertices[i], count);
					compressedPointers[i] = count;
					vertices[count] = vertices[i];
					count++;
				} else {
					compressedPointers[i] = ind;
				}
			}

			// Create the triangle array or reuse the existing buffer
			outTriangleCount = triangles.Count;
			if (outTriangles == null || outTriangles.Length < outTriangleCount)
				outTriangles = new int[outTriangleCount];

			// Remap the triangles to the new compressed indices
			for (int i = 0; i < outTriangleCount; i++) {
				outTriangles[i] = compressedPointers[triangles[i]];
			}

			// Create the vertex array or reuse the existing buffer
			outVertexCount = count;
			if (outVertices == null || outVertices.Length < outVertexCount)
				outVertices = new Int3[outVertexCount];

			for (int i = 0; i < outVertexCount; i++)
				outVertices[i] = vertices[i];
		}

		/** Refine a mesh using delaunay refinement.
		 * Loops through all pairs of neighbouring triangles and check if it would be better to flip the diagonal joining them
		 * using the delaunay criteria.
		 *
		 * Does not require triangles to be clockwise, triangles will be checked for if they are clockwise and made clockwise if not.
		 * The resulting mesh will have all triangles clockwise.
		 *
		 * \see https://en.wikipedia.org/wiki/Delaunay_triangulation
		 */
		void DelaunayRefinement (Int3[] verts, int[] tris, ref int vCount, ref int tCount, bool delaunay, bool colinear, Int3 worldOffset) {
			if (tCount % 3 != 0) throw new System.ArgumentException("Triangle array length must be a multiple of 3");

			Dictionary<Int2, int> lookup = cached_Int2_int_dict;
			lookup.Clear();

			for (int i = 0; i < tCount; i += 3) {
				if (!VectorMath.IsClockwiseXZ(verts[tris[i]], verts[tris[i+1]], verts[tris[i+2]])) {
					int tmp = tris[i];
					tris[i] = tris[i+2];
					tris[i+2] = tmp;
				}

				lookup[new Int2(tris[i+0], tris[i+1])] = i+2;
				lookup[new Int2(tris[i+1], tris[i+2])] = i+0;
				lookup[new Int2(tris[i+2], tris[i+0])] = i+1;
			}

			for (int i = 0; i < tCount; i += 3) {
				for (int j = 0; j < 3; j++) {
					int opp;

					if (lookup.TryGetValue(new Int2(tris[i+((j+1)%3)], tris[i+((j+0)%3)]), out opp)) {
						// The vertex which we are using as the viewpoint
						Int3 po = verts[tris[i+((j+2)%3)]];

						// Right vertex of the edge
						Int3 pr = verts[tris[i+((j+1)%3)]];

						// Left vertex of the edge
						Int3 pl = verts[tris[i+((j+3)%3)]];

						// Opposite vertex (in the other triangle)
						Int3 popp = verts[tris[opp]];

						po.y = 0;
						pr.y = 0;
						pl.y = 0;
						popp.y = 0;

						bool noDelaunay = false;

						if (!VectorMath.RightOrColinearXZ(po, pl, popp) || VectorMath.RightXZ(po, pr, popp)) {
							if (colinear) {
								noDelaunay = true;
							} else {
								continue;
							}
						}

						if (colinear) {
							const int MaxError = 3 * 3;

							// Check if op - right shared - opposite in other - is colinear
							// and if the edge right-op is not shared and if the edge opposite in other - right shared is not shared
							if (VectorMath.SqrDistancePointSegmentApproximate(po, popp, pr) < MaxError &&
								!lookup.ContainsKey(new Int2(tris[i+((j+2)%3)], tris[i+((j+1)%3)])) &&
								!lookup.ContainsKey(new Int2(tris[i+((j+1)%3)], tris[opp]))) {
								tCount -= 3;

								int root = (opp/3)*3;

								// Move right vertex to the other triangle's opposite
								tris[i+((j+1)%3)] = tris[opp];

								// Move last triangle to delete
								if (root != tCount) {
									tris[root+0] = tris[tCount+0];
									tris[root+1] = tris[tCount+1];
									tris[root+2] = tris[tCount+2];
									lookup[new Int2(tris[root+0], tris[root+1])] = root+2;
									lookup[new Int2(tris[root+1], tris[root+2])] = root+0;
									lookup[new Int2(tris[root+2], tris[root+0])] = root+1;

									tris[tCount+0] = 0;
									tris[tCount+1] = 0;
									tris[tCount+2] = 0;
								} else {
									tCount += 3;
								}

								// Since the above mentioned edges are not shared, we don't need to bother updating them

								// However some need to be updated
								// left - new right (previously opp) should have opposite vertex po
								//lookup[new Int2(tris[i+((j+3)%3)],tris[i+((j+1)%3)])] = i+((j+2)%3);

								lookup[new Int2(tris[i+0], tris[i+1])] = i+2;
								lookup[new Int2(tris[i+1], tris[i+2])] = i+0;
								lookup[new Int2(tris[i+2], tris[i+0])] = i+1;
								continue;
							}
						}

						if (delaunay && !noDelaunay) {
							float beta = Int3.Angle(pr-po, pl-po);
							float alpha = Int3.Angle(pr-popp, pl-popp);

							if (alpha > (2*Mathf.PI - 2*beta)) {
								// Denaunay condition not holding, refine please
								tris[i+((j+1)%3)] = tris[opp];

								int root = (opp/3)*3;
								int off = opp-root;
								tris[root+((off-1+3) % 3)] = tris[i+((j+2)%3)];

								lookup[new Int2(tris[i+0], tris[i+1])] = i+2;
								lookup[new Int2(tris[i+1], tris[i+2])] = i+0;
								lookup[new Int2(tris[i+2], tris[i+0])] = i+1;

								lookup[new Int2(tris[root+0], tris[root+1])] = root+2;
								lookup[new Int2(tris[root+1], tris[root+2])] = root+0;
								lookup[new Int2(tris[root+2], tris[root+0])] = root+1;
							}
						}
					}
				}
			}
		}

		/** Converts between point types.
		 * Internal method used for debugging
		 */
		Vector3 Point2D2V3 (Poly2Tri.TriangulationPoint p) {
			return new Vector3((float)p.X, 0, (float)p.Y)*Int3.PrecisionFactor;
		}

		/** Converts between point types.
		 * Internal method used for debugging
		 */
		Int3 IntPoint2Int3 (IntPoint p) {
			return new Int3((int)p.X, 0, (int)p.Y);
		}

		/** Clear the tile at the specified tile coordinates */
		public void ClearTile (int x, int z) {
			if (AstarPath.active == null) return;

			if (x < 0 || z < 0 || x >= graph.tileXCount || z >= graph.tileZCount) return;

			AstarPath.active.AddWorkItem(new AstarWorkItem((context, force) => {
				//Replace the tile using the final vertices and triangles
				graph.ReplaceTile(x, z, new Int3[0], new int[0], false);

				activeTileTypes[x + z*graph.tileXCount] = null;
				//Trigger post update event
				//This can trigger for example recalculation of navmesh links
				GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);

				//Flood fill everything to make sure graph areas are still valid
				//This tends to take more than 50% of the calculation time
				context.QueueFloodFill();

				return true;
			}));
		}

		/** Reloads all tiles intersecting with the specified bounds */
		public void ReloadInBounds (Bounds b) {
			Int2 min = graph.GetTileCoordinates(b.min);
			Int2 max = graph.GetTileCoordinates(b.max);
			var r = new IntRect(min.x, min.y, max.x, max.y);

			// Make sure the rect is inside graph bounds
			r = IntRect.Intersection(r, new IntRect(0, 0, graph.tileXCount-1, graph.tileZCount-1));

			if (!r.IsValid()) return;

			for (int z = r.ymin; z <= r.ymax; z++) {
				for (int x = r.xmin; x <= r.xmax; x++) {
					ReloadTile(x, z);
				}
			}
		}

		/** Reload tile at tile coordinate.
		 * The last tile loaded at that position will be reloaded (e.g to account for moved NavmeshCut components)
		 */
		public void ReloadTile (int x, int z) {
			if (x < 0 || z < 0 || x >= graph.tileXCount || z >= graph.tileZCount) return;

			int index = x + z*graph.tileXCount;
			if (activeTileTypes[index] != null) LoadTile(activeTileTypes[index], x, z, activeTileRotations[index], activeTileOffsets[index]);
		}

		public void CutShapeWithTile (int x, int z, Int3[] shape, ref Int3[] verts, ref int[] tris, out int vCount, out int tCount) {
			if (isBatching) {
				throw new Exception("Cannot cut with shape when batching. Please stop batching first.");
			}

			int index = x + z*graph.tileXCount;

			if (x < 0 || z < 0 || x >= graph.tileXCount || z >= graph.tileZCount || activeTileTypes[index] == null) {
				verts = new Int3[0];
				tris = new int[0];
				vCount = 0;
				tCount = 0;
				return;
			}

			Int3[] tverts;
			int[] ttris;

			activeTileTypes[index].Load(out tverts, out ttris, activeTileRotations[index], activeTileOffsets[index]);

			//Calculate tile bounds so that the correct cutting offset can be used
			//The tile will be cut in local space (i.e it is at the world origin) so cuts need to be translated
			//to that point from their world space coordinates
			Bounds r = graph.GetTileBounds(x, z);
			var cutOffset = (Int3)r.min;
			cutOffset = -cutOffset;

			CutPoly(tverts, ttris, ref verts, ref tris, out vCount, out tCount, shape, cutOffset, r, CutMode.CutExtra);

			for (int i = 0; i < verts.Length; i++) verts[i] -= cutOffset;
		}

		/** Returns a new array with at most length \a newLength.
		 * The array will contain a copy of all elements of \a arr up to but excluding the index newLength.
		 */
		protected static T[] ShrinkArray<T>(T[] arr, int newLength) {
			newLength = Math.Min(newLength, arr.Length);
			var shrunkArr = new T[newLength];

			// Unrolling
			if (newLength % 4 == 0) {
				for (int i = 0; i < newLength; i += 4) {
					shrunkArr[i+0] = arr[i+0];
					shrunkArr[i+1] = arr[i+1];
					shrunkArr[i+2] = arr[i+2];
					shrunkArr[i+3] = arr[i+3];
				}
			} else if (newLength % 3 == 0) {
				for (int i = 0; i < newLength; i += 3) {
					shrunkArr[i+0] = arr[i+0];
					shrunkArr[i+1] = arr[i+1];
					shrunkArr[i+2] = arr[i+2];
				}
			} else if (newLength % 2 == 0) {
				for (int i = 0; i < newLength; i += 2) {
					shrunkArr[i+0] = arr[i+0];
					shrunkArr[i+1] = arr[i+1];
				}
			} else {
				for (int i = 0; i < newLength; i++) {
					shrunkArr[i+0] = arr[i+0];
				}
			}
			return shrunkArr;
		}

		/** Load a tile at tile coordinate \a x, \a z.
		 *
		 * \param tile Tile type to load
		 * \param x Tile x coordinate (first tile is at (0,0), second at (1,0) etc.. ).
		 * \param z Tile z coordinate.
		 * \param rotation Rotate tile by 90 degrees * value.
		 * \param yoffset Offset Y coordinates by this amount. In Int3 space, so if you have a world space
		 *      offset, multiply by Int3.Precision and round to the nearest integer before calling this function.
		 */
		public void LoadTile (TileType tile, int x, int z, int rotation, int yoffset) {
			if (tile == null) throw new ArgumentNullException("tile");

			if (AstarPath.active == null) return;

			int index = x + z*graph.tileXCount;
			rotation = rotation % 4;

			// If loaded during this batch with the same settings, skip it
			if (isBatching && reloadedInBatch[index] && activeTileOffsets[index] == yoffset && activeTileRotations[index] == rotation && activeTileTypes[index] == tile) {
				return;
			}

			reloadedInBatch[index] |= isBatching;

			activeTileOffsets[index] = yoffset;
			activeTileRotations[index] = rotation;
			activeTileTypes[index] = tile;

			// Add a work item
			// This will pause pathfinding as soon as possible
			// and call the delegate when it is safe to update graphs
			AstarPath.active.AddWorkItem(new AstarWorkItem((context, force) => {
				// If this was not the correct settings to load with, ignore
				if (!(activeTileOffsets[index] == yoffset && activeTileRotations[index] == rotation && activeTileTypes[index] == tile)) return true;

				GraphModifier.TriggerEvent(GraphModifier.EventType.PreUpdate);

				Int3[] verts;
				int[] tris;

				tile.Load(out verts, out tris, rotation, yoffset);

				// Calculate tile bounds so that the correct cutting offset can be used
				// The tile will be cut in local space (i.e it is at the world origin) so cuts need to be translated
				// to that point from their world space coordinates
				Bounds r = graph.GetTileBounds(x, z, tile.Width, tile.Depth);
				var cutOffset = (Int3)r.min;
				cutOffset = -cutOffset;

				Int3[] outVerts = null;
				int[] outTris = null;
				int vCount, tCount;

				UnityEngine.Profiling.Profiler.BeginSample("Cut Poly");
				// Cut the polygon
				CutPoly(verts, tris, ref outVerts, ref outTris, out vCount, out tCount, null, cutOffset, r);
				UnityEngine.Profiling.Profiler.EndSample();

				UnityEngine.Profiling.Profiler.BeginSample("Delaunay Refinement");
				// Refine to remove bad triangles
				DelaunayRefinement(outVerts, outTris, ref vCount, ref tCount, true, false, -cutOffset);
				UnityEngine.Profiling.Profiler.EndSample();

				if (tCount != outTris.Length) outTris = ShrinkArray(outTris, tCount);
				if (vCount != outVerts.Length) outVerts = ShrinkArray(outVerts, vCount);

				// Rotate the mask correctly
				// and update width and depth to match rotation
				// (width and depth will swap if rotated 90 or 270 degrees )
				int newWidth = rotation % 2 == 0 ? tile.Width : tile.Depth;
				int newDepth = rotation % 2 == 0 ? tile.Depth : tile.Width;

				UnityEngine.Profiling.Profiler.BeginSample("ReplaceTile");
				// Replace the tile using the final vertices and triangles
				// The vertices are still in local space
				graph.ReplaceTile(x, z, newWidth, newDepth, outVerts, outTris, false);
				UnityEngine.Profiling.Profiler.EndSample();

				// Trigger post update event
				// This can trigger for example recalculation of navmesh links
				GraphModifier.TriggerEvent(GraphModifier.EventType.PostUpdate);

				// Flood fill everything to make sure graph areas are still valid
				// This tends to take more than 50% of the calculation time
				context.QueueFloodFill();

				return true;
			}));
		}
	}
}
