using UnityEngine;
using System.Collections.Generic;

namespace WeatherSystem {

	[System.Serializable]
	public class Weathers : ScriptableObject, IResourceComposite {

		[SerializeField]//[HideInInspector]
		public List<string> CyclesPaths = new List<string>();

		[SerializeField]
		public	List<WeatherCycle>	LoadedCycles = new List<WeatherCycle>();


		// Questa funzione viene chiamata all'avvio dello script
		private void Awake()
		{
			LoadedCycles.Clear();
		}



		bool	IResourceComposite.NeedToBeLoaded()
		{
			if ( LoadedCycles.Count < CyclesPaths.Count )
				return true;

			bool bAreLoaded = true;
			for ( int i = 0; i < CyclesPaths.Count; i++ )
			{
				bAreLoaded &= LoadedCycles[i] != null;
			}

			return !bAreLoaded;
		}


		void		IResourceComposite.Reinit()
		{
			LoadedCycles = new List<WeatherCycle>();
		}

		string[]	IResourceComposite.GetChildPaths()
		{
			return CyclesPaths.ToArray();
		}

		void		IResourceComposite.AddChild( UnityEngine.Object child )
		{
			WeatherCycle childConverted = child as WeatherCycle;
			LoadedCycles.Add( childConverted );
		}

	}

}
