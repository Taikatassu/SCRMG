using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchData
{
    public MatchData(int newMatchID, int newGameMode)
    {
        matchID = newMatchID;
        Debug.Log("Creating new matchData (ID: " + newMatchID + ") on " + DateTime.Now.ToLongDateString() 
            + ", at " + DateTime.Now.ToLongTimeString());
        startDate = DateTime.Now.ToLongDateString();
        startTime = DateTime.Now.ToLongTimeString();
        matchDuration = -1;
        gameMode = newGameMode;

        powerUpsPickedUpOverall = 0;
        powerUpPlatformOneUsed = 0;
        powerUpPlatformTwoUsed = 0;
        powerUpPlatformThreeUsed = 0;
        powerUpPlatformFourUsed = 0;
    }

    public int matchID = -1;
    public string startDate = "NA";
    public string startTime = "NA";
    public float matchDuration = -1;
    public int gameMode = -1;
    public bool humanPlayerWon = false;

    public List<bool> playersAndIfHumanControlled = new List<bool>();

    public int powerUpsPickedUpOverall = -1;
    public int powerUpPlatformOneUsed = -1;
    public int powerUpPlatformTwoUsed = -1;
    public int powerUpPlatformThreeUsed = -1;
    public int powerUpPlatformFourUsed = -1;

}
