using UnityEngine;
using System.Collections.Generic;

namespace WeatherSystem {

	[System.Serializable]
	public class Weathers : ScriptableObject, IResourceComposite {

		[SerializeField]//[HideInInspector]
		public List<string>				CyclesPaths		= new List<string>();

		[SerializeField]
		public	List<WeatherCycle>		LoadedCycles	= new List<WeatherCycle>();


		// Questa funzione viene chiamata all'avvio dello script
		// utile solo nell'editor

		//////////////////////////////////////////////////////////////////////////
		private void Awake()
		{
			LoadedCycles.Clear();
		}


		//////////////////////////////////////////////////////////////////////////
		bool	IResourceComposite.NeedToBeLoaded()
		{
			if ( LoadedCycles.Count < CyclesPaths.Count )
				return true;

			bool bAreLoaded = !CyclesPaths.TrueForAll( c => c != null );
			return bAreLoaded;
		}


		//////////////////////////////////////////////////////////////////////////
		void		IResourceComposite.Reinit()
		{
			LoadedCycles = new List<WeatherCycle>();
		}


		//////////////////////////////////////////////////////////////////////////
		string[]	IResourceComposite.GetChildPaths()
		{
			return CyclesPaths.ToArray();
		}


		//////////////////////////////////////////////////////////////////////////
		void		IResourceComposite.AddChild( UnityEngine.Object child )
		{
			WeatherCycle childConverted = child as WeatherCycle;
			LoadedCycles.Add( childConverted );
		}

		//////////////////////////////////////////////////////////////////////////
		public	static	void	OnEndPlay( Weathers weather )
		{
			weather.LoadedCycles.ForEach( c => WeatherCycle.OnEndPlay(c) );
		}

	}

}
