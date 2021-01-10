/*
//  Copyright (c) 2015 José Guerreiro. All rights reserved.
//
//  MIT license, see http://www.opensource.org/licenses/mit-license.php
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
*/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class OutlineEffectManager : SingletonMonoBehaviour<OutlineEffectManager> {

	public enum HighlightType {
		Glow = 0,
		Solid = 1
	}

	public enum SortingType {
		Overlay = 3,
		DepthFiltered = 4,
	}

	public enum DepthInvertPass {
		StencilMapper = 5,
		StencilDrawer = 6
	}

	public enum FillType {
		Fill,
		Outline
	}
	public enum RTResolution {
		Quarter = 4,
		Half = 2,
		Full = 1
	}

	public enum BlurType {
		StandardGauss = 0,
		SgxGauss = 1,
	}


	public struct OutlineData {
		public Color color;
		public SortingType sortingType;
	}

	[Header("Outline Settings")]

	public HighlightType                                m_SelectionType         = HighlightType.Glow;
	public FillType                                     m_FillType              = FillType.Outline;
	public RTResolution                                 m_Resolution            = RTResolution.Full;

	[Range(0f, 1f)]
	public float                                        m_ControlValue          = 0.5f;

	[Header("BlurOptimized Settings")]

	public BlurType                                     m_BlurType              = BlurType.StandardGauss;
	[Range(0, 2)]
	public int                                          m_Downsample            = 0;
	[Range(0.0f, 10.0f)]
	public float                                        m_BlurSize              = 3.0f;
	[Range(1, 4)]
	public int                                          m_BlurIterations        = 2;


	private     int                                     m_HighlightRTID         = 0;
	private     int                                     m_BlurredRTID           = 0;
	private     int                                     m_TemporaryRTID         = 0;

	private     int                                     m_RTWidth               = 512;
	private     int                                     m_RTHeight              = 512;

	private		int										m_CurrentResolutionX	= 0;
	private		int										m_CurrentResolutionY	= 0;

	private class CustomOutlineData {
		public Renderer[] renderers;
		public OutlineData outlineData;
	}

	private     Dictionary<uint, CustomOutlineData>     m_ObjectRenderers       = new Dictionary<uint, CustomOutlineData>();

	private     Material                                m_HighlightMaterial		= null;
	private     Material                                m_BlurMaterial			= null;


	private     static		CommandBuffer				m_CommandBuffer			= null;
	private		static		Camera						m_Camera				= null;
	private		static		DepthTextureMode			m_PrevDepthTextureMode	= DepthTextureMode.None;
	private		static		uint						id						= 1;
	private		static		uint						NewId() => id++;


	private	static	CommandBuffer commandBuffer
	{
		get {
			if ( m_CommandBuffer == null )
			{
				m_CommandBuffer = new CommandBuffer();
				m_CommandBuffer.name = "HighlightFX Command Buffer";
			}
			return m_CommandBuffer;
		}
	}


	// STATIC CONSTRUCTOR
	static OutlineEffectManager()
	{
		UnityEngine.SceneManagement.SceneManager.sceneLoaded += delegate( UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1 )
		{
			SetEffectCamera( Camera.main );
		};
	}

	
	//////////////////////////////////////////////////////////////////////////
	/// <summary> if given camera is valid is then used for command buffer </summary>
	public	static void	SetEffectCamera( Camera camera )
	{
		if ( camera )
		{
			if ( camera == m_Camera )
				return;

			if ( m_Camera )
			{
				m_Camera.depthTextureMode = m_PrevDepthTextureMode;
				m_Camera.RemoveCommandBuffer( CameraEvent.BeforeImageEffects, commandBuffer );
			}

			m_PrevDepthTextureMode = camera.depthTextureMode;
			camera.depthTextureMode = DepthTextureMode.Depth;
			camera.AddCommandBuffer( CameraEvent.BeforeImageEffects, commandBuffer );
		}
		m_Camera = camera;
	}


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		m_ObjectRenderers = new Dictionary<uint, CustomOutlineData>();

		m_HighlightRTID		= Shader.PropertyToID( "_HighlightRT" );
		m_BlurredRTID		= Shader.PropertyToID( "_BlurredRT" );
		m_TemporaryRTID		= Shader.PropertyToID( "_TemporaryRT" );

		m_HighlightMaterial	= new Material( Shader.Find( "Custom/Highlight" ) );
		m_BlurMaterial		= new Material( Shader.Find( "Hidden/FastBlur" ) );

		m_CurrentResolutionX = Screen.width;
		m_CurrentResolutionY = Screen.height;
		m_RTWidth  = ( int ) (m_CurrentResolutionX / ( float )m_Resolution );
		m_RTHeight = ( int ) (m_CurrentResolutionY / ( float )m_Resolution );
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Add the renderers to use for outline effect and assign to newID the unique id of the outline effect slot.
	/// A newID of value Zero express failure.
	/// 'Color.Clear' color is not accepted
	/// </summary>
	public void AddRenderers( Renderer[] renderers, OutlineData outlineData, ref uint newID )
	{
		if ( renderers == null || outlineData.color == Color.clear )
		{
			newID = 0;
			return;
		}

		CustomOutlineData customOutlineData = new CustomOutlineData() {
			renderers = renderers,
			outlineData = outlineData
		};
		
		newID = NewId();
		m_ObjectRenderers.Add( newID, customOutlineData );
		RecreateCommandBuffer();
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Using the slot id, if registered, remove renderers </summary>
	public void RemoveRenderers( uint id )
	{
		if ( id == 0 )
		{
			return;
		}

		m_ObjectRenderers.Remove( id );
		RecreateCommandBuffer();
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Update the renderers using the given id </summary>
	public	void	UpdateRenderers( uint id, Renderer[] newRenderers )
	{
		if ( id == 0 || newRenderers == null )
			return;

		m_ObjectRenderers[id].renderers = newRenderers;
		RecreateCommandBuffer();
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Update the outline data using the given id </summary>
	public void UpdateRenderers( uint id, OutlineData outlineData )
	{
		if ( id == 0 )
			return;

		m_ObjectRenderers[id].outlineData = outlineData;
		RecreateCommandBuffer();
	}
	

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Clear every renderer from internal collection </summary>
	public void ClearOutlineData()
	{
		m_ObjectRenderers.Clear();
		commandBuffer.Clear();
	}


	/// <summary> OnPreRender is called before a camera starts rendering the scene </summary>
	/// This could be usefull if resolution is changed
	private void OnPreRender()
	{
		if ( Screen.width != m_CurrentResolutionX || Screen.height != m_CurrentResolutionY )
		{
			m_CurrentResolutionX = Screen.width;
			m_CurrentResolutionY = Screen.height;
			m_RTWidth  = ( int ) (m_CurrentResolutionX / ( float )m_Resolution );
			m_RTHeight = ( int ) (m_CurrentResolutionY / ( float )m_Resolution );
			RecreateCommandBuffer();
		}
	}


	/// <summary> This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only)</summary>
	private void OnValidate()
	{
		if ( m_CommandBuffer != null )
		{
			RecreateCommandBuffer();
		}
	}

	//
	private void RecreateCommandBuffer()
	{
		commandBuffer.Clear();

		if (m_ObjectRenderers.Count == 0 || m_Camera == null )
			return;

		// Initialization
		commandBuffer.GetTemporaryRT(m_HighlightRTID, m_RTWidth, m_RTHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32 );
		commandBuffer.SetRenderTarget(m_HighlightRTID, BuiltinRenderTextureType.CurrentActive );
		commandBuffer.ClearRenderTarget( false, true, Color.clear );

		// Rendering into texture
		foreach ( KeyValuePair<uint, CustomOutlineData> superCollection in m_ObjectRenderers )
		{
			CustomOutlineData customOutlineData = superCollection.Value;
			commandBuffer.SetGlobalColor( "_Color", customOutlineData.outlineData.color );

			int pass = ( int ) customOutlineData.outlineData.sortingType;
			foreach ( Renderer render in customOutlineData.renderers )
			{
				commandBuffer.DrawRenderer( render, m_HighlightMaterial, 0, pass );
			}
		}

		// Bluring texture
		float widthMod = 1.0f / ( 1.0f * ( 1 << m_Downsample ) );

		int rtW = m_RTWidth >> m_Downsample;
		int rtH = m_RTHeight >> m_Downsample;

		commandBuffer.GetTemporaryRT(m_BlurredRTID, rtW, rtH, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32 );
		commandBuffer.GetTemporaryRT(m_TemporaryRTID, rtW, rtH, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32 );

		commandBuffer.Blit(m_HighlightRTID, m_TemporaryRTID, m_BlurMaterial, 0 );

		int passOffs = m_BlurType == BlurType.StandardGauss ? 0 : 2;

		for ( int i = 0; i < m_BlurIterations; i++ )
		{
			float iterationOffs = ( i * 1.0f );
			float blurHorizParam = ( m_BlurSize * widthMod ) + iterationOffs;
			float blurVertParam = -( m_BlurSize * widthMod ) - iterationOffs;

			commandBuffer.SetGlobalVector( "_Parameter", new Vector4( blurHorizParam, blurVertParam ) );

			commandBuffer.Blit(m_TemporaryRTID, m_BlurredRTID, m_BlurMaterial, 1 + passOffs );
			commandBuffer.Blit(m_BlurredRTID, m_TemporaryRTID, m_BlurMaterial, 2 + passOffs );
		}

		// Occlusion
		if (m_FillType == FillType.Outline )
		{
			// Excluding the original image from the blurred image, leaving out the areal alone
			commandBuffer.SetGlobalTexture( "_SecondaryTex", m_HighlightRTID );
			commandBuffer.Blit(m_TemporaryRTID, m_BlurredRTID, m_HighlightMaterial, 2 );

			commandBuffer.SetGlobalTexture( "_SecondaryTex", m_BlurredRTID );
		}
		else // FillType.Fill
		{
			commandBuffer.SetGlobalTexture( "_SecondaryTex", m_TemporaryRTID );
		}

		// Back buffer
		commandBuffer.Blit( BuiltinRenderTextureType.CameraTarget, m_HighlightRTID );

		// Overlay
		commandBuffer.SetGlobalFloat( "_ControlValue", m_ControlValue );
		commandBuffer.Blit(m_HighlightRTID, BuiltinRenderTextureType.CameraTarget, m_HighlightMaterial, ( int )m_SelectionType );

		commandBuffer.ReleaseTemporaryRT(m_TemporaryRTID );
		commandBuffer.ReleaseTemporaryRT(m_BlurredRTID );
		commandBuffer.ReleaseTemporaryRT(m_HighlightRTID );
	}

}

