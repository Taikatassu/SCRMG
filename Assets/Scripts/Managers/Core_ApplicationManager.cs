using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Core_ApplicationManager : MonoBehaviour
{

    public static Core_ApplicationManager instance;

    Core_Toolbox toolbox;
    Core_EventManager em;

	void Awake ()
    {   
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
    }

    private void OnEnable()
    {
        em.OnRequestSceneSingleLevel01 += OnRequestSceneSingleLevel01;
    }

    private void OnDisable()
    {
        em.OnRequestSceneSingleLevel01 -= OnRequestSceneSingleLevel01;
    }


    private void OnRequestSceneSingleLevel01()
    {
        Debug.Log("SceneManager: Received Level01 load request!");
        //Load scene "Level01" in single mode
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }



}
