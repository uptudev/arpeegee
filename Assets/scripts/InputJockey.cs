// InputJockey by uptu
// Used for input handling and camera control of player character

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Must have all necessary Unity components
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]

public class InputJockey : MonoBehaviour {
    // Create public vars and init with default values
    public float maxSpeed = 3.4f;
    public float jumpHeight = 1f;
    public GameObject cameraParent;

    // Create private vars and init
    private Vector3 cameraPos;
    private Rigidbody r3d;
    private Transform tf;
    private float mass;
    private float xVel;
    private float yVel;
    private float zVel;
    private enum cameraFace {NORTH, EAST, SOUTH, WEST}
    private cameraFace cf;
    private Vector3 tmpMv;
    private float horiz;
    private bool grounded;

    // Import all components and initialize composite variables
    private void Start() {
        tf = transform;
        r3d = GetComponent<Rigidbody>();
        r3d.freezeRotation = true;
        r3d.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        xVel = r3d.velocity.x;
        yVel = r3d.velocity.y;
        zVel = r3d.velocity.z;
        mass = r3d.mass;
        cf = cameraFace.NORTH;
        if (cameraParent) {   // Error handling in case of missing cam
            cameraPos = cameraParent.transform.position;
        }
    }

    // Update is called once per frame
    private void Update() {
        // Update camera position
        if (cameraParent) {
            cameraParent.transform.position = new Vector3(tf.position.x, tf.position.y, tf.position.z);
        }
    }

    // Physics update; independent of framerate
    private void FixedUpdate() {
        tmpMv = moveAlign(new Vector3(xVel, 0, zVel), cf);
        if (exp(Math.Abs(r3d.velocity.x), 2) + exp(Math.Abs(r3d.velocity.z), 2) < exp(maxSpeed, 2)) { // Hard cap speed if a^2 + b^2 < maxC^2
            r3d.AddForce(tmpMv * maxSpeed);
        }
    }

    // Faster than Math.Pow
    static double exp(double num, int exp) {
        double result = 1.0;
        while (exp > 0) {
            if (exp % 2 == 1) {result *= num;}
            exp >>= 1;
            num *= num;
        }
        return result;
    }

    // Movement handling
    private void OnMove(InputValue moveVal) {
        if (grounded) {
            Vector2 moveVec = moveVal.Get<Vector2>();
            xVel = moveVec.x;
            zVel = moveVec.y;
        }
    }

    // On 'Q' press
    private void OnTurnLeft() {
        cf = TurnLeft(cf);
        Vector3 newV = new Vector3(0,-90,0);
        tf.Rotate(newV);
        cameraParent.transform.Rotate(newV);
    }

    // On 'E' press
    private void OnTurnRight() {
        cf = TurnRight(cf);
        Vector3 newV = new Vector3(0,90,0);
        tf.Rotate(newV);
        cameraParent.transform.Rotate(newV);
    }

    // Returns direction for camera to face after a left turn given current face
    private Vector3 moveAlign(Vector3 v, cameraFace f) => f switch {
        cameraFace.NORTH => v,
        cameraFace.EAST => new Vector3(zVel, 0, -xVel),
        cameraFace.SOUTH => new Vector3(-xVel, 0, -zVel),
        cameraFace.WEST => new Vector3(-zVel, 0, xVel),
        _ => throw new ArgumentOutOfRangeException(nameof(f), $"Not an expected facing value: {f}"),
    };

    // Returns direction for camera to face after a left turn given current face
    private static cameraFace TurnLeft(cameraFace f) => f switch {
        cameraFace.NORTH => cameraFace.WEST,
        cameraFace.EAST => cameraFace.NORTH,
        cameraFace.SOUTH => cameraFace.EAST,
        cameraFace.WEST => cameraFace.SOUTH,
        _ => throw new ArgumentOutOfRangeException(nameof(f), $"Not an expected facing value: {f}"),
    };

    // Returns direction for camera to face after a right turn given current face
    private static cameraFace TurnRight(cameraFace f) => f switch {
        cameraFace.NORTH => cameraFace.EAST,
        cameraFace.EAST => cameraFace.SOUTH,
        cameraFace.SOUTH => cameraFace.WEST,
        cameraFace.WEST => cameraFace.NORTH,
        _ => throw new ArgumentOutOfRangeException(nameof(f), $"Not an expected facing value: {f}"),
    };

    //Flag as grounded if foot collider is contacting an object
    private void OnTriggerEnter(Collider other) {
        if (other.tag != "player") {
            grounded = true;
        }
    }
    //Flag as ungrounded if foot collider has stopped contacting an object
    private void OnTriggerExit(Collider other) {
        if (other.tag != "player") {
            grounded = false;
        }
    }
}
