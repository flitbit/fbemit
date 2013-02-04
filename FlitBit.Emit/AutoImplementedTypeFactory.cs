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
			public Delegate Factory;

			internal object CreateInstance()
			{
				return (TargetType == null) ? Factory.DynamicInvoke(null) : Activator.CreateInstance(TargetType);
			}
		}
		ConcurrentDictionary<object, TypeRecord> _emittedTypes = new ConcurrentDictionary<object, TypeRecord>();

		/// <summary>
		/// Creates a new instance of type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns></returns>
		public T CreateInstance<T>()
		{
			if (typeof(T).IsAbstract && typeof(T).IsDefined(typeof(AutoImplementedAttribute), true))
			{
				return CreateAutoImplemented<T>();
			}
			return (T)Activator.CreateInstance<T>();
		}

		private T CreateAutoImplemented<T>()
		{
			var type = typeof(T);
			var typeLock = type.GetLockForType();
			TypeRecord rec;
			if (!_emittedTypes.TryGetValue(typeLock, out rec))
			{
				var emitted = false;
				foreach (AutoImplementedAttribute attr in type.GetCustomAttributes(typeof(AutoImplementedAttribute), false))
				{
					if (attr.GetImplementation<T>((impl, factory) =>
					{
						// use the implementation type if provided
						rec = new TypeRecord { TargetType = impl, Factory = factory };
						_emittedTypes.TryAdd(typeLock, rec);
					}))
					{
						emitted = true;
						break;
					}
				}
				if (!emitted)
				{
					throw new InvalidOperationException(String.Concat("Unable to access implementation of ", type.GetReadableFullName()));
				}
			}
			return (T)rec.CreateInstance();
		}
	}
}
