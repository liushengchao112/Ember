using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.RVO;

namespace Pathfinding {
	[RequireComponent(typeof(Seeker))]
	[AddComponentMenu("Pathfinding/AI/RichAI (3D, for navmesh)")]
	/** Advanced AI for navmesh based graphs.
	 * \astarpro
	 */
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_rich_a_i.php")]
	public class RichAI : MonoBehaviour {
		public Transform target;

		/** Draw gizmos in the scene view */
		public bool drawGizmos = true;

		/** Search for new paths repeatedly */
		public bool repeatedlySearchPaths = false;

		/** Delay (seconds) between path searches */
		public float repathRate = 0.5f;

		/** Max speed of the agent.
		* World units per second */
		public float maxSpeed = 1;
		/** Max acceleration of the agent.
		 * World units per second per second */
		public float acceleration = 5;
		/** How much time to use for slowdown in the end of the path.
		 * A lower value give more abrupt stops
		 */
		public float slowdownTime = 0.5f;
		/** Max rotation speed of the agent.
		 * In degrees per second.
		 */
		public float rotationSpeed = 360;
		/** Max distance to the endpoint to consider it reached */
		public float endReachedDistance = 0.01f;
		/** Force to avoid walls with.
		 * The agent will try to steer away from walls slightly. */
		public float wallForce = 3;
		/** Walls within this range will be used for avoidance.
		 * Setting this to zero disables wall avoidance and may improve performance slightly */
		public float wallDist = 1;

		/** Gravity to use in case no character controller is attached */
		public Vector3 gravity = new Vector3(0, -9.82f, 0);

		/** Raycast for ground placement (when not having a CharacterController).
		 * A raycast from position + up*#centerOffset downwards will be done and the agent will be placed at this point.
		 */
		public bool raycastingForGroundPlacement = true;

		/** Layer mask to use for ground placement.
		 * Make sure this does not include the layer of any colliders attached to this gameobject.
		 */
		public LayerMask groundMask = -1;
		public float centerOffset = 1;

		/** Mode for funnel simplification.
		 * On tiled navmesh maps, but sometimes on normal ones as well, it can be good to simplify
		 * the funnel as a post-processing step.
		 */
		public RichFunnel.FunnelSimplification funnelSimplification = RichFunnel.FunnelSimplification.None;
		public Animation anim;

		/** Use a 3rd degree equation for calculating slowdown acceleration instead of a 2nd degree.
		 * A 3rd degree equation can also make sure that the velocity when reaching the target is roughly zero and therefore
		 * it will have a more direct stop. In contrast solving a 2nd degree equation which will just make sure the target is reached but
		 * will usually have a larger velocity when reaching the target and therefore look more "bouncy".
		 */
		public bool preciseSlowdown = true;

		/** Slow down when not facing the target direction.
		 * Incurs at a small overhead.
		 */
		public bool slowWhenNotFacingTarget = true;

		/** Current velocity of the agent.
		 * Includes eventual velocity due to gravity */
		Vector3 velocity;

		/** Current velocity of the agent.
		 * Includes eventual velocity due to gravity */
		public Vector3 Velocity {
			get {
				return velocity;
			}
		}

		protected RichPath rp;

		protected Seeker seeker;
		protected Transform tr;
		CharacterController controller;
		RVOController rvoController;

		Vector3 lastTargetPoint;
		Vector3 currentTargetDirection;

		protected bool waitingForPathCalc;
		protected bool canSearchPath;
		protected bool delayUpdatePath;
		protected bool traversingSpecialPath;
		protected bool lastCorner;
		float distanceToWaypoint = 999;

		protected List<Vector3> nextCorners = new List<Vector3>();
		protected List<Vector3> wallBuffer = new List<Vector3>();

		bool startHasRun;
		protected float lastRepath = -9999;

		void Awake () {
			seeker = GetComponent<Seeker>();
			controller = GetComponent<CharacterController>();
			rvoController = GetComponent<RVOController>();
			tr = transform;
		}

		/** Starts searching for paths.
		 * If you override this function you should in most cases call base.Start () at the start of it.
		 * \see OnEnable
		 * \see SearchPaths
		 */
		protected virtual void Start () {
			startHasRun = true;
			OnEnable();
		}

