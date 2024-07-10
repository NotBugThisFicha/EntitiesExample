using System.Collections;
using System.Collections.Generic;
using Unity.Entities.Content;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    private const string remoteUrlRoot = "http://localhost:80/pathToContent/";
    private const string initialContentSet = "all";

    private void Start()
    {
#if ENABLE_CONTENT_DELIVERY
            RuntimeContentSystem.LoadContentCatalog(remoteUrlRoot, Application.persistentDataPath + "/content-cache", initialContentSet);
            ContentDeliveryGlobalState.Initialize(remoteUrlRoot, Application.persistentDataPath + "/content-cache", initialContentSet, 
                s => {
                     if (s >= ContentDeliveryGlobalState.ContentUpdateState.ContentReady)
                     {
                         SceneManager.LoadScene(1);
                     }
                     if(s == ContentDeliveryGlobalState.ContentUpdateState.NoContentAvailable) Debug.Log("wtf");
                });
#else
        SceneManager.LoadScene(1);
#endif
    }
}
