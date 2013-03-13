#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace FlitBit.Emit
{
	/// <summary>
	///   Contains extension methods for the MethodInfo and MethodBase types.
	/// </summary>
	public static class MethodInfoExtensions
	{
		/// <summary>
		///   Gets the parameter types for a method.
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		public static Type[] GetParameterTypes(this MethodBase method)
		{
			Contract.Requires<ArgumentNullException>(method != null);
			return (from p in method.GetParameters()
							select p.ParameterType).ToArray();
		}
	}
}