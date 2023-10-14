using Battlehub.RTCommon;
using TMPro;
using UnityEngine;
using Battlehub;

public class LocalizedDropdown : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown m_dd;

    private ILocalization m_localization;

    [SerializeField]
    private bool m_localizeOnAwake = false;

    [SerializeField]
    private bool m_localizeOnStart = true;

    private void Awake()
    {
        if(m_localizeOnAwake)
        {
            Localize();
        }
    }

    private void Start()
    {
        if(m_localizeOnStart)
        {
            Localize();
        }
    }

    private void Localize()
    {
        if (m_dd == null)
        {
            m_dd = GetComponentInChildren<TMP_Dropdown>();
        }

        if (m_dd != null)
        {  
            m_localization = IOC.Resolve<ILocalization>();
            if (m_localization != null)
            {
                foreach (TMP_Dropdown.OptionData option in m_dd.options)
                {
                        option.text = m_localization.GetString(option.text, null);
                }
            }
        }
    }
}
