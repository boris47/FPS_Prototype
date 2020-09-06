using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent( typeof( Renderer ) )]
public class OutlineWithCommandBuffer : BaseHighlighter {


	private			OutlineEffectManager.OutlineData	m_OutlineData	= new OutlineEffectManager.OutlineData();

	[SerializeField]
	protected		OutlineEffectManager.SortingType	sortingType		= OutlineEffectManager.SortingType.Overlay;

	[SerializeField, ReadOnly]
	protected		uint								m_ID			= 0;

	protected		Renderer[]							m_Renderers		= null;

	
	//
	public override bool Highlight( Color? color = null )
	{
		if (this.m_IsActive == false )
		{
			this.m_OutlineData.color = color ?? this.m_OutlineData.color;
			OutlineEffectManager.Instance?.AddRenderers
			(
				renderers: this.m_Renderers,
				outlineData: this.m_OutlineData,
				newID:			ref this.m_ID
			);
			this.m_IsActive = true;
		}
		return this.m_IsActive;
	}


	//
	public override bool UnHighlight()
	{
		if (this.m_IsActive == true )
		{
			OutlineEffectManager.Instance?.RemoveRenderers(this.m_ID );
			this.m_IsActive = false;
		}
		return !this.m_IsActive;
	}


	//
	protected override void Awake()
	{
		this.m_Renderers = this.GetComponentsInChildren<Renderer>( includeInactive: true );
	}

	private void OnValidate()
	{
		if (this.m_Renderers?.Length > 0 && this.m_IsActive )
		{
			this.m_OutlineData.color = this.m_ColorToUse;
			this.m_OutlineData.sortingType = this.sortingType;

			OutlineEffectManager.Instance?.UpdateRenderers(this.m_ID, this.m_OutlineData );
		}
	}

	//
	protected override void OnEnable()
	{
		this.m_OutlineData.color = this.m_ColorToUse;
		this.m_OutlineData.sortingType = this.sortingType;
	}

	//
	protected override void OnDisable()
	{
		this.UnHighlight();
	}


	//
	protected override void OnDestroy()
	{
		
	}
	
}