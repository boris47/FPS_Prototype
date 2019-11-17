using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent( typeof( Renderer ) )]
public class BaseHighlighter : MonoBehaviour
{

	protected const string OTULINE_SHADER_BUFFER_PATH = "Shaders/Outline/OutlineBufferShader";

	public		static	Material	outlineEraseMaterial		{ get; private set; }
	private		static	Shader		outlineBufferShader			= null;
	
	public		Renderer				Renderer				{ get; private set; }
	public		SkinnedMeshRenderer		SkinnedMeshRenderer		{ get; private set; }
	public		MeshFilter				MeshFilter				{ get; private set; }
    public		Material[]				SharedMaterials			{ get { return Renderer.sharedMaterials; } }

	[Range(1.0f, 6.0f)]
	public		float					lineThickness			= 1.25f;

	public		Color					color					= Color.red;
	public		bool					eraseRenderer			= false;

	private		Material				m_MatToUse				= null;
	private		Material[]				_SharedMaterials		= null;


	public Material MatToUse {

		get {
			return m_MatToUse;
		}
	}



	static protected	Material	CreateMaterial(Color color)
	{
		Material m = new Material(outlineBufferShader);
		m.SetColor("_Color", color);
		m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
		m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
		m.SetInt("_ZWrite", 0);
		m.DisableKeyword("_ALPHATEST_ON");
		m.EnableKeyword("_ALPHABLEND_ON");
		m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
		m.renderQueue = 3000;
		return m;
	}



	private void Awake()
	{
		Renderer = GetComponent<Renderer>();
		SkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
		MeshFilter = GetComponent<MeshFilter>();

		if ( outlineBufferShader == null )
			outlineBufferShader = Resources.Load<Shader>(OTULINE_SHADER_BUFFER_PATH);

		if ( outlineEraseMaterial == null )
			outlineEraseMaterial	= BaseHighlighter.CreateMaterial( Color.clear );

		m_MatToUse = BaseHighlighter.CreateMaterial( color );

		_SharedMaterials = Renderer.sharedMaterials;

	}

	private void OnDestroy()
	{
		outlineBufferShader = null;
	}

	void OnEnable()
	{
		OutlineEffectManager.Instance?.AddOutline( this );
	}

	void OnDisable()
	{
		OutlineEffectManager.Instance?.RemoveOutline( this );
	}




}