		/** Run at start and when reenabled.
		 * Starts RepeatTrySearchPath.
		 *
		 * \see Start
		 */
		protected virtual void OnEnable () {
			lastRepath = -9999;
			waitingForPathCalc = false;
			canSearchPath = true;

			if (startHasRun) {
				//Make sure we receive callbacks when paths complete
				seeker.pathCallback += OnPathComplete;

				StartCoroutine(SearchPaths());
			}
		}

		public void OnDisable () {
			// Abort calculation of path
			if (seeker != null && !seeker.IsDone()) seeker.GetCurrentPath().Error();

			//Make sure we receive callbacks when paths complete
			seeker.pathCallback -= OnPathComplete;
		}

		/** Force recalculation of the current path.
		 * If there is an ongoing path calculation, it will be canceled (so make sure you leave time for the paths to get calculated before calling this function again).
		 */
		public virtual void UpdatePath () {
			canSearchPath = true;
			waitingForPathCalc = false;
			Path p = seeker.GetCurrentPath();

			//Cancel any eventual pending pathfinding request
			if (p != null && !seeker.IsDone()) {
				p.Error();
				// Make sure it is recycled. We won't receive a callback for this one since we
				// replace the path directly after this
				p.Claim(this);
				p.Release(this);
			}

			waitingForPathCalc = true;
			lastRepath = Time.time;
			seeker.StartPath(tr.position, target.position);
		}

		IEnumerator SearchPaths () {
			while (true) {
				while (!repeatedlySearchPaths || waitingForPathCalc || !canSearchPath || Time.time - lastRepath < repathRate) yield return null;
				//canSearchPath = false;

				//waitingForPathCalc = true;
				//lastRepath = Time.time;
				//seeker.StartPath (tr.position, target.position);
				UpdatePath();

				yield return null;
			}
		}

		void OnPathComplete (Path p) {
			waitingForPathCalc = false;
			p.Claim(this);

			if (p.error) {
				p.Release(this);
				return;
			}

			if (traversingSpecialPath) {
				delayUpdatePath = true;
			} else {
				if (rp == null) rp = new RichPath();
				rp.Initialize(seeker, p, true, funnelSimplification);
			}
			p.Release(this);
		}

		public bool TraversingSpecial {
			get {
				return traversingSpecialPath;
			}
		}

		/** Current target point.
		 */
		public Vector3 TargetPoint {
			get {
				return lastTargetPoint;
			}
		}

		/** True if approaching the last waypoint in the current path */
		public bool ApproachingPartEndpoint {
			get {
				return lastCorner;
			}
		}

		/** True if approaching the last waypoint of all parts in the current path */
		public bool ApproachingPathEndpoint {
			get {
				return rp != null && ApproachingPartEndpoint && !rp.PartsLeft();
			}
		}

		/** Distance to the next waypoint */
		public float DistanceToNextWaypoint {
			get {
				return distanceToWaypoint;
			}
		}

		/** Declare that the AI has completely traversed the current part.
		 * This will skip to the next part, or call OnTargetReached if this was the last part
		 */
		void NextPart () {
			rp.NextPart();
			lastCorner = false;
			if (!rp.PartsLeft()) {
				//End
				OnTargetReached();
			}
		}

		/** Smooth delta time to avoid getting overly affected by e.g GC */
		static float deltaTime;

		/** Called when the end of the path is reached */
		protected virtual void OnTargetReached () {
		}

		protected virtual Vector3 UpdateTarget (RichFunnel fn) {
			nextCorners.Clear();

			/* Current position. We read and write to tr.position as few times as possible since doing so
			 * is much slower than to read and write from/to a local variable
			 */
			Vector3 position = tr.position;
			bool requiresRepath;
			position = fn.Update(position, nextCorners, 2, out lastCorner, out requiresRepath);

			if (requiresRepath && !waitingForPathCalc) {
				UpdatePath();
			}

			return position;
		}

		static Vector2 To2D (Vector3 v) {
			return new Vector2(v.x, v.z);
		}

