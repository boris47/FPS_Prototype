
using UnityEngine;

namespace Localization
{
	public sealed class LocalizedAudioClip : LocalizedBase
	{
		[SerializeField]
		private AudioSource m_AudioSourceComponent = null;


		//////////////////////////////////////////////////////////////////////////
		private void Awake()
		{
			Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_AudioSourceComponent));
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnValidate()
		{
			if (m_AudioSourceComponent == null)
			{
				gameObject.TryGetComponent(out m_AudioSourceComponent);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void UpdateValue(in LocalizationValue InNewValue)
		{
			if (m_AudioSourceComponent.IsNotNull())
			{
				m_AudioSourceComponent.Stop();
				m_AudioSourceComponent.clip = InNewValue.AudioClip;
			}
		}
	}
}

