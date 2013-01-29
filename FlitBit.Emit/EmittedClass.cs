#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FlitBit.Core;

namespace FlitBit.Emit
{
	/// <summary>
	/// Helper class for working with a class in the IL stream.
	/// </summary>
	public class EmittedClass : EmittedMember
	{
		/// <summary>
		/// Default type attributes.
		/// </summary>
		public static readonly TypeAttributes DefaultTypeAttributes = TypeAttributes.BeforeFieldInit | TypeAttributes.Public;
		/// <summary>
		/// Static type attributes
		/// </summary>
		public static readonly TypeAttributes StaticTypeAttributes = TypeAttributes.Sealed | TypeAttributes.Abstract;

		readonly Dictionary<string, EmittedField> _fields = new Dictionary<string, EmittedField>();
		readonly Dictionary<string, EmittedGenericArgument> _genericArguments = new Dictionary<string, EmittedGenericArgument>();
		readonly List<TypeRef> _implementedInterfaces = new List<TypeRef>();
		readonly Dictionary<string, List<EmittedMember>> _members = new Dictionary<string, List<EmittedMember>>();
		readonly Dictionary<string, List<EmittedMethodBase>> _methods = new Dictionary<string, List<EmittedMethodBase>>();
		readonly Dictionary<string, EmittedProperty> _properties = new Dictionary<string, EmittedProperty>();
		readonly Dictionary<string, EmittedClass> _types = new Dictionary<string, EmittedClass>();
		readonly ModuleBuilder _module;
		readonly string _name;
		readonly EmittedClass _nestParent;
		readonly Type _supertype;
		TypeAttributes _attributes;
		TypeRef _ref;

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="module">emitted module, owner</param>
		/// <param name="name">the class' name</param>
		internal EmittedClass(ModuleBuilder module, string name)
			: this(module, name, TypeAttributes.BeforeFieldInit | TypeAttributes.Public, null, Type.EmptyTypes)
		{
			Contract.Requires<ArgumentNullException>(module != null);
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="module">emitted module, owner</param>
		/// <param name="name">the class' name</param>
		/// <param name="attributes">the class' attributes</param>
		internal EmittedClass(ModuleBuilder module, string name, TypeAttributes attributes)
			: this(module, name, attributes, null, Type.EmptyTypes)
		{
			Contract.Requires<ArgumentNullException>(module != null);
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="module">emitted module, owner</param>
		/// <param name="name">the class' name</param>
		/// <param name="attributes">the class' attributes</param>
		/// <param name="supertype">the class' supertype</param>
		/// <param name="interfaces">an array of interfaces the type will implement</param>
		internal EmittedClass(ModuleBuilder module, string name, TypeAttributes attributes
			, Type supertype, Type[] interfaces)
			: base(null, name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);
			Contract.Requires<ArgumentNullException>(module != null);

			_module = module;
			_name = name;
			_supertype = supertype;
			_implementedInterfaces = (interfaces == null)
				? new List<TypeRef>()
				: new List<TypeRef>(from i in interfaces
														select new TypeRef(i));

			_attributes = attributes;
			_ref = new EmittedTypeRef(this);
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="module">module</param>
		/// <param name="eclass">parent class, owner</param>
		/// <param name="name">the class' name</param>
		/// <param name="attributes">the class' attributes</param>
		/// <param name="supertype">the class' supertype</param>
		/// <param name="interfaces">an array of interfaces the type will implement</param>
		internal EmittedClass(ModuleBuilder module, EmittedClass eclass, string name, TypeAttributes attributes
			, Type supertype, Type[] interfaces)
			: base(eclass, name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);
			Contract.Requires<ArgumentNullException>(eclass != null, "eclass cannot be null");

			_module = module;
			_nestParent = eclass;
			_name = name;
			_supertype = supertype;
			_implementedInterfaces = (interfaces == null)
				? new List<TypeRef>()
				: new List<TypeRef>(from i in interfaces
														select new TypeRef(i));

			_attributes = attributes;
			_members = new Dictionary<string, List<EmittedMember>>();
			_fields = new Dictionary<string, EmittedField>();
			_properties = new Dictionary<string, EmittedProperty>();
			_methods = new Dictionary<string, List<EmittedMethodBase>>();
			_genericArguments = new Dictionary<string, EmittedGenericArgument>();
			_types = new Dictionary<string, EmittedClass>();
			_ref = new EmittedTypeRef(this);
		}

		/// <summary>
		/// Gets the class' attributes.
		/// </summary>
		public TypeAttributes Attributes
		{
			get { return _attributes; }
			set
			{
				_attributes = value;
				base.IsStatic = value.HasFlag(StaticTypeAttributes);
			}
		}

		TypeBuilder _builder;
		/// <summary>
		/// Gets the class' builder.
		/// </summary>
		public TypeBuilder Builder
		{
			get
			{
				if (_builder == null)
				{
					if (_module != null)
					{
						_builder = _module.DefineType(_name, _attributes, _supertype ?? typeof(Object)
							, (from i in _implementedInterfaces
								 select i.Target).ToArray()
									);
					}
					else
					{
						_nestParent.Compile();
						_builder = _nestParent.Builder.DefineNestedType(_name, _attributes, _supertype ?? typeof(Object)
							, (from i in _implementedInterfaces
								 select i.Target).ToArray()
								);
					}
					if (_genericArguments.Count > 0)
					{
						foreach (var arg in this.Builder.DefineGenericParameters(
							(from a in _genericArguments.Values
							 orderby a.Position
							 select a.Name).ToArray()
							))
						{
							_genericArguments[arg.Name].FinishDefinition(arg);
						}
					}
				}
				return _builder;
			}
		}

		/// <summary>
		/// Gets the class' fields.
		/// </summary>
		public IEnumerable<EmittedField> Fields
		{
			get
			{
				return _fields.Values.ToList();
			}
		}

		/// <summary>
		/// Get's a type ref to the emitted class.
		/// </summary>
		public TypeRef Ref
		{
			get { return _ref; }
		}

		/// <summary>
		/// Adds an interface to the list of interfaces the class implements.
		/// </summary>
		/// <param name="type">interface type</param>
		public void AddInterfaceImplementation(Type type)
		{
			Contract.Requires<ArgumentNullException>(type != null);
			Contract.Requires<ArgumentException>(type.IsInterface, "type must be an interface");

			if (_builder != null)
				_builder.AddInterfaceImplementation(type);
			else
				_implementedInterfaces.Add(new TypeRef(type));
		}

		/// <summary>
		/// Defines a default constructor.
		/// </summary>
		/// <returns>the emitted constructor</returns>
		public EmittedConstructor DefineCCtor()
		{
			EmittedConstructor result = new EmittedConstructor(this, "cctor");
			result.ExcludeAttributes(MethodAttributes.Public);
			result.IncludeAttributes(MethodAttributes.Private | MethodAttributes.Static);
			this.AddMethod(result);
			return result;
		}

		/// <summary>
		/// Defines a constructor.
		/// </summary>
		/// <returns>the constructor</returns>
		public EmittedConstructor DefineCtor()
		{
			EmittedConstructor result = new EmittedConstructor(this, "ctor");
			this.AddMethod(result);
			return result;
		}

		/// <summary>
		/// Defines a default constructor.
		/// </summary>
		/// <returns>the constructor</returns>
		public EmittedConstructor DefineDefaultCtor()
		{
			EmittedConstructor result = new EmittedConstructor(this, "ctor");
			this.AddMethod(result);
			result.ContributeInstructions((m, il) =>
			{
				if (_supertype != null)
				{
					ConstructorInfo superCtor = _supertype.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
						, null, Type.EmptyTypes, null
						);
					if (superCtor != null && !superCtor.IsPrivate)
					{
						il.LoadArg_0();
						il.Call(superCtor);
						il.Nop();
					}
				}
			});
			return result;
		}

