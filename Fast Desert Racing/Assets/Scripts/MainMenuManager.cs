using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    private Animator _animator;
    private bool _started;

    [SerializeField]
    private TMP_Dropdown dropdownLang;
    void Start()
    {
        _animator = GetComponent<Animator>();

        dropdownLang.value = PlayerPrefs.HasKey("Lang") ? PlayerPrefs.GetInt("Lang") : 0;

        dropdownLang.onValueChanged.AddListener((int index) =>
        {
            PlayerPrefs.SetInt("Lang", index);
            LanguageHandler.OnUpdateLanguage?.Invoke();
        });
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
