using System;
using System.Collections.Concurrent;
using FlitBit.Core;
using FlitBit.Core.Factory;
using FlitBit.Emit.Meta;

namespace FlitBit.Emit
{
	/// <summary>
	/// Factory capable of constructing auto-implemented types.
	/// </summary>
	public sealed class AutoImplementedTypeFactory : IFactory
	{
		struct TypeRecord
		{
			public Type TargetType;
			public Delegate Functor;

			internal object CreateInstance()
			{
				return (TargetType == null) ? Functor.DynamicInvoke(null) : Activator.CreateInstance(TargetType);
			}
		}
		ConcurrentDictionary<object, TypeRecord> _types = new ConcurrentDictionary<object, TypeRecord>();

		/// <summary>
		/// Creates a new instance of type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns></returns>
		public T CreateInstance<T>()
		{
			var key = typeof(T).GetKeyForType();
			TypeRecord rec;
			if (this.CanConstruct<T>() && _types.TryGetValue(key, out rec))
			{
				return (T)rec.CreateInstance();
			}
			throw new InvalidOperationException(String.Concat("No suitable implementation found: ", typeof(T).GetReadableFullName(), "."));
		}
		
		/// <summary>
		/// Determins if the factory can construct instances of type T.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public bool CanConstruct<T>()
		{
			bool res = false;
			var type = typeof(T);
			var key = type.GetKeyForType();
			TypeRecord rec;
			if (_types.TryGetValue(key, out rec))
			{
				return true;
			}
			if (!type.IsAbstract)
			{
				if (type.GetConstructor(Type.EmptyTypes) != null)
				{
					rec = new TypeRecord { TargetType = type };
					_types.TryAdd(key, rec);
					return true;
				}
			}
			else
			{
				var gotImpl = false;
				foreach (AutoImplementedAttribute attr in type.GetCustomAttributes(typeof(AutoImplementedAttribute), false))
				{
					if (attr.GetImplementation<T>(this, (impl, functor) =>
					{
						if (impl == null || functor == null)
						{
							// use the implementation type if provided
							rec = new TypeRecord { TargetType = impl, Functor = functor };
							_types.TryAdd(key, rec);
							gotImpl = true;
						}
					}) && gotImpl)
					{
						return true;
					}
				}
			}
			return res;
		}

		/// <summary>
		/// Gets the factory's implementation type for type T.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public Type GetImplementationType<T>()
		{
			var key = typeof(T).GetKeyForType();
			TypeRecord rec;
			if (this.CanConstruct<T>() && _types.TryGetValue(key, out rec))
			{
				return rec.TargetType;
			}
			return null;
		}
	}
}