		/** Update is called once per frame */
		protected virtual void Update () {
			deltaTime = Mathf.Min(Time.smoothDeltaTime*2, Time.deltaTime);

			if (rp != null) {
				RichPathPart currentPart = rp.GetCurrentPart();
				var fn = currentPart as RichFunnel;
				if (fn != null) {
					// Clamp the current position to the navmesh
					// and update the list of upcoming corners in the path
					// and store that in the 'nextCorners' variable
					Vector3 position = UpdateTarget(fn);

					// Only get walls every 5th frame to save on performance
					if (Time.frameCount % 5 == 0 && wallForce > 0 && wallDist > 0) {
						wallBuffer.Clear();
						fn.FindWalls(wallBuffer, wallDist);
					}

					// Target point
					int tgIndex = 0;
					Vector3 targetPoint = nextCorners[tgIndex];
					Vector3 dir = targetPoint-position;
					dir.y = 0;

					bool passedTarget = Vector3.Dot(dir, currentTargetDirection) < 0;
					// Check if passed target in another way
					if (passedTarget && nextCorners.Count-tgIndex > 1) {
						tgIndex++;
						targetPoint = nextCorners[tgIndex];
					}

					// Check if the target point changed compared to last frame
					if (targetPoint != lastTargetPoint) {
						currentTargetDirection = targetPoint - position;
						currentTargetDirection.y = 0;
						currentTargetDirection.Normalize();
						lastTargetPoint = targetPoint;
					}

					// Direction to target
					dir = targetPoint - position;
					dir.y = 0;

					// Normalized direction
					Vector3 normdir = VectorMath.Normalize(dir, out distanceToWaypoint);

					// Is the endpoint of the path (part) the current target point
					bool targetIsEndPoint = lastCorner && nextCorners.Count-tgIndex == 1;

					// When very close to the target point, move directly towards the target
					// instead of using accelerations as they tend to be a bit jittery in this case
					if (targetIsEndPoint && distanceToWaypoint < 0.01f * maxSpeed) {
						// Velocity will be at most 1 times max speed, it will be further clamped below
						velocity = (targetPoint - position) * 100;
					} else {
						// Calculate force from walls
						Vector3 wallForceVector = CalculateWallForce(position, normdir);
						Vector2 accelerationVector;

						if (targetIsEndPoint) {
							accelerationVector = CalculateAccelerationToReachPoint(To2D(targetPoint - position), Vector2.zero, To2D(velocity));
							//accelerationVector = Vector3.ClampMagnitude(accelerationVector, acceleration);

							// Reduce the wall avoidance force as we get closer to our target
							wallForceVector *= System.Math.Min(distanceToWaypoint/0.5f, 1);

							if (distanceToWaypoint < endReachedDistance) {
								// END REACHED
								NextPart();
							}
						} else {
							var nextNextCorner = tgIndex < nextCorners.Count-1 ? nextCorners[tgIndex+1] : (targetPoint - position)*2 + position;
							var targetVelocity = (nextNextCorner - targetPoint).normalized * maxSpeed;

							accelerationVector = CalculateAccelerationToReachPoint(To2D(targetPoint - position), To2D(targetVelocity), To2D(velocity));
						}

						// Update the velocity using the acceleration
						velocity += (new Vector3(accelerationVector.x, 0, accelerationVector.y) + wallForceVector*wallForce)*deltaTime;
					}

					var currentNode = fn.CurrentNode;

					Vector3 closestOnNode;
					if (currentNode != null) {
						closestOnNode = currentNode.ClosestPointOnNode(position);
					} else {
						closestOnNode = position;
					}

					// Distance to the end of the path (as the crow flies)
					var distToEndOfPath = (fn.exactEnd - closestOnNode).magnitude;

					// Max speed to use for this frame
					var currentMaxSpeed = maxSpeed;
					currentMaxSpeed *= Mathf.Sqrt(Mathf.Min(1, distToEndOfPath / (maxSpeed * slowdownTime)));

					// Check if the agent should slow down in case it is not facing the direction it wants to move in
					if (slowWhenNotFacingTarget) {
						// 1 when normdir is in the same direction as tr.forward
						// 0.2 when they point in the opposite directions
						float directionSpeedFactor = Mathf.Max((Vector3.Dot(normdir, tr.forward)+0.5f)/1.5f, 0.2f);
						currentMaxSpeed *= directionSpeedFactor;
						float currentSpeed = VectorMath.MagnitudeXZ(velocity);
						float prevy = velocity.y;
						velocity.y = 0;
						currentSpeed = Mathf.Min(currentSpeed, currentMaxSpeed);

						// Make sure the agent always moves in the forward direction
						// except when getting close to the end of the path in which case
						// the velocity can be in any direction
						velocity = Vector3.Lerp(velocity.normalized * currentSpeed, tr.forward * currentSpeed, Mathf.Clamp(targetIsEndPoint ? distanceToWaypoint*2 : 1, 0.0f, 0.5f));

						velocity.y = prevy;
					} else {
						velocity = VectorMath.ClampMagnitudeXZ(velocity, currentMaxSpeed);
					}

					// Apply gravity
					velocity += deltaTime * gravity;

					if (rvoController != null && rvoController.enabled) {
						// Send a message to the RVOController that we want to move
						// with this velocity. In the next simulation step, this velocity
						// will be processed and it will be fed back the rvo controller
						// and finally it will be used by this script when calling the
						// CalculateMovementDelta method below

						// Make sure that we don't move further than to the end point of the path
						// If the RVO simulation FPS is low and we did not do this, the agent
						// might overshoot the target a lot.
						var rvoTarget = position + VectorMath.ClampMagnitudeXZ(velocity, distToEndOfPath);
						rvoController.SetTarget(rvoTarget, VectorMath.MagnitudeXZ(velocity), maxSpeed);
					}

					// Direction and distance to move during this frame
					Vector3 deltaPosition;
					if (rvoController != null && rvoController.enabled) {
						// Use RVOController to get a processed delta position
						// such that collisions will be avoided if possible
						deltaPosition = rvoController.CalculateMovementDelta(position, deltaTime);

						// The RVOController does not know about gravity
						// so we copy it from the normal velocity calculation
						deltaPosition.y = velocity.y * deltaTime;
					} else {
						deltaPosition = velocity * deltaTime;
					}

					if (targetIsEndPoint) {
						// Rotate towards the direction that the agent was in
						// when the target point was seen for the first time
						// TODO: Some magic constants here, should probably compute them from other variables
						// or expose them as separate variables
						Vector3 trotdir = Vector3.Lerp(deltaPosition.normalized, currentTargetDirection, System.Math.Max(1 - distanceToWaypoint*2, 0));
						RotateTowards(trotdir);
					} else {
						// Rotate towards the direction we are moving in
						RotateTowards(deltaPosition);
					}

					if (controller != null && controller.enabled) {
						// Use CharacterController
						tr.position = position;
						controller.Move(deltaPosition);
						// Grab the position after the movement to be able to take physics into account
						position = tr.position;
					} else {
						// Use Transform
						float lastY = position.y;
						position += deltaPosition;
						// Position the character on the ground
						position = RaycastPosition(position, lastY);
					}

					// Clamp the position to the navmesh after movement is done
					var clampedPosition = fn.ClampToNavmesh(position);

					if (position != clampedPosition) {
						// The agent was outside the navmesh. Remove that component of the velocity
						// so that the velocity only goes along the direction of the wall, not into it
						var difference = clampedPosition - position;
						velocity -= difference * Vector3.Dot(difference, velocity) / difference.sqrMagnitude;

						// Make sure the RVO system knows that there was a collision here
						// Otherwise other agents may think this agent continued to move forwards
						// and avoidance quality may suffer
						if (rvoController != null && rvoController.enabled) {
							rvoController.SetCollisionNormal(difference);
						}
					}

					tr.position = clampedPosition;
				} else {
					if (rvoController != null && rvoController.enabled) {
						//Use RVOController
						rvoController.Move(Vector3.zero);
					}
				}
				if (currentPart is RichSpecial) {
					// The current path part is a special part, for example a link
					// Movement during this part of the path is handled by the TraverseSpecial coroutine
					if (!traversingSpecialPath) {
						StartCoroutine(TraverseSpecial(currentPart as RichSpecial));
					}
				}
			} else {
				if (rvoController != null && rvoController.enabled) {
					// Use RVOController
					rvoController.Move(Vector3.zero);
				} else
				if (controller != null && controller.enabled) {
				} else {
					tr.position = RaycastPosition(tr.position, tr.position.y);
				}
			}
		}

