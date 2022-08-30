
using UnityEngine;

namespace Entities
{
	public interface IConfigurable<T>
	{
		public void Configure(T param1);
	}

	public interface IConfigurable<T1, T2>
	{
		public void Configure(T1 param1, T2 param2);
	}
	public interface IConfigurable<T1, T2, T3>
	{
		public void Configure(T1 param1, T2 param2, T3 param3);
	}
	public interface IConfigurable<T1, T2, T3, T4>
	{
		public void Configure(T1 param1, T2 param2, T3 param3, T4 param4);
	}
	public interface IConfigurable<T1, T2, T3, T4, T5>
	{
		public void Configure(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5);
	}
}