// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.
// Ref: https://bitbucket.org/rotorz/classtypereference-for-unity/src/master/Assets/

using UnityEngine;

/// <summary> Reference to a class <see cref="System.Type"/> with support for Unity serialization. </summary>
[System.Serializable]
public sealed class ClassTypeReference : ISerializationCallbackReceiver
{
	[SerializeField]
	private string _classRef = null;

	/// <summary> Gets type of class reference. </summary>
	public System.Type Type { get; private set; }


	/// <summary> Initializes a new instance of the <see cref="ClassTypeReference"/> class. </summary>
	/// <param name="type">Class type.</param>
	public ClassTypeReference(System.Type type)
	{
		this.Type = type;
		this._classRef = type.FullName;
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (string.IsNullOrEmpty(this._classRef))
		{
			this.Type = null;
		}
		else
		{
			this.Type = System.Type.GetType(this._classRef);
			if (this.Type == null)
			{
				Debug.LogWarning(string.Format("'{0}' was referenced but class type was not found.", this._classRef));
			}
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	public static implicit operator string(ClassTypeReference typeReference)
	{
		return typeReference._classRef;
	}

	public static implicit operator System.Type(ClassTypeReference typeReference)
	{
		return typeReference.Type;
	}

	public static implicit operator ClassTypeReference(System.Type type)
	{
		return new ClassTypeReference(type);
	}

	public override string ToString()
	{
		return this.Type != null ? this.Type.FullName : "(None)";
	}

}



/*
[System.Serializable]
public sealed class SubClassOf<T> where T : class
{
	private System.Type Type
	{
		get
		{
			return string.IsNullOrEmpty(this.m_TypeName) ? null : System.Type.GetType(this.m_TypeName);
		}
	}

	private readonly string m_TypeName = null;

	public SubClassOf()
	{
		this.m_TypeName = typeof(T).FullName;
	}
}
*/