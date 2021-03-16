﻿using System.Collections.Generic;
using UnityEngine;

public sealed class UI_NotificationSlot : UI_Base, IStateDefiner
{
	private				bool								m_IsInitialized						= false;
						bool								IStateDefiner.IsInitialized			=> m_IsInitialized;


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.PreInit()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.Initialize()
	{
		if (!m_IsInitialized)
		{

			m_IsInitialized = true;
		}
	}

	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.ReInit()
	{

	}


	//////////////////////////////////////////////////////////////////////////
	bool IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}
}
