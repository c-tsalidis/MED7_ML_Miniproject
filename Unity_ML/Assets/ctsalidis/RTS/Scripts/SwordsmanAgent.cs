using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace ctsalidis.RTS.Scripts {
    public enum Team {
        Blue = 0,
        Purple = 1
    }

    public enum HitDamage {
        Low,
        Critical
    }

    public class SwordsmanAgent : Agent {
        // ref --> Created new ML agent environment based on --> https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Learning-Environment-Create-New.md
        private Rigidbody rBody;
        private MeshRenderer _meshRenderer;
        public float forceMultiplier = 10;
        public float minDistTarget = 1.5f;
        public Team Team;
        [SerializeField] private float health = 5;
        private Sword _sword;
        private Vector3 originalPos;

        /// <summary>
        /// Idle, Attack, Defense
        /// </summary>
        [SerializeField] private Color[] stateColors;

        public enum AgentState {
            IdleOrMoving,
            Attacking,
            Defending
        }

        public AgentState _state = AgentState.IdleOrMoving;
        private float originalHealth;

        void Start() {
            rBody = GetComponent<Rigidbody>();
            _meshRenderer = GetComponent<MeshRenderer>();
            Team = (Team) GetComponent<BehaviorParameters>().TeamId;
            _sword = new Sword(this);
            originalPos = transform.localPosition;
            originalHealth = health;
        }

        public override void OnEpisodeBegin() {
            // If the Agent fell, zero its momentum
            if (this.transform.localPosition.y < 0) {
                this.rBody.angularVelocity = Vector3.zero;
                this.rBody.velocity = Vector3.zero;
                this.transform.localPosition = new Vector3(0, 0.5f, 0);
                this.transform.localRotation = Quaternion.identity;
                health = originalHealth;
            }

            // Move the target to a new spot
            // reset the position of the agent who died
        }

        /*
        public override void CollectObservations(VectorSensor sensor) {
            // Target and Agent positions
            sensor.AddObservation(Target.localPosition);
            sensor.AddObservation(this.transform.localPosition);

            // Agent velocity
            sensor.AddObservation(rBody.velocity.x);
            sensor.AddObservation(rBody.velocity.z);
        }
        */

        public override void OnActionReceived(ActionBuffers actionBuffers) {
            // continuous Actions, size = 2 --> for moving the agent
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = actionBuffers.ContinuousActions[0];
            controlSignal.z = actionBuffers.ContinuousActions[1];
            // rBody.AddForce(controlSignal * forceMultiplier);
            Vector3 rotateDir = Vector3.zero;
            switch (actionBuffers.DiscreteActions[1])
            {
                case 1:
                    rotateDir = transform.up * -1f;
                    break;
                case 2:
                    rotateDir = transform.up * 1f;
                    break;
            }
            transform.localPosition += controlSignal * forceMultiplier;
            transform.Rotate(rotateDir, Time.deltaTime * 100f);
            
            
            // discrete actions --> for choosing state, size = 1 --> idle, attack, defense
            var stateSignal = actionBuffers.DiscreteActions[0];
            // _meshRenderer.material.color = stateColors[stateSignal];
            // Rewards
            // float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

            // if(stateSignal == 0) { print(0); }
            if (stateSignal == 0) {
                _meshRenderer.material.color = stateColors[0];
            }
            if (stateSignal == 1) {
                AttackIfPossible();
            }

            if (stateSignal == 2) {
                EnterDefensiveMode();
            }
            
            /*
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, minDistTarget )) {
                var other = hit.collider.gameObject;
                if (other.CompareTag("swordsmanAgent")) {
                    var agent = other.GetComponent<SwordsmanAgent>();
                    if (agent.Team != this.Team) {
                        if(stateSignal == 0) { print(0); }
                        // idle
                        // if(stateSignal == 1) { AddReward(); }
                        // attack
                        if(stateSignal == )
                    }
                }
            }
            */
            /*
            // Reached target
            if (distanceToTarget < minDistTarget) {
                SetReward(1.0f);
                EndEpisode();
            }
            */

            // Fell off platform
            if (this.transform.localPosition.y < 0) {
                EndEpisode();
            }
        }

        private void EnterDefensiveMode() {
            _state = AgentState.Defending;
            _meshRenderer.material.color = stateColors[2];
        }

        private void AttackIfPossible() {
            if (_state != AgentState.Attacking) {
                StartCoroutine(Attack());
            }
        }

        private IEnumerator Attack() {
            _state = AgentState.Attacking;
            _meshRenderer.material.color = stateColors[1];
            _sword.Attack();
            yield return new WaitForSeconds(0.5f);
            _state = AgentState.IdleOrMoving;
        }

        /*
         // Get the action index for movement
        int movement = actionBuffers.DiscreteActions[0];
        // Get the action index for jumping
        int jump = actionBuffers.DiscreteActions[1];

        // Look up the index in the movement action list:
        if (movement == 1) { directionX = -1; }
        if (movement == 2) { directionX = 1; }
        if (movement == 3) { directionZ = -1; }
        if (movement == 4) { directionZ = 1; }
        // Look up the index in the jump action list:
        if (jump == 1 && IsGrounded()) { directionY = 1; }

        // Apply the action results to move the Agent
        gameObject.GetComponent<Rigidbody>().AddForce(
            new Vector3(
                directionX * 40f, directionY * 300f, directionZ * 40f));
         */

        public override void Heuristic(in ActionBuffers actionsOut) {
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = Input.GetAxis("Horizontal");
            continuousActionsOut[1] = Input.GetAxis("Vertical");

            var discreteActions = actionsOut.DiscreteActions;
            if (Input.GetKey(KeyCode.Space)) discreteActions[0] = (int) AgentState.Attacking;
            else if (Input.GetKey(KeyCode.E)) discreteActions[0] = (int) AgentState.Defending;
            else discreteActions[0] = (int) AgentState.IdleOrMoving;

            if (Input.GetKey(KeyCode.R)) discreteActions[1] = 1;
            else if (Input.GetKey(KeyCode.T)) discreteActions[1] = 2;
        }

        public void OtherAgentHit(HitDamage damage) {
            if (damage == HitDamage.Low) AddReward(0.1f);
            else if (damage == HitDamage.Critical) AddReward(1.0f);
        }

        /*
        public void RewardForDefendingItself() {
        // TODO --> Add reward for defending themeselves
            AddReward(1.0f);
        }
        */

        public void TakeHitFromOtherAgent(HitDamage damage) {
            if (damage == HitDamage.Low) AddReward(-0.1f);
            else if (damage == HitDamage.Critical) {
                AddReward(-1.0f);
                health--;
                if (health <= 0) {
                    EndEpisode();
                }
            }
        }
    }

    public class Sword {
        private SwordsmanAgent _agent;

        public Sword(SwordsmanAgent agent) {
            this._agent = agent;
        }

        public void Attack() {
            // check if the agent hit another agent from the other team
            RaycastHit hit;
            if (Physics.Raycast(_agent.transform.localPosition, _agent.transform.forward, out hit, _agent.minDistTarget)) {
                var other = hit.collider.gameObject;
                if (other.CompareTag("swordsmanAgent")) {
                    var otherAgent = other.GetComponent<SwordsmanAgent>();
                    if (otherAgent.Team != _agent.Team) {
                        if (otherAgent._state != SwordsmanAgent.AgentState.Defending) {
                            _agent.OtherAgentHit(HitDamage.Critical);
                            otherAgent.TakeHitFromOtherAgent(HitDamage.Critical);
                        }

                        /*
                        else {
                            agent.RewardForHittingOtherAgent(HitDamage.Low);
                            // otherAgent.RewardForDefendingItself();
                        }
                        */
                    }
                }
            }
            else {
                // TODO --> if there is nothing, then penalize --> don't want them to think attacking nothing doesn't have consequences
            }
        }
    }
}