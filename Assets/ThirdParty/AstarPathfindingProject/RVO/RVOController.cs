using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

namespace Pathfinding.RVO {
	/** RVO Character Controller.
	 * Similar to Unity's CharacterController. It handles movement calculations and takes other agents into account.
	 * It does not handle movement itself, but allows the calling script to get the calculated velocity and
	 * use that to move the object using a method it sees fit (for example using a CharacterController, using
	 * transform.Translate or using a rigidbody).
	 *
	 * \code
	 * public void Update () {
	 *     // Just some point far away
	 *     var targetPoint = transform.position + transform.forward * 100;
	 *
	 *     // Set the desired point to move towards using a desired speed of 10 and a max speed of 12
	 *     controller.SetTarget(targetPoint, 10, 12);
	 *
	 *     // Calculate how much to move during this frame
	 *     // This information is based on movement commands from earlier frames
	 *     // as local avoidance is calculated globally at regular intervals by the RVOSimulator component
	 *     var delta = controller.CalculateMovementDelta(transform.position, Time.deltaTime);
	 *     transform.position = transform.position + delta;
	 * }
	 * \endcode
	 *
	 * For documentation of many of the variables of this class: refer to the Pathfinding.RVO.IAgent interface.
	 *
	 * \note Requires a single RVOSimulator component in the scene
	 *
	 * \see Pathfinding.RVO.IAgent
	 * \see RVOSimulator
	 * \see \ref local-avoidance
	 *
	 * \astarpro
	 */
	[AddComponentMenu("Pathfinding/Local Avoidance/RVO Controller")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_r_v_o_1_1_r_v_o_controller.php")]
	public class RVOController : MonoBehaviour {
		/** Determines if the XY (2D) or XZ (3D) plane is used for movement */
		public MovementMode movementMode = MovementMode.XZ;

		/** Radius of the agent in world units */
		[Tooltip("Radius of the agent")]
		public float radius = 5;

		/** Height of the agent in world units */
		[Tooltip("Height of the agent. In world units")]
		public float height = 1;

		/** A locked unit cannot move. Other units will still avoid it but avoidance quality is not the best. */
		[Tooltip("A locked unit cannot move. Other units will still avoid it. But avoidance quality is not the best")]
		public bool locked;

		/** Automatically set #locked to true when desired velocity is approximately zero.
		 * This prevents other units from pushing them away when they are supposed to e.g block a choke point.
		 */
		[Tooltip("Automatically set #locked to true when desired velocity is approximately zero")]
		public bool lockWhenNotMoving = true;

		/** How far into the future to look for collisions with other agents */
		[Tooltip("How far in the time to look for collisions with other agents")]
		public float agentTimeHorizon = 2;

		/** How far into the future to look for collisions with obstacles */
		public float obstacleTimeHorizon = 2;

		/** Maximum distance to other agents to take them into account for collisions.
		 * Decreasing this value can lead to better performance, increasing it can lead to better quality of the simulation.
		 */
		[Tooltip("Maximum distance to other agents to take them into account for collisions.\n" +
			 "Decreasing this value can lead to better performance, increasing it can lead to better quality of the simulation")]
		public float neighbourDist = 10;

		/** Max number of other agents to take into account.
		 * A smaller value can reduce CPU load, a higher value can lead to better local avoidance quality.
		 */
		[Tooltip("Max number of other agents to take into account.\n" +
			 "A smaller value can reduce CPU load, a higher value can lead to better local avoidance quality.")]
		public int maxNeighbours = 10;

		/** Specifies the avoidance layer for this agent.
		 * The #collidesWith mask on other agents will determine if they will avoid this agent.
		 */
		public RVOLayer layer = RVOLayer.DefaultAgent;

		/** Layer mask specifying which layers this agent will avoid.
		 * You can set it as CollidesWith = RVOLayer.DefaultAgent | RVOLayer.Layer3 | RVOLayer.Layer6 ...
		 *
		 * This can be very useful in games which have multiple teams of some sort.
		 * For example you usually want the agents in one team avoid each other, but you do not want
		 * them to avoid the enemies.
		 *
		 * \see http://en.wikipedia.org/wiki/Mask_(computing)
		 */
		[Pathfinding.AstarEnumFlag]
		public RVOLayer collidesWith = (RVOLayer)(-1);

		/** An extra force to avoid walls.
		 * This can be good way to reduce "wall hugging" behaviour.
		 *
		 * \todo This feature is currently disabled
		 */
		[HideInInspector]
		public float wallAvoidForce = 1;

		/** How much the wallAvoidForce decreases with distance.
		 * The strenght of avoidance is:
		 * \code str = 1/dist*wallAvoidFalloff \endcode
		 *
		 * \see wallAvoidForce
		 *
		 * \todo This feature is currently disabled
		 */
		[HideInInspector]
		public float wallAvoidFalloff = 1;

		/** \copydoc Pathfinding::RVO::IAgent::Priority */
		[Tooltip("How strongly other agents will avoid this agent")]
		[UnityEngine.Range(0, 1)]
		public float priority = 0.5f;

		/** Center of the agent relative to the pivot point of this game object */
		[Tooltip("Center of the agent relative to the pivot point of this game object")]
		public float center;

		/** Reference to the internal agent */
		public IAgent rvoAgent { get; private set; }

		/** Reference to the rvo simulator */
		public Simulator simulator { get; private set; }

		/** Cached tranform component */
		private Transform tr;

		/** Enables drawing debug information in the scene view */
		public bool debug;

		/** To avoid having to use FindObjectOfType every time */
		static RVOSimulator cachedSimulator;

		/** Current position of the agent */
		public Vector3 position {
			get {
				return To3D(rvoAgent.Position, rvoAgent.ElevationCoordinate);
			}
		}

		/** Current calculated velocity of the agent.
		 * This is not necessarily the velocity the agent is actually moving with
		 * but it is the velocity that the RVO system has calculated is best for
		 * avoiding obstacles and reaching the target.
		 *
		 * \see CalculateMovementDelta
		 */
		public Vector3 velocity {
			get {
				if (Time.deltaTime > 0.00001f) {
					return CalculateMovementDelta(Time.deltaTime) / Time.deltaTime;
				} else {
					return Vector3.zero;
				}
			}
		}

		/** Direction and distance to move in a single frame to avoid obstacles.
		 * \param deltaTime How far to move [seconds].
		 *      Usually set to Time.deltaTime.
		 */
		public Vector3 CalculateMovementDelta (float deltaTime) {
			return To3D(Vector2.ClampMagnitude(rvoAgent.CalculatedTargetPoint - To2D(tr.position), rvoAgent.CalculatedSpeed * deltaTime), 0);
		}

		/** Direction and distance to move in a single frame to avoid obstacles.
		 * \param position Position of the agent.
		 * \param deltaTime How far to move [seconds].
		 *      Usually set to Time.deltaTime.
		 */
		public Vector3 CalculateMovementDelta (Vector3 position, float deltaTime) {
			return To3D(Vector2.ClampMagnitude(rvoAgent.CalculatedTargetPoint - To2D(position), rvoAgent.CalculatedSpeed * deltaTime), 0);
		}

		/** \copydoc Pathfinding::RVO::IAgent::SetCollisionNormal */
		public void SetCollisionNormal (Vector3 normal) {
			rvoAgent.SetCollisionNormal(To2D(normal));
		}

		/** \copydoc Pathfinding::RVO::IAgent::ForceSetVelocity */
		public void ForceSetVelocity (Vector3 velocity) {
			rvoAgent.ForceSetVelocity(To2D(velocity));
		}

		Vector2 To2D (Vector3 p) {
			float dummy;

			return To2D(p, out dummy);
		}

		Vector2 To2D (Vector3 p, out float elevation) {
			if (movementMode == MovementMode.XY) {
				elevation = p.z;
				return new Vector2(p.x, p.y);
			} else {
				elevation = p.y;
				return new Vector2(p.x, p.z);
			}
		}

		Vector3 To3D (Vector2 p, float elevationCoordinate) {
			if (movementMode == MovementMode.XY) {
				return new Vector3(p.x, p.y, elevationCoordinate);
			} else {
				return new Vector3(p.x, elevationCoordinate, p.y);
			}
		}

		public void OnDisable () {
			if (simulator == null) return;

			// Remove the agent from the simulation but keep the reference
			// this component might get enabled and then we can simply
			// add it to the simulation again
			simulator.RemoveAgent(rvoAgent);
		}

		public void Awake () {
			tr = transform;

			// Find the RVOSimulator in this scene
			if (cachedSimulator == null) {
				cachedSimulator = FindObjectOfType<RVOSimulator>();
			}

			if (cachedSimulator == null) {
				Debug.LogError("No RVOSimulator component found in the scene. Please add one.");
			} else {
				simulator = cachedSimulator.GetSimulator();
			}
		}

		public void OnEnable () {
			if (simulator == null) return;

			// We might have an rvoAgent
			// which was disabled previously
			// if so, we can simply add it to the simulation again
			if (rvoAgent != null) {
				simulator.AddAgent(rvoAgent);
			} else {
				float elevation;
				var pos = To2D(transform.position, out elevation);
				rvoAgent = simulator.AddAgent(pos, elevation);
				rvoAgent.PreCalculationCallback = UpdateAgentProperties;
			}

			UpdateAgentProperties();
			// TODO: Add teleport call
		}

		protected void UpdateAgentProperties () {
			rvoAgent.Radius = Mathf.Max(0.001f, radius);
			rvoAgent.AgentTimeHorizon = agentTimeHorizon;
			rvoAgent.ObstacleTimeHorizon = obstacleTimeHorizon;
			rvoAgent.Locked = locked;
			rvoAgent.MaxNeighbours = maxNeighbours;
			rvoAgent.DebugDraw = debug;
			rvoAgent.NeighbourDist = neighbourDist;
			rvoAgent.Layer = layer;
			rvoAgent.CollidesWith = collidesWith;
			rvoAgent.MovementMode = movementMode;
			rvoAgent.Priority = priority;

			float elevation;
			rvoAgent.Position = To2D(transform.position, out elevation);

			if (movementMode == MovementMode.XZ) {
				rvoAgent.Height = height;
				rvoAgent.ElevationCoordinate = elevation + center - 0.5f * height;
			} else {
				rvoAgent.Height = 1;
				rvoAgent.ElevationCoordinate = 0;
			}
		}

		/** Set the target point for the agent to move towards.
		 * Similar to the #Move method but this is more flexible.
		 * It is also better to use near the end of the path as when using the Move
		 * method the agent does not know where to stop, so it may overshoot the target.
		 * When using this method the agent will not overshoot the target.
		 *
		 * The target point is assumed to stay the same until something else is requested (as opposed to being reset every frame).
		 *
		 * \param pos Point in world space to move towards.
		 * \param speed Desired speed in world units per second.
		 * \param maxSpeed Maximum speed in world units per second.
		 *		The agent will use this speed if it is necessary to avoid collisions with other agents.
		 *
		 * \see Move
		 */
		public void SetTarget (Vector3 pos, float speed, float maxSpeed) {
			rvoAgent.SetTarget(To2D(pos), speed, maxSpeed);

			if (lockWhenNotMoving) {
				locked = speed < 0.001f;
			}
		}

		/** Set the desired velocity for the agent.
		 * Note that this is a velocity (units/second), not a movement delta (units/frame).
		 *
		 * This is assumed to stay the same until something else is requested (as opposed to being reset every frame).
		 *
		 * \note In most cases the SetTarget method is better to use
		 * \see SetTarget
		 */
		public void Move (Vector3 vel) {
			var velocity2D = To2D(vel);
			var speed = velocity2D.magnitude;

			rvoAgent.SetTarget(To2D(tr.position) + velocity2D, speed, speed);

			if (lockWhenNotMoving) {
				locked = speed < 0.001f;
			}
		}

		/** Teleport the agent to a new position.
		 * The agent will be moved instantly and not show ugly interpolation artifacts during a split second.
		 * Manually changing the position of the transform will in most cases be picked up as a teleport automatically
		 * by the script.
		 *
		 * During the simulation frame the agent was moved manually, local avoidance cannot fully be applied to the
		 * agent, so try to avoid using it too much or local avoidance quality will degrade.
		 */
		public void Teleport (Vector3 pos) {
			tr.position = pos;
			// TODO: Teleport call!
		}

		public void Update () {
#if FALSE
			// TODO: Not functional
			if (wallAvoidFalloff > 0 && wallAvoidForce > 0) {
				List<ObstacleVertex> obst = rvoAgent.NeighbourObstacles;

				if (obst != null) for (int i = 0; i < obst.Count; i++) {
						Vector3 a = obst[i].position;
						Vector3 b = obst[i].next.position;

						Vector3 closest = position - VectorMath.ClosestPointOnSegment(a, b, position);

						if (closest == a || closest == b) continue;

						float dist = closest.sqrMagnitude;
						closest /= dist*wallAvoidFalloff;
						force += closest;
					}
			}
#endif
		}

		private static readonly Color GizmoColor = new Color(240/255f, 213/255f, 30/255f);

		static void DrawCircle (Vector3 p, float radius, float a0, float a1) {
			while (a0 > a1) a0 -= 2*Mathf.PI;

			Vector3 prev = new Vector3(Mathf.Cos(a0)*radius, 0, Mathf.Sin(a0)*radius);
			const float steps = 40.0f;
			for (int i = 0; i <= steps; i++) {
				Vector3 c = new Vector3(Mathf.Cos(Mathf.Lerp(a0, a1, i/steps))*radius, 0, Mathf.Sin(Mathf.Lerp(a0, a1, i/steps))*radius);
				Gizmos.DrawLine(p+prev, p+c);
				prev = c;
			}
		}

		static void DrawCylinder (Vector3 p, Vector3 up, float height, float radius) {
			var tangent = Vector3.Cross(up, Vector3.one).normalized;

			Gizmos.matrix = Matrix4x4.TRS(p, Quaternion.LookRotation(tangent, up), new Vector3(radius, height, radius));
			DrawCircle(new Vector2(0, 0), 1, 0, 2 * Mathf.PI);

			if (height > 0) {
				DrawCircle(new Vector2(0, 1), 1, 0, 2 * Mathf.PI);
				Gizmos.DrawLine(new Vector3(1, 0, 0), new Vector3(1, 1, 0));
				Gizmos.DrawLine(new Vector3(-1, 0, 0), new Vector3(-1, 1, 0));
				Gizmos.DrawLine(new Vector3(0, 0, 1), new Vector3(0, 1, 1));
				Gizmos.DrawLine(new Vector3(0, 0, -1), new Vector3(0, 1, -1));
			}
		}

		void OnDrawGizmos () {
			if (locked) {
				Gizmos.color = GizmoColor * 0.5f;
			} else {
				Gizmos.color = GizmoColor;
			}

			if (movementMode == MovementMode.XY) {
				DrawCylinder(transform.position, Vector3.forward, 0, radius);
			} else {
				DrawCylinder(transform.position + To3D(Vector2.zero, center - height * 0.5f), To3D(Vector2.zero, 1), height, radius);
			}
		}

		void OnDrawGizmosSelected () {
#if UNITY_EDITOR
			// Only show the neighbourDist circle when there is only a single agent selected to reduce clutter
			if (UnityEditor.Selection.transforms.Length == 1) {
				var col = GizmoColor*0.9f;
				col.a = 0.5f;
				Gizmos.color = col;

				var up = To3D(Vector2.zero, 1);
				var tangent = Vector3.Cross(up, Vector3.one).normalized;
				Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.LookRotation(tangent, up), new Vector3(1, 1, 1));
				DrawCircle(Vector3.zero, neighbourDist, 0, Mathf.PI * 2);
			}
#endif
		}
	}
}
