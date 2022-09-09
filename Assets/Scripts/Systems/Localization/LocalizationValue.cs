
using System;
using UnityEngine;

namespace Localization
{
	[System.Serializable]
	public sealed class LocalizationValue
	{
		[SerializeField]
		private uint m_KeyId = 0u;

		[SerializeField]
		private string m_Text = string.Empty;

		[SerializeField]
		private AudioClip m_AudioClip = null;

		[SerializeField]
		private Texture m_Texture = null;

		public uint ReferencedKeyId => m_KeyId;

		public string Text => m_Text;
		public AudioClip AudioClip => m_AudioClip;
		public Texture Texture => m_Texture;

		public LocalizationValue(in uint InKeyId)
		{
			m_KeyId = InKeyId;
		}

#if UNITY_EDITOR
		public static class Editor
		{
			/// <summary> Draw the gui for this localization key </summary>
			/// <param name="InAudioSource"> An AudioSource to use to reproduce the sound preview </param>
			/// <returns>Return true if any value has changed in thid draw</returns>
			public static bool DrawGUI(in LocalizationValue InLocalizationValue, in AudioSource InAudioSource)
			{
				bool bIsDirty = false;

				// m_Text
				{
					GUILayout.Label("Text:", GUILayout.Width(40f));
					string newValue = GUILayout.TextArea(InLocalizationValue.m_Text, GUILayout.Width(300f));
					if (InLocalizationValue.m_Text != newValue)
					{
						InLocalizationValue.m_Text = newValue;
						bIsDirty = true;
					}
				}

				// m_AudioClip
				{
					GUILayout.Label("Audio Clip:", GUILayout.Width(70f));
					AudioClip newValue = (AudioClip)UnityEditor.EditorGUILayout.ObjectField(InLocalizationValue.m_AudioClip, typeof(AudioClip), false, GUILayout.Width(150f));
					if (InLocalizationValue.m_AudioClip != newValue)
					{
						InLocalizationValue.m_AudioClip = newValue;
						bIsDirty = true;
					}
					if (InLocalizationValue.m_AudioClip.IsNotNull() && GUILayout.Button("Play"))
					{
						InAudioSource.PlayOneShot(InLocalizationValue.m_AudioClip);
					}
				}
				// m_Texture
				{
					GUILayout.Label("Texture:", GUILayout.Width(50f));
					Texture newValue = (Texture)UnityEditor.EditorGUILayout.ObjectField(InLocalizationValue.m_Texture, typeof(Texture), false, GUILayout.Width(150f));
					if (InLocalizationValue.m_Texture != newValue)
					{
						InLocalizationValue.m_Texture = newValue;
						bIsDirty = true;
					}
					GUILayout.Label(InLocalizationValue.m_Texture, GUILayout.Width(130f), GUILayout.Height(130f));
				}

				return bIsDirty;
			}
		}
#endif
	}
}

