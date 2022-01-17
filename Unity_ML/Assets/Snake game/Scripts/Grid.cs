using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Snake_game.Scripts {
    public class Grid : MonoBehaviour {
        public int gridEdgeSize = 5;
        [SerializeField] private SnakeHead snakeHead;
        [SerializeField] private Food food;
        [SerializeField] private GameObject bodyPartPrefab;

        [SerializeField] private float timeBetweenFrames = 0.25f;
        private bool timeFinished = false;

        [SerializeField] private Material snakeMaterial;
        [SerializeField] private Material gridCubeMaterial;

        public int xIndex, zIndex, cubeIndex; // indexes corresponding to the position in the grid
        public GridCube[] cubes;
        public List<GridCube> _bodyparts = new List<GridCube>();
        public GridCube recentlyInstantiatedCube;
        public Vector3 originalPosition;

        private void Start() {
            for (int i = 0; i < cubes.Length; i++) {
                cubes[i] = transform.GetChild(i).GetComponent<GridCube>();
                cubes[i].cubeIndex = i;
                if (i == 0) originalPosition = cubes[i].transform.position;
            }

            // SetFoodPosition();
            // if (timeBetweenFrames > 0 && !isTraining) {
            if (!snakeHead.isTraining) {
                StartCoroutine(WaitBetweenFrames());
            }
        }

        private IEnumerator WaitBetweenFrames() {
            while (true) {
                timeFinished = false;
                yield return new WaitForSeconds(timeBetweenFrames);
                timeFinished = true;
                SetSnakeHeadPosition(snakeHead.direction);
            }
        }

        // private void Update() {
        //     foreach (var c in cubes) {
        //         Debug.DrawRay(c.transform.localPosition, c.transform.up * 10, Color.green);
        //     }
        // }

        public void SetFoodPosition() {
            bool foundFreeSpotForFood = false;
            while (!foundFreeSpotForFood) {
                var randomCubeIndex = Random.Range(0, cubes.Length);
                var cube = cubes[randomCubeIndex];

                int counter = 0;
                foreach (var b in _bodyparts.Where(b => cube.cubeIndex == b.cubeIndex)) {
                    counter++;
                    break;
                }

                if (counter == 0) {
                    // if there is no body part in the new random cube index, then spawn the food
                    food.transform.localPosition = cube.transform.localPosition;
                    food.gridIndex = randomCubeIndex;
                    foundFreeSpotForFood = true;
                }
            }
        }

        public void SetSnakeHeadPosition(int direction) {
            // if (!timeFinished && timeBetweenFrames > 0) return;
            if (!snakeHead.isTraining)
                if (!timeFinished && timeBetweenFrames > 0)
                    return;

            switch (direction) {
                case 0:
                    xIndex++;
                    break; // right
                case 1:
                    xIndex--;
                    break; // left
                case 2:
                    zIndex++;
                    break; // up
                case 3:
                    zIndex--;
                    break; // down
            }

            // add constraints
            if (xIndex < 0) xIndex = gridEdgeSize - 1;
            else if (xIndex > gridEdgeSize - 1) xIndex = 0;
            if (zIndex < 0) zIndex = gridEdgeSize - 1;
            else if (zIndex > gridEdgeSize - 1) zIndex = 0;

            // get the previous cube index for the body parts
            snakeHead.previousCubeIndex = cubeIndex;

            // map the indexes to get the correct cubIndex
            cubeIndex = xIndex + zIndex * gridEdgeSize; // cubeIndex = x + y * gridWidth;
            snakeHead.transform.localPosition = cubes[cubeIndex].transform.localPosition + Vector3.up;

            if (_bodyparts.Count > 0) {
                for (int i = _bodyparts.Count - 1; i > 0; i--) {
                    _bodyparts[i].transform.localPosition = _bodyparts[i - 1].transform.localPosition;
                    _bodyparts[i].cubeIndex = _bodyparts[i - 1].cubeIndex;
                }

                _bodyparts[0].transform.localPosition =
                    cubes[snakeHead.previousCubeIndex].transform.localPosition + Vector3.up;
                _bodyparts[0].cubeIndex = snakeHead.previousCubeIndex;
            }

            // if the snake head is at the same as the food, then that's a score
            if (food.gridIndex == cubeIndex) {
                // get the maximum snake body parts and only instantiate if it's below the maximum body parts allowed (config yaml file for curriculum learning)
                var maxBodyParts = Academy.Instance.EnvironmentParameters.GetWithDefault("max_snake_body_count", 0.0f);
                if ((_bodyparts.Count + 1) <= maxBodyParts || !snakeHead.isTraining) {
                    var bodyPart = Instantiate(bodyPartPrefab, originalPosition, Quaternion.identity);
                    bodyPart.transform.SetParent(transform);
                    int pos = 0;
                    if (_bodyparts.Count > 0) {
                        pos = _bodyparts[_bodyparts.Count - 1].cubeIndex;
                        bodyPart.GetComponent<GridCube>().cubeIndex = pos;
                    }
                    else {
                        pos = snakeHead.previousCubeIndex;
                        bodyPart.GetComponent<GridCube>().cubeIndex = snakeHead.previousCubeIndex;
                    }

                    bodyPart.transform.localPosition = cubes[pos].transform.localPosition + Vector3.up;
                    _bodyparts.Add(bodyPart.GetComponent<GridCube>());
                }
                // make sure that the agent gets a reward for getting the food
                if (_bodyparts.Count == cubes.Length - 1) {
                    print("Won the game!!");
                    snakeHead.WonGame();
                }
                else snakeHead.Score();
            }
        }

        private int CalculateConstrainedIndex(int index) {
            if (index > 99) index = 99 - index;
            else if (index < 0) index = 99 + index;
            return index;
        }

        public void ClearBodyParts() {
            foreach (var b in _bodyparts) { Destroy(b.gameObject); }
            _bodyparts.Clear();
        }
    }
}