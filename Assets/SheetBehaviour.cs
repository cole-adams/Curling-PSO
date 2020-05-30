using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ScoreEvent : UnityEvent<int, Vector2>
{
}
public class SheetBehaviour : MonoBehaviour
{
    public const float MAX_SPEED = 4.25f; //About 5.5s Hog to Hog
    public const float MIN_SPEED = 2.185f; //Just over the hogline, 20.7s Hog to Hog

    public const float MIN_ANGLE = -4.75f;
    public const float MAX_ANGLE = 4.75f;

    public StoneBehaviour blueStone;
    public StoneBehaviour yellowStone;

    List<KeyValuePair <Vector3, bool>> stonePositions;
    List<StoneBehaviour> stones;
    Vector2 searchPosition;
    int bestScore = -9;
    Vector2 bestKnownPosition;
    Vector2 velocity;
    bool turn;
    bool thrown;

    ScoreEvent scoreUpdate;

    public void InitializeObject(bool turn, ScoreEvent se) 
    {
        this.turn = turn;
        scoreUpdate = se;
    }

    public void SetupComputation(List<KeyValuePair <Vector3, bool>> stonePositions, Vector2 searchPosition, Vector2 velocity)
    {
        this.stonePositions = stonePositions;
        this.searchPosition = searchPosition;
        this.velocity = velocity;

        this.bestKnownPosition = searchPosition;
        stones = new List<StoneBehaviour>();
        ResetSheet();
    }


    public void UpdateVelocity(Vector2 globalBestKnown) 
    {
        float r1 = Random.value;
        float r2 = Random.value;

        velocity = velocity + r1 * (bestKnownPosition - searchPosition) + r2 * (globalBestKnown - searchPosition);

        Vector2.ClampMagnitude(velocity, 3f);
        searchPosition = velocity + searchPosition;
        BoundPosition();
    }

    public void BoundPosition()
    {
        if (searchPosition.x > MAX_SPEED) {
            searchPosition.x = MAX_SPEED;
            velocity = Vector2.zero;
        }
        if (searchPosition.x < MIN_SPEED) {
            searchPosition.x = MIN_SPEED;
            velocity = Vector2.zero;
        }
        if (searchPosition.y > MAX_ANGLE) {
            searchPosition.y = MAX_ANGLE;
            velocity = Vector2.zero;
        }
        if (searchPosition.y < MIN_ANGLE) {
            searchPosition.y = MIN_ANGLE;
            velocity = Vector2.zero;
        }
    }

    public void Update()
    {
        if (thrown) {
            bool shotComplete = true;
            float maxMag = 0;
            foreach (StoneBehaviour s in stones)
            {
                if (s !=null){
                    Rigidbody rb = s.GetComponent<Rigidbody>();
                    if (rb.velocity.magnitude > 0.01) {
                        shotComplete = false;
                        if (rb.velocity.magnitude > maxMag) {
                            maxMag = rb.velocity.magnitude;
                        }
                    }
                }
            }

            if (shotComplete || maxMag > MAX_SPEED + 2f) {
                thrown = false;
                calculateScore();
                ResetSheet();
            }
        }
    }

    public void calculateScore()
    {
        Vector3 button = transform.position + new Vector3(17.3735f, 0f, 0f); //This is where the button is relative to the center of the sheet
        var distanceColor = new List<KeyValuePair<float, bool>>();
        int scoreResult;

        foreach (StoneBehaviour g in stones) {
            if (g != null){
                Transform tr = g.GetComponent<Transform>();
                Vector3 buttonToRock = tr.position - button;
                Vector3 buttonToEdgeOfRock = Vector3.ClampMagnitude(buttonToRock, buttonToRock.magnitude - 0.146f);
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

        if (scoreResult > bestScore) {
            bestScore = scoreResult;
            bestKnownPosition = searchPosition;
        }
        scoreUpdate.Invoke(scoreResult, searchPosition);
    }

    public void ResetSheet()
    {
        foreach(StoneBehaviour g in stones) {
            if (g != null){
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

    public void Compute()
    {
        StoneBehaviour spawn = Instantiate<StoneBehaviour>(blueStone);

        spawn.transform.position = transform.position + new Vector3(-21.0315f, 0.55f, 0f); //This is where the hack is relative to the center of the sheet
        spawn.transform.rotation = Quaternion.identity;
        spawn.initialSpeed = searchPosition.x;
        spawn.initialAngle = searchPosition.y;
        spawn.inTurn = turn;
        stones.Add(spawn);

        foreach(StoneBehaviour s in stones) {
            s.inPlay = true;
        }
        spawn.Throw();
        thrown = true;
    }
}