		/// <summary>
		/// Defines a field of type T
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="fieldName">the field's name</param>
		/// <returns>the emitted field</returns>
		public EmittedField DefineField<T>(string fieldName)
		{
			Contract.Requires(fieldName != null);
			Contract.Requires(fieldName.Length > 0);

			return DefineField(fieldName, TypeRef.FromType<T>());
		}
		/// <summary>
		/// Defines a field.
		/// </summary>
		/// <param name="fieldName">the field's name</param>
		/// <param name="fieldType">the field's type</param>
		/// <returns>the emitted field</returns>
		public EmittedField DefineField(string fieldName, Type fieldType)
		{
			Contract.Requires<ArgumentNullException>(fieldName != null);
			Contract.Requires<ArgumentNullException>(fieldName.Length > 0);
			Contract.Requires<ArgumentNullException>(fieldType != null);

			return DefineField(fieldName, new TypeRef(fieldType));
		}
		/// <summary>
		/// Defines a field.
		/// </summary>
		/// <param name="fieldName">the field's name</param>
		/// <param name="fieldType">the field's type (ref)</param>
		/// <returns>the emitted field</returns>
		public EmittedField DefineField(string fieldName, TypeRef fieldType)
		{
			Contract.Requires<ArgumentNullException>(fieldName != null);
			Contract.Requires<ArgumentNullException>(fieldName.Length > 0);
			Contract.Requires<ArgumentNullException>(fieldType != null);

			EmittedField fld = new EmittedField(this, fieldName, fieldType);
			this.AddField(fld);
			return fld;
		}

