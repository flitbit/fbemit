#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;

namespace FlitBit.Emit
{
	/// <summary>
	///   Helper class for emiting assemblies.
	/// </summary>
	public class EmittedAssembly
	{
		private Dictionary<string, EmittedModule> _modules;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="name">the assembly's name</param>
		/// <param name="rootNamespace">the assembly's root namespace</param>
		public EmittedAssembly(string name, string rootNamespace)
			: this(name, rootNamespace, new Version(1, 0, 0, 0), new CultureInfo("en"), null, null)
		{
		}

		/// <summary>
		///   Creates a new instance
		/// </summary>
		/// <param name="name">the assembly's name</param>
		/// <param name="rootNamespace">the assembly's root namespace</param>
		/// <param name="version">the assembly's version</param>
		/// <param name="culture">the assembly's culture</param>
		public EmittedAssembly(string name, string rootNamespace, Version version, CultureInfo culture)
			: this(name, rootNamespace, version, culture, null, null)
		{
		}

		/// <summary>
		///   Creates a new instance
		/// </summary>
		/// <param name="name">the assembly's name</param>
		/// <param name="rootNamespace">the assembly's root namespace</param>
		/// <param name="version">the assembly's version</param>
		/// <param name="culture">the assembly's culture</param>
		/// <param name="publicKey">the assembly's public key</param>
		/// <param name="publicKeyToken">the assembly's public key token</param>
		public EmittedAssembly(string name, string rootNamespace, Version version, CultureInfo culture
			, byte[] publicKey, byte[] publicKeyToken)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);
			Contract.Requires<ArgumentNullException>(version != null);
			Contract.Requires<ArgumentNullException>(culture != null);

			var assemName = new AssemblyName(name) {Version = version, CultureInfo = culture};
			assemName.SetPublicKey(publicKey);
			assemName.SetPublicKeyToken(publicKeyToken);
			FinishConstruction(assemName, rootNamespace);
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="name">the assembly's name</param>
		/// <param name="rootNamespace">the assembly's root namespace</param>
		public EmittedAssembly(AssemblyName name, string rootNamespace)
		{
			Contract.Requires<ArgumentNullException>(name != null);

			FinishConstruction(name, rootNamespace);
		}

		/// <summary>
		///   Gets the assembly's base module.
		/// </summary>
		public EmittedModule BaseModule { get; private set; }

		/// <summary>
		///   Gets the assembly's builder.
		/// </summary>
		public AssemblyBuilder Builder { get; private set; }

		/// <summary>
		///   Indicates whether the assembly has been compiled.
		/// </summary>
		public bool IsCompiled { get; private set; }

		/// <summary>
		///   Gets the assembly's name
		/// </summary>
		public AssemblyName Name { get; private set; }

		/// <summary>
		///   Gets the assembly's root namespace.
		/// </summary>
		public string RootNamespace { get; private set; }

		/// <summary>
		///   Compiles the assembly.
		/// </summary>
		/// <returns></returns>
		public Assembly Compile()
		{
			Contract.Requires<InvalidOperationException>(!IsCompiled, "already compiled");

			foreach (EmittedModule m in _modules.Values)
			{
				if (!m.IsCompiled)
				{
					m.Compile();
				}
			}
			IsCompiled = true;
			return Builder;
		}

		/// <summary>
		///   Defines a class.
		/// </summary>
		/// <param name="name">the class' name</param>
		/// <returns>the emitted class</returns>
		public EmittedClass DefineClass(string name)
		{
			return BaseModule.DefineClass(name);
		}

		/// <summary>
		///   Defines a class.
		/// </summary>
		/// <param name="name">the class' name</param>
		/// <param name="attributes">the class' attributes</param>
		/// <param name="supertype">the class' supertype</param>
		/// <param name="interfaces">a list of interfaces the class will implement</param>
		/// <returns>the emitted class</returns>
		public EmittedClass DefineClass(string name,
			TypeAttributes attributes,
			Type supertype,
			Type[] interfaces)
		{
			return BaseModule.DefineClass(name, attributes, supertype, interfaces);
		}

		/// <summary>
		///   Defines a new module in the assembly.
		/// </summary>
		/// <param name="name">the module's name</param>
		/// <param name="namespace">a root namespace for the module</param>
		/// <returns>An emitted module builder</returns>
		public EmittedModule DefineModule(string name, string @namespace)
		{
			CheckModuleName(name);

			var module = new EmittedModule(this, name, @namespace);
			_modules.Add(name, module);
			return module;
		}

		internal Assembly Save()
		{
			Contract.Requires<ArgumentNullException>(IsCompiled, "must be compiled before save");

			Builder.Save(BaseModule.Name + ".dll");
			return Builder;
		}

		internal Assembly Save(EmittedModule module)
		{
			Contract.Requires<ArgumentNullException>(IsCompiled, "must be compiled before save");

			Builder.Save(module.Name + ".dll");
			return Builder;
		}

		private void CheckModuleName(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			if (_modules.ContainsKey(name))
			{
				throw new InvalidOperationException(String.Concat(
					"Unable to generate duplicate class. The class name is already in use: module = ",
					Name, ", class = ", name)
					);
			}
		}

		private void FinishConstruction(AssemblyName name, string rootNamespace)
		{
			_modules = new Dictionary<string, EmittedModule>();
			RootNamespace = rootNamespace ?? name.Name;
			Name = name;
			Builder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
			BaseModule = new EmittedModule(this, name.Name, RootNamespace);
			_modules.Add(BaseModule.Name, BaseModule);
		}
	}
}