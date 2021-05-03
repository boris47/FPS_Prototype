using UnityEngine.UI;

public sealed class UI_Settings : UI_Base, IStateDefiner
{
	private				Button								m_BindingsButton					= null;
	private				Button								m_GraphicsButton					= null;
	private				Button								m_AudioButton						= null;
	private				Button								m_BackButton						= null;
	private				bool								m_IsInitialized						= false;
						bool								IStateDefiner.IsInitialized			=> m_IsInitialized;

	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.PreInit()
	{

	}

	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.Initialize()
	{
		if (!m_IsInitialized)
		{
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_Bindings", out m_BindingsButton)))
			{
				m_BindingsButton.onClick.AddListener(() => UIManager.Instance.GoToSubMenu(UIManager.Bindings));
			}

			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_Graphics", out m_GraphicsButton)))
			{
				m_GraphicsButton.onClick.AddListener(() => UIManager.Instance.GoToSubMenu(UIManager.Graphics));
			}

			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_Audio", out m_AudioButton)))
			{
				m_AudioButton.onClick.AddListener(() => UIManager.Instance.GoToSubMenu(UIManager.Audio));
			}

			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_Back", out m_BackButton)))
			{
				m_BackButton.onClick.AddListener(() => UIManager.Instance.GoBack());
			}

			m_IsInitialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	void	IStateDefiner.ReInit()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	bool	 IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}
}
