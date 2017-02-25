using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Core_Level01Info : MonoBehaviour {

    //TODO: Implement GlobalVariableLibrary and get variables through it

    Core_Toolbox toolbox;
    Core_GameManager gameManager;
    Core_UIManager uiManager;
    List<Transform> respawnPoints = new List<Transform>();
    int numberOfShips = 4;
    int matchBeginTimerLength = 3;
    float waitTimeBeforeStartingMatchBeginTimer = 0.5f;

    //UI Elements
    GameObject currentCanvas;
    Image fullscreenBlackImage;
    Text matchBeginTimerText;

    void Awake()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        gameManager = toolbox.GetComponent<Core_GameManager>();
        uiManager = toolbox.GetComponent<Core_UIManager>();
        currentCanvas = GameObject.FindWithTag("Canvas");
        fullscreenBlackImage = currentCanvas.transform.
            FindChild("FullscreenBlackImage").GetComponent<Image>();
        fullscreenBlackImage.gameObject.SetActive(true);
        matchBeginTimerText = currentCanvas.transform.
            FindChild("MatchBeginTimerText").GetComponent<Text>();
        matchBeginTimerText.gameObject.SetActive(false);

        Transform respawnPointHolder = transform.
            GetComponentInChildren<Core_RespawnPointHolderTag>().transform;
        foreach (Transform child in respawnPointHolder)
        {
            respawnPoints.Add(child);
        }

        gameManager.SetRespawnPoints(respawnPoints);
        gameManager.SetShipCount(numberOfShips);

        uiManager.SetCurrentCanvas(currentCanvas);
        uiManager.SetFullscreenBlackImage(fullscreenBlackImage);
        uiManager.SetMatchBeginTimerText(matchBeginTimerText);

        gameManager.InitializeGame();
        StartCoroutine(WaitForSceneLoadedAndStartMatchTimer(waitTimeBeforeStartingMatchBeginTimer,
            matchBeginTimerLength));
    }

    //void Update()
    //{
    //    LayerMask mouseRayCollisionLayer = LayerMask.NameToLayer("MouseRayCollider");
    //    Debug.Log("mouseCollisionLayer index: " + mouseRayCollisionLayer.value);
    //    RaycastHit hit;
    //    Debug.DrawRay(new Vector3(0, 20, 0), -Vector3.up * 40, Color.red);
    //    if (Physics.Raycast(new Vector3(0, 20, 0), -Vector3.up, out hit, 40, mouseRayCollisionLayer))
    //    {
    //        Debug.Log("hit.point" + hit.point + " collider.layer: " + hit.collider.gameObject.layer);
    //    }
    //}

    IEnumerator WaitForSceneLoadedAndStartMatchTimer(float waitTime, int newMatchBeginTimerLength)
    {
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Level01Info: gameManager.StartMatchBeginTimer");
        gameManager.StartMatchBeginTimer(newMatchBeginTimerLength);
    }
}
