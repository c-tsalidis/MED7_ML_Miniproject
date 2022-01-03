using System;
using UnityEditor;
using UnityEngine;

namespace Snake_game.Scripts {
    public class MoveCamera : MonoBehaviour {
        private Camera cam;
        [SerializeField] private Transform toFollow;
        [SerializeField] private float scale, speed, moveStep;
        private void Start() => cam = GetComponent<Camera>();

        private void Update() {
            var x = Input.GetAxisRaw("Horizontal") * speed;
            var z = Input.GetAxisRaw("Vertical") * speed;
            // toFollow.position += new Vector3(x, 0, z);
            
            transform.position += new Vector3(x, 0, z) * Time.deltaTime;
            // transform.position = Vector3.Lerp(transform.position, toFollow.position, moveStep);

            // because it's an orthographic camera, for the y to have an effect, change the size in orthographic camera
            cam.orthographicSize += Input.mouseScrollDelta.y * scale * Time.deltaTime;
        }
    }
}