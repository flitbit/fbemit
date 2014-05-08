#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace FlitBit.Emit
{
	/// <summary>
	///   Various PropertyInfo extensions.
	/// </summary>
	public static class PropertyInfoExtensions
	{
		/// <summary>
		///   Produces a backing field name for the given member
		/// </summary>
		/// <param name="member">the member</param>
		/// <returns>returns a backing field name</returns>
		public static string FormatBackingFieldName(this MemberInfo member)
		{
			Contract.Requires<ArgumentNullException>(member != null);
			Contract.Requires<ArgumentException>(member.DeclaringType != null);
			Contract.Ensures(Contract.Result<string>() != null);

			// The intent is to produce a backing field name that won't clash with
			// other backing field names regardless of the depth/composition of 
			// class inheritance for the target type. Prefixing the declaring type
			// should do the trick.
			return String.Concat(member.DeclaringType.Name, "_", member.Name);
		}
	}
}