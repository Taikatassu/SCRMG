using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level01Info : MonoBehaviour
{

    #region References & Variables
    //References
    Toolbox toolbox;
    GameManager gameManager;
    List<Transform> respawnPoints = new List<Transform>();
    #endregion

    #region Start
    void Start()
    {
        //Find references
        toolbox = FindObjectOfType<Toolbox>();
        gameManager = toolbox.GetComponent<GameManager>();
        Transform respawnPointHolder = transform.GetChild(0);
        foreach (Transform child in respawnPointHolder)
        {
            respawnPoints.Add(child);
        }
        
        gameManager.SetRespawnPoints(respawnPoints);
    }
    #endregion
}
