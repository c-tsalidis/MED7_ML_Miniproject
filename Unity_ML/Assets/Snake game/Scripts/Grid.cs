using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grid : MonoBehaviour {
    [SerializeField] private int size;
    [SerializeField] private Transform snakeHeadTransform;

    public int xIndex, zIndex; // indexes corresponding to the position in the grid
    public Transform[] cubes;

    private void Start() {
        for (int i = 0; i < cubes.Length; i++) {
            cubes[i] = transform.GetChild(i);
        }
    }

    public void SetSnakeHeadPosition(int direction) {
        // snakeHeadTransform.position = new Vector3(_rows[xPosition], 0, _columns[zPosition]);
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
            default: break;
        }

        // add constraints
        if (xIndex < 0) xIndex = 9;
        else if (xIndex > 9) xIndex = 0;
        if (zIndex < 0) zIndex = 9;
        else if (zIndex > 9) zIndex = 0;
        int cubeIndex = xIndex + zIndex * 10; // cubeIndex = x + y * width;
        print("Cube index: " + cubeIndex);
        snakeHeadTransform.localPosition = cubes[cubeIndex].localPosition;
    }
}