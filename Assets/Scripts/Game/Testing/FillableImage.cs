using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FillableImage : MonoBehaviour
{
	[SerializeField, ReadOnly]
	private Image m_FillableImage = null;

	private void Awake()
	{
		enabled = Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_FillableImage));
		m_FillableImage.fillAmount = 0f;
	}

	public void Set01FilledValue(in float In01Value)
	{
		if (enabled)
		{
			m_FillableImage.fillAmount = Mathf.Clamp01(In01Value);
		}
	}
}
