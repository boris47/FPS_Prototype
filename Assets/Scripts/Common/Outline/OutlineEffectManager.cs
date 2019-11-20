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

	private	const	string	OUTLINE_SHADER_PATH = "Shaders/Outline/OutlineShader";

	private static readonly List<BaseHighlighter> outlines = new List<BaseHighlighter>();

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
	private     List<List<Renderer>>                    m_ObjectExcluders       = new List<List<Renderer>>();

	private     Material                                m_HighlightMaterial     = null;
	private     Material                                m_BlurMaterial          = null;
	private     static CommandBuffer					m_CommandBuffer         = null;



	private static  uint id = 0;
	private static uint NewId() => id++;

	public void AddRenderers( Renderer[] renderers, OutlineData outlineData, ref uint newID )
	{
		CustomOutlineData customOutlineData = new CustomOutlineData() {
			renderers = renderers,
			outlineData = outlineData
		};
		
		newID = NewId();
		m_ObjectRenderers.Add( newID, customOutlineData );
		RecreateCommandBuffer();
	}

	public void RemoveRenderers( uint id )
	{
		m_ObjectRenderers.Remove( id );
		RecreateCommandBuffer();
	}


	public void UpdateRenderers( uint id, OutlineData outlineData )
	{
		m_ObjectRenderers[id].outlineData = outlineData;
		RecreateCommandBuffer();
	}

	private void AddExcluders( List<Renderer> renderers )
	{
		m_ObjectExcluders.Add( renderers );
		RecreateCommandBuffer();
	}

	private void RemoveExcluders( List<Renderer> renderers )
	{
		m_ObjectExcluders.Remove( renderers );
		RecreateCommandBuffer();
	}

	public void ClearOutlineData()
	{
		m_ObjectRenderers.Clear();
		m_ObjectExcluders.Clear();
		m_CommandBuffer.Clear();
	}


	private static     Camera                                  m_Camera                = null;

	static OutlineEffectManager()
	{
		UnityEngine.SceneManagement.SceneManager.sceneLoaded += delegate( UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1 )
		{
			SetEffectCamera( Camera.main );
		};
	}

	//
	private void Awake()
	{
		m_ObjectRenderers = new Dictionary<uint, CustomOutlineData>();
		m_ObjectExcluders = new List<List<Renderer>>();

		m_CommandBuffer = new CommandBuffer();
		m_CommandBuffer.name = "HighlightFX Command Buffer";

		m_HighlightRTID		= Shader.PropertyToID( "_HighlightRT" );
		m_BlurredRTID		= Shader.PropertyToID( "_BlurredRT" );
		m_TemporaryRTID		= Shader.PropertyToID( "_TemporaryRT" );

		m_HighlightMaterial	= new Material( Shader.Find( "Custom/Highlight" ) );
		m_BlurMaterial		= new Material( Shader.Find( "Hidden/FastBlur" ) );

		m_CurrentResolutionX = Screen.width;
		m_CurrentResolutionY = Screen.height;
		m_RTWidth  = ( int ) ( m_CurrentResolutionX / ( float ) m_Resolution );
		m_RTHeight = ( int ) ( m_CurrentResolutionY / ( float ) m_Resolution );
	}


	//
	public	static void	SetEffectCamera( Camera camera )
	{
		if ( camera )
		{
			camera.depthTextureMode = DepthTextureMode.Depth;
			camera.AddCommandBuffer( CameraEvent.BeforeImageEffects, m_CommandBuffer );
		}
		m_Camera = camera;
	}


	private void Update()
	{
		if ( Screen.width != m_CurrentResolutionX || Screen.height != m_CurrentResolutionY )
		{
			m_CurrentResolutionX = Screen.width;
			m_CurrentResolutionY = Screen.height;
			m_RTWidth  = ( int ) ( m_CurrentResolutionX / ( float ) m_Resolution );
			m_RTHeight = ( int ) ( m_CurrentResolutionY / ( float ) m_Resolution );
			RecreateCommandBuffer();
		}
	}

	private void OnPreRender()
	{
		if ( Screen.width != m_CurrentResolutionX || Screen.height != m_CurrentResolutionY )
		{
			m_CurrentResolutionX = Screen.width;
			m_CurrentResolutionY = Screen.height;
			m_RTWidth  = ( int ) ( m_CurrentResolutionX / ( float ) m_Resolution );
			m_RTHeight = ( int ) ( m_CurrentResolutionY / ( float ) m_Resolution );
			RecreateCommandBuffer();
		}
	}

	//
	private void RecreateCommandBuffer()
	{
		m_CommandBuffer.Clear();

		if ( m_ObjectRenderers.Count == 0 || m_Camera == null )
			return;

		// Initialization

		m_CommandBuffer.GetTemporaryRT( m_HighlightRTID, m_RTWidth, m_RTHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32 );
		m_CommandBuffer.SetRenderTarget( m_HighlightRTID, BuiltinRenderTextureType.CurrentActive );
		m_CommandBuffer.ClearRenderTarget( false, true, Color.clear );

		// Rendering into texture
		foreach ( KeyValuePair<uint, CustomOutlineData> superCollection in m_ObjectRenderers )
		{
			CustomOutlineData customOutlineData = superCollection.Value;
			m_CommandBuffer.SetGlobalColor( "_Color", customOutlineData.outlineData.color );

			int pass = ( int ) customOutlineData.outlineData.sortingType;
			foreach ( Renderer render in customOutlineData.renderers )
			{
				m_CommandBuffer.DrawRenderer( render, m_HighlightMaterial, 0, pass );
			}
		}

		// Excluding from texture 
		m_CommandBuffer.SetGlobalColor( "_Color", Color.clear );
		foreach ( List<Renderer> collection in m_ObjectExcluders )
		{
			foreach ( Renderer render in collection )
			{
				m_CommandBuffer.DrawRenderer( render, m_HighlightMaterial, 0, ( int ) SortingType.Overlay );
			}
		}

		// Bluring texture
		float widthMod = 1.0f / ( 1.0f * ( 1 << m_Downsample ) );

		int rtW = m_RTWidth >> m_Downsample;
		int rtH = m_RTHeight >> m_Downsample;

		m_CommandBuffer.GetTemporaryRT( m_BlurredRTID, rtW, rtH, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32 );
		m_CommandBuffer.GetTemporaryRT( m_TemporaryRTID, rtW, rtH, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32 );

		m_CommandBuffer.Blit( m_HighlightRTID, m_TemporaryRTID, m_BlurMaterial, 0 );

		int passOffs = m_BlurType == BlurType.StandardGauss ? 0 : 2;

		for ( int i = 0; i < m_BlurIterations; i++ )
		{
			float iterationOffs = ( i * 1.0f );
			float blurHorizParam = m_BlurSize * widthMod + iterationOffs;
			float blurVertParam = -m_BlurSize * widthMod - iterationOffs;

			m_CommandBuffer.SetGlobalVector( "_Parameter", new Vector4( blurHorizParam, blurVertParam ) );

			m_CommandBuffer.Blit( m_TemporaryRTID, m_BlurredRTID, m_BlurMaterial, 1 + passOffs );
			m_CommandBuffer.Blit( m_BlurredRTID, m_TemporaryRTID, m_BlurMaterial, 2 + passOffs );
		}

		// Occlusion
		if ( m_FillType == FillType.Outline )
		{
			// Excluding the original image from the blurred image, leaving out the areal alone
			m_CommandBuffer.SetGlobalTexture( "_SecondaryTex", m_HighlightRTID );
			m_CommandBuffer.Blit( m_TemporaryRTID, m_BlurredRTID, m_HighlightMaterial, 2 );

			m_CommandBuffer.SetGlobalTexture( "_SecondaryTex", m_BlurredRTID );
		}
		else
		{
			m_CommandBuffer.SetGlobalTexture( "_SecondaryTex", m_TemporaryRTID );
		}

		// Back buffer
		m_CommandBuffer.Blit( BuiltinRenderTextureType.CameraTarget, m_HighlightRTID );

		// Overlay
		m_CommandBuffer.SetGlobalFloat( "_ControlValue", m_ControlValue );
		m_CommandBuffer.Blit( m_HighlightRTID, BuiltinRenderTextureType.CameraTarget, m_HighlightMaterial, ( int ) m_SelectionType );

		m_CommandBuffer.ReleaseTemporaryRT( m_TemporaryRTID );
		m_CommandBuffer.ReleaseTemporaryRT( m_BlurredRTID );
		m_CommandBuffer.ReleaseTemporaryRT( m_HighlightRTID );
	}

}

