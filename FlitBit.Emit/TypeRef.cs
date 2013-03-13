#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;

namespace FlitBit.Emit
{
	/// <summary>
	///   A wrapper object for a type reference.
	/// </summary>
	public class TypeRef
	{
		/// <summary>
		///   An empty type ref.
		/// </summary>
		public static readonly TypeRef Empty = new TypeRef();

		readonly Type _type;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="type">the type that is referenced</param>
		public TypeRef(Type type)
		{
			Contract.Requires<ArgumentNullException>(type != null);

			_type = type;
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		protected TypeRef() { }

		/// <summary>
		///   Gets the reference target.
		/// </summary>
		public virtual Type Target
		{
			get { return _type; }
		}

		/// <summary>
		///   Creates a type ref from an emitted type (possibly before compilation).
		/// </summary>
		/// <param name="class">the emitted type</param>
		/// <returns>a type ref</returns>
		public static TypeRef FromEmittedClass(EmittedClass @class) { return new EmittedTypeRef(@class); }

		/// <summary>
		///   Creates a type ref from type T
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>a type ref</returns>
		public static TypeRef FromType<T>() { return new TypeRef(typeof(T)); }

		/// <summary>
		///   Creates a type ref from a type.
		/// </summary>
		/// <param name="type">the type</param>
		/// <returns>the type ref</returns>
		public static TypeRef FromType(Type type) { return new TypeRef(type); }
	}

	/// <summary>
	///   A specialized TypeRef for emitted types.
	/// </summary>
	public class EmittedTypeRef : TypeRef
	{
		readonly EmittedClass _eclass;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="eclass">the emitted type</param>
		public EmittedTypeRef(EmittedClass eclass)
		{
			Contract.Requires<ArgumentNullException>(eclass != null);

			_eclass = eclass;
		}

		/// <summary>
		///   Gets the referenced type.
		/// </summary>
		public override Type Target
		{
			get { return _eclass.Builder; }
		}
	}
}