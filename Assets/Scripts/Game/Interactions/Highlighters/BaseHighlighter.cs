using UnityEngine;


[RequireComponent( typeof( Renderer ) )]
public abstract class BaseHighlighter : MonoBehaviour {

	[SerializeField]
	protected				Color		m_ColorToUse	= Color.red;

	[SerializeField, ReadOnly]
	protected				bool		m_IsActive		= false;

	public					bool		IsActive => m_IsActive;


	public		abstract	bool		Highlight( Color? color = null );

	public		abstract	bool		UnHighlight();

	protected	abstract	void		Awake();

	protected	abstract	void		OnEnable();

	protected	abstract	void		OnDisable();

	protected	abstract	void		OnDestroy();
	
}