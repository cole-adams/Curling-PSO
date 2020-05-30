using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Math = System.Math;

public class PSOController : MonoBehaviour
{
    public CanvasController iterationText;

    private const int SWARM_SIZE = 20;
    private const int ITERATIONS = 50;

    public const float MAX_SPEED = 4.25f; //About 5.5s Hog to Hog
    public const float MIN_SPEED = 2.185f; //Just over the hogline, 20.7s Hog to Hog

    public const float MIN_ANGLE = -4.75f;
    public const float MAX_ANGLE = 4.75f;

    public SheetBehaviour sheetPrefab;

    public ShowcaseSheetBehaviour showcaseSheet;
    public GameObject showcasePlayZone;

    int maxInTurnScore = -9;
    List<Vector2> bestInturns;
    Vector2 bestInturn;
    float inTurnRange;
    int inTurnScores = 0;

    int maxOutTurnScore = -9;
    List<Vector2> bestOutturns;
    Vector2 bestOutturn;
    float outTurnRange;
    int outTurnScores = 0;

    public int inTurnIteration = 0;
    public int outTurnIteration = 0;

    List<SheetBehaviour> inTurnSpace;
    List<SheetBehaviour> outTurnSpace;

    void Start()
    {
        iterationText.totalIterations = ITERATIONS;

        inTurnSpace = new List<SheetBehaviour>();
        outTurnSpace = new List<SheetBehaviour>();

        int zOffset = 15;
        int yOffset = 7;
        ScoreEvent inTurnScore = new ScoreEvent();
        ScoreEvent outTurnScore = new ScoreEvent();
        inTurnScore.AddListener(inTurnUpdate);
        outTurnScore.AddListener(outTurnUpdate);
        for (int i = 0; i < SWARM_SIZE/5; i++) 
        {
            for (int j = 0; j < 5; j++) 
            {
                SheetBehaviour inTurnSheet = Instantiate<SheetBehaviour>(sheetPrefab);
                SheetBehaviour outTurnSheet = Instantiate<SheetBehaviour>(sheetPrefab);

                inTurnSheet.transform.position = new Vector3(0, yOffset * i, zOffset * (j + 1));
                outTurnSheet.transform.position = new Vector3(0, yOffset * i, -(zOffset * (j + 1)));

                inTurnSheet.InitializeObject(true, inTurnScore);
                outTurnSheet.InitializeObject(false, outTurnScore);

                inTurnSpace.Add(inTurnSheet);
                outTurnSpace.Add(outTurnSheet);
            }
        }
    }

    public void Update()
    {
        iterationText.inTurnIterations = inTurnIteration;
        iterationText.outTurnIterations = outTurnIteration;
    }

    public void StartSearch()
    {
        Collider playZone = showcasePlayZone.GetComponent<Collider>();

        List<KeyValuePair<Vector3, bool>> stonePositions = new List<KeyValuePair<Vector3, bool>>();

        foreach(StoneBehaviour s in showcaseSheet.stones)
        {
            if (playZone.bounds.Contains(s.transform.position))
            {
                stonePositions.Add(new KeyValuePair<Vector3, bool>(s.transform.position - showcaseSheet.transform.position, s.CompareTag("Blue")));
            }
        }

        foreach (SheetBehaviour sb in inTurnSpace)
        {
            Vector2 position = new Vector2(Random.Range(MIN_SPEED, MAX_SPEED), Random.Range(MIN_ANGLE, MAX_ANGLE));
            Vector2 velocity = new Vector2(Random.Range(-(MAX_SPEED-MIN_SPEED), MAX_SPEED-MIN_SPEED), Random.Range(-(MAX_ANGLE-MIN_ANGLE), MAX_ANGLE-MIN_ANGLE));
            sb.SetupComputation(stonePositions, position, velocity);
            sb.Compute();
        }

        foreach (SheetBehaviour sb in outTurnSpace)
        {
            Vector2 position = new Vector2(Random.Range(MIN_SPEED, MAX_SPEED), Random.Range(MIN_ANGLE, MAX_ANGLE));
            Vector2 velocity = new Vector2(Random.Range(-(MAX_SPEED-MIN_SPEED), MAX_SPEED-MIN_SPEED), Random.Range(-(MAX_ANGLE-MIN_ANGLE), MAX_ANGLE-MIN_ANGLE));
            sb.SetupComputation(stonePositions, position, velocity);
            sb.Compute();
        }

        showcaseSheet.stonePositions = stonePositions;
        showcaseSheet.computing = true;
    }

    public void inTurnUpdate(int score, Vector2 position)
    {
        if (score > maxInTurnScore)
        {
            maxInTurnScore = score;
            bestInturns = new List<Vector2>();
            Vector2 duplicateShot = new Vector2(position.x, position.y);
            bestInturns.Add(new Vector2(position.x, position.y));
            bestInturn = duplicateShot;
            inTurnRange = 0.0f;
        } else if (score == maxInTurnScore)
        {
            bestInturns.Add(new Vector2(position.x, position.y));
        }
        inTurnScores++;
        //Debug.Log("Inturns reported: " + inTurnScores);
        if (inTurnScores >= SWARM_SIZE)
        {
            inTurnScores = 0;
            inTurnIteration++;
            if (inTurnIteration <= ITERATIONS){
                NextInteration(inTurnSpace, bestInturn);
                if (inTurnIteration % 2 == 0 && inTurnIteration!=0){
                    Pair<Vector2, float> newBestShot = ComputeBest(bestInturns);
                    bestInturn = newBestShot.Key;
                    inTurnRange = newBestShot.Value;
                }
            }
        }
    }

