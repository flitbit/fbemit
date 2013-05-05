using System;
using System.Diagnostics.Contracts;
using System.Reflection.Emit;
using FlitBit.Core;

namespace FlitBit.Emit
{
	/// <summary>
	/// Value reference for the current object (this).
	/// </summary>
	public sealed class ThisRef : IValueRef
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="cls"></param>
		public ThisRef(EmittedClass cls)
			:this(cls.Builder)
		{
			Contract.Requires<ArgumentNullException>(cls != null);
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="type"></param>
		public ThisRef(Type type)
		{
			Contract.Requires<ArgumentNullException>(type != null);
			this.Name = type.GetReadableSimpleName();
			this.TargetType = type;
		}

		/// <summary>
		///   Name of the value.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		///   Gets the value's type.
		/// </summary>
		public Type TargetType { get; private set; }

		/// <summary>
		///   Loads the address of the value by pushing it onto the stack.
		/// </summary>
		/// <param name="il">the il generator.</param>
		public void LoadAddress(ILGenerator il)
		{
			il.LoadArgAddress(0);
		}

		/// <summary>
		///   Loads the value by pushing it onto the stack.
		/// </summary>
		/// <param name="il">the il generator.</param>
		public void LoadValue(ILGenerator il)
		{
			il.LoadArg_0();
		}

		/// <summary>
		///   Stores the value by popping it off of the stack.
		/// </summary>
		/// <param name="il">the il generator.</param>
		public void StoreValue(ILGenerator il)
		{
			throw new NotImplementedException();
		}
	}
}