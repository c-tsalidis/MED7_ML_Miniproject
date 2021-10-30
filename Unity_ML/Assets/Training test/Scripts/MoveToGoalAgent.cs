using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class MoveToGoalAgent : Agent {
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Target")) {
            floorMeshRenderer.material = winMaterial;
            SetReward(1);
            EndEpisode();
        }
        else if (other.CompareTag("Wall")) {
            floorMeshRenderer.material = loseMaterial;
            SetReward(-1);
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        print(actions.ContinuousActions[0]);
        float moveX = actions.ContinuousActions[0]; // x axis
        float moveZ = actions.ContinuousActions[1]; // z axis
        transform.localPosition += new Vector3(moveX, 0, moveZ) * (Time.deltaTime * moveSpeed);
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical"); 
    }

    public override void OnEpisodeBegin() {
        transform.localPosition = new Vector3(Random.Range(-3.8f,3.8f), 0, Random.Range(-3.8f,3.8f));
        targetTransform.localPosition = new Vector3(Random.Range(-3.8f,3.8f), 0, Random.Range(-3.8f,3.8f));
    }
}