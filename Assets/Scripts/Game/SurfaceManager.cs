// Ref: https://www.assetstore.unity3d.com/en/#!/content/47967

using System.Linq;
using UnityEngine;


public sealed class SurfaceManager : InGameSingleton<SurfaceManager>
{
	[System.Serializable]
	public struct SurfaceDefinition
	{
		public string name;
		public AudioClip[] footsteps;
		public Material decal;
	}

	[System.Serializable]
	public struct RegisteredTexture
	{
		public Texture texture;
		public int surfaceIndex;
	}

	[SerializeField]
	private		RegisteredTexture[]			m_RegisteredTextures	= null;
	[SerializeField]
	private		SurfaceDefinition[]			m_DefinedSurfaces		= null;


	//////////////////////////////////////////////////////////////////////////
	public		bool	TryGetFootstep( out AudioClip footStepClip, in Collider collider, in Ray ray )
	{
		footStepClip = default;
		if (TryGetSurfaceIndex(collider, ray, out int surfaceIndex))
		{
			AudioClip[] footsteps = m_DefinedSurfaces[surfaceIndex].footsteps;
			footStepClip = footsteps[ Random.Range( 0, footsteps.Length ) ];
			return true;
		}
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool	TryGetDecal( out Material decal, in Collider collider, in Ray ray )
	{
		decal = default;
		if (TryGetSurfaceIndex(collider, ray, out int surfaceIndex))
		{
			decal = m_DefinedSurfaces[surfaceIndex].decal;
			return true;
		}
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	public		string[]	GetAllSurfaceNames()
	{
		return m_DefinedSurfaces.Select((SurfaceDefinition surface) => (!string.IsNullOrEmpty(surface.name))?surface.name:"None" ).ToArray();
	}


	//////////////////////////////////////////////////////////////////////////
	// This is for bullet hit particles
	private		bool			TryGetSurfaceIndex( in Collider col, in Ray ray, out int index )
	{
		int textureInstanceID = index = - 1;

		// When the collider belongs to a terrain.
		if (col.GetType() == typeof(TerrainCollider))
		{
			col.TryGetComponent(out Terrain terrain);
			TerrainData terrainData = terrain.terrainData;
			float[] textureMix = GetTerrainTextureMix( ray.origin, terrainData, terrain.GetPosition() );
			int textureIndex = GetTextureIndex( textureMix );
			textureInstanceID = terrainData.terrainLayers[ textureIndex ].diffuseTexture.GetInstanceID();

		}
		// When the collider belongs to a normal mesh.
		else
		{
			if (Physics.Raycast(ray, out RaycastHit hit) && Utils.Base.TrySearchComponent( hit.collider.gameObject, ESearchContext.LOCAL, out Renderer renderer))
			{
				if (renderer.sharedMaterial && renderer.sharedMaterial.mainTexture)
				{
					MeshCollider meshCollider = hit.collider as MeshCollider;
					if (!meshCollider || meshCollider.convex)
					{
						textureInstanceID = renderer.material.mainTexture.GetInstanceID();
					}
					else
					{
						// Not Convex MeshCollider
						Mesh mesh = meshCollider.sharedMesh;
						int triangleIdx = hit.triangleIndex * 3;
						int lookupIdx1 = mesh.triangles[triangleIdx + 0];
						int lookupIdx2 = mesh.triangles[triangleIdx + 1];
						int lookupIdx3 = mesh.triangles[triangleIdx + 2];
						int materialIndex = -1;
						for( int i = 0; i < mesh.subMeshCount && materialIndex == -1; i ++ )
						{
							int[] tr = mesh.GetTriangles(i);
							for( int j = 0; j < tr.Length && materialIndex == -1; j += 3 )
							{
								if (tr[j]  == lookupIdx1 && tr[j+1] == lookupIdx2 && tr[j+2] == lookupIdx3)
								{
									materialIndex = i;
								}
							}
						}
						textureInstanceID = materialIndex == -1 ? -1 : renderer.materials[materialIndex].mainTexture.GetInstanceID();
					}
				}
			}
		}

		// Searching for the found texture / material name in registered materials.
		int regTextureIndex = System.Array.FindIndex(m_RegisteredTextures, m => m.texture.GetInstanceID() == textureInstanceID );
		if (m_RegisteredTextures.IsValidIndex(regTextureIndex))
		{
			index = m_RegisteredTextures[regTextureIndex].surfaceIndex;
			return true;
		}
		UnityEngine.Debug.LogWarning($"Cannot retrieve texture index");
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	private		float[]		GetTerrainTextureMix( in Vector3 worldPos, in TerrainData terrainData, in Vector3 terrainPos )
	{
		// returns an array containing the relative mix of textures
		// on the main terrain at this world position.

		// The number of values in the array will equal the number
		// of textures added to the terrain.

		// calculate which splat map cell the worldPos falls within (ignoring y)
		int mapX = (int) ( ( worldPos.x - terrainPos.x ) / terrainData.size.x * terrainData.alphamapWidth );
		int mapZ = (int) ( ( worldPos.z - terrainPos.z ) / terrainData.size.z * terrainData.alphamapHeight );

		// get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
		float[,,] splatmapData = terrainData.GetAlphamaps( mapX, mapZ, 1, 1 );

		// extract the 3D array data to a 1D array:
		float[] cellMix = new float[ splatmapData.GetUpperBound(2) + 1 ];
		for( int n = 0; n < cellMix.Length; n++ )
		{
			cellMix[ n ] = splatmapData[ 0, 0, n ];
		}
		return cellMix;
	}


	//////////////////////////////////////////////////////////////////////////
	private		int			GetTextureIndex(in float[] textureMix )
	{
		// returns the zero-based index of the most dominant texture
		// on the terrain at this world position.
		float maxMix = 0;
		int maxIndex = 0;

		// loop through each mix value and find the maximum
		for (int n = 0; n < textureMix.Length; n++)
		{
			if ( textureMix[n] > maxMix )
			{
				maxIndex = n;
				maxMix = textureMix[n];
			}
		}

		return maxIndex;
	}
}


