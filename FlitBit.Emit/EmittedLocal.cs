#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Reflection.Emit;

namespace FlitBit.Emit
{
	/// <summary>
	///   Helper class for working with local variables in the IL stream.
	/// </summary>
	public class EmittedLocal : IValueRef
	{
		/// <summary>
		///   Creates a new instance
		/// </summary>
		/// <param name="name">the local's name</param>
		/// <param name="index">the index</param>
		/// <param name="localType">the type</param>
		public EmittedLocal(string name, int index, TypeRef localType)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);
			Contract.Requires<ArgumentNullException>(localType != null, "localType cannot be null");

			Name = name;
			LocalIndex = index;
			LocalType = localType;
		}

		/// <summary>
		///   Gets the local's builder.
		/// </summary>
		public LocalBuilder Builder { get; private set; }

		/// <summary>
		///   Indicates the local's declaration index.
		/// </summary>
		public int LocalIndex { get; private set; }

		/// <summary>
		///   Indicates the local's type (ref).
		/// </summary>
		public TypeRef LocalType { get; private set; }

		/// <summary>
		///   Compiles the local.
		/// </summary>
		/// <param name="il"></param>
		public void Compile(ILGenerator il)
		{
			if (Builder == null)
			{
				Builder = il.DeclareLocal(TargetType, false);
			}
		}

		#region IValueRef Members

		/// <summary>
		///   Gets the local's name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		///   Emits instructions to load the local's address.
		/// </summary>
		/// <param name="il">IL</param>
		public void LoadAddress(ILGenerator il)
		{
			Contract.Assert(il != null);
			Contract.Assert(Builder != null, "not compiled");
			il.LoadLocalAddress(LocalIndex);
		}

		/// <summary>
		///   Emits instructions to load the local's value.
		/// </summary>
		/// <param name="il">IL</param>
		public void LoadValue(ILGenerator il)
		{
			Contract.Assert(il != null);
			Contract.Assert(Builder != null, "not compiled");
			il.LoadLocal(LocalIndex);
		}

		/// <summary>
		///   Emits instructions to store the local's value.
		/// </summary>
		/// <param name="il">IL</param>
		public void StoreValue(ILGenerator il)
		{
			Contract.Assert(il != null);
			Contract.Assert(Builder != null, "not compiled");
			il.StoreLocal(LocalIndex);
		}

		/// <summary>
		///   Gets the local's target type.
		/// </summary>
		public Type TargetType
		{
			get { return LocalType.Target; }
		}

		#endregion
	}
}