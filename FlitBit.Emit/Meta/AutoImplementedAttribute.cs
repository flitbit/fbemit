using System;
using FlitBit.Core.Meta;
using FlitBit.Core.Factory;

namespace FlitBit.Emit.Meta
{
	/// <summary>
	/// Indicates that an implementation of the interface is generated.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface)]
	public abstract class AutoImplementedAttribute : Attribute
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public AutoImplementedAttribute()
		{
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="recommemdedScope">Recommended scope for the resultant type.</param>
		public AutoImplementedAttribute(InstanceScopeKind recommemdedScope)
		{
			this.RecommemdedScope = recommemdedScope;
		}

		/// <summary>
		/// Gets the implementation for target type T.
		/// </summary>
		/// <typeparam name="T">target type T</typeparam>
		/// <param name="factory">the factory from which the type was requsted.</param>
		/// <param name="complete">callback invoked when the implementation is available</param>
		/// <returns><em>true</em> if implemented; otherwise <em>false</em>.</returns>
		/// <exception cref="ArgumentException">thrown if type T is not eligible for implementation</exception>
		/// <remarks>
		/// If the <paramref name="complete"/> callback is invoked, it must be given either an implementation type
		/// assignable to type T, or a factory function that creates implementations of type T.
		/// </remarks>
		public abstract bool GetImplementation<T>(IFactory factory, Action<Type,Func<T>> complete);

		/// <summary>
		/// Indicates the recommended instance scope for implementations.
		/// </summary>
		public InstanceScopeKind RecommemdedScope { get; set; }
	}
}