		/** Calculate an acceleration to move deltaPosition units and get there with approximately a velocity of targetVelocity.
		 *
		 * When preciseSlowdown is false, only the requirement that we should reach the target is used, not that
		 * our velocity should be zero when we reach the target.
		 */
		Vector2 CalculateAccelerationToReachPoint (Vector2 deltaPosition, Vector2 targetVelocity, Vector2 currentVelocity) {
			// If the target velocity is zero we can use a more fancy approach
			// and calculate a nicer path.
			// In particular, this is the case at the end of the path.
			if (targetVelocity == Vector2.zero) {
				// Run a binary search over the time to get
				// to the target point.
				float mn = 0.05f;
				float mx = 10;
				while (mx - mn > 0.01f) {
					var time = (mx + mn) * 0.5f;

					// Given that we want to move \a deltaPosition units from out current position, that our current velocity is given and
					// that when we reach the target we want our velocity to be zero. Also assume that our acceleration will
					// vary linearly during the slowdown. Then we can calculate what our acceleration should be during this frame.

					//{ t = slowdownTime
					//{ deltaPosition = vt + at^2/2 + qt^3/6
					//{ 0 = v + at + qt^2/2
					//{ solve for a
					var a2 = (6*deltaPosition - 4*time*currentVelocity)/(time*time);
					var q2 = 6*(time*currentVelocity - 2*deltaPosition)/(time*time*time);

					// Make sure the acceleration is not greater than our maximum allowed acceleration.
					// If it is we increase the time we want to use to get to the target
					// and if it is not, we decrease the time to get there faster.
					if (a2.sqrMagnitude > acceleration*acceleration || (a2 + q2*time).sqrMagnitude > acceleration*acceleration) {
						mn = time;
					} else {
						mx = time;
					}
				}

				var a = (6*deltaPosition - 4*mx*currentVelocity)/(mx*mx);
				return a;
			} else {
				var distance = deltaPosition.magnitude;

				// How much to strive for making sure we reach the target point with the target velocity
				const float TargetVelocityWeight = 0.5f;

				// Limit to how much to care about the target velocity. In seconds.
				// This prevents the character from moving away from the path too much when the target point is far away
				const float TargetVelocityWeightLimit = 2;
				float currentSpeed = currentVelocity.magnitude;
				float targetSpeed;
				var normalizedTargetVelocity = VectorMath.Normalize(targetVelocity, out targetSpeed);

				var targetPoint = deltaPosition - normalizedTargetVelocity * System.Math.Min(TargetVelocityWeight * distance * targetSpeed / (currentSpeed + targetSpeed), maxSpeed*TargetVelocityWeightLimit);
				return targetPoint.normalized * acceleration;
			}
		}

