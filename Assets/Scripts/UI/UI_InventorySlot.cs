using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UI_InventorySlot : UI_Base, IPointerClickHandler, IStateDefiner
{
	private				Texture2D			m_Texture							= null;
	private				Image				m_Image								= null;


	private				bool				m_IsSet								= false;
	public				bool				IsSet								=> m_IsSet;

	private				Database.Section	m_ItemSection						= null;
	public				Database.Section	Section								=> m_ItemSection;

	private				bool				m_IsInitialized						= false;
						bool				IStateDefiner.IsInitialized			=> m_IsInitialized;


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.PreInit()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.Initialize()
	{
		if (!m_IsInitialized)
		{
			CustomAssertions.IsTrue(transform.TrySearchComponent<Image>(ESearchContext.LOCAL, out m_Image));

			m_IsInitialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.ReInit()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	bool IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	void IPointerClickHandler.OnPointerClick( PointerEventData eventData )
	{
		if ( eventData.button == PointerEventData.InputButton.Left )
		{
			
		}
		if ( eventData.button == PointerEventData.InputButton.Middle )
		{
			
		}
		if ( eventData.button == PointerEventData.InputButton.Right )
		{
			
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Reset()
	{
		m_Texture = null;
		m_ItemSection = null;
		m_IsSet = false;
	}


	//////////////////////////////////////////////////////////////////////////
	public bool TrySet(Texture2D texture, Database.Section section)
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		if (m_IsInitialized && texture.IsNotNull())
		{
			m_Texture = texture;
			m_ItemSection = section;
			m_IsSet = true;

			m_Image.sprite = Sprite.Create(m_Texture, Rect.MinMaxRect(0, 0, m_Texture.width, m_Texture.height), new Vector2(0.5f, 0.5f));
			return true;
		}
		return false;
	}
}
