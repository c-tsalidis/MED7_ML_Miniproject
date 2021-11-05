using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Snake_game.Scripts {
    public class Grid : MonoBehaviour {
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

        private void Start() {
            for (int i = 0; i < cubes.Length; i++) {
                cubes[i] = transform.GetChild(i).GetComponent<GridCube>();
                cubes[i].cubeIndex = i;
            }

            SetFoodPosition();
            if (timeBetweenFrames > 0) {
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
                
                /*
                if (!cube.GetComponent<GridCube>().hasBodyPart) {
                    food.transform.localPosition = cube.transform.localPosition;
                    food.gridIndex = randomCubeIndex;
                    foundFreeSpotForFood = true;
                }
                */
            }
        }

        public void SetSnakeHeadPosition(int direction) {
            if (!timeFinished && timeBetweenFrames > 0) return;
            
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
            if (xIndex < 0) xIndex = 9;
            else if (xIndex > 9) xIndex = 0;
            if (zIndex < 0) zIndex = 9;
            else if (zIndex > 9) zIndex = 0;
            
            // get the previous cube index for the body parts
            snakeHead.previousCubeIndex = cubeIndex;

            // map the indexes to get the correct cubIndex
            cubeIndex = xIndex + zIndex * 10; // cubeIndex = x + y * gridWidth;
            snakeHead.transform.localPosition = cubes[cubeIndex].transform.localPosition;
            // cubes[cubeIndex].GetComponent<MeshRenderer>().material = snakeMaterial;
            
            if (_bodyparts.Count > 0) {
                // for (int i = 1; i < _bodyparts.Count; i++) {
                //    _bodyparts[i].transform.localPosition = _bodyparts[i - 1].transform.localPosition;
                // }
                
                for (int i = _bodyparts.Count - 1; i > 0; i--) {
                    _bodyparts[i].transform.localPosition = _bodyparts[i - 1].transform.localPosition;
                    _bodyparts[i].cubeIndex = _bodyparts[i - 1].cubeIndex;
                }
                _bodyparts[0].transform.localPosition = cubes[snakeHead.previousCubeIndex].transform.localPosition;
                _bodyparts[0].cubeIndex = snakeHead.previousCubeIndex;
            }

            /*
        // get the last body part if it exists and change its color back to the ground
        if (_bodyparts.Count > 0) {
            // var lastBodyPart = _bodyparts.Dequeue();
            var lastBodyPart = _bodyparts[_bodyparts.Count - 1];
            print(lastBodyPart.name);
            lastBodyPart.hasBodyPart = false;
            lastBodyPart.GetComponent<MeshRenderer>().material = gridCubeMaterial;
        }
        */
            // if (_bodyparts.Count > 0) {
                // _bodyparts[0] = cubes[snakeHead.previousCubeIndex];
                // _bodyparts[0].cubeIndex = snakeHead.previousCubeIndex;
                
                // for (int i = 1; i < _bodyparts.Count; i++) {
                   // _bodyparts[i].transform.localPosition = _bodyparts[i - 1].transform.localPosition;
                    // _bodyparts[i].cubeIndex = _bodyparts[i - 1].cubeIndex;
                // }
                // _bodyparts[0].transform.localPosition = cubes[snakeHead.previousCubeIndex].transform.localPosition;
                
                
                /*
            if (direction == 0) _bodyparts[0] = cubes[cubeIndex - 1]; // right
            if (direction == 1) _bodyparts[0] = cubes[cubeIndex + 1]; // left
            if (direction == 2) _bodyparts[0] = cubes[cubeIndex - 10]; // up
            if (direction == 3) _bodyparts[0] = cubes[cubeIndex + 10]; // down
            */


                /*
                for (int i = _bodyparts.Count - 1; i > 0; i--) {
                    _bodyparts[i - 1].GetComponent<MeshRenderer>().material = snakeMaterial;
                    _bodyparts[i].GetComponent<MeshRenderer>().material = gridCubeMaterial;
                   _bodyparts[i] = _bodyparts[i - 1];
                }
                */

                /*
                var bodyPartsIndex = cubeIndex;
                for (int i = 0; i < _bodyparts.Count; i++) {
                    _bodyparts[i].meshRenderer.material = gridCubeMaterial; // reset its material
                    _bodyparts[i].hasBodyPart = false;
                    if (i == 0) {
                        _bodyparts[0] = cubes[snakeHead.previousCubeIndex];
                        bodyPartsIndex = snakeHead.previousCubeIndex;
                        _bodyparts[0].GetComponent<MeshRenderer>().material = snakeMaterial;
                        _bodyparts[0].hasBodyPart = true;
                    }
                    else {
                        if (direction == 0) bodyPartsIndex = CalculateConstrainedIndex(bodyPartsIndex - 1); // right
                        if (direction == 1) bodyPartsIndex = CalculateConstrainedIndex(bodyPartsIndex + 1); // left
                        if (direction == 2) bodyPartsIndex = CalculateConstrainedIndex(bodyPartsIndex - 10); // up
                        if (direction == 3) bodyPartsIndex = CalculateConstrainedIndex(bodyPartsIndex + 10); // down
                        _bodyparts[i] = cubes[bodyPartsIndex];
                        _bodyparts[i].meshRenderer.material = snakeMaterial;
                        _bodyparts[i].hasBodyPart = true;
                    }
                }
                */

                /*
                for (int i = 1; i < _bodyparts.Count; i++) {
                    _bodyparts[i].meshRenderer.material = gridCubeMaterial; // reset its material
                    _bodyparts[i].hasBodyPart = false;
                    _bodyparts[i] = cubes[_bodyparts[i - 1].cubeIndex]; 
                    _bodyparts[i].meshRenderer.material = snakeMaterial;
                    _bodyparts[i].hasBodyPart = true;
                }
                _bodyparts[0] = cubes[snakeHead.previousCubeIndex];
                // bodyPartsIndex = snakeHead.previousCubeIndex;
                _bodyparts[0].GetComponent<MeshRenderer>().material = snakeMaterial;
                _bodyparts[0].hasBodyPart = true;
                */

                /*
                for (int i = _bodyparts.Count - 1; i > 0; i--) {
                    _bodyparts[i].meshRenderer.material = gridCubeMaterial; // reset its material
                    _bodyparts[i].hasBodyPart = false;
                    _bodyparts[i] = cubes[_bodyparts[i - 1].cubeIndex];
                    _bodyparts[i].meshRenderer.material = snakeMaterial;
                    _bodyparts[i].hasBodyPart = true;
                }
                _bodyparts[0] = cubes[snakeHead.previousCubeIndex];
                // bodyPartsIndex = snakeHead.previousCubeIndex;
                _bodyparts[0].GetComponent<MeshRenderer>().material = snakeMaterial;
                _bodyparts[0].hasBodyPart = true;
                */

                // for (int i = 1; i < _bodyparts.Count; i++) {
                //     _bodyparts[i] = _bodyparts[i - 1];
                //     _bodyparts[i].meshRenderer.material = gridCubeMaterial;
                // }
            // }

            // if the snake head is at the same as the food, then that's a score
            // if (food.gridIndex == cubes[cubeIndex].cubeIndex) {
            if (food.gridIndex == cubeIndex) {
                // TODO --> Instead of keeping track of indexes, just change this current cube's material to snake body part
                // var bodyGridCube = cubes[cubeIndex];
                
                var bodyPart = Instantiate(bodyPartPrefab);
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
                
                /*
                if (direction == 0) bodyGridCube = cubes[cubeIndex - 1]; // right
                if (direction == 1) bodyGridCube = cubes[cubeIndex + 1]; // left
                if (direction == 2) bodyGridCube = cubes[cubeIndex - 10]; // up
                if (direction == 3) bodyGridCube = cubes[cubeIndex + 10]; // down
                */
                /*
                if (_bodyparts.Count == 0) bodyGridCube = cubes[snakeHead.previousCubeIndex];
                else {
                    var lastBodyPartIndex = _bodyparts[_bodyparts.Count - 1].cubeIndex;
                    if (direction == 0) bodyGridCube = cubes[CalculateConstrainedIndex(lastBodyPartIndex - 1)]; // right
                    if (direction == 1) bodyGridCube = cubes[CalculateConstrainedIndex(lastBodyPartIndex + 1)]; // left
                    if (direction == 2) bodyGridCube = cubes[CalculateConstrainedIndex(lastBodyPartIndex - 10)]; // up
                    if (direction == 3) bodyGridCube = cubes[CalculateConstrainedIndex(lastBodyPartIndex + 10)]; // down
                }
                bodyGridCube.hasBodyPart = true;
                bodyGridCube.GetComponent<MeshRenderer>().material = snakeMaterial;
                */
                /*
            int lastX = 0, lastZ = 0; // last body parts indexes
            if (_bodyparts.Count > 0) {
                lastX = _bodyparts[_bodyparts.Count - 1].xIndex;
                lastZ = _bodyparts[_bodyparts.Count - 1].zIndex;
            }
            else {
                lastX = xIndex;
                lastZ = zIndex;
            }
            if (direction == 0) bodyGridCube.cubeIndex = lastX - 1;
            else if (direction == 1) bodyGridCube.cubeIndex = lastX + 1;
            else if (direction == 2) bodyGridCube.cubeIndex = lastZ - 1;
            else if (direction == 3) bodyGridCube.cubeIndex = lastZ + 1;
            */
                // _bodyparts.Enqueue(bodyGridCube);
                // _bodyparts.Add(bodyGridCube);
                // snakeHead.Score();

                /*
             if (isXAxis) {
                    body[0].x = grid.x[xCounter - xDirection];
                    body[0].y = grid.y[yCounter];
                }
                else {
                    body[0].x = grid.x[xCounter];
                    body[0].y = grid.y[yCounter - yDirection];
                }
             for (let i = bodyCount - 1; i > 0; i--) {
                    body[i].x = body[i - 1].x;
                    body[i].y = body[i - 1].y;
                    rect(body[i].x, body[i].y, size, size);
                }
             */
            }
            else if (cubes[cubeIndex].hasBodyPart) {
                /*
                foreach (var c in cubes) {
                    c.GetComponent<MeshRenderer>().material = gridCubeMaterial;
                    c.hasBodyPart = false;
                }
                */
                foreach (var b in _bodyparts) { Destroy(b.gameObject); }
                _bodyparts.Clear();
                snakeHead.Punish();
            }
        }

        private int CalculateConstrainedIndex(int index) {
            if (index > 99) index = 99 - index;
            else if (index < 0) index = 99 + index;
            return index;
        }

        private void SetBodyPartPosition(GridCube gridCube) { }

        public void SetBodyPartsPositions() {
            foreach (var bp in _bodyparts) {
                SetBodyPartPosition(bp.GetComponent<GridCube>());
            }
        }
    }
}