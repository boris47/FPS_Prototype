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

	private		static		SurfaceManager	m_Instance				= null;
	public		static		SurfaceManager Instance
	{
		get { return m_Instance; }
	}

	[SerializeField]
	private		RegisteredMaterial[]		m_RegisteredTextures	= null;
	[SerializeField]
	private		SurfaceDefinition[]			m_DefinedSurfaces		= null;


	//////////////////////////////////////////////////////////////////////////
	private		void		Start()
	{
		m_Instance = this;
	}


	//////////////////////////////////////////////////////////////////////////
	public		AudioClip	GetFootstep( Collider groundCollider, Vector3 worldPosition )
	{
		int surfaceIndex = GetSurfaceIndex( groundCollider, worldPosition );
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
		int textureInstanceID = -1;

		// Case when the ground is a terrain.
		if ( col.GetType() == typeof( TerrainCollider ) )
		{
			Terrain terrain = col.GetComponent<Terrain>();
			TerrainData terrainData = terrain.terrainData;
			float[] textureMix = GetTerrainTextureMix( worldPos, terrainData, terrain.GetPosition() );
			int textureIndex = GetTextureIndex( textureMix );
			textureInstanceID = terrainData.splatPrototypes[ textureIndex ].texture.GetInstanceID();

		}
		// Case when the ground is a normal mesh.
		else
		{
			textureInstanceID = GetMeshMaterialAtPoint( worldPos, ray );
		}

		// Searching for the found texture / material name in registered materials.
		int regTextureIndex = System.Array.FindIndex( m_RegisteredTextures, m => m.texture.GetInstanceID() == textureInstanceID );
		return m_RegisteredTextures[regTextureIndex].surfaceIndex;
	}


	//////////////////////////////////////////////////////////////////////////
	// This is for footsteps
	private		int			GetSurfaceIndex( Collider col, Vector3 worldPos )
	{
		int textureInstanceID = -1;

		// Case when the ground is a terrain.
		if ( col.GetType() == typeof( TerrainCollider ) )
		{
			Terrain terrain = col.GetComponent<Terrain>();
			TerrainData terrainData = terrain.terrainData;
			float[] textureMix = GetTerrainTextureMix( worldPos, terrainData, terrain.GetPosition() );
			int textureIndex = GetTextureIndex( textureMix );
			textureInstanceID = terrainData.splatPrototypes[ textureIndex ].texture.GetInstanceID();

		}
		// Case when the ground is a normal mesh.
		else
		{
			textureInstanceID = GetMeshMaterialAtPoint( worldPos, null );
		}

		// Searching for the found texture / material name in registered materials.
		int regTextureIndex = System.Array.FindIndex( m_RegisteredTextures, m => m.texture.GetInstanceID() == textureInstanceID );
		if ( regTextureIndex == -1 || regTextureIndex >= m_RegisteredTextures.Length )
		{
			Debug.DebugBreak();
			return 0;
		}

		return m_RegisteredTextures[regTextureIndex].surfaceIndex;
	}


	//////////////////////////////////////////////////////////////////////////
	private		int		GetMeshMaterialAtPoint( Vector3 worldPosition, Ray? ray )
	{
		if( ray.HasValue == false )
		{
			// direction down
			ray = new Ray( worldPosition + Vector3.up * 0.01f, Vector3.down );
		}

		RaycastHit hit;
		if ( Physics.Raycast( ray.Value, out hit ) == false )
			return -1;

		Renderer r = null;
		bool bHasRender = Utils.Base.SearchComponent( hit.collider.gameObject, ref r, SearchContext.LOCAL );
		if ( bHasRender == false || r.sharedMaterial == null || r.sharedMaterial.mainTexture == null )
			return -1;

		bool bIsMeshCollider = hit.collider.GetType() == typeof( MeshCollider );
		MeshCollider mc = hit.collider as MeshCollider;
		if ( bIsMeshCollider == false || mc.convex )
			return r.material.mainTexture.GetInstanceID();

		Mesh m = mc.sharedMesh;
		int materialIndex = -1;
		int triangleIdx = hit.triangleIndex;
		int lookupIdx1	= m.triangles[ triangleIdx * 3 ];
		int lookupIdx2	= m.triangles[ triangleIdx * 3 + 1 ];
		int lookupIdx3	= m.triangles[ triangleIdx * 3 + 2 ];
		int subMeshesNr = m.subMeshCount;

		for( int i = 0;i < subMeshesNr && materialIndex == -1; i ++ )
		{
			int[] tr = m.GetTriangles( i );
			for( int j = 0; j < tr.Length && materialIndex == -1; j += 3 )
			{
				if ( tr[ j ] == lookupIdx1 && tr[ j+1 ] == lookupIdx2 && tr[ j+2 ] == lookupIdx3 )
				{
					materialIndex = i;
				}
			}
		}

		return r.materials[materialIndex].mainTexture.GetInstanceID();
	}


	//////////////////////////////////////////////////////////////////////////
	private		float[]		GetTerrainTextureMix( Vector3 worldPos, TerrainData terrainData, Vector3 terrainPos )
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
	private		int			GetTextureIndex( float[] textureMix )
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