		Vector3 CalculateWallForce (Vector3 position, Vector3 directionToTarget) {
			if (wallForce > 0 && wallDist > 0) {
				float wLeft = 0;
				float wRight = 0;

				for (int i = 0; i < wallBuffer.Count; i += 2) {
					Vector3 closest = VectorMath.ClosestPointOnSegment(wallBuffer[i], wallBuffer[i+1], tr.position);
					float dist = (closest-position).sqrMagnitude;

					if (dist > wallDist*wallDist) continue;

					Vector3 tang = (wallBuffer[i+1]-wallBuffer[i]).normalized;

					// Using the fact that all walls are laid out clockwise (seeing from inside)
					// Then left and right (ish) can be figured out like this
					float dot = Vector3.Dot(directionToTarget, tang) * (1 - System.Math.Max(0, (2*(dist / (wallDist*wallDist))-1)));
					if (dot > 0) wRight = System.Math.Max(wRight, dot);
					else wLeft = System.Math.Max(wLeft, -dot);
				}

				Vector3 norm = Vector3.Cross(Vector3.up, directionToTarget);
				return norm*(wRight-wLeft);
			}
			return Vector3.zero;
		}

		Vector3 RaycastPosition (Vector3 position, float lasty) {
			if (raycastingForGroundPlacement) {
				RaycastHit hit;
				float up = Mathf.Max(centerOffset, lasty-position.y+centerOffset);

				if (Physics.Raycast(position+Vector3.up*up, Vector3.down, out hit, up, groundMask)) {
					//Debug.DrawRay (tr.position+Vector3.up*centerOffset,Vector3.down*centerOffset, Color.red);
					if (hit.distance < up) {
						//grounded
						position = hit.point;//.up * -(hit.distance-centerOffset);
						velocity.y = 0;
					}
				}
			}
			return position;
		}

