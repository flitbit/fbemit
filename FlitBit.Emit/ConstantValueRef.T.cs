#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Reflection.Emit;

namespace FlitBit.Emit
{
	/// <summary>
	///   Reference to a raw value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ValueRef<T> : IValueRef
	{
		readonly T _const;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="value"></param>
		public ValueRef(T value) { _const = value; }

		#region IValueRef Members

		/// <summary>
		///   The value's name.
		/// </summary>
		public string Name
		{
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		///   Get's the value's target type.
		/// </summary>
		public Type TargetType
		{
			get { return typeof(T); }
		}

		/// <summary>
		///   Loads the address of the value.
		/// </summary>
		/// <param name="il"></param>
		public void LoadAddress(ILGenerator il) { throw new InvalidOperationException("Cannot load address of a constant"); }

		/// <summary>
		///   Loads the value.
		/// </summary>
		/// <param name="il"></param>
		public void LoadValue(ILGenerator il) { il.LoadValue(_const); }

		/// <summary>
		///   Stores the value.
		/// </summary>
		/// <param name="il"></param>
		public void StoreValue(ILGenerator il) { throw new InvalidOperationException("Cannot store value on a constant"); }

		#endregion
	}
}