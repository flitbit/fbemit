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
	/// Helper class for working with properties in the IL stream.
	/// </summary>
	public class EmittedProperty : EmittedMember, IPropertyRef
	{
		PropertyBuilder _builder;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="type">the property's type</param>
		/// <param name="name">the property's name</param>
		/// <param name="propertyType">the property's type</param>
		/// <param name="isStatic">whether the property is a static property</param>
		public EmittedProperty(EmittedClass type, string name, Type propertyType, bool isStatic)
			: this(type, name, new TypeRef(propertyType), Type.EmptyTypes, isStatic)
		{
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="type">the property's type</param>
		/// <param name="name">the property's name</param>
		/// <param name="propertyType">the property's type (ref)</param>
		/// <param name="isStatic">whether the property is a static property</param>
		public EmittedProperty(EmittedClass type, string name, TypeRef propertyType, bool isStatic)
			: this(type, name, propertyType, Type.EmptyTypes, isStatic)
		{      
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="type">the property's type</param>
		/// <param name="name">the property's name</param>
		/// <param name="propertyType">the property's type (ref)</param>
		/// <param name="paramTypes">the property's parameter types</param>
		/// <param name="isStatic">whether the property is a static property</param>
		public EmittedProperty(EmittedClass type, string name, TypeRef propertyType, Type[] paramTypes, bool isStatic)
			: base(type, name)
		{
			Contract.Requires<ArgumentNullException>(propertyType != null);
			Contract.Requires<ArgumentNullException>(paramTypes != null);

			this.PropertyType = propertyType;
			this.ParameterTypes = paramTypes;
			this.IsStatic = isStatic;
		}

		/// <summary>
		/// Gets the property's attributes.
		/// </summary>
		public PropertyAttributes Attributes { get; protected set; }

		/// <summary>
		/// Gets the property's builder.
		/// </summary>
		public PropertyBuilder Builder
		{
			get
			{
				if (_builder == null)
				{
					this._builder = this.TargetClass.Builder.DefineProperty(this.Name
					, this.Attributes
					, this.PropertyType.Target
					, this.ParameterTypes);
				}
				return _builder;
			}
		}
		/// <summary>
		/// Gets the property's calling conventions.
		/// </summary>
		public CallingConventions CallingConventions { get; protected set; }
		
		/// <summary>
		/// Gets the property's getter method.
		/// </summary>
		public EmittedMethod Getter { get; private set; }
		/// <summary>
		/// Gets the property's setter method.
		/// </summary>
		public EmittedMethod Setter { get; private set; }
		/// <summary>
		/// Indicates whether the property is readonly.
		/// </summary>
		public bool IsReadonly { get; set; }
		/// <summary>
		/// Gets the property's parameter types.
		/// </summary>
		public Type[] ParameterTypes { get; private set; }
		/// <summary>
		/// Gets the property's type.
		/// </summary>
		public TypeRef PropertyType { get; private set; }
		/// <summary>
		/// Gets a reference to the field to which the property is bound. (for bound properties)
		/// </summary>
		public IFieldRef BoundField { get; private set; }

		/// <summary>
		/// Adds a getter to a property.
		/// </summary>
		/// <returns>the emitted getter method</returns>
		public EmittedMethod AddGetter()
		{
			Contract.Requires<ArgumentNullException>(Getter == null, "Getter already assigned");

			Getter = this.TargetClass.DefineMethod(String.Format("get_{0}", this.Name));
			if (IsStatic) Getter.IncludeAttributes(MethodAttributes.Static);
			Getter.IncludeAttributes(MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.NewSlot);
			Getter.ReturnType = this.PropertyType;
			return Getter;
		}

		/// <summary>
		/// Adds a getter to a property by overriding the given method.
		/// </summary>
		/// <returns>the emitted getter method</returns>
		public EmittedMethod AddGetter(MethodInfo method)
		{
			Contract.Requires<ArgumentNullException>(Getter == null, "Getter already assigned");

			Getter = this.TargetClass.DefineOverrideMethod(method);
			return Getter;
		}

		/// <summary>
		/// Adds asetter to a property.
		/// </summary>
		/// <returns>the emitted setter method</returns>
		public EmittedMethod AddSetter()
		{
			Contract.Requires<ArgumentNullException>(Setter == null, "Setter already assigned");

			Setter = this.TargetClass.DefineMethod(String.Concat("set_", this.Name));
			if (IsStatic) Setter.IncludeAttributes(MethodAttributes.Static);
			Setter.IncludeAttributes(MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.NewSlot);
			Setter.DefineParameter("value", PropertyType);
			return Setter;
		}

		/// <summary>
		/// Adds a setter to a property by overriding the given method.
		/// </summary>
		/// <returns>the emitted setter method</returns>
		public EmittedMethod AddSetter(MethodInfo method)
		{
			Contract.Requires<ArgumentNullException>(Setter == null, "Setter already assigned");

			Setter = this.TargetClass.DefineOverrideMethod(method);
			return Setter;
		}

		/// <summary>
		/// Binds the property to an underlying field (simple getter and setter).
		/// </summary>
		/// <param name="field">a field ref</param>
		public void BindField(FieldInfo field)
		{
			Contract.Requires<ArgumentNullException>(field != null);
			Contract.Requires<ArgumentNullException>(field.IsStatic == IsStatic, "property scope must agree with backing field scope");
			Contract.Assert(BoundField == null, "cannot rebind field");

			BoundField = new RawFieldRef(field);
		}

		/// <summary>
		/// Binds the property to an underlying field (simple getter and setter).
		/// </summary>
		/// <param name="field">the field</param>
		public void BindField(EmittedField field)
		{
			Contract.Requires<ArgumentNullException>(field != null);
			Contract.Requires<ArgumentNullException>(field.IsStatic == IsStatic, "property scope must agree with backing field scope");
			Contract.Assert(BoundField == null, "cannot rebind field");
			
			BoundField = field;
		}

		/// <summary>
		/// Gets property info from the emitted property (not implemented).
		/// </summary>
		/// <returns>not implemented</returns>
		public PropertyInfo GetPropertyInfo()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Loads the address of a property (not implemented).
		/// </summary>
		/// <param name="il">IL</param>
		public void LoadAddress(ILGenerator il)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Emits instructions to load the property's value.
		/// </summary>
		/// <param name="il">IL</param>
		public void LoadValue(ILGenerator il)
		{
			if (!IsCompiled) Compile();
			il.LoadProperty(Builder, true);
		}

		/// <summary>
		/// Emits instructions to store the property's value.
		/// </summary>
		/// <param name="il">IL</param>
		public void StoreValue(ILGenerator il)
		{
			if (!IsCompiled) Compile();
			il.StoreProperty(Builder, true);
		}

		/// <summary>
		/// Compiles the property.
		/// </summary>
		protected internal override void OnCompile()
		{
			if (Getter == null && BoundField != null)
			{
				Getter = AddGetter();
				Getter.ContributeInstructions((m,il) =>
					{
						il.LoadArg_0();
						il.LoadField(BoundField);
					});
			}
			if (Getter != null)
			{
				if (!Getter.IsCompiled) Getter.Compile();
				Builder.SetGetMethod(Getter.Builder);
			}
			if (Setter == null && BoundField != null)
			{
				Setter = AddSetter();

				Setter.ContributeInstructions((m, il) =>
					{
						il.LoadArg_0();
						il.LoadArg_1();
						il.StoreField(BoundField);
					});
			}
			if (Setter != null)
			{
				if (!Setter.IsCompiled) Setter.Compile();
				Builder.SetSetMethod(Setter.Builder);
			}
		}

		/// <summary>
		/// Gets the property's target type.
		/// </summary>
		public Type TargetType
		{
			get { return PropertyType.Target; }
		}
	}
}