		/** Rotates along the Y-axis the transform towards \a trotdir.
		 * \returns True when the characters is approximately facing the desired direction.
		 */
		bool RotateTowards (Vector3 trotdir) {
			trotdir.y = 0;
			if (trotdir != Vector3.zero) {
				Quaternion rot = tr.rotation;

				Vector3 trot = Quaternion.LookRotation(trotdir).eulerAngles;
				Vector3 eul = rot.eulerAngles;
				eul.y = Mathf.MoveTowardsAngle(eul.y, trot.y, rotationSpeed*deltaTime);
				tr.rotation = Quaternion.Euler(eul);
				// Magic number, should expose as variable
				return Mathf.Abs(eul.y-trot.y) < 5f;
			}
			return false;
		}

		public static readonly Color GizmoColorRaycast = new Color(118.0f/255, 206.0f/255, 112.0f/255);
		public static readonly Color GizmoColorPath = new Color(8.0f/255, 78.0f/255, 194.0f/255);

		public void OnDrawGizmos () {
			if (drawGizmos) {
				if (raycastingForGroundPlacement) {
					Gizmos.color = GizmoColorRaycast;
					Gizmos.DrawLine(transform.position, transform.position+Vector3.up*centerOffset);
					Gizmos.DrawLine(transform.position + Vector3.left*0.1f, transform.position + Vector3.right*0.1f);
					Gizmos.DrawLine(transform.position + Vector3.back*0.1f, transform.position + Vector3.forward*0.1f);
				}

				if (tr != null && nextCorners != null) {
					Gizmos.color = GizmoColorPath;
					Vector3 p = tr.position;
					for (int i = 0; i < nextCorners.Count; p = nextCorners[i], i++) {
						Gizmos.DrawLine(p, nextCorners[i]);
					}
				}
			}
		}

		IEnumerator TraverseSpecial (RichSpecial rs) {
			traversingSpecialPath = true;
			velocity = Vector3.zero;

			var al = rs.nodeLink as AnimationLink;
			if (al == null) {
				Debug.LogError("Unhandled RichSpecial");
				yield break;
			}

			//Rotate character to face the correct direction
			while (!RotateTowards(rs.first.forward)) yield return null;

			//Reposition
			tr.parent.position = tr.position;

			tr.parent.rotation = tr.rotation;
			tr.localPosition = Vector3.zero;
			tr.localRotation = Quaternion.identity;

			//Set up animation speeds
			if (rs.reverse && al.reverseAnim) {
				anim[al.clip].speed = -al.animSpeed;
				anim[al.clip].normalizedTime = 1;
				anim.Play(al.clip);
				anim.Sample();
			} else {
				anim[al.clip].speed = al.animSpeed;
				anim.Rewind(al.clip);
				anim.Play(al.clip);
			}

			//Fix required for animations in reverse direction
			tr.parent.position -= tr.position-tr.parent.position;

			//Wait for the animation to finish
			yield return new WaitForSeconds(Mathf.Abs(anim[al.clip].length/al.animSpeed));

			traversingSpecialPath = false;
			NextPart();

			//If a path completed during the time we traversed the special connection, we need to recalculate it
			if (delayUpdatePath) {
				delayUpdatePath = false;
				UpdatePath();
			}
		}
	}
}
