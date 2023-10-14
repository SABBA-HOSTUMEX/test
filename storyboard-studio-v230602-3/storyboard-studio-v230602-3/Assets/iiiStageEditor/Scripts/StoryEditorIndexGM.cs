using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlehub;
using TMPro;
public class StoryEditorIndexGM : MonoBehaviour
{

    public Localization m_localization;

    // Start is called before the first frame update
    public TextMeshProUGUI lang_go_tw;
    public TextMeshProUGUI lang_go_en;
    void Start()
    {
        SetLangWith(m_localization.Locale);
    }
    public void SetLangWith(string langCode)
    {
        m_localization.Locale = langCode;
        PlayerPrefs.SetString("LocaleSet",langCode);


        if (langCode == "zh-TW")
        {
            lang_go_tw.color = Color.white;
            lang_go_en.color = Color.gray;

        }
        else if (langCode == "en-US")
        {

            lang_go_tw.color = Color.gray;
            lang_go_en.color = Color.white;
        }
        initIndexLang();

    }

    public void initIndexLang()
    {
        TextMeshProUGUI[] texts = GameObject.FindObjectsOfType<TextMeshProUGUI>();
        string _key;
        OriginalText _otext;
        foreach (TextMeshProUGUI textGUI in texts)
        {
            _otext = textGUI.GetComponent<OriginalText>();
            if(_otext != null)
            {

                _key = _otext.text;
            }
            else
            {

                _key = textGUI.text;
                _otext = textGUI.gameObject.AddComponent<OriginalText>();
                _otext.text = _key;
            }

            textGUI.SetText(m_localization.GetString(_key));
           // textGUI.text = m_localization.GetString(_key);
        }
    }
}
