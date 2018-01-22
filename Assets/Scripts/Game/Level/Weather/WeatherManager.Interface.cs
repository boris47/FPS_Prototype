
using System;
using System.Collections;
using UnityEngine;

namespace WeatherSystem {

	public interface IWeatherManager {

		bool	IsDynamic { get; set; }

		void	SetTime( float DayTime );

		void	SetTime( int H, int M, int S );

		void	SetTime( string sTime );

		string	GetTimeAsString( float t );

		string	GetActualTimeAsString();

		Light	GetSun();

		Color	GetAmbientColor();

	}


	public partial class WeatherManager : IWeatherManager {


		/// //////////////////////////////////////////////////////////////////////////
		/// GetActualTimeAsString
		public string GetActualTimeAsString()
		{
			return GetTimeAsString( m_DayTimeNow );
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// GetSun
		public Light GetSun()
		{
			return m_GlobalLight;
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// SetTime
		public void SetTime( float DayTime )
		{
			m_DayTimeNow = DayTime;
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// SetTime
		public void SetTime( int H, int M, int S )
		{
			m_DayTimeNow = ( float )( ( H * 3600 ) + ( M * 60 ) + S );
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// SetTime
		public void SetTime( string sTime )
		{
			TansformTime( sTime, ref m_DayTimeNow );
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// GetAmbientColor
		public	Color	GetAmbientColor(  )
		{
			return Color.clear;
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// GetTimeAsString
		public	string GetTimeAsString( float f )
		{
			int iH = ( int ) ( f / ( 60 * 60 ) );
			int iM = ( int ) ( f / 60 ) % 60;
			int iS = ( int ) f % 60;

			return string.Format( "%02d:%02d:%02d", iH, iM, iS );
		}
	}

}