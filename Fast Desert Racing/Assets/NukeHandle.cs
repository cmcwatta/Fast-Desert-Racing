using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class NukeHandle : MonoBehaviour
{
    [SerializeField]
    private GameObject nukeScene;

    [SerializeField]
    private Transform nukeSpawnTransform;

    [SerializeField]
    private GameObject nukeObject;

    [SerializeField]
    private GameObject nukeExplosion;

    [SerializeField]
    private Transform nukeExplosionTransform;

    [SerializeField]
    private Light globalLight;

    private bool exploded;
    public void StartNuke()
    {
        nukeScene.SetActive(true);
        RenderSettings.fogDensity = 0.001f;

        Instantiate(nukeObject, nukeSpawnTransform.position, nukeObject.transform.rotation);

        Nuke.OnExplode += () =>
        {
            if (exploded) return;
            Instantiate(nukeExplosion, nukeExplosionTransform.position, Quaternion.identity);

            StartCoroutine(IncreaseBright());
            exploded = true;
        };
    }

    private IEnumerator IncreaseBright()
    {
        while(true)
        {
            if (globalLight.intensity > 100) break;

            yield return new WaitForSeconds(0.1f);
            globalLight.intensity += 0.5f;
        }
        if (!Application.isEditor) ShowError("Boom! System destroyed. Goodbye, cruel world!", "Critical Error: Nuke!");
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    private void ShowError(string message, string title)
    {
        MessageBox(IntPtr.Zero, message, title, 0x10);
        MessageBox(IntPtr.Zero, "THANKS FOR PLAYING!", "THANKS", 0x10);
        Application.Quit();
    }
}
