using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Rename class to GameManager??
public class Core_GameManager : MonoBehaviour {

    Core_Toolbox toolbox;
    Core_EventManager em;
    List<Transform> respawnPoints = new List<Transform>();
    List<int> usedSpawnPoints = new List<int>();
    bool resetUsedSpawnPointsList = false;
    int numberOfShips;
    int matchBeginTimer = 0;
    List<Color> shipColorOptions = new List<Color>();
    List<int> usedShipColors = new List<int>();
    bool resetUsedShipColors = false;

    void Awake ()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
        shipColorOptions.Add(Color.magenta);
        shipColorOptions.Add(Color.cyan);
        shipColorOptions.Add(Color.yellow);
        shipColorOptions.Add(Color.green);
        shipColorOptions.Add(Color.black);
        shipColorOptions.Add(Color.white);
    }

    #region InitializeGame
    public void InitializeGame()
    {
        /*TODO: Have server tell level manager how many ships to spawn, and which ship is whose
        *   - Add AIPlayerController or NetworkPlayerController to other ships
        *   - Give other controllers unique indieces so AIPlayerManager / NetworkPlayerManager 
        *       knows which is which
        */
        #region Instantiate ships
        for (int i = 0; i < numberOfShips; i++ )
        {
            Transform spawnPoint = FindAvailableSpawnPoint();
            GameObject newShip = Instantiate(Resources.Load("Ship", typeof(GameObject)),
                spawnPoint.position, spawnPoint.rotation) as GameObject;
            Core_ShipController newShipController = newShip.GetComponent<Core_ShipController>();
            newShipController.GiveIndex(i + 1);
            newShipController.SetShipColor(FindNewShipColor());

            if (i == 0)
            {
                newShip.AddComponent<Core_LocalPlayerController>();
                GameObject playerIndicator = Instantiate(Resources.Load("PlayerIndicator",
                    typeof(GameObject)), newShip.transform.position, Quaternion.identity, 
                    newShip.transform) as GameObject;
            }
        }
        #endregion
    }
    #endregion

    #region SetVariables
    public void SetRespawnPoints(List<Transform> newRespawnPoints)
    {
        Debug.Log("SetRespawnPoints");
        respawnPoints = newRespawnPoints;
    }

    public void SetShipCount(int newNumberOfShips)
    {
        Debug.Log("SetShipCount");
        numberOfShips = newNumberOfShips;
    }
    #endregion

    #region Find available ship color
    private Color FindNewShipColor()
    {
        if (resetUsedShipColors)
        {
            usedShipColors.Clear();
            resetUsedShipColors = false;
        }

        int r = Random.Range(0, shipColorOptions.Count);
        while (usedShipColors.Contains(r))
        {
            Debug.Log("Core_LevelManager: Inside FindNewShipColor while loop");
            r = Random.Range(0, shipColorOptions.Count);
        }
        usedShipColors.Add(r);

        if (usedShipColors.Count == shipColorOptions.Count)
        {
            resetUsedShipColors = true;
        }
        Debug.Log("Available ship color found");
        return shipColorOptions[r];
    }
    #endregion

    #region Find available spawn point
    private Transform FindAvailableSpawnPoint()
    {
        if (resetUsedSpawnPointsList)
        {
            usedSpawnPoints.Clear();
            resetUsedSpawnPointsList = false;
        }

        int r = Random.Range(0, respawnPoints.Count);
        while (usedSpawnPoints.Contains(r))
        {
            Debug.Log("Core_LevelManager: Inside FindInitialSpawnPoint while loop");
            r = Random.Range(0, respawnPoints.Count);
        }
        usedSpawnPoints.Add(r);

        if (usedSpawnPoints.Count == respawnPoints.Count)
        {
            resetUsedSpawnPointsList = true;
        }

        return respawnPoints[r];
    }

    //public Vector3 FindRepawnPoint()
    //{
    //    int i = Random.Range(0, respawnPoints.Count);
    //    return respawnPoints[i].position;
    //}
    #endregion

    #region Match beginning
    public void StartMatchBeginTimer(int timerLenghtInSeconds)
    {
        matchBeginTimer = timerLenghtInSeconds;
        //TODO: Show timer visuals
        StartCoroutine(BroadcastAndDecreaseMatchBeginTimer(timerLenghtInSeconds));
    }

    IEnumerator BroadcastAndDecreaseMatchBeginTimer(int count)
    {
        em.BroadcastMatchBeginTimerValue(matchBeginTimer);
        for (int i = 0; i < count; i++)
        {
            yield return new WaitForSeconds(1);
            matchBeginTimer--;
            em.BroadcastMatchBeginTimerValue(matchBeginTimer);
            //TODO: Update timer visuals
        }
    }
    #endregion

}
