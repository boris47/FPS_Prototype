
using UnityEngine;

namespace Localization
{
	public sealed class LocalizedTextLegacy : LocalizedBase
	{
		[SerializeField]
		private UnityEngine.UI.Text m_TextComponent = null;


		//////////////////////////////////////////////////////////////////////////
		private void Awake()
		{
			Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_TextComponent));
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnValidate()
		{
			if (m_TextComponent == null)
			{
				gameObject.TryGetComponent(out m_TextComponent);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void UpdateValue(in LocalizationValue InNewValue)
		{
			if (m_TextComponent.IsNotNull())
			{
				m_TextComponent.text = InNewValue.Text;
			}
		}
	}
}

