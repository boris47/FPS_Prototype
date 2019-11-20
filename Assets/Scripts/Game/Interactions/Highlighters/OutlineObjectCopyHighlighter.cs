


using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
/*
public class OutlineObjectCopyHighlighter : BaseHighlighter {

	public float thickness = 1f;

	[Tooltip("If the mesh has multiple sub-meshes to highlight then this should be checked, otherwise only the first mesh will be highlighted.")]
	public bool enableSubmeshHighlight = false;

	protected Material stencilOutline = null;
	protected Renderer highlightRender = null;

	protected readonly System.Type[] copyComponents = { typeof(MeshFilter), typeof(MeshRenderer) };
	

	//
	protected override void Awake()
	{
		if ( stencilOutline == null )
		{
			stencilOutline = Instantiate( ( Material ) Resources.Load( "Materials/OutlineBasic" ) );
		}

		highlightRender = CreateHighlightModel();
		Highlight();
	}



	//
	protected override void OnDestroy()
	{
		Destroy( highlightRender.gameObject );
		Destroy( stencilOutline );
	}



	//
	public override bool Highlight( Color? color = null )
	{
		m_ColorToUse = color ?? m_ColorToUse;

		if ( stencilOutline && highlightRender )
		{
			stencilOutline.SetFloat( "_Thickness", thickness );
			stencilOutline.SetColor( "_OutlineColor", color ?? m_ColorToUse );

			highlightRender.gameObject.SetActive( true );
			highlightRender.material = stencilOutline;
		}
		return true;
	}



	//
	private void OnRenderImage( RenderTexture source, RenderTexture destination )
	{
		if ( highlightRender.gameObject.activeSelf )
		{
			stencilOutline.SetFloat( "_Thickness", thickness );
			stencilOutline.SetColor( "_OutlineColor", m_ColorToUse );
		}
	}


	//
	public override bool UnHighlight()
	{
		highlightRender.gameObject.SetActive( false );
		return true;
	}



	//
	protected Renderer CreateHighlightModel()
	{
		GameObject copyModel = gameObject.GetComponentInChildren<Renderer>()?.gameObject;

		Renderer returnHighlightModel = null;

		if ( copyModel )
		{

			GameObject highlightModel = new GameObject( gameObject.name + "_HighlightModel" );
			highlightModel.transform.SetParent( copyModel.transform.parent, false );
			highlightModel.transform.localPosition = copyModel.transform.localPosition;
			highlightModel.transform.localRotation = copyModel.transform.localRotation;
			highlightModel.transform.localScale = copyModel.transform.localScale;
			highlightModel.transform.SetParent( gameObject.transform );

			Utils.Base.Clone( ref copyModel, ref highlightModel, copyProperties: false, copyComponents: copyComponents );

			MeshFilter copyMesh = copyModel.GetComponent<MeshFilter>();
			MeshFilter highlightMesh = highlightModel.GetComponent<MeshFilter>();
			returnHighlightModel = highlightModel.GetComponent<Renderer>();
			if ( highlightMesh )
			{
				if ( enableSubmeshHighlight )
				{
					HashSet<CombineInstance> combine = new HashSet<CombineInstance>();
					for ( int i = 0; i < copyMesh.mesh.subMeshCount; i++ )
					{
						CombineInstance ci = new CombineInstance();
						ci.mesh = copyMesh.mesh;
						ci.subMeshIndex = i;
						ci.transform = copyMesh.transform.localToWorldMatrix;
						combine.Add( ci );
					}

					highlightMesh.mesh = new Mesh();
					highlightMesh.mesh.CombineMeshes( combine.ToArray(), true, false );
				}
				else
				{
					highlightMesh.mesh = copyMesh.mesh;
				}
				returnHighlightModel.material = stencilOutline;
				returnHighlightModel.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			}
			highlightModel.SetActive( false );

		}
		return returnHighlightModel;
		
	}

}

	*/