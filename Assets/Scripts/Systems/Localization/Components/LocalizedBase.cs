
using UnityEngine;

namespace Localization
{
	/// <summary>
	/// nel custom editor di questo componente mostrare le key dome una dropdown
	/// </summary>
	public abstract class LocalizedBase : MonoBehaviour
	{
		[SerializeField]
		private LocalizationKey m_LocalizationKey = null;

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnEnable()
		{
			if (m_LocalizationKey.IsNotNull())
			{
				LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
				if (Utils.CustomAssertions.IsTrue(LocalizationManager.Instance.TryGetLocalized(m_LocalizationKey, out LocalizationValue outValue)))
				{
					UpdateValue(outValue);
				}
			}
			else
			{
				Debug.LogWarning($"LOcaliztion: Object {name} with component {GetType().Name} has no localization key assigned.", this);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnDisable()
		{
			LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnLanguageChanged(SystemLanguage InSystemLanguage)
		{
			if (Utils.CustomAssertions.IsTrue(LocalizationManager.Instance.TryGetLocalized(m_LocalizationKey, out LocalizationValue outValue)))
			{
				UpdateValue(outValue);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected abstract void UpdateValue(in LocalizationValue InNewValue);
	}
}

