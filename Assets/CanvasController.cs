using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    public Text[] inTurnTexts;
    public Text[] outTurnTexts;

    public int totalIterations;
    public int inTurnIterations;
    public int outTurnIterations;

    void LateUpdate()
    {
        foreach(Text t in inTurnTexts)
        {
            t.text = "Inturn Iteration " + inTurnIterations + " of " + totalIterations;
        }

        foreach(Text t in outTurnTexts)
        {
            t.text = "Outturn Iteration " + outTurnIterations + " of " + totalIterations;
        }
    }
}
