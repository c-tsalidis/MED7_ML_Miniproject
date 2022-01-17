using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Snake_game.Scripts {
    public class SnakeHead : Agent {
        [SerializeField] private Grid grid;
        [SerializeField] private Food _food;
        [SerializeField] private float moveSpeed = 1.0f;
        public int cubeIndex = 0, previousCubeIndex;

        public int score = 0;
        public int direction = 0;

        [SerializeField] private Text scoreText;
        public bool isTraining = true;

        private BehaviorParameters behaviorParameters;

        private float distanceToFood, previousDistanceToFood;

        private void Awake() {
            behaviorParameters = GetComponent<BehaviorParameters>();
        }

        public override void OnEpisodeBegin() {
            scoreText.text = score.ToString();
            grid.SetFoodPosition();
        }

        // public override void OnActionReceived(ActionBuffers actions) => grid.SetSnakeHeadPosition(actions.DiscreteActions[0]); // set the direction of the snake
        
        // set the direction of the snake
        public override void OnActionReceived(ActionBuffers actions) {
            direction = actions.DiscreteActions[0];
            if(isTraining) grid.SetSnakeHeadPosition(actions.DiscreteActions[0]); // set the direction of the snake
        }

        public override void CollectObservations(VectorSensor sensor) {
            // add observation to x and z pos of snake
            var headPos = transform.localPosition;
            sensor.AddObservation(headPos.x);
            sensor.AddObservation(headPos.z);
            
            // add observation to x and z pos of food
            var foodPos = _food.transform.localPosition;
            sensor.AddObservation(foodPos.x);
            sensor.AddObservation(foodPos.z);
            
            
            
            // add observation to each body part
            foreach (var bp in grid._bodyparts) {
                var pos = bp.transform.localPosition;
                sensor.AddObservation(pos.x);
                sensor.AddObservation(pos.z);
            }

            // AddReward( -1f / MaxStep); // punish to try to make the snake approach move towards the food faster
            AddReward( -0.01f); // punish to try to make the snake approach move towards the food faster
            // TODO --> Maybe consider adding more reward if the distance to the food is smaller each frame?
            distanceToFood = Vector3.Distance(headPos, foodPos);
            if (distanceToFood < previousDistanceToFood) AddReward(0.01f);
            previousDistanceToFood = distanceToFood;
        }

        public override void Heuristic(in ActionBuffers actionsOut) {
            ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
            if (Input.GetKey(KeyCode.RightArrow)) discreteActions[0] = 0; // right
            if (Input.GetKey(KeyCode.LeftArrow)) discreteActions[0] = 1; // left
            if (Input.GetKey(KeyCode.UpArrow)) discreteActions[0] = 2; // up
            if (Input.GetKey(KeyCode.DownArrow)) discreteActions[0] = 3; // down
        }

        public void Score() {
            score++;
            AddReward(1f);
            // update the vector observation size based on the grid body parts count
            behaviorParameters.BrainParameters.VectorObservationSize = 4 + grid._bodyparts.Count; // 4 --> One for each x and z of headPos and foodPos
            EndEpisode();
        }

        private void Punish() {
            score = 0;
            AddReward(-1);
            EndEpisode();
        }

        public void WonGame() {
            AddReward(1);
            grid.ClearBodyParts();
            score = 0;
            EndEpisode();
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Wall")) { // wall is actually also snake body part
                grid.ClearBodyParts();
                Punish();
            }
        }
    }
}