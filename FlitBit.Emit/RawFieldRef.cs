#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Reflection.Emit;

namespace FlitBit.Emit
{
	internal class RawFieldRef : IFieldRef
	{
		FieldInfo _field;

		public RawFieldRef(FieldInfo field)
		{
			Contract.Requires<ArgumentNullException>(field != null);
			
			_field = field;
		}

		public string Name
		{
			get { return _field.Name; }
		}

		public FieldInfo FieldInfo { get { return _field; } }

		public void LoadAddress(ILGenerator il)
		{
			Contract.Assert(il != null);

			il.LoadFieldAddress(_field);
		}

		public void LoadValue(ILGenerator il)
		{
			Contract.Assert(il != null);

			il.LoadField(_field);
		}

		public void StoreValue(ILGenerator il)
		{
			Contract.Assert(il != null);

			il.StoreField(_field);
		}
		
		public Type TargetType
		{
			get { return _field.FieldType; }
		}
	}
}