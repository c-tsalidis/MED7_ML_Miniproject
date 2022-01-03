using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
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
    
        public override void OnEpisodeBegin() {
            scoreText.text = score.ToString();
            grid.SetFoodPosition();
        }

        // public override void OnActionReceived(ActionBuffers actions) => grid.SetSnakeHeadPosition(actions.DiscreteActions[0]); // set the direction of the snake
        
        // set the direction of the snake
        public override void OnActionReceived(ActionBuffers actions) {
            direction = actions.DiscreteActions[0];
            if(grid.isTraining) grid.SetSnakeHeadPosition(actions.DiscreteActions[0]); // set the direction of the snake
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
            AddReward(grid._bodyparts.Count); // snake body parts
            EndEpisode();
        }

        public void Punish() {
            // if the snake happens to find itself in the situation where the food and body part are in the same spot,
            // then it should avoid its body more than it should go for the food --> Therefore:
            // we need to set the punish slightly higher than the reward
            score = 0;
            var count = grid._bodyparts.Count; // snake body parts
            AddReward(-count-1);
            
            // AddReward(-1f / (count+1));
            
            // if(count > 0) AddReward(-1f / count);
            // else AddReward(1f);
            EndEpisode();
        }

        private void PunishSeverely() {
            AddReward(-2*grid._bodyparts.Count);
            EndEpisode();
        }

        public void WonGame() {
            Score();
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Wall")) {
                foreach (var b in grid._bodyparts) { Destroy(b.gameObject); }
                grid._bodyparts.Clear();
                if (other.transform == grid.cubes[0].transform) PunishSeverely(); // if snake head hits its first body, punish more severely
                else Punish();
            }
        }

        
    }
}