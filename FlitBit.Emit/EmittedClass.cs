#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FlitBit.Core;

namespace FlitBit.Emit
{
	/// <summary>
	///   Helper class for working with a class in the IL stream.
	/// </summary>
	public class EmittedClass : EmittedMember
	{
		/// <summary>
		///   Default type attributes.
		/// </summary>
		public static readonly TypeAttributes DefaultTypeAttributes = TypeAttributes.BeforeFieldInit | TypeAttributes.Public;

		/// <summary>
		///   Static type attributes
		/// </summary>
		public static readonly TypeAttributes StaticTypeAttributes = TypeAttributes.Sealed | TypeAttributes.Abstract;

		private readonly IList<CustomAttributeDescriptor> _customAttr = new List<CustomAttributeDescriptor>();
		private readonly Dictionary<string, EmittedField> _fields = new Dictionary<string, EmittedField>();

		private readonly Dictionary<string, EmittedGenericArgument> _genericArguments =
			new Dictionary<string, EmittedGenericArgument>();

		private readonly List<TypeRef> _implementedInterfaces = new List<TypeRef>();
		private readonly Dictionary<string, List<EmittedMember>> _members = new Dictionary<string, List<EmittedMember>>();

		private readonly Dictionary<string, List<EmittedMethodBase>> _methods =
			new Dictionary<string, List<EmittedMethodBase>>();

		private readonly ModuleBuilder _module;
		private readonly string _name;
		private readonly EmittedClass _nestParent;
		private readonly Dictionary<string, EmittedProperty> _properties = new Dictionary<string, EmittedProperty>();
		private readonly Type _supertype;
		private readonly Dictionary<string, EmittedClass> _types = new Dictionary<string, EmittedClass>();
		private TypeAttributes _attributes;
		private TypeBuilder _builder;
		private TypeRef _ref;

		/// <summary>
		///   Creates a new instance.
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
		///   Creates a new instance.
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
		///   Creates a new instance.
		/// </summary>
		/// <param name="module">emitted module, owner</param>
		/// <param name="name">the class' name</param>
		/// <param name="attributes">the class' attributes</param>
		/// <param name="supertype">the class' supertype</param>
		/// <param name="interfaces">an array of interfaces the type will implement</param>
		internal EmittedClass(ModuleBuilder module, string name, TypeAttributes attributes
			, Type supertype, IEnumerable<Type> interfaces)
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
		///   Creates a new instance.
		/// </summary>
		/// <param name="module">module</param>
		/// <param name="eclass">parent class, owner</param>
		/// <param name="name">the class' name</param>
		/// <param name="attributes">the class' attributes</param>
		/// <param name="supertype">the class' supertype</param>
		/// <param name="interfaces">an array of interfaces the type will implement</param>
		internal EmittedClass(ModuleBuilder module, EmittedClass eclass, string name, TypeAttributes attributes
			, Type supertype, IEnumerable<Type> interfaces)
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
		///   Gets the class' attributes.
		/// </summary>
		public TypeAttributes Attributes
		{
			get { return _attributes; }
			set
			{
				_attributes = value;
				IsStatic = value.HasFlag(StaticTypeAttributes);
			}
		}

