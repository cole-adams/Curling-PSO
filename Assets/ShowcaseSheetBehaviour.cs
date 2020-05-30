using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowcaseSheetBehaviour : MonoBehaviour
{
    public StoneBehaviour blueStone;
    public StoneBehaviour yellowStone;
    public List<KeyValuePair <Vector3, bool>> stonePositions;
    public List<StoneBehaviour> stones;
    public float initialSpeed;
    public float initialAngle;
    public bool turn;
    public bool computing = false;

    int scoreResult;
    bool thrown = false;
    
    public void UpdateSheet(List<StoneBehaviour> stones, float initialSpeed, float initialAngle) 
    {
        this.stones = stones;
        this.initialSpeed = initialSpeed;
        this.initialAngle = initialAngle;
    }

    public void Start()
    {
        
    }

    public void Update()
    {
        if (thrown) {
            bool shotComplete = true;
            foreach (StoneBehaviour s in stones)
            {
                if (s !=null){
                    Rigidbody rb = s.GetComponent<Rigidbody>();
                    if (rb.velocity.magnitude > 0.001) {
                        shotComplete = false;
                    }
                }
            }

            if (shotComplete) {
                calculateScore();
            }
        }
    }

    public void calculateScore()
    {
        Vector3 button = transform.position + new Vector3(17.3735f, 0f, 0f); //This is where the button is relative to the center of the sheet
        var distanceColor = new List<KeyValuePair<float, bool>>();

        foreach (StoneBehaviour g in stones) {
            if (g != null){
                Transform tr = g.GetComponent<Transform>();
                Vector3 buttonToRock = tr.position - button;
                Vector3 buttonToEdgeOfRock = Vector3.ClampMagnitude(buttonToRock, buttonToRock.magnitude - 0.146f); //Radius of rock
                bool color = g.CompareTag("Blue");
                distanceColor.Add(new KeyValuePair <float, bool> (buttonToEdgeOfRock.magnitude, color));
            }
        }
        if (distanceColor.Count == 0) {
            scoreResult = 0;
        } else {
            StoneComparer sc = new StoneComparer();
            distanceColor.Sort(sc);
            int i = 0;
            bool scoring = distanceColor[0].Value;
            while (i < distanceColor.Count && distanceColor[i].Value == scoring && distanceColor[i].Key < 1.829f)
            {
                i++;
            }
            if (!scoring) {
                i = -i;
            }
            scoreResult = i;
        }
    }

    public void TrowRock()
    {
        if (computing) {
            ResetSheet();
        }
        StoneBehaviour spawn = Instantiate<StoneBehaviour>(blueStone);
        spawn.transform.position = transform.position + new Vector3(-21.0315f, 0.5f, 0f); //This is where the hack is relative to the center of the sheet
        spawn.transform.rotation = Quaternion.identity;
        spawn.initialSpeed = initialSpeed;
        spawn.initialAngle = initialAngle;
        spawn.inTurn = turn;
        spawn.name = "ShowcaseThrown";
        stones.Add(spawn);

        foreach(StoneBehaviour s in stones) {
            s.inPlay = true;
        }
        spawn.Throw();
        thrown = true;
    }

    public void ResetSheet()
    {
        foreach(StoneBehaviour g in stones) {
            if (g!=null) {
                Destroy(g.gameObject);
            }
        }
        stones = new List<StoneBehaviour>();

        foreach(KeyValuePair<Vector3, bool> kp in stonePositions) {
            StoneBehaviour spawn;
            if (kp.Value) {
                spawn = Instantiate<StoneBehaviour>(blueStone);
            } else {
                spawn = Instantiate<StoneBehaviour>(yellowStone);
            }
            spawn.transform.position = transform.position + kp.Key;
            stones.Add(spawn);
        }
    }
}

class StoneComparer : IComparer<KeyValuePair<float, bool>>
{
    public int Compare(KeyValuePair<float, bool> x, KeyValuePair<float, bool> y)
    {
        return x.Key.CompareTo(y.Key);
    }
}