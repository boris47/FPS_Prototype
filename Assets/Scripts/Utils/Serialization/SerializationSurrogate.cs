
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SurrogatesInstaller
{
	private sealed class SerializationSurrogate_Generic<T> : ISerializationSurrogate
	{
		public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
		{
			// JsonUtility can serialize all public or serializable fields. (No properties!!)

			var wrapper = new ToJsonWrapper<T>((T)obj);
			info.AddValue(obj.GetType().Name, JsonUtility.ToJson(wrapper));
		}

		public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			string serializedValue = (string)info.GetValue(obj.GetType().Name, typeof(string));
			var wrapper = JsonUtility.FromJson<ToJsonWrapper<T>>(serializedValue);
			return obj = wrapper.content;
		}
	}

	private sealed class SerializationSurrogate_Transform : ISerializationSurrogate
	{
		public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
		{
			Transform t = (Transform)obj;
			// Position
			Vector3 position = t.position;
			{
				info.AddValue("p.x", position.x);
				info.AddValue("p.y", position.y);
				info.AddValue("p.z", position.z);
			}

			// Rotation
			Quaternion rotation = t.rotation;
			{
				info.AddValue("r.x", rotation.x);
				info.AddValue("r.y", rotation.y);
				info.AddValue("r.z", rotation.z);
				info.AddValue("r.w", rotation.w);
			}

			// Scale
			Vector3 localScale = t.localScale;
			{
				info.AddValue("s.x", localScale.x);
				info.AddValue("s.y", localScale.y);
				info.AddValue("s.z", localScale.z);
			}
		}

		public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Transform t = (Transform)obj;

			try
			{
				var a = t.position;
			}
			catch (System.Exception)
			{
				return null;
			}

			// Position
			t.position = new Vector3
			(
				(float)info.GetValue("p.x", typeof(float)),
				(float)info.GetValue("p.y", typeof(float)),
				(float)info.GetValue("p.z", typeof(float))
			);

			// Rotation
			t.rotation = new Quaternion
			( 
				(float)info.GetValue("r.x", typeof(float)),
				(float)info.GetValue("r.y", typeof(float)),
				(float)info.GetValue("r.z", typeof(float)),
				(float)info.GetValue("r.w", typeof(float))
			);

			// Scale
			t.localScale = new Vector3
			(
				(float)info.GetValue("s.x", typeof(float)),
				(float)info.GetValue("s.y", typeof(float)),
				(float)info.GetValue("s.z", typeof(float))
			);

			obj = t;
			return obj;
		}
	}

	private sealed class SerializationSurrogate_Bounds : ISerializationSurrogate
	{
		public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
		{
			Bounds b = (Bounds)obj;

			// Center
			Vector3 center = b.center;
			{
				info.AddValue("c.x", center.x);
				info.AddValue("c.y", center.y);
				info.AddValue("c.z", center.z);
			}

			// Size
			Vector3 size = b.size;
			{
				info.AddValue("s.x", size.x);
				info.AddValue("s.y", size.y);
				info.AddValue("s.z", size.z);
			}

		}

		public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Bounds b = (Bounds)obj;

			// Center
			b.center = new Vector3
			(
				(float)info.GetValue("c.x", typeof(float)),
				(float)info.GetValue("c.y", typeof(float)),
				(float)info.GetValue("c.z", typeof(float))
			);

			// Size
			b.size = new Vector3
			(
				(float)info.GetValue("s.x", typeof(float)),
				(float)info.GetValue("s.y", typeof(float)),
				(float)info.GetValue("s.z", typeof(float))
			);

			return obj = b;
		}
	}

	private sealed class SerializationSurrogat_RaycastHit : ISerializationSurrogate
	{
		public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
		{
			RaycastHit r = (RaycastHit)obj;

			foreach(PropertyInfo propertyInfo in (r.GetType().GetProperties(BindingFlags.Public)))
			{
				if (ReflectionHelper.GetPropertyValue(r, propertyInfo.Name, out object value))
				{
					info.AddValue(propertyInfo.Name, value);
				}
			}
		}

		public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			RaycastHit r = (RaycastHit)obj;

			foreach (PropertyInfo propertyInfo in (r.GetType().GetProperties(BindingFlags.Public)))
			{
				object value = info.GetValue(propertyInfo.Name, typeof(object));
				ReflectionHelper.SetPropertyValue(r, propertyInfo.Name, value);
			}
		
			return obj = r;
		}
	}

	private sealed class SerializationSurrogat_LayerMask : ISerializationSurrogate
	{
		public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
		{
			LayerMask l = (LayerMask)obj;
			{
				var wrapper = new ToJsonWrapper<LayerMask>(l);
				info.AddValue("layerMask", JsonUtility.ToJson(wrapper));
			}
		}

		public System.Object SetObjectData(System.Object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			LayerMask l = (LayerMask)obj;
			{
				var wrapper = JsonUtility.FromJson<ToJsonWrapper<LayerMask>>((string)info.GetValue("layerMask", typeof(string)));
				l = wrapper.content.value;
			}	
			return obj = l;
		}
	}

	public static BinaryFormatter InstallSurrogates(this BinaryFormatter formatter)
	{
		// 1. Construct a SurrogateSelector object
		SurrogateSelector ss = new SurrogateSelector();

		ss.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), new SerializationSurrogate_Generic<Vector2>());
		ss.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), new SerializationSurrogate_Generic<Vector3>());
		ss.AddSurrogate(typeof(Vector4), new StreamingContext(StreamingContextStates.All), new SerializationSurrogate_Generic<Vector4>());
		ss.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), new SerializationSurrogate_Generic<Quaternion>());
		ss.AddSurrogate(typeof(Transform), new StreamingContext(StreamingContextStates.All), new SerializationSurrogate_Transform());

		ss.AddSurrogate(typeof(Matrix4x4), new StreamingContext(StreamingContextStates.All), new SerializationSurrogate_Generic<Matrix4x4>());
		ss.AddSurrogate(typeof(Bounds), new StreamingContext(StreamingContextStates.All), new SerializationSurrogate_Bounds());
		ss.AddSurrogate(typeof(RaycastHit), new StreamingContext(StreamingContextStates.All), new SerializationSurrogat_RaycastHit());
		ss.AddSurrogate(typeof(LayerMask), new StreamingContext(StreamingContextStates.All), new SerializationSurrogate_Generic<LayerMask>());

		// 2. Have the formatter use our surrogate selector
		formatter.SurrogateSelector = ss;
		return formatter;
	}
}