		/// <summary>
		///   Gets the class' builder.
		/// </summary>
		public TypeBuilder Builder
		{
			get
			{
				if (_builder == null)
				{
					if (_module != null)
					{
						_builder = _module.DefineType(_name, _attributes, _supertype ?? typeof (Object)
							, (from i in _implementedInterfaces
								select i.Target).ToArray()
							);
					}
					else
					{
						_nestParent.Compile();
						_builder = _nestParent.Builder.DefineNestedType(_name, _attributes, _supertype ?? typeof (Object)
							, (from i in _implementedInterfaces
								select i.Target).ToArray()
							);
					}
					if (_genericArguments.Count > 0)
					{
						foreach (GenericTypeParameterBuilder arg in Builder.DefineGenericParameters(
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
		///   Gets the class' fields.
		/// </summary>
		public IEnumerable<EmittedField> Fields
		{
			get { return _fields.Values.ToList(); }
		}

		/// <summary>
		///   Get's a type ref to the emitted class.
		/// </summary>
		public TypeRef Ref
		{
			get { return _ref; }
		}

		/// <summary>
		///   Adds an interface to the list of interfaces the class implements.
		/// </summary>
		/// <param name="type">interface type</param>
		public void AddInterfaceImplementation(Type type)
		{
			Contract.Requires<ArgumentNullException>(type != null);
			Contract.Requires<ArgumentException>(type.IsInterface, "type must be an interface");

			if (_builder != null)
			{
				_builder.AddInterfaceImplementation(type);
			}
			else
			{
				_implementedInterfaces.Add(new TypeRef(type));
			}
		}

		/// <summary>
		///   Defines a default constructor.
		/// </summary>
		/// <returns>the emitted constructor</returns>
		public EmittedConstructor DefineCCtor()
		{
			var result = new EmittedConstructor(this, "cctor");
			result.ExcludeAttributes(MethodAttributes.Public);
			result.IncludeAttributes(MethodAttributes.Private | MethodAttributes.Static);
			AddMethod(result);
			return result;
		}

		/// <summary>
		///   Defines a constructor.
		/// </summary>
		/// <returns>the constructor</returns>
		public EmittedConstructor DefineCtor()
		{
			var result = new EmittedConstructor(this, "ctor");
			AddMethod(result);
			return result;
		}

		/// <summary>
		///   Defines a default constructor.
		/// </summary>
		/// <returns>the constructor</returns>
		public EmittedConstructor DefineDefaultCtor()
		{
			var result = new EmittedConstructor(this, "ctor");
			AddMethod(result);
			result.ContributeInstructions((m, il) =>
			{
				if (_supertype != null)
				{
					ConstructorInfo superCtor =
						_supertype.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
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
		///   Defines a field of type T
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
		///   Defines a field.
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
		///   Defines a field.
		/// </summary>
		/// <param name="fieldName">the field's name</param>
		/// <param name="fieldType">the field's type (ref)</param>
		/// <returns>the emitted field</returns>
		public EmittedField DefineField(string fieldName, TypeRef fieldType)
		{
			Contract.Requires<ArgumentNullException>(fieldName != null);
			Contract.Requires<ArgumentNullException>(fieldName.Length > 0);
			Contract.Requires<ArgumentNullException>(fieldType != null);

			var fld = new EmittedField(this, fieldName, fieldType);
			AddField(fld);
			return fld;
		}

		/// <summary>
		///   Defines generic arguments as defined on another generic type.
		/// </summary>
		/// <param name="generic">the generic type</param>
		public void DefineGenericParamentersFromType(Type generic)
		{
			Contract.Requires<ArgumentNullException>(generic != null);
			Contract.Assert(_builder == null, "generic arguments must be defined before the Builder is accessed");

			foreach (Type a in generic.GetGenericArguments())
			{
				var arg = new EmittedGenericArgument
				{
					Name = a.Name,
					Position = a.GenericParameterPosition,
					Attributes = a.GenericParameterAttributes
				};
				AddGenericArgument(arg);
				foreach (Type c in a.GetGenericParameterConstraints())
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
		///   Defines a method.
		/// </summary>
		/// <param name="methodName">the method's name</param>
		/// <returns>the emitted method</returns>
		public EmittedMethod DefineMethod(string methodName)
		{
			Contract.Requires<ArgumentNullException>(methodName != null);

			var method = new EmittedMethod(this, methodName);
			AddMethod(method);
			return method;
		}

		/// <summary>
		///   Defines a method based on another method.
		/// </summary>
		/// <param name="method">the other method</param>
		/// <returns>the emitted method</returns>
		public EmittedMethod DefineMethodFromInfo(MethodInfo method)
		{
			Contract.Requires<ArgumentNullException>(method != null);

			var result = new EmittedMethod(this, method, false);
			AddMethod(result);
			return result;
		}

		/// <summary>
		///   Defines a nested type.
		/// </summary>
		/// <returns>the nested emitted type</returns>
		public EmittedClass DefineNestedType()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Defines a method that overrides another method.
		/// </summary>
		/// <param name="method">the method to override</param>
		/// <returns>an emitted method</returns>
		public EmittedMethod DefineOverrideMethod(MethodInfo method)
		{
			Contract.Requires<ArgumentNullException>(method != null);

			var result = new EmittedMethod(this, method, true);
			AddMethod(result);
			return result;
		}

		/// <summary>
		///   Defines a property of type T
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="propertyName">the property's name</param>
		/// <returns>the emitted property</returns>
		public EmittedProperty DefineProperty<T>(string propertyName)
		{
			return DefineProperty(propertyName, typeof (T));
		}

		/// <summary>
		///   Defines a property
		/// </summary>
		/// <param name="propertyName">the property's name</param>
		/// <param name="propertyType">the property's type</param>
		/// <returns>the emitted property</returns>
		public EmittedProperty DefineProperty(string propertyName, Type propertyType)
		{
			Contract.Requires<ArgumentNullException>(propertyName != null);
			Contract.Requires<ArgumentNullException>(propertyName.Length > 0);
			Contract.Requires<ArgumentNullException>(propertyType != null);

			var prop = new EmittedProperty(this, propertyName, new TypeRef(propertyType), false);
			AddProperty(prop);
			return prop;
		}

		/// <summary>
		///   Defines a property based on another property.
		/// </summary>
		/// <param name="property">the other property</param>
		/// <returns>the emitted property</returns>
		public EmittedProperty DefinePropertyFromPropertyInfo(PropertyInfo property)
		{
			Contract.Requires<ArgumentNullException>(property != null, "property cannot be null");
			Type[] @params = property.GetIndexParameters().Select(parameter => parameter.ParameterType).ToArray();
			bool isStatic = (property.CanRead && property.GetGetMethod().IsStatic) ||
			                (property.CanWrite && property.GetSetMethod().IsStatic);
			var prop = new EmittedProperty(this,
				property.Name,
				new TypeRef(property.PropertyType),
				@params,
				isStatic
				);
			AddProperty(prop);
			return prop;
		}

		/// <summary>
		///   Defines a property with a backing field of type T
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="propertyName">the property's name</param>
		/// <returns>the emitted property</returns>
		public EmittedProperty DefinePropertyWithBackingField<T>(string propertyName)
		{
			Contract.Requires<ArgumentNullException>(propertyName != null);
			Contract.Requires<ArgumentNullException>(propertyName.Length > 0);
			return DefinePropertyWithBackingField(propertyName, typeof (T));
		}

		/// <summary>
		///   Defines a property with a backing field.
		/// </summary>
		/// <param name="propertyName">the property's name</param>
		/// <param name="propertyType">the property's type</param>
		/// <returns>an emitted property</returns>
		public EmittedProperty DefinePropertyWithBackingField(string propertyName, Type propertyType)
		{
			Contract.Requires<ArgumentNullException>(propertyName != null);
			Contract.Requires<ArgumentNullException>(propertyName.Length > 0);
			Contract.Requires<ArgumentNullException>(propertyType != null);

			var prop = new EmittedProperty(this, propertyName, new TypeRef(propertyType), false);
			prop.BindField(DefineField(String.Concat("<", propertyName, ">_field"), propertyType));
			AddProperty(prop);
			return prop;
		}

		/// <summary>
		///   Sets a custom attribute for the emitted class.
		/// </summary>
		/// <param name="constructor">the attribute's constructor</param>
		/// <param name="constructorArgs">arguments for the constructor</param>
		public void SetCustomAttribute(ConstructorInfo constructor, object[] constructorArgs)
		{
			Contract.Requires<ArgumentNullException>(constructor != null);
			var descr = new CustomAttributeDescriptor
			{
				Ctor = constructor,
				Args = constructorArgs ?? new object[0]
			};
			_customAttr.Add(descr);
		}

		/// <summary>
		///   Sets a custom attribute for the emitted class; for constructors that don't take arguments.
		/// </summary>
		/// <param name="constructor">the attribute's constructor</param>
		public void SetCustomAttribute(ConstructorInfo constructor)
		{
			Contract.Requires<ArgumentNullException>(constructor != null);

			SetCustomAttribute(constructor, new object[0]);
		}

		/// <summary>
		///   Sets a custom attribute for the emitted class; uses the attribute's default constructor.
		/// </summary>
		public void SetCustomAttribute<T>()
			where T : Attribute
		{
			ConstructorInfo ctor = typeof (T).GetConstructor(Type.EmptyTypes);
			SetCustomAttribute(ctor, new object[0]);
		}

		/// <summary>
		///   Produces stubs for all methods of an interface.
		/// </summary>
		/// <param name="intf">the interface</param>
		/// <param name="skipGetters">whether to skip getters</param>
		/// <param name="skipSetters">whether to skip setters</param>
		public void StubMethodsForInterface(Type intf, bool skipGetters, bool skipSetters)
		{
			Contract.Requires<ArgumentNullException>(intf != null);
			foreach (MethodInfo m in intf.GetMethods())
			{
				if (skipGetters && m.Name.StartsWith("get_"))
				{
					PropertyInfo p = intf.GetProperty(m.Name.Substring(4));
					if (p != null && p.GetGetMethod() == m)
					{
						continue;
					}
				}

				if (skipSetters && m.Name.StartsWith("set_"))
				{
					PropertyInfo p = intf.GetProperty(m.Name.Substring(4));
					if (p != null && p.GetSetMethod() == m)
					{
						continue;
					}
				}

				if (_supertype != null && _supertype.GetMethod(m.Name, m.GetParameterTypes()) == null)
				{
					DefineOverrideMethod(m).ContributeInstructions((mb, il) =>
					{
						il.Nop();
						il.NewObj(typeof (NotImplementedException).GetConstructor(Type.EmptyTypes));
						il.Throw(typeof (NotImplementedException));
					});
				}
			}
		}

		/// <summary>
		///   Tries to get a property by name.
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

		internal ConstructorInfo GetConstructor(Type[] args)
		{
			Contract.Requires<ArgumentNullException>(args != null);

			List<EmittedMethodBase> methods;
			if (_methods.TryGetValue(".ctor", out methods))
			{
				EmittedMethodBase mm = (from m in methods
					where m.ParameterTypes.EqualsOrItemsEqual(args)
					select m).SingleOrDefault();
				if (mm != null)
				{
					var constructor = mm as EmittedConstructor;
					if (constructor != null)
					{
						return constructor.Builder;
					}
				}
			}
			return null;
		}

		internal MethodInfo GetMethod(string name, Type[] args)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);
			Contract.Requires<ArgumentNullException>(args != null);

			List<EmittedMethodBase> methods;
			if (_methods.TryGetValue(name, out methods))
			{
				EmittedMethodBase mm = (from m in methods
					where m.ParameterTypes.EqualsOrItemsEqual(args)
					select m).SingleOrDefault();
				if (mm != null)
				{
					var method = mm as EmittedMethod;
					if (method != null)
					{
						return method.Builder;
					}
				}
			}
			return null;
		}

		private void AddField(EmittedField field)
		{
			Contract.Requires<ArgumentNullException>(field != null);
			Contract.Requires<ArgumentNullException>(field.Name != null);
			Contract.Requires<ArgumentNullException>(field.Name.Length > 0);

			CheckMemberName(field.Name);
			_fields.Add(field.Name, field);
			AddMember(field);
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
			AddMember(method);
		}

		private void AddProperty(EmittedProperty prop)
		{
			Contract.Requires<ArgumentNullException>(prop != null);

			CheckMemberName(prop.Name);
			_properties.Add(prop.Name, prop);
			AddMember(prop);
		}

		private void CheckMemberName(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			if (_members.ContainsKey(name))
			{
				throw new InvalidOperationException(String.Format(
					@"Type already contains a member by the same name: member name = {0} type = {1}", name,
					Name));
			}
		}

		/// <summary>
		///   Specializes the base class' GetHashCode method.
		/// </summary>
		/// <param name="hashCodeSeed">a value reference to a hash code seed value.</param>
		/// <param name="filtered">function that filters the class' fields from inclusion in the hashcode</param>
		/// <param name="specialize">function that specializes the hashcode algorithm for a particular field</param>
		/// <returns>the emitted, specialized GetHashCode method</returns>
		/// <exception cref="ArgumentNullException">thrown if <paramref name="hashCodeSeed" /> is null.</exception>
		/// <exception cref="InvalidOperationException">
		///   thrown if a field member cannot be included in the hashcode; these should
		///   be handled by the provided <paramref name="specialize" /> method.
		/// </exception>
		public EmittedMethod SpecializeGetHashCode(IValueRef hashCodeSeed,
			Func<EmittedField, bool> filtered,
			Func<EmittedClass, EmittedField, int, LocalBuilder, ILGenerator, bool> specialize)
		{
			Contract.Requires<ArgumentNullException>(hashCodeSeed != null);

			EmittedMethod method = DefineMethod("GetHashCode");
			method.ClearAttributes();
			method.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual);
			method.ReturnType = TypeRef.FromType<int>();
			method.ContributeInstructions((m, il) =>
			{
				LocalBuilder result = il.DeclareLocal(typeof (Int32));
				il.DeclareLocal(typeof (Int32));
				il.DeclareLocal(typeof (bool));
				il.Nop();
				il.LoadValue(hashCodeSeed);
				il.LoadValue(Constants.NotSoRandomPrime);
				il.Multiply();
				il.StoreLocal(result);
				Label exit = il.DefineLabel();
				var fields =
					new List<EmittedField>(
						Fields.Where(f => f.IsStatic == false && (filtered == null || !filtered(f))));
				foreach (EmittedField field in fields)
				{
					if (specialize != null && specialize(this, field, Constants.NotSoRandomPrime, result, il))
					{
						continue;
					}
					Type fieldType = field.FieldType.Target;
					TypeCode tc = Type.GetTypeCode(fieldType);
					Label lbl;
					switch (tc)
					{
						case TypeCode.Boolean:
							il.LoadLocal(result);
							il.LoadValue(Constants.NotSoRandomPrime);
							il.LoadArg_0();
							il.LoadField(field);
							MethodInfo conv = typeof (Convert).GetMethod("ToInt32", BindingFlags.Static | BindingFlags.Public, null,
								new[] {typeof (bool)}, null);
							il.Call(conv);
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							break;
						case TypeCode.Byte:
						case TypeCode.Char:
						case TypeCode.Int16:
						case TypeCode.Int32:
						case TypeCode.SByte:
						case TypeCode.UInt16:
						case TypeCode.UInt32:
							il.LoadLocal(result);
							il.LoadValue(Constants.NotSoRandomPrime);
							il.LoadArg_0();
							il.LoadField(field);
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							break;
						case TypeCode.Single:
							il.LoadLocal(result);
							il.LoadValue(Constants.NotSoRandomPrime);
							il.ConvertToFloat32();
							il.LoadArg_0();
							il.LoadField(field);
							il.Multiply();
							il.ConvertToInt32();
							il.Xor();
							il.StoreLocal(result);
							break;
						case TypeCode.DateTime:
							il.LoadLocal(result);
							il.LoadValue(Constants.NotSoRandomPrime);
							il.LoadArg_0();
							il.LoadFieldAddress(field);
							il.Constrained(typeof (DateTime));
							il.CallVirtual<object>("GetHashCode");
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							break;
						case TypeCode.Decimal:
							il.LoadLocal(result);
							il.LoadValue(Constants.NotSoRandomPrime);
							il.LoadArg_0();
							il.LoadFieldAddress(field);
							il.Call<Decimal>("GetHashCode");
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							break;
						case TypeCode.Double:
							il.LoadLocal(result);
							il.LoadValue(Constants.NotSoRandomPrime);
							il.LoadArg_0();
							il.LoadFieldAddress(field);
							il.Call<Double>("GetHashCode");
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							break;
						case TypeCode.Int64:
							il.LoadLocal(result);
							il.LoadValue(Constants.NotSoRandomPrime);
							il.LoadArg_0();
							il.LoadFieldAddress(field);
							il.Constrained(typeof (Int64));
							il.CallVirtual<object>("GetHashCode");
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							break;
						case TypeCode.Object:
							if (fieldType.IsValueType)
							{
								il.LoadLocal(result);
								il.LoadValue(Constants.NotSoRandomPrime);
								il.LoadArg_0();
								il.LoadFieldAddress(field);
								il.Constrained(fieldType);
								il.CallVirtual<object>("GetHashCode");
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
							}
							else if (fieldType.IsArray)
							{
								Type elmType = fieldType.GetElementType();
								il.LoadLocal(result);
								il.LoadValue(Constants.NotSoRandomPrime);
								il.LoadArg_0();
								il.LoadField(field);
								il.LoadLocal(result);
								il.Call(typeof (Core.Extensions).GetMethod("CalculateCombinedHashcode",
									BindingFlags.Public | BindingFlags.Static)
									.MakeGenericMethod(elmType));
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
							}
							else
							{
								il.LoadArg_0();
								il.LoadField(field);
								il.LoadNull();
								il.CompareEqual();
								il.StoreLocal_2();
								il.LoadLocal_2();
								lbl = il.DefineLabel();
								il.BranchIfTrue_ShortForm(lbl);
								il.LoadLocal(result);
								il.LoadValue(Constants.NotSoRandomPrime);
								il.LoadArg_0();
								il.LoadField(field);
								il.CallVirtual<object>("GetHashCode");
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
								il.MarkLabel(lbl);
							}
							break;
						case TypeCode.String:
							il.LoadArg_0();
							il.LoadField(field);
							il.LoadNull();
							il.CompareEqual();
							il.StoreLocal_2();
							il.LoadLocal_2();
							lbl = il.DefineLabel();
							il.BranchIfTrue_ShortForm(lbl);
							il.Nop();
							il.LoadLocal(result);
							il.LoadValue(Constants.NotSoRandomPrime);
							il.LoadArg_0();
							il.LoadField(field);
							il.CallVirtual<object>("GetHashCode");
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							il.MarkLabel(lbl);
							break;
						case TypeCode.UInt64:
							il.LoadLocal(result);
							il.LoadValue(Constants.NotSoRandomPrime);
							il.LoadArg_0();
							il.LoadFieldAddress(field);
							il.Constrained(typeof (UInt64));
							il.CallVirtual<object>("GetHashCode");
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							break;
						default:
							throw new InvalidOperationException(String.Concat("Unable to produce hashcode for type: ",
								fieldType.GetReadableFullName()));
					}
				}
				il.LoadLocal(result);
				il.StoreLocal_1();
				il.Branch(exit);
				il.MarkLabel(exit);
				il.LoadLocal_1();
			});
			return method;
		}

		/// <summary>
		///   Specializes the base class' Equals method.
		/// </summary>
		/// <param name="equatables">the types to be implemented as IEquatable&lt;></param>
		/// <param name="filtered">function that filters the class' fields from inclusion in equality</param>
		/// <param name="specialize">function that specializes the equality for a particular field</param>
		/// <returns>the emitted, specialized Equals method</returns>
		public EmittedMethod SpecializeEquals(
			IEnumerable<Type> equatables,
			Func<EmittedField, bool> filtered,
			Func<EmittedClass, EmittedField, ILGenerator, Label, bool> specialize)
		{
			Type equatable = typeof (IEquatable<>).MakeGenericType(Builder);
			AddInterfaceImplementation(equatable);

			EmittedMethod specializedEquals = DefineMethod("Equals");
			specializedEquals.ClearAttributes();
			specializedEquals.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
			                                    MethodAttributes.Virtual | MethodAttributes.Final);
			specializedEquals.ReturnType = TypeRef.FromType<bool>();
			EmittedParameter other = specializedEquals.DefineParameter("other", Ref);

			specializedEquals.ContributeInstructions((m, il) =>
			{
				il.DeclareLocal(typeof (bool));
				Label exitFalse = il.DefineLabel();
				il.Nop();

				var fields =
					new List<EmittedField>(
						Fields.Where(f => f.IsStatic == false && (filtered == null || !filtered(f))));
				for (int i = 0; i < fields.Count; i++)
				{
					EmittedField field = fields[i];
					if (specialize != null && specialize(this, field, il, exitFalse))
					{
						continue;
					}
					Type fieldType = field.FieldType.Target;
					if (fieldType.IsArray)
					{
						Type elmType = fieldType.GetElementType();
						LoadFieldsFromThisAndParam(il, field, other);
						il.Call(typeof (Core.Extensions).GetMethod("EqualsOrItemsEqual", BindingFlags.Static | BindingFlags.Public)
							.MakeGenericMethod(elmType));
					}
					else if (fieldType.IsClass)
					{
						if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof (ObservableCollection<>))
						{
							// compare observable collections for member equality...
							Type genericArg = fieldType.GetGenericArguments()[0];
							Type etype = typeof (IEnumerable<>).MakeGenericType(genericArg);
							MethodInfo sequenceEquals = typeof (Enumerable).MatchGenericMethod("SequenceEqual",
								BindingFlags.Static | BindingFlags.Public, 1, typeof (bool), etype, etype);
							LoadFieldsFromThisAndParam(il, field, other);
							il.Call(sequenceEquals.MakeGenericMethod(genericArg));
						}
						else
						{
							MethodInfo opEquality = fieldType.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);
							if (opEquality != null)
							{
								LoadFieldsFromThisAndParam(il, field, other);
								il.Call(opEquality);
							}
							else
							{
								il.Call(typeof (EqualityComparer<>).MakeGenericType(fieldType)
									.GetMethod("get_Default", BindingFlags.Static | BindingFlags.Public));
								LoadFieldsFromThisAndParam(il, field, other);
								il.CallVirtual(typeof (IEqualityComparer<>).MakeGenericType(fieldType)
									.GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance,
										null,
										new[] {fieldType, fieldType},
										null
									));
							}
						}
					}
					else
					{
						LoadFieldsFromThisAndParam(il, field, other);
						il.CompareEquality(fieldType);
					}
					if (i < fields.Count - 1)
					{
						il.BranchIfFalse(exitFalse);
					}
				}
				Label exit = il.DefineLabel();
				il.Branch(exit);
				il.MarkLabel(exitFalse);
				il.Load_I4_0();
				il.MarkLabel(exit);
				il.StoreLocal_0();
				Label fin = il.DefineLabel();
				il.Branch(fin);
				il.MarkLabel(fin);
				il.LoadLocal_0();
			});

			var contributedEquals = new Action<EmittedMethodBase, ILGenerator>((m, il) =>
			{
				Label exitFalse2 = il.DefineLabel();
				Label exit = il.DefineLabel();
				il.DeclareLocal(typeof (bool));
				il.Nop();
				il.LoadArg_1();
				il.IsInstance(Builder);
				il.BranchIfFalse(exitFalse2);
				il.LoadArg_0();
				il.LoadArg_1();
				il.CastClass(Builder);
				il.Call(specializedEquals);
				il.Branch(exit);
				il.MarkLabel(exitFalse2);
				il.LoadValue(false);
				il.MarkLabel(exit);
				il.StoreLocal_0();
				Label fin = il.DefineLabel();
				il.Branch(fin);
				il.MarkLabel(fin);
				il.LoadLocal_0();
			});

			if (equatables != null)
			{
				foreach (Type typ in equatables)
				{
					Type equatableT = typeof (IEquatable<>).MakeGenericType(typ);
					AddInterfaceImplementation(equatableT);
					EmittedMethod equalsT = DefineMethod("Equals");
					equalsT.ClearAttributes();
					equalsT.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
					                          MethodAttributes.Virtual | MethodAttributes.Final);
					equalsT.ReturnType = TypeRef.FromType<bool>();
					equalsT.DefineParameter("other", typ);
					equalsT.ContributeInstructions(contributedEquals);
				}
			}

			DefineOverrideMethod(typeof (Object).GetMethod("Equals", BindingFlags.Instance | BindingFlags.Public, null,
				new[] {typeof (object)}, null))
				.ContributeInstructions(contributedEquals);

			return specializedEquals;
		}

		private static void LoadFieldsFromThisAndParam(ILGenerator il, EmittedField field, EmittedParameter parm)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(field != null);
			il.LoadArg_0();
			il.LoadField(field);
			il.LoadArg(parm);
			il.LoadField(field);
		}

		[ContractInvariantMethod]
		private void InvariantContracts()
		{
			Contract.Invariant(_module != null);
			Contract.Invariant(_fields != null);
			Contract.Invariant(_members != null);
			Contract.Invariant(Name != null);
			Contract.Invariant(Name.Length > 0);
		}

		/// <summary>
		///   Compiles the emitted type.
		/// </summary>
		protected internal override void OnCompile()
		{
			TypeBuilder builder = Builder;
			foreach (CustomAttributeDescriptor a in _customAttr)
			{
				builder.SetCustomAttribute(new CustomAttributeBuilder(a.Ctor, a.Args));
			}
			foreach (EmittedField m in _fields.Values)
			{
				m.Compile();
			}
			foreach (var mm in _methods.Values)
			{
				// May be multiple methods, overloaded
				foreach (EmittedMethodBase m in mm)
				{
					m.Compile();
				}
			}
			foreach (EmittedProperty m in _properties.Values)
			{
				m.Compile();
			}
			foreach (EmittedClass m in _types.Values)
			{
				m.Compile();
			}
			Type runtimeType = builder.CreateType();
			_ref = new TypeRef(runtimeType);
		}

		private class CustomAttributeDescriptor
		{
			internal object[] Args { get; set; }
			internal ConstructorInfo Ctor { get; set; }
		}
	}
}