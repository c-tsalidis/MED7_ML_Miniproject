using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Snake_game.Scripts {
    public class SnakeHead : Agent {
        [SerializeField] private Grid grid;
        [SerializeField] private Food _food;
        [SerializeField] private float moveSpeed = 1.0f;
        public int cubeIndex = 0, previousCubeIndex;

        public int score = 0;
        public int direction = 0;
    
        public override void OnEpisodeBegin() => grid.SetFoodPosition();

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
            for (int i = 0; i < grid.cubes.Length; i++) {
                if (grid._bodyparts.Count-1 >= i) {
                    var pos = grid._bodyparts[i].transform.localPosition;
                    sensor.AddObservation(pos.x);
                    sensor.AddObservation(pos.z);
                }
                else {
                    sensor.AddObservation(0f); // x
                    sensor.AddObservation(0f); // z
                }
            }

            /*
            
            foreach (var bp in grid._bodyparts) {
                sensor.AddObservation(bp.transform.localPosition);
            }

            foreach (var cube in grid.cubes) {
                sensor.AddObservation(cube.transform.localPosition);
            }
            */
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
            SetReward(1);
            EndEpisode();
        }

        public void Punish() {
            score = 0;
            SetReward(-1);
            EndEpisode();
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Wall")) {
                foreach (var b in grid._bodyparts) { Destroy(b.gameObject); }
                grid._bodyparts.Clear();
                Punish();
            }
        }
    }
}