using System;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum LangsEnum
{
    English = 0,
    French = 1,
    German = 2,
    Spanish = 3,
    Chinese = 4,
}

[Serializable]
public class LanguageField
{
    [ReadOnly]
    public LangsEnum Lang;
    public string Text;
}

public class LanguageHandler : MonoBehaviour
{
    public static Action OnUpdateLanguage;
    public static int _langIndex = 0;


    [SerializeField]
    private LanguageField[] languageFields =
   {
        new LanguageField { Lang = LangsEnum.English, Text = "" },
        new LanguageField { Lang = LangsEnum.French, Text = "" },
        new LanguageField { Lang = LangsEnum.German, Text = "" },
        new LanguageField { Lang = LangsEnum.Spanish, Text = "" },
        new LanguageField { Lang = LangsEnum.Chinese, Text = "" }
    };

    void Start()
    {
        OnUpdateLanguage += delegate () { _langIndex = PlayerPrefs.HasKey("Lang") ? PlayerPrefs.GetInt("Lang") : 0; };
        OnUpdateLanguage += HandleUpdateLanguage;
        OnUpdateLanguage?.Invoke();
    }

    void HandleUpdateLanguage()
    {
        LanguageField languageField = languageFields?.FirstOrDefault(x => (int)x.Lang == _langIndex);
        if (languageField != null)
        {
            TMP_Text text = GetComponent<TMP_Text>();
            if (text) text.text = languageField?.Text;
        }
    }

    private void OnDestroy()
    {
        OnUpdateLanguage -= HandleUpdateLanguage;
    }
}
