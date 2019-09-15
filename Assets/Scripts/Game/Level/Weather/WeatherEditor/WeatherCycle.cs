using UnityEngine;
using System.Collections.Generic;

namespace WeatherSystem {

	[System.Serializable]
	public class WeatherCycle : ScriptableObject, IResourceComposite {

		[SerializeField][ReadOnly]//[HideInInspector]
		public	string					AssetPath			= string.Empty;

		[SerializeField]
		public	string[]				DescriptorsPaths	= new string[ 24 ];

		[SerializeField]
		public	List<EnvDescriptor>		LoadedDescriptors	= new List<EnvDescriptor>( 24 );


		//////////////////////////////////////////////////////////////////////////
		bool	IResourceComposite.NeedToBeLoaded()
		{
			if ( LoadedDescriptors.Count < DescriptorsPaths.Length )
				return true;

			bool bAreLoaded = !LoadedDescriptors.TrueForAll( d => d != null );
			return bAreLoaded;
		}



		//////////////////////////////////////////////////////////////////////////
		void		IResourceComposite.Reinit()
		{
			LoadedDescriptors.Clear();
		}


		//////////////////////////////////////////////////////////////////////////
		string[]	IResourceComposite.GetChildPaths()
		{
			return DescriptorsPaths;
		}


		//////////////////////////////////////////////////////////////////////////
		void		IResourceComposite.AddChild( UnityEngine.Object child )
		{
			EnvDescriptor childConverted = child as EnvDescriptor;
			LoadedDescriptors.Add( childConverted );
		}

	}

}