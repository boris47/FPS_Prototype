using UnityEngine;
using System.Collections;

public class Laser : WeaponAttachment {

	private		LineRenderer		m_LineRenderer	= null;

	private void Awake()
	{
		m_LineRenderer = GetComponent<LineRenderer>();
	}

	private void Update()
	{
		m_LineRenderer.SetPosition( 0, transform.position );
		m_LineRenderer.SetPosition( 1, transform.position + ( -transform.up ) * 100f );
	}

	private void LateUpdate()
	{
		m_LineRenderer.SetPosition( 0, transform.position );
		m_LineRenderer.SetPosition( 1, transform.position + ( -transform.up ) * 100f );
	}

	private void FixedUpdate()
	{
		m_LineRenderer.SetPosition( 0, transform.position );
		m_LineRenderer.SetPosition( 1, transform.position + ( -transform.up ) * 100f );
	}

}