    public void outTurnUpdate(int score, Vector2 position)
    {
        if (score > maxOutTurnScore)
        {
            maxOutTurnScore = score;
            bestOutturns = new List<Vector2>();
            Vector2 duplicateShot = new Vector2(position.x, position.y);
            bestOutturns.Add(new Vector2(position.x, position.y));
            bestOutturn = duplicateShot;
            outTurnRange = 0.0f;
        } else if (score == maxOutTurnScore)
        {
            bestOutturns.Add(new Vector2(position.x, position.y));
        }
        outTurnScores++;
        if (outTurnScores >= SWARM_SIZE)
        {
            outTurnScores = 0;
            outTurnIteration++;
            if (outTurnIteration <= ITERATIONS)
            {
                NextInteration(outTurnSpace, bestOutturn);
                if (outTurnIteration % 2 == 0 && outTurnIteration!=0){
                    Pair<Vector2, float> newBestShot = ComputeBest(bestOutturns);
                    bestOutturn = newBestShot.Key;
                    outTurnRange = newBestShot.Value;
                }
            }
        }
    }

    public void NextInteration(List<SheetBehaviour> sheets, Vector2 bestPosition)
    {
        foreach(SheetBehaviour sb in sheets)
        {
            sb.UpdateVelocity(bestPosition);
            sb.Compute();
        }

        if (maxInTurnScore > maxOutTurnScore || (maxInTurnScore == maxOutTurnScore && inTurnRange >= outTurnRange))
        {
            showcaseSheet.initialSpeed = bestInturn.x;
            showcaseSheet.initialAngle = bestInturn.y;
            showcaseSheet.turn = true;
        } else {
            showcaseSheet.initialSpeed = bestOutturn.x;
            showcaseSheet.initialAngle = bestOutturn.y;
            showcaseSheet.turn = false;
        }
    }

    public Pair<Vector2, float> ComputeBest(List<Vector2> shotList)
    {
        if (shotList.Count == 0) {return null;}

        List<Pair<Vector2, int>> angleSorted = new List<Pair<Vector2, int>>();

        foreach(Vector2 s in shotList)
        {
            Pair<Vector2, int> shot = new Pair<Vector2, int>(s, -1);
            angleSorted.Add(shot);
        }

        angleSorted.Sort(new AngleComparer());

        List<List<Pair<Vector2, int>>> shots = new List<List<Pair<Vector2, int>>>();
        int shotCounter = 0;

        for (int i = 0; i < angleSorted.Count; i++)
        {
            if (angleSorted[i].Value == -1) {
                angleSorted[i].Value = shotCounter;
                shotCounter++;
                List<Pair<Vector2, int>> newShot = new List<Pair<Vector2, int>>();
                newShot.Add(angleSorted[i]);
                shots.Add(newShot);
            }
            for (int j = i + 1; j < angleSorted.Count; j++)
            {
                if (angleSorted[j].Key.y - angleSorted[i].Key.y <= 0.1) {
                    if (Math.Abs(angleSorted[j].Key.x - angleSorted[i].Key.x) <= 0.015) {
                        if (angleSorted[j].Value == -1) {
                            angleSorted[j].Value = angleSorted[i].Value;
                            shots[angleSorted[i].Value].Add(angleSorted[j]);
                        } else if (angleSorted[j].Value != angleSorted[i].Value){
                            List<Pair<Vector2, int>> oShot = shots[angleSorted[j].Value];
                            List<Pair<Vector2, int>> nShot = shots[angleSorted[i].Value];

                            foreach(Pair<Vector2, int> shot in nShot)
                            {
                                shot.Value = angleSorted[j].Value;
                                oShot.Add(shot);
                            }
                            nShot.RemoveAll(item => true);
                        }
                    }
                } else {
                    break;
                }
            }
        }

        float maxRange = 0.0f;
        List<Pair<Vector2, int>> bestShot = shots[0];

        foreach(List<Pair<Vector2, int>> sameShot in shots)
        {
            if (sameShot.Count > 0) {
                float minAngle = sameShot[0].Key.y;
                float maxAngle = sameShot[0].Key.y;
                foreach(Pair<Vector2, int> shot in sameShot)
                {
                    if (shot.Key.y < minAngle) {
                        minAngle = shot.Key.y;
                    }
                    if (shot.Key.y > maxAngle) {
                        maxAngle = shot.Key.y;
                    }
                }
                if (maxAngle - minAngle > maxRange) {
                    maxRange = maxAngle - minAngle;
                    bestShot = sameShot;
                }
            }
        }

        float totalAngle = 0.0f;
        float totalSpeed = 0.0f;

        foreach(Pair<Vector2, int> shot in bestShot)
        {
            totalAngle+=shot.Key.y;
            totalSpeed+=shot.Key.x;
        }

        Vector2 averageCentre = new Vector2(totalSpeed/(float)bestShot.Count, totalAngle/(float)bestShot.Count);

        float minDist = Vector2.Distance(averageCentre, bestShot[0].Key);

        Vector2 middleShot = bestShot[0].Key;

        foreach(Pair<Vector2, int> shot in bestShot)
        {
            float dist = Vector2.Distance(averageCentre, shot.Key);
            if (dist < minDist) {
                minDist = dist;
                middleShot = shot.Key;
            }
        }

        return new Pair<Vector2, float>(middleShot, maxRange);
    }
}

class AngleComparer : IComparer<Pair<Vector2, int>>
{
    public int Compare(Pair<Vector2, int> x, Pair<Vector2, int> y)
    {
        return x.Key.y.CompareTo(y.Key.y);
    }
}

public class Pair<T, V>
{
    public T Key {get; set;}
    public V Value {get; set;}

    public Pair(T key, V val)
    {
        this.Key = key;
        this.Value = val;
    }
}
