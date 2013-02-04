using FlitBit.Core;
using FlitBit.Wireup;
using FlitBit.Wireup.Meta;

[assembly: Wireup(typeof(FlitBit.Emit.WireupThisAssembly))]

namespace FlitBit.Emit
{
	/// <summary>
	/// Wires this assembly.
	/// </summary>
	public sealed class WireupThisAssembly: WireupCommand
	{
		/// <summary>
		/// Performs the wireup.
		/// </summary>
		/// <param name="coordinator"></param>
		protected override void PerformWireup(IWireupCoordinator coordinator)
		{
			// Register auto-implemented factory as the global; enables emitted types.
			FactoryFactory.Instance = new AutoImplementedTypeFactory();
		}
	}
}
