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
[RequireComponent(typeof(Camera))]
public class OutlineEffectManager : MonoBehaviour
{
	private	const	string	OUTLINE_SHADER_PATH = "Shaders/Outline/OutlineShader";

	public static OutlineEffectManager Instance { get; private set; }

	private static readonly List<BaseHighlighter> outlines = new List<BaseHighlighter>();

	[Range(0, 10)]
	public float lineIntensity = .5f;
	[Range(0, 1)]
	public float fillAmount = 0.2f;

	public bool backfaceCulling = true;


	[Header("Advanced settings")]
	public bool scaleWithScreenSize = true;
	[Range(0.1f, .9f)]
	public float alphaCutoff = .5f;
	public Camera sourceCamera;

	[HideInInspector]
	public Camera outlineCamera;
	Shader outlineShader;
	[HideInInspector]
	public Material outlineShaderMaterial;
	[HideInInspector]
	public RenderTexture renderTexture;

	private	CommandBuffer commandBuffer = null;

	List<Material> materialBuffer = new List<Material>();


	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(this);
			throw new System.Exception("you can only have one outline camera in the scene");
		}

		Instance = this;

		sourceCamera = GetComponent<Camera>();
	}

	void Start()
	{
		if(outlineShader == null)
			outlineShader = Resources.Load<Shader>(OUTLINE_SHADER_PATH);
	
		if(outlineShaderMaterial == null)
		{
			outlineShaderMaterial = new Material(outlineShader);
			outlineShaderMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		
		GameObject cameraGameObject = new GameObject("Outline Camera");
		cameraGameObject.transform.parent = sourceCamera.transform;
		outlineCamera = cameraGameObject.AddComponent<Camera>();

		renderTexture = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 16/*depth*/, RenderTextureFormat.Default);
		UpdateOutlineCameraFromSource();

		commandBuffer = new CommandBuffer();
		outlineCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer);

		outlineCamera.targetTexture = renderTexture;
		commandBuffer.SetRenderTarget(renderTexture);
	}


	//
	private bool RenderTheNextFrame = false;
	public void OnPreRender()
	{
		// the first frame during which there are no outlines, we still need to render 
		// to clear out any outlines that were being rendered on the previous frame
		if (outlines.Count == 0)
		{
			if (!RenderTheNextFrame)
				return;

			RenderTheNextFrame = false;
		}
		else
		{
			RenderTheNextFrame = true;
		}

		if ( renderTexture.width != sourceCamera.pixelWidth || renderTexture.height != sourceCamera.pixelHeight )
        {
			Destroy(renderTexture);
		    renderTexture = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 16, RenderTextureFormat.Default);
			commandBuffer.SetRenderTarget(renderTexture);
			outlineCamera.targetTexture = renderTexture;
		}

		commandBuffer.Clear();

		outlineCamera.fieldOfView = sourceCamera.fieldOfView;

		for ( int i = outlines.Count - 1; i >= 0; i-- )
		{
			BaseHighlighter outline = outlines[i];
			if ( outline == null )
			{
				outlines.RemoveAt(i);
				continue;
			}

			LayerMask cameraCullingMask = sourceCamera.cullingMask;
			bool bIsNotCulled = cameraCullingMask == ( cameraCullingMask | (1 << outline.gameObject.layer ) );
			if ( bIsNotCulled )
			{
				UpdateMaterialsPublicProperties( outline );
				for ( int v = 0; v < outline.SharedMaterials.Length; v++ )
				{
					Material MaterialToRender = null;
					if ( outline.SharedMaterials[v]?.mainTexture != null )
					{
						foreach( Material matBuffer in materialBuffer )
						{
							if ( matBuffer.mainTexture == outline.SharedMaterials[v].mainTexture )
							{
								if ( outline.eraseRenderer && matBuffer.color == BaseHighlighter.outlineEraseMaterial.color )
									MaterialToRender = matBuffer;
								else if ( matBuffer.color == outline.color )
									MaterialToRender = matBuffer;
							}
						}

						if ( MaterialToRender == null )
						{
							if( outline.eraseRenderer )
								MaterialToRender = new Material(BaseHighlighter.outlineEraseMaterial);
							else
								MaterialToRender = new Material(outline.MatToUse);

							MaterialToRender.mainTexture = outline.SharedMaterials[v].mainTexture;
							materialBuffer.Add(MaterialToRender);
						}
					}
					else
					{
						MaterialToRender = outline.eraseRenderer ? BaseHighlighter.outlineEraseMaterial : outline.MatToUse;
					}

						
					MaterialToRender.SetInt("_Culling", (int)UnityEngine.Rendering.CullMode.Back);

					commandBuffer.DrawRenderer(outline.Renderer, MaterialToRender, 0, 0);
					MeshFilter meshFilter = outline.MeshFilter;
					if ( meshFilter && meshFilter.sharedMesh )
					{
						for( int j = 1; j < meshFilter.sharedMesh.subMeshCount; j++ )
						{
							commandBuffer.DrawRenderer(outline.Renderer, MaterialToRender, j, 0);
						}
					}

					SkinnedMeshRenderer skinnedMeshRenderer = outline.SkinnedMeshRenderer;
					if ( skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh )
					{
						for( int j = 1; j < skinnedMeshRenderer.sharedMesh.subMeshCount; j++ )
						{
							commandBuffer.DrawRenderer(outline.Renderer, MaterialToRender, j, 0);
						}
					}
				}
			}
		}

		outlineCamera.Render();
	}

	private void OnEnable()
	{
            
	}

	private void OnDisable()
	{
			
	}

	void OnDestroy()
	{
		if(renderTexture != null)
			renderTexture.Release();
		
		foreach(Material m in materialBuffer)
			DestroyImmediate(m);
		materialBuffer.Clear();
		DestroyImmediate(outlineShaderMaterial);
		outlineShader = null;
		outlineShaderMaterial = null;

		outlines.Clear();
	}

	// OnRenderImage is called after all rendering is complete to render image
	private void OnRenderImage( RenderTexture source, RenderTexture destination )
	{
		outlineShaderMaterial.SetTexture("_OutlineSource", renderTexture);

		Graphics.Blit(source, destination, outlineShaderMaterial, 1);
	}

	public void UpdateMaterialsPublicProperties(BaseHighlighter highlighter)
	{
		float scalingFactor = 1;
		if(scaleWithScreenSize)
		{
			// If Screen.height gets bigger, outlines gets thicker
			scalingFactor = Screen.height / 360.0f;
		}

		// If scaling is too small (height less than 360 pixels), make sure you still render the outlines, but render them with 1 thickness
		if(scaleWithScreenSize && scalingFactor < 1)
		{
			outlineShaderMaterial.SetFloat("_LineThicknessX", (1.0f / 1000.0f) * (1.0f / Screen.width) * 1000.0f);
			outlineShaderMaterial.SetFloat("_LineThicknessY", (1.0f / 1000.0f) * (1.0f / Screen.height) * 1000.0f);
		}
		else
		{
			outlineShaderMaterial.SetFloat("_LineThicknessX", scalingFactor * (highlighter.lineThickness / 1000.0f) * (1.0f / Screen.width) * 1000.0f);
			outlineShaderMaterial.SetFloat("_LineThicknessY", scalingFactor * (highlighter.lineThickness / 1000.0f) * (1.0f / Screen.height) * 1000.0f);
		}
		outlineShaderMaterial.SetFloat("_LineIntensity", lineIntensity);
		outlineShaderMaterial.SetFloat("_FillAmount", fillAmount);
		outlineShaderMaterial.SetColor("_LineColor", highlighter.color);

		Shader.SetGlobalFloat("_OutlineAlphaCutoff", alphaCutoff);
	}

	void UpdateOutlineCameraFromSource()
	{
		outlineCamera.CopyFrom(sourceCamera);
		outlineCamera.renderingPath = RenderingPath.Forward;
		outlineCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		outlineCamera.clearFlags = CameraClearFlags.SolidColor;
		outlineCamera.rect = new Rect(0, 0, 1, 1);
		outlineCamera.cullingMask = 0;
		outlineCamera.targetTexture = renderTexture;
		outlineCamera.enabled = false;
#if UNITY_5_6_OR_NEWER
		outlineCamera.allowHDR = false;
#else
		outlineCamera.hdr = false;
#endif
	}

	public void AddOutline(BaseHighlighter outline)
		=> outlines.Add(outline);

	public void RemoveOutline(BaseHighlighter outline)
		=> outlines.Remove(outline);
}

