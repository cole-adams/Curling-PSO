/* Object drag aspects retrieved from: [https://www.patreon.com/posts/unity-3d-drag-22917454](https://www.patreon.com/posts/unity-3d-drag-22917454) Author: Jayanam Games 
*/
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StoneBehaviour : MonoBehaviour
{
    public float initialSpeed;
    public float initialAngle;
    public bool inTurn = false;
    public bool thrown = false;
    public bool finalStone = false;

    public bool inPlay = false;

    private Vector3 mOffset;
    private float mZCoord;
    
    void FixedUpdate()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        float mag = body.velocity.magnitude;
        if (thrown) {

            Vector3 perp = new Vector3(-body.velocity.z, body.velocity.y, body.velocity.x).normalized;
            Vector3 curl = perp * (0.18f/mag);

            if (inTurn) {
                curl = -curl;
            }
            body.AddForce(curl);
        }
        if (mag < 0.05) {
            body.angularVelocity = Vector3.zero;
            thrown = false;
        }

    }

    public void Throw()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        Vector3 dir = Quaternion.AngleAxis(initialAngle, Vector3.up) * Vector3.right;
        Vector3 velocity = initialSpeed * dir;
        body.velocity = velocity;
        finalStone = true;
    }

    void OnTriggerEnter (Collider enteredCollider)
    {
        if (enteredCollider.CompareTag("PlayZone") && finalStone)
        {
            Vector3 turn;
            if (inTurn) {
                turn = new Vector3(0f, 1f, 0f);
            } else {
                turn = new Vector3(0f, -1f, 0f);
            }
            Rigidbody body = GetComponent<Rigidbody>();
            body.angularVelocity = turn;
            thrown = true;
        }
    }

    void OnTriggerExit(Collider exitedCollider)
    {
        if (exitedCollider.CompareTag("PlayZone") && inPlay)
        {
            Destroy(gameObject);
        }
    }

    private Vector3 GetMouseAsWorldPoint()
    {
        // Pixel coordinates of mouse (x,y)
        Vector3 mousePoint = Input.mousePosition;

        // z coordinate of game object on screen
        mousePoint.z = mZCoord;

        // Convert it to world points
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnMouseDown()
    {
        if (!inPlay) {
            mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

            mOffset = gameObject.transform.position - GetMouseAsWorldPoint();
            Vector3 resPos = GetMouseAsWorldPoint() + mOffset;
            transform.position = new Vector3(resPos.x, 3f, resPos.z);
        }
    }

    void OnMouseDrag()
    {
        if (!inPlay) {
            Vector3 resPos = GetMouseAsWorldPoint() + mOffset;
            transform.position = new Vector3(resPos.x, 3f, resPos.z);
            Rigidbody body = GetComponent<Rigidbody>();
            body.velocity = Vector3.zero;
        }
    }

    void OnMouseUp()
    {
        if (!inPlay) {
            transform.position = new Vector3(transform.position.x, 0.55f, transform.position.z);
        }
    }
}