		/// <summary>
		/// Defines generic arguments as defined on another generic type.
		/// </summary>
		/// <param name="generic">the generic type</param>
		public void DefineGenericParamentersFromType(Type generic)
		{
			Contract.Requires<ArgumentNullException>(generic != null);
			Contract.Assert(_builder == null, "generic arguments must be defined before the Builder is accessed");

			foreach (var a in generic.GetGenericArguments())
			{
				EmittedGenericArgument arg = new EmittedGenericArgument { Name = a.Name, Position = a.GenericParameterPosition, Attributes = a.GenericParameterAttributes };
				AddGenericArgument(arg);
				foreach (var c in a.GetGenericParameterConstraints())
				{
					if (c.IsInterface)
					{
						arg.AddInterfaceConstraint(c);
					}
					else
					{
						arg.AddBaseTypeConstraint(c);
					}
				}
			}
		}
		/// <summary>
		/// Defines a method.
		/// </summary>
		/// <param name="methodName">the method's name</param>
		/// <returns>the emitted method</returns>
		public EmittedMethod DefineMethod(string methodName)
		{
			Contract.Requires<ArgumentNullException>(methodName != null);

			EmittedMethod method = new EmittedMethod(this, methodName);
			this.AddMethod(method);
			return method;
		}
		/// <summary>
		/// Defines a method based on another method.
		/// </summary>
		/// <param name="method">the other method</param>
		/// <returns>the emitted method</returns>
		public EmittedMethod DefineMethodFromInfo(MethodInfo method)
		{
			Contract.Requires<ArgumentNullException>(method != null);

			EmittedMethod result = new EmittedMethod(this, method, false);
			this.AddMethod(result);
			return result;
		}

		/// <summary>
		/// Defines a nested type.
		/// </summary>
		/// <returns>the nested emitted type</returns>
		public EmittedClass DefineNestedType()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Defines a method that overrides another method.
		/// </summary>
		/// <param name="method">the method to override</param>
		/// <returns>an emitted method</returns>
		public EmittedMethod DefineOverrideMethod(MethodInfo method)
		{
			Contract.Requires<ArgumentNullException>(method != null);

			EmittedMethod result = new EmittedMethod(this, method, true);
			this.AddMethod(result);
			return result;
		}

		/// <summary>
		/// Defines a property of type T
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="propertyName">the property's name</param>
		/// <returns>the emitted property</returns>
		public EmittedProperty DefineProperty<T>(string propertyName)
		{
			return DefineProperty(propertyName, typeof(T));
		}

