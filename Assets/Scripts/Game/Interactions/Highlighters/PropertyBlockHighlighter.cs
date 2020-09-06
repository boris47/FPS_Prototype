
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PropertyBlockHighlighter : BaseHighlighter {

	protected	Dictionary<string, MaterialPropertyBlock> m_OriginalMaterialPropertyBlocks = new Dictionary<string, MaterialPropertyBlock>();

	protected	Dictionary<string, MaterialPropertyBlock> m_HighlightMaterialPropertyBlocks = new Dictionary<string, MaterialPropertyBlock>();

	protected	Renderer[]		m_Renderers = null;


	//
	public override bool Highlight( Color? color = null )
	{
		if (this.m_IsActive == false )
		{
			Color colorToUse = color ?? this.m_ColorToUse;
			for ( int i = 0; i < this.m_Renderers.Length; i++ )
			{
				Renderer renderer = this.m_Renderers[i];
				string objectReference = renderer.gameObject.GetInstanceID().ToString();

				if (this.m_HighlightMaterialPropertyBlocks.TryGetValue(objectReference, out MaterialPropertyBlock highlightMaterialPropertyBlock))
				{
					renderer.GetPropertyBlock(highlightMaterialPropertyBlock);
					highlightMaterialPropertyBlock.SetColor("_Color", colorToUse);
					highlightMaterialPropertyBlock.SetColor("_EmissionColor", colorToUse);
					renderer.SetPropertyBlock(highlightMaterialPropertyBlock);
				}
			}
			return true;
		
		}
		return this.m_IsActive;
	}

	
	//
	public override bool UnHighlight()
	{
		if (this.m_IsActive == true )
		{
			for ( int i = 0; i < this.m_Renderers.Length; i++ )
			{
				Renderer renderer = this.m_Renderers[i];
				string objectReference = renderer.gameObject.GetInstanceID().ToString();

				if (this.m_OriginalMaterialPropertyBlocks.TryGetValue(objectReference, out MaterialPropertyBlock storedPropertyBlock))
				{
					renderer.SetPropertyBlock(storedPropertyBlock);
				}
			}
		}
		return !this.m_IsActive;
	}


	//
	protected override void Awake()
	{
		this.m_OriginalMaterialPropertyBlocks.Clear();
		this.m_HighlightMaterialPropertyBlocks.Clear();

		this.m_Renderers = this.gameObject.GetComponentsInChildren<Renderer>( includeInactive: true );
		for ( int i = 0; i < this.m_Renderers.Length; i++ )
		{
			Renderer renderer = this.m_Renderers[i];
			string objectReference = renderer.gameObject.GetInstanceID().ToString();

			// get the initial material property block to restore the original material properties later on
			MaterialPropertyBlock originalPropertyBlock = new MaterialPropertyBlock();
			renderer.GetPropertyBlock( originalPropertyBlock );
			this.m_OriginalMaterialPropertyBlocks.Add( objectReference, originalPropertyBlock );
			
			// we need a second instance of the original material property block which will be modified for highlighting
			MaterialPropertyBlock highlightPropertyBlock = new MaterialPropertyBlock();
			renderer.GetPropertyBlock( highlightPropertyBlock );
			this.m_HighlightMaterialPropertyBlocks.Add( objectReference, highlightPropertyBlock );
		}
	}


	//
	protected override void OnEnable()
	{
		
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
