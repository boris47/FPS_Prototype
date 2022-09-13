﻿using System.Collections;
using UnityEngine;

public class GameManagerConfiguration : ConfigurationBase
{
	[SerializeField, Min(0.200f)]
	private float m_ThinkIntervalMS = 0.2f;


	public float ThinkIntervalMS => m_ThinkIntervalMS;


}