		/// <summary>
		/// Defines a property
		/// </summary>
		/// <param name="propertyName">the property's name</param>
		/// <param name="propertyType">the property's type</param>
		/// <returns>the emitted property</returns>
		public EmittedProperty DefineProperty(string propertyName, Type propertyType)
		{
			Contract.Requires<ArgumentNullException>(propertyName != null);
			Contract.Requires<ArgumentNullException>(propertyName.Length > 0);
			Contract.Requires<ArgumentNullException>(propertyType != null);

			EmittedProperty prop = new EmittedProperty(this, propertyName, new TypeRef(propertyType), false);
			this.AddProperty(prop);
			return prop;
		}

		/// <summary>
		/// Defines a property based on another property.
		/// </summary>
		/// <param name="property">the other property</param>
		/// <returns>the emitted property</returns>
		public EmittedProperty DefinePropertyFromPropertyInfo(PropertyInfo property)
		{
			Contract.Requires<ArgumentNullException>(property != null, "property cannot be null");
			Type[] @params = property.GetIndexParameters().Select(parameter => parameter.ParameterType).ToArray();
			var isStatic = (property.CanRead && property.GetGetMethod().IsStatic) || (property.CanWrite && property.GetSetMethod().IsStatic);
			EmittedProperty prop = new EmittedProperty(this,
				property.Name,
				new TypeRef(property.PropertyType),
				@params,
				isStatic
				);
			this.AddProperty(prop);
			return prop;
		}

		/// <summary>
		/// Defines a property with a backing field of type T
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="propertyName">the property's name</param>
		/// <returns>the emitted property</returns>
		public EmittedProperty DefinePropertyWithBackingField<T>(string propertyName)
		{
			Contract.Requires<ArgumentNullException>(propertyName != null);
			Contract.Requires<ArgumentNullException>(propertyName.Length > 0);
			return DefinePropertyWithBackingField(propertyName, typeof(T));
		}

		/// <summary>
		/// Defines a property with a backing field.
		/// </summary>
		/// <param name="propertyName">the property's name</param>
		/// <param name="propertyType">the property's type</param>
		/// <returns>an emitted property</returns>
		public EmittedProperty DefinePropertyWithBackingField(string propertyName, Type propertyType)
		{
			Contract.Requires<ArgumentNullException>(propertyName != null);
			Contract.Requires<ArgumentNullException>(propertyName.Length > 0);
			Contract.Requires<ArgumentNullException>(propertyType != null);

			EmittedProperty prop = new EmittedProperty(this, propertyName, new TypeRef(propertyType), false);
			prop.BindField(DefineField(String.Concat("<", propertyName, ">_field"), propertyType));
			this.AddProperty(prop);
			return prop;
		}

		/// <summary>
		/// Compiles the emitted type.
		/// </summary>
		protected internal override void OnCompile()
		{
			var builder = this.Builder;
			foreach (var m in _fields.Values)
			{
				m.Compile();
			}
			foreach (var mm in _methods.Values)
			{
				// May be multiple methods, overloaded
				foreach (var m in mm)
				{					
					m.Compile();
				}
			}
			foreach (var m in _properties.Values)
			{
				m.Compile();
			}
			foreach (var m in _types.Values)
			{
				m.Compile();
			}
			var runtimeType = builder.CreateType();
			_ref = new TypeRef(runtimeType);
		}

		private void AddField(EmittedField field)
		{
			Contract.Requires<ArgumentNullException>(field != null);
			Contract.Requires<ArgumentNullException>(field.Name != null);
			Contract.Requires<ArgumentNullException>(field.Name.Length > 0);

			CheckMemberName(field.Name);
			_fields.Add(field.Name, field);
			this.AddMember(field);
		}

		private void AddGenericArgument(EmittedGenericArgument arg)
		{
			Contract.Assert(arg != null);
			Contract.Assert(_genericArguments != null);
			Contract.Assert(_builder == null, "generic arguments must be defined before the Builder is accessed");
			_genericArguments.Add(arg.Name, arg);
		}

		private void AddMember(EmittedMember m)
		{
			Contract.Requires<ArgumentNullException>(m != null);

			List<EmittedMember> members;
			if (_members.TryGetValue(m.Name, out members))
			{
				members.Add(m);
			}
			else
			{
				members = new List<EmittedMember>();
				_members.Add(m.Name, members);
				members.Add(m);
			}
		}

		private void AddMethod(EmittedMethodBase method)
		{
			Contract.Requires<ArgumentNullException>(method != null);

			List<EmittedMethodBase> methods;
			if (_methods.TryGetValue(method.Name, out methods))
			{
				methods.Add(method);
			}
			else
			{
				methods = new List<EmittedMethodBase>();
				_methods.Add(method.Name, methods);
				methods.Add(method);
			}
			this.AddMember(method);
		}

		private void AddProperty(EmittedProperty prop)
		{
			Contract.Requires<ArgumentNullException>(prop != null);

			CheckMemberName(prop.Name);
			_properties.Add(prop.Name, prop);
			this.AddMember(prop);
		}

		private void CheckMemberName(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			if (_members.ContainsKey(name)) throw new InvalidOperationException(String.Format(
				@"Type already contains a member by the same name: member name = {0} type = {1}", name, this.Name));
		}

		internal MethodInfo GetMethod(string name, Type[] args)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);
			Contract.Requires<ArgumentNullException>(args != null);

			List<EmittedMethodBase> methods;
			if (_methods.TryGetValue(name, out methods))
			{
				var mm = (from m in methods
									where m.ParameterTypes.EqualsOrItemsEqual(args)
									select m).SingleOrDefault();
				if (mm != null)
				{
					if (mm is EmittedMethod)
						return ((EmittedMethod)mm).Builder;
				}
			}
			return null;
		}

		internal ConstructorInfo GetConstructor(Type[] args)
		{
			Contract.Requires<ArgumentNullException>(args != null);

			List<EmittedMethodBase> methods;
			if (_methods.TryGetValue(".ctor", out methods))
			{
				var mm = (from m in methods
									where m.ParameterTypes.EqualsOrItemsEqual(args)
									select m).SingleOrDefault();
				if (mm != null)
				{
					if (mm is EmittedMethod)
						return ((EmittedConstructor)mm).Builder;
				}
			}
			return null;
		}

		/// <summary>
		/// Tries to get a property by name.
		/// </summary>
		/// <param name="name">the property's name</param>
		/// <param name="prop">variable to hold the property upon success</param>
		/// <returns>true if successful; otherwise false</returns>
		public bool TryGetProperty(string name, out EmittedProperty prop)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);
			return _properties.TryGetValue(name, out prop);
		}

		/// <summary>
		/// Produces stubs for all methods of an interface.
		/// </summary>
		/// <param name="intf">the interface</param>
		/// <param name="skipGetters">whether to skip getters</param>
		/// <param name="skipSetters">whether to skip setters</param>
		public void StubMethodsForInterface(Type intf, bool skipGetters, bool skipSetters)
		{
			Contract.Requires<ArgumentNullException>(intf != null);
			foreach (var m in intf.GetMethods())
			{
				if (skipGetters && m.Name.StartsWith("get_"))
				{
					var p = intf.GetProperty(m.Name.Substring(4));
					if (p != null && p.GetGetMethod() == m)
						continue;
				}

				if (skipSetters && m.Name.StartsWith("set_"))
				{
					var p = intf.GetProperty(m.Name.Substring(4));
					if (p != null && p.GetSetMethod() == m)
						continue;
				}

				if (this._supertype != null && this._supertype.GetMethod(m.Name, m.GetParameterTypes()) == null)
				{
					DefineOverrideMethod(m).ContributeInstructions((mb, il) =>
					{
						il.Nop();
						il.NewObj(typeof(NotImplementedException).GetConstructor(Type.EmptyTypes));
						il.Throw(typeof(NotImplementedException));
					});
				}
			}
		}

		[ContractInvariantMethod]
		void InvariantContracts()
		{
			Contract.Invariant(_module != null);
			Contract.Invariant(_fields != null);
			Contract.Invariant(_members != null);
			Contract.Invariant(this.Name != null);
			Contract.Invariant(this.Name.Length > 0);
		}
	}
}