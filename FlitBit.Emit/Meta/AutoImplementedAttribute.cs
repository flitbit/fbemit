using System;
using FlitBit.Core.Meta;

namespace FlitBit.Emit.Meta
{
	/// <summary>
	/// Indicates that an implementation of the interface is generated.
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface)]
	public abstract class AutoImplementedAttribute : Attribute
	{
		/// <summary>
		/// Gets the implementation for target type T.
		/// </summary>
		/// <typeparam name="T">target type T</typeparam>
		/// <param name="complete">callback invoked when the implementation is available</param>
		/// <returns><em>true</em> if implemented; otherwise <em>false</em>.</returns>
		/// <exception cref="ArgumentException">thrown if type T is not eligible for implementation</exception>
		/// <remarks>
		/// If the <paramref name="complete"/> callback is invoked, it must be given either an implementation type
		/// assignable to type T, or a factory function that creates implementations of type T.
		/// </remarks>
		public abstract bool GetImplementation<T>(Action<Type,Func<T>> complete);

		/// <summary>
		/// Indicates the recommended instance scope for implementations.
		/// </summary>
		public InstanceScopeKind RecommemdedScope { get; set; }
	}
}