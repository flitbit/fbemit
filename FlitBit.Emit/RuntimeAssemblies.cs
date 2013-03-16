#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading;
using FlitBit.Core;

namespace FlitBit.Emit
{
	/// <summary>
	///   Utility class for emitting assemblies and tracking those assemblies so
	///   that type resolution works for the emitted types.
	/// </summary>
	public static class RuntimeAssemblies
	{
		static readonly Dictionary<string, EmittedAssembly> Assemblies = new Dictionary<string, EmittedAssembly>();

		static readonly Lazy<EmittedAssembly> LazyDynamicAssembly = new Lazy<EmittedAssembly>(() =>
		{
			var now = DateTime.Now.ToString("O").Replace(':', '_');
			var current = Assembly.GetExecutingAssembly().GetName();
			var name = new AssemblyName(String.Concat(RuntimeAssembliesConfigSection.Current.DynamicAssemblyPrefix, now))
			{
				Version = current.Version,
				CultureInfo = current.CultureInfo
			};
			return GetEmittedAssemblyWithEmitWhenNotFound(name, asm => { });
		}, LazyThreadSafetyMode.ExecutionAndPublication);

		[SuppressMessage("Microsoft.Performance", "CA1810")]
		static RuntimeAssemblies()
		{
			AppDomain.CurrentDomain.TypeResolve += CurrentDomain_TypeResolve;
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
			AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
		}

		/// <summary>
		///   Gets the dynamic assembly.
		/// </summary>
		public static EmittedAssembly DynamicAssembly
		{
			get { return LazyDynamicAssembly.Value; }
		}

		/// <summary>
		///   Indicates whether the dynamic assembly should be writen to disk upon exit.
		/// </summary>
		public static bool WriteDynamicAssemblyOnExit { get; set; }

		/// <summary>
		///   Generates an emittable full name for a type by name mangling.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string GetEmittableFullName(this Type type)
		{
			Contract.Requires<ArgumentNullException>(type != null);

			var tt = (type.IsArray) ? type.GetElementType() : type;
			var simpleName = tt.Name;

			Contract.Assume(simpleName != null);
			Contract.Assert(simpleName.Length >= 0);

			var tick = simpleName.IndexOf('`');
			if (tick >= 0)
			{
				simpleName = simpleName.Substring(0, tick);
				var args = tt.GetGenericArguments();
				for (var i = 0; i < args.Length; i++)
				{
					simpleName = String.Concat(simpleName, i == 0 ? '<' : '`', args[i].GetEmittableFullName().Replace('.', '|'));
				}
				simpleName = String.Concat(simpleName, '>');
			}
			return tt.IsNested
				? String.Concat(tt.DeclaringType.GetReadableFullName(), "^", simpleName)
				: String.Concat(tt.Namespace, ".", simpleName);
		}

		/// <summary>
		///   Gets an emitted assembly by name.
		/// </summary>
		/// <param name="name">the assembly's name</param>
		/// <returns>the assembly if it exists; otherwise null</returns>
		public static EmittedAssembly GetEmittedAssembly(string name)
		{
			Contract.Requires<ArgumentNullException>(name != null);

			EmittedAssembly asm;
			lock (Assemblies)
			{
				Assemblies.TryGetValue(name, out asm);
			}
			return asm;
		}

		/// <summary>
		///   Gets an emitted assembly by name; if it doesn't exist usess the emitter callback to
		///   generate it.
		/// </summary>
		/// <param name="name">the assembly's name</param>
		/// <param name="emitter">a callback method that will emit the assembly</param>
		/// <returns>the assembly</returns>
		public static EmittedAssembly GetEmittedAssemblyWithEmitWhenNotFound(AssemblyName name,
			Action<EmittedAssembly> emitter)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(emitter != null, "emitter cannot be null");

			EmittedAssembly asm;
			lock (Assemblies)
			{
				if (!Assemblies.TryGetValue(name.FullName, out asm))
				{
					asm = new EmittedAssembly(name, null);
					emitter(asm);
					asm.Compile();
					Assemblies.Add(name.FullName, asm);
				}
			}
			return asm;
		}

		/// <summary>
		///   Creates an emitted assembly based on information taken from the target assembly.
		/// </summary>
		/// <param name="nameFormat">used to format the emitted assembly's name</param>
		/// <param name="target">the target assembly</param>
		/// <returns>an assembly name</returns>
		public static AssemblyName MakeEmittedAssemblyNameFromAssembly(string nameFormat, Assembly target)
		{
			Contract.Requires<ArgumentNullException>(nameFormat != null, "nameFormat cannot be null");
			Contract.Requires<ArgumentNullException>(target != null, "target cannot be null");

			var tasmName = target.GetName();
			var asmName = new AssemblyName(String.Format(nameFormat
																									, tasmName.Name
																									, tasmName.Version
																									, Assembly.GetCallingAssembly().GetName().Version).Replace('.', '_')
				) {Version = tasmName.Version, CultureInfo = tasmName.CultureInfo};
			return asmName;
		}

		/// <summary>
		///   Prepares a type name.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="suffix"></param>
		/// <returns></returns>
		public static string PrepareTypeName(Type type, string suffix)
		{
			Contract.Requires<ArgumentNullException>(type != null);
			Contract.Requires<ArgumentNullException>(suffix != null);
			Contract.Requires<ArgumentNullException>(suffix.Length > 0);
			Contract.Ensures(Contract.Result<string>() != null);

			return String.Concat(type.GetEmittableFullName(), '$', suffix);
		}

		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			if (LazyDynamicAssembly.IsValueCreated)
			{
				var asm = LazyDynamicAssembly.Value;
				var reqName = new AssemblyName(args.Name);
				if (reqName.Name.StartsWith(RuntimeAssembliesConfigSection.Current.DynamicAssemblyPrefix)
					&& reqName.Version == asm.Name.Version)
				{
					return asm.Builder;
				}
			}
			return null;
		}

		static void CurrentDomain_DomainUnload(object sender, EventArgs e)
		{
			try
			{
				if (RuntimeAssembliesConfigSection.Current.WriteAssembliesOnExit || WriteDynamicAssemblyOnExit)
				{
					foreach (var asm in Assemblies.Values)
					{
						if (!asm.IsCompiled)
						{
							asm.Compile();
						}
						asm.Save();
					}
				}
			}
// ReSharper disable EmptyGeneralCatchClause
			catch
			{ /* purposely suppressing exceptions while trying to write dynamic assembly on exit */ }
// ReSharper restore EmptyGeneralCatchClause
		}

		static Assembly CurrentDomain_TypeResolve(object sender, ResolveEventArgs args)
		{
			// todo: reconstitue generated classes (stereotypical implementations) not already present in the dynamic assembly...
			return null;
		}
	}
}