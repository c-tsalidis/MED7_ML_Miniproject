using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Snake_game.Scripts {
    public class Grid : MonoBehaviour {
        public bool isTraining = true;
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

            SetFoodPosition();
            // if (timeBetweenFrames > 0 && !isTraining) {
            if (!isTraining) {
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

        public void SetFoodPosition() {
            bool foundFreeSpotForFood = false;
            while (!foundFreeSpotForFood) {
                var randomCubeIndex = Random.Range(0, cubes.Length);
                var cube = cubes[randomCubeIndex];
                foreach (var b in _bodyparts.Where(b => cube.cubeIndex == b.cubeIndex)) break;
                
                food.transform.localPosition = cube.transform.localPosition; 
                food.gridIndex = randomCubeIndex; 
                foundFreeSpotForFood = true;
            }
        }

        public void SetSnakeHeadPosition(int direction) {
            // if (!timeFinished && timeBetweenFrames > 0) return;
            if(!isTraining) if (!timeFinished && timeBetweenFrames > 0) return;

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
            snakeHead.transform.localPosition = cubes[cubeIndex].transform.localPosition;

            if (_bodyparts.Count > 0) {
                for (int i = _bodyparts.Count - 1; i > 0; i--) {
                    _bodyparts[i].transform.localPosition = _bodyparts[i - 1].transform.localPosition;
                    _bodyparts[i].cubeIndex = _bodyparts[i - 1].cubeIndex;
                }
                _bodyparts[0].transform.localPosition = cubes[snakeHead.previousCubeIndex].transform.localPosition;
                _bodyparts[0].cubeIndex = snakeHead.previousCubeIndex;
            }
            
            // if the snake head is at the same as the food, then that's a score
            if (food.gridIndex == cubeIndex) {
                var bodyPart = Instantiate(bodyPartPrefab, originalPosition, Quaternion.identity);
                int pos = 0;
                if (_bodyparts.Count > 0) {
                    pos = _bodyparts[_bodyparts.Count - 1].cubeIndex;
                    bodyPart.GetComponent<GridCube>().cubeIndex = pos;
                }
                else {
                    pos = snakeHead.previousCubeIndex;
                    bodyPart.GetComponent<GridCube>().cubeIndex = snakeHead.previousCubeIndex;
                }
                bodyPart.transform.localPosition = cubes[pos].transform.localPosition;
                _bodyparts.Add(bodyPart.GetComponent<GridCube>());
                snakeHead.Score();
            }
        }

        private int CalculateConstrainedIndex(int index) {
            if (index > 99) index = 99 - index;
            else if (index < 0) index = 99 + index;
            return index;
        }
    }
}