using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SnakeHead : Agent {
    [SerializeField] private Grid grid;
    [SerializeField] private Food _food;
    [SerializeField] private float moveSpeed = 1.0f;


    // private int moveUp, moveDown, moveLeft, moveRight;
    private int xPosition, zPosition;

    private int direction = 0;
    /*
    private void Update() {
        // grid.SetSnakeHeadPosition(xPosition, zPosition);
        grid.SetSnakeHeadPosition();
        // transform.localPosition += new Vector3(moveRight, 0, moveUp) * ((Time.deltaTime) * moveSpeed);
    }*/

    public override void OnActionReceived(ActionBuffers actions) {
        print("Direction: " + actions.DiscreteActions[0]);
        grid.SetSnakeHeadPosition(actions.DiscreteActions[0]);
        // zPosition = actions.DiscreteActions[1];
        /*
        if (direction == 0) { // going to the right
            
        }
        else if (direction == 1) { // it means it's going to the left
            
        }
        else if (direction == 2) { // going down
            
        }
        else if (direction == 3) { // going up
            
        }
        */
        // xPosition = actions.DiscreteActions[0];
        // zPosition = actions.DiscreteActions[1];
        // moveRight = actions.DiscreteActions[2];
        // moveLeft = actions.DiscreteActions[3];
        // print(moveUp + " - " + moveDown + " - " + moveRight + " - " + moveLeft);
        // print(actions.ContinuousActions[0]);
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(_food.transform.localPosition);
    }
    
    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.RightArrow)) discreteActions[0] = 0; // right
        if (Input.GetKey(KeyCode.LeftArrow)) discreteActions[0] = 1; // left
        if (Input.GetKey(KeyCode.UpArrow)) discreteActions[0] = 2; // up
        if (Input.GetKey(KeyCode.DownArrow)) discreteActions[0] = 3; // down
        // ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        // discreteActions[0] = Input.GetKeyDown(KeyCode.UpArrow) ? 1 : 0; // up --> 1 in z axis
        // discreteActions[1] = Input.GetKeyDown(KeyCode.DownArrow) ? -1 : 0; // down --> -1 in z axis
        // discreteActions[2] = Input.GetKeyDown(KeyCode.RightArrow) ? 1 : 0; // right --> 1 in x axis
        // discreteActions[3] = Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : 0; // left --> -1 in x axis
    }
}