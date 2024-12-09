using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NukeHandle : MonoBehaviour
{
    [SerializeField]
    private GameObject nukeScene;

    [SerializeField]
    private Transform nukeSpawnTransform;

    [SerializeField]
    private GameObject nukeObject;

    public void StartNuke()
    {
        nukeScene.SetActive(true);
        RenderSettings.fogDensity = 0.001f;

        Instantiate(nukeObject, nukeSpawnTransform);

        Nuke.OnExplode += () =>
        {

        };
    }
}
