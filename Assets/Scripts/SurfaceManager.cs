// Ref: https://www.assetstore.unity3d.com/en/#!/content/47967

using UnityEngine;

[System.Serializable]
public struct SurfaceDefinition {
	public string name;
	public AudioClip[] footsteps;
}

[System.Serializable]
public struct RegisteredMaterial {
	public Texture texture;
	public int surfaceIndex;
}

public class SurfaceManager : MonoBehaviour {

	public		static		SurfaceManager	Instance				= null;

	[SerializeField]
	private		RegisteredMaterial[]		m_RegisteredTextures	= null;
	[SerializeField]
	private		SurfaceDefinition[]			m_DefinedSurfaces		= null;


	//////////////////////////////////////////////////////////////////////////
	private		void		Start()
	{
		Instance = this;
	}


	//////////////////////////////////////////////////////////////////////////
	public		AudioClip	GetFootstep( ref Collider groundCollider, Vector3 worldPosition )
	{
		int surfaceIndex = GetSurfaceIndex( ref groundCollider, worldPosition );
		if( surfaceIndex == -1 )
			return null;

		// Getting the footstep sounds based on surface index.
		AudioClip[] footsteps = m_DefinedSurfaces[surfaceIndex].footsteps;
		return footsteps[ Random.Range( 0, footsteps.Length ) ];
	}


	//////////////////////////////////////////////////////////////////////////
	public		string[]	GetAllSurfaceNames()
	{
		string[] names = new string[ m_DefinedSurfaces.Length ];

		for ( int i = 0; i < names.Length; i ++ )
			names[i] = m_DefinedSurfaces[i].name;

		return names;
	}


	//////////////////////////////////////////////////////////////////////////
	// This is for bullet hit particles
	private		int			GetSurfaceIndex( Ray ray, Collider col, Vector3 worldPos )
	{
		string textureName = "";

		// Case when the ground is a terrain.
		if ( col.GetType() == typeof( TerrainCollider ) )
		{
			Terrain terrain = col.GetComponent<Terrain>();
			TerrainData terrainData = terrain.terrainData;
			float[] textureMix = GetTerrainTextureMix( worldPos, ref terrainData, terrain.GetPosition() );
			int textureIndex = GetTextureIndex( ref textureMix );
			textureName = terrainData.splatPrototypes[ textureIndex ].texture.name;

		}
		// Case when the ground is a normal mesh.
		else
		{
			textureName = GetMeshMaterialAtPoint( worldPos, ray );
		}

		// Searching for the found texture / material name in registered materials.
		foreach( RegisteredMaterial pMaterial in m_RegisteredTextures )
			if( pMaterial.texture.name == textureName )
				return pMaterial.surfaceIndex;

		return -1;
	}


	//////////////////////////////////////////////////////////////////////////
	// This is for footsteps
	private		int			GetSurfaceIndex( ref Collider col, Vector3 worldPos )
	{
		string textureName = "";

		// Case when the ground is a terrain.
		if ( col.GetType() == typeof( TerrainCollider ) )
		{
			Terrain terrain = col.GetComponent<Terrain>();
			TerrainData terrainData = terrain.terrainData;
			float[] textureMix = GetTerrainTextureMix( worldPos, ref terrainData, terrain.GetPosition() );
			int textureIndex = GetTextureIndex( ref textureMix );
			textureName = terrainData.splatPrototypes[ textureIndex ].texture.name;

		}
		// Case when the ground is a normal mesh.
		else
		{
			textureName = GetMeshMaterialAtPoint( worldPos, new Ray( Vector3.zero, Vector3.zero ) );
		}

		// Searching for the found texture / material name in registered materials.
		foreach( RegisteredMaterial pMaterial in m_RegisteredTextures )
			if( pMaterial.texture != null && pMaterial.texture.name == textureName )
				return pMaterial.surfaceIndex;

		return -1;
	}


	//////////////////////////////////////////////////////////////////////////
	private		string		GetMeshMaterialAtPoint( Vector3 worldPosition, Ray ray )
	{
		if( ray.direction == Vector3.zero )
		{
			// direction down
			ray = new Ray( worldPosition + Vector3.up * 0.01f, Vector3.down );
		}

		RaycastHit hit;
		if ( Physics.Raycast( ray, out hit ) == false )
			return "";

		Renderer r = hit.collider.GetComponent<Renderer>();
		MeshCollider mc = hit.collider as MeshCollider;

		if ( r == null || r.sharedMaterial == null || r.sharedMaterial.mainTexture == null || r == null )
			return "";

		if ( mc == null || mc.convex )
			return r.material.mainTexture.name;

		int materialIndex = -1;
		Mesh m = mc.sharedMesh;
		int triangleIdx = hit.triangleIndex;
		int lookupIdx1	= m.triangles[ triangleIdx * 3 ];
		int lookupIdx2	= m.triangles[ triangleIdx * 3 + 1 ];
		int lookupIdx3	= m.triangles[ triangleIdx * 3 + 2 ];
		int subMeshesNr = m.subMeshCount;

		for( int i = 0;i < subMeshesNr; i ++ )
		{
			int[] tr = m.GetTriangles( i );

			for( int j = 0; j < tr.Length; j += 3 )
			{
				if ( tr[ j ] == lookupIdx1 && tr[ j+1 ] == lookupIdx2 && tr[ j+2 ] == lookupIdx3 )
				{
					materialIndex = i;
					break;
				}
			}

			if ( materialIndex != -1 )
				break;
		}

		return r.materials[materialIndex].mainTexture.name;
	}


	//////////////////////////////////////////////////////////////////////////
	private		float[]		GetTerrainTextureMix( Vector3 worldPos, ref TerrainData terrainData, Vector3 terrainPos )
	{
		// returns an array containing the relative mix of textures
		// on the main terrain at this world position.

		// The number of values in the array will equal the number
		// of textures added to the terrain.

		// calculate which splat map cell the worldPos falls within (ignoring y)
		int mapX = ( int )( ( ( worldPos.x - terrainPos.x ) / terrainData.size.x ) * terrainData.alphamapWidth  );
		int mapZ = ( int )( ( ( worldPos.z - terrainPos.z ) / terrainData.size.z ) * terrainData.alphamapHeight );

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
	private		int			GetTextureIndex( ref float[] textureMix )
	{
		// returns the zero-based index of the most dominant texture
		// on the terrain at this world position.
		float maxMix = 0;
		int maxIndex = 0;

		// loop through each mix value and find the maximum
		for ( int n = 0; n < textureMix.Length; n ++ )
		{
			if ( textureMix[n] > maxMix )
			{
				maxIndex = n;
				maxMix = textureMix[ n ];
			}
		}

		return maxIndex;
	}
}


