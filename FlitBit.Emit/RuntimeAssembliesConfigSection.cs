#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Configuration;

namespace FlitBit.Emit
{
	/// <summary>
	///   Configuration section for cache settings.
	/// </summary>
	public class RuntimeAssembliesConfigSection : ConfigurationSection
	{
		private const string CDefaultDynamicAssemblyPrefix = "FlitBit.Dynamic";

		private const bool CDefaultWriteAssembliesOnExit = false;
		private const string PropertyNameDynamicAssemblyPrefix = "dynamicAssemblyPrefix";
		private const string PropertyNameWriteAssembliesOnExit = "writeAssembliesOnExit";

		/// <summary>
		///   Configuration section name for cache settings
		/// </summary>
		public static readonly string SectionName = "flitbit.emit";

		/// <summary>
		///   Prefix for the emitted assembly.
		/// </summary>
		[ConfigurationProperty(PropertyNameDynamicAssemblyPrefix, DefaultValue = CDefaultDynamicAssemblyPrefix)]
		public string DynamicAssemblyPrefix
		{
			get { return (string) this[PropertyNameDynamicAssemblyPrefix]; }
			set { this[PropertyNameDynamicAssemblyPrefix] = value; }
		}

		/// <summary>
		///   Indicates whether emitted assemblies should be written to disk on exit.
		/// </summary>
		[ConfigurationProperty(PropertyNameWriteAssembliesOnExit, DefaultValue = CDefaultWriteAssembliesOnExit)]
		public bool WriteAssembliesOnExit
		{
			get { return (bool) this[PropertyNameWriteAssembliesOnExit]; }
			set { this[PropertyNameWriteAssembliesOnExit] = value; }
		}

		/// <summary>
		///   Gets the current configuration section.
		/// </summary>
		public static RuntimeAssembliesConfigSection Current
		{
			get
			{
				var config = ConfigurationManager.GetSection(
					SectionName) as RuntimeAssembliesConfigSection;
				return config ?? new RuntimeAssembliesConfigSection();
			}
		}
	}
}