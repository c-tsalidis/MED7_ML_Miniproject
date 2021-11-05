using System;
using UnityEngine;

namespace Snake_game.Scripts {
    public class GridCube : MonoBehaviour {
        public bool hasBodyPart = false;
        public int cubeIndex = 0;
        public int xIndex, zIndex;
        public MeshRenderer meshRenderer;
        public int previousIdx;
        
        private void Start() {
            meshRenderer = GetComponent<MeshRenderer>();
        }
    }
}