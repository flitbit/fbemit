#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Reflection.Emit;

namespace FlitBit.Emit
{	
	/// <summary>
	/// Helper class for working with fields in the IL stream.
	/// </summary>
	public class EmittedField : EmittedMember, IFieldRef
	{
		FieldBuilder _builder;
		FieldAttributes _attributes;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="type">the emitted type</param>
		/// <param name="name">the field's name</param>
		/// <param name="fieldType">the field's type (ref)</param>
		public EmittedField(EmittedClass type, string name, TypeRef fieldType)
			: base(type, name)
		{
			Contract.Requires<ArgumentNullException>(type != null);
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);
			Contract.Requires<ArgumentNullException>(fieldType != null, "fieldType cannot be null");

			this.FieldType = fieldType;
			this.Attributes = FieldAttributes.Private;
		}

		/// <summary>
		/// Gets the field's attributes.
		/// </summary>
		public FieldAttributes Attributes
		{
			get { return _attributes; }
			set
			{
				Contract.Assert(_builder == null, "Attrubutes must be set before the FieldBuilder is created");

				_attributes = value;
				base.IsStatic = value.HasFlag(FieldAttributes.Static);
			}
		}

		/// <summary>
		/// Gets the field's builder.
		/// </summary>
		public FieldBuilder Builder
		{
			get
			{
				if (_builder == null)
				{
					_builder = this.TargetClass.Builder.DefineField(this.Name
						, this.FieldType.Target
						, this.Attributes);
				}
				return _builder;
			}
		}

		/// <summary>
		/// Gets a reference to the field's type
		/// </summary>
		public TypeRef FieldType { get; private set; }

		/// <summary>
		/// Excludes the attributes given.
		/// </summary>
		/// <param name="attr">the attributes to exclude</param>
		public void ExcludeAttributes(FieldAttributes attr)
		{
			Attributes &= (~attr);
		}

		/// <summary>
		/// Clears the field's attributes.
		/// </summary>
		public void ClearAttributes()
		{
			Attributes = default(FieldAttributes);
		}

		/// <summary>
		/// Includes the attributes given.
		/// </summary>
		/// <param name="attr">the attributes to include</param>
		public void IncludeAttributes(FieldAttributes attr)
		{
			Attributes |= attr;
		}

		/// <summary>
		/// Gets the reflection FieldInfo for the field.
		/// </summary>
		public FieldInfo FieldInfo { get { return Builder; } }

		/// <summary>
		/// Emits instructions to load the field's address.
		/// </summary>
		/// <param name="il">IL</param>
		public void LoadAddress(ILGenerator il)
		{
			il.LoadFieldAddress(Builder);
		}
		/// <summary>
		/// Emits instructions to load the field's value.
		/// </summary>
		/// <param name="il"></param>
		public void LoadValue(ILGenerator il)
		{
			il.LoadField(Builder);
		}
		/// <summary>
		/// Emits instructions to store the field's value.
		/// </summary>
		/// <param name="il">IL</param>
		public void StoreValue(ILGenerator il)
		{
			il.StoreField(Builder);
		}

		/// <summary>
		/// Compiles the field.
		/// </summary>
		protected internal override void OnCompile()
		{
			// Access the builder to make sure it is declared on the underlying type...
			var builder = this.Builder;
		}

		/// <summary>
		/// Gets the field's target type.
		/// </summary>
		public Type TargetType
		{
			get { return FieldType.Target; }
		}

		Action<EmittedConstructor, ILGenerator> _init;
		/// <summary>
		/// Assigns the field's initial value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public EmittedField WithInit(IValueRef value)
		{
			Contract.Assert(_init == null, "Field has already been initialized.");
			_init = new Action<EmittedConstructor, ILGenerator>((m, il) =>
			{
				if (!IsStatic)
				{
					il.LoadArg_0();
				}
				il.LoadValue(value);
				StoreValue(il);
			});
			return this;
		}

		internal void EmitInit(EmittedConstructor ctor, ILGenerator il)
		{
			if (_init != null)
			{
				_init(ctor, il);
			}
		}
	}
}