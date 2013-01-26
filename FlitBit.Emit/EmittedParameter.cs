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
	/// Helper class for working with parameters in the IL stream.
	/// </summary>
	public class EmittedParameter : IValueRef
	{
		ParameterBuilder _builder;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="method">the method, owner</param>
		/// <param name="index">the parameter's index</param>
		/// <param name="name">the parameter's name</param>
		/// <param name="type">the parameter's type (ref)</param>
		internal EmittedParameter(EmittedMethodBase method, int index, string name, TypeRef type)
		{
			Contract.Requires<ArgumentNullException>(method != null);
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			this.Method = method;
			this.Index = index;
			this.Name = name;
			this.ParameterType = type;
			this.Attributes = ParameterAttributes.None;
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="builder">a parameter builder</param>
		/// <param name="type">the parameter's type (ref)</param>
		internal EmittedParameter(ParameterBuilder builder, Type type)
		{
			_builder = builder;
			this.Name = _builder.Name;
			this.ParameterType = new TypeRef(type);
			this.Attributes = (ParameterAttributes)_builder.Attributes;
		}

		/// <summary>
		/// Gets the method that defines the parameter.
		/// </summary>
		public EmittedMethodBase Method { get; private set; }

		/// <summary>
		/// Gets the method's attributes.
		/// </summary>
		public ParameterAttributes Attributes { get; private set; }

		/// <summary>
		/// Gets the parameter's builder.
		/// </summary>
		public ParameterBuilder Builder { get { return _builder; } }

		/// <summary>
		/// Gets the parameter's index.
		/// </summary>
		public int Index { get; private set; }

		/// <summary>
		/// Gets the parameter's name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets a reference to the parameter's type.
		/// </summary>
		public TypeRef ParameterType { get; private set; }

		/// <summary>
		/// Clears the parameter's attributes.
		/// </summary>
		public void ClearAttributes()
		{
			Attributes = (ParameterAttributes)0;
		}

		/// <summary>
		/// Excludes the attributes given.
		/// </summary>
		/// <param name="attr">attributes to be excluded</param>
		public void ExcludeAttributes(ParameterAttributes attr)
		{
			Attributes &= (~attr);
		}

		/// <summary>
		/// Includes the attributes given.
		/// </summary>
		/// <param name="attr">attributes to be encluded</param>
		public void IncludeAttributes(ParameterAttributes attr)
		{
			Attributes |= attr;
		}

		/// <summary>
		/// Compiles the parameter.
		/// </summary>
		/// <param name="m">method builder</param>
		internal void Compile(MethodBuilder m)
		{
			if (_builder == null)
			{
				int ofs = (m.IsStatic ? 0 : 1);
				_builder = m.DefineParameter(this.Index + ofs, this.Attributes, this.Name);
			}
		}

		internal void Compile(ConstructorBuilder c)
		{
			if (_builder == null)
			{
				int ofs = (c.IsStatic ? 0 : 1);
				_builder = c.DefineParameter(this.Index + ofs, this.Attributes, this.Name);
			}
		}

		/// <summary>
		/// Gets the parameter target's type.
		/// </summary>
		public Type TargetType
		{
			get { return ParameterType.Target; }
		}

		/// <summary>
		/// Emits IL to load the parameter's address.
		/// </summary>
		/// <param name="il"></param>
		public void LoadAddress(ILGenerator il)
		{
			int idx = (this.Method.IsStatic) ? Index : Index + 1;
			il.LoadArgAddress(idx);
		}

		/// <summary>
		/// Emits IL to load the parameter's value.
		/// </summary>
		/// <param name="il"></param>
		public void LoadValue(ILGenerator il)
		{
			int idx = (this.Method.IsStatic) ? Index : Index + 1;
			il.LoadArg(idx);
		}

		/// <summary>
		/// Emits IL to store the parameter's value.
		/// </summary>
		/// <param name="il"></param>
		public void StoreValue(ILGenerator il)
		{
			int idx = (this.Method.IsStatic) ? Index : Index + 1;
			il.StoreArg(idx);
		}
	}
}