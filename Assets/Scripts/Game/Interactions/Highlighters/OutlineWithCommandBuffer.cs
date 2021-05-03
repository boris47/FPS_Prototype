using UnityEngine;


[RequireComponent( typeof( Renderer ) )]
public class OutlineWithCommandBuffer : BaseHighlighter {


	private			OutlineEffectManager.OutlineData	m_OutlineData	= new OutlineEffectManager.OutlineData();

	[SerializeField]
	protected		OutlineEffectManager.SortingType	sortingType		= OutlineEffectManager.SortingType.Overlay;

	[SerializeField, ReadOnly]
	protected		uint								m_Id			= 0;

	protected		Renderer[]							m_Renderers		= null;

	
	//
	public override bool Highlight( Color? color = null )
	{
		if (m_IsActive == false )
		{
			m_OutlineData.color = color ?? m_OutlineData.color;
			OutlineEffectManager.Instance.AddRenderers
			(
				renderers: m_Renderers,
				outlineData: m_OutlineData,
				newID:			ref m_Id
			);
			m_IsActive = true;
		}
		return m_IsActive;
	}


	//
	public override bool UnHighlight()
	{
		if (m_IsActive == true )
		{
			OutlineEffectManager.Instance.RemoveRenderers(m_Id );
			m_IsActive = false;
		}
		return !m_IsActive;
	}


	//
	protected override void Awake()
	{
		m_Renderers = GetComponentsInChildren<Renderer>( includeInactive: true );
	}

	private void OnValidate()
	{
		if (m_Renderers?.Length > 0 && m_IsActive )
		{
			m_OutlineData.color = m_ColorToUse;
			m_OutlineData.sortingType = sortingType;

			OutlineEffectManager.Instance.UpdateRenderers(m_Id, m_OutlineData );
		}
	}

	//
	protected override void OnEnable()
	{
		m_OutlineData.color = m_ColorToUse;
		m_OutlineData.sortingType = sortingType;
	}

	//
	protected override void OnDisable()
	{
		UnHighlight();
	}


	//
	protected override void OnDestroy()
	{
		
	}
	
}