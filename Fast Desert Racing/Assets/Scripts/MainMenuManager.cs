using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    private Animator _animator;
    private bool _started;
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        _animator.SetBool("started", _started);
    }

    public void StartGame()
    {
        _started = true;
    }
}
