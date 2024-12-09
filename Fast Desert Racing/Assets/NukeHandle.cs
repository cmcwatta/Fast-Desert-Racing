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

    public void StartNuke()
    {
        nukeScene.SetActive(true);
        RenderSettings.fogDensity = 0.001f;

        Instantiate(nukeObject, nukeSpawnTransform.position, nukeObject.transform.rotation);

        Nuke.OnExplode += () =>
        {
            ShowError("Boom! System destroyed. Goodbye, cruel world!", "Critical Error: Nuke!");
        };
    }

    // Import the MessageBox function from user32.dll
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    private void ShowError(string message, string title)
    {
        MessageBox(IntPtr.Zero, message, title, 0x10);
        Application.Quit();
    }
}
