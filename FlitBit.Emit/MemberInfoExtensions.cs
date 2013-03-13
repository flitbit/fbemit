#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace FlitBit.Emit
{
	/// <summary>
	///   Contains extension methods for the MemberInfo.
	/// </summary>
	public static class MemberInfoExtensions
	{
		/// <summary>
		///   Gets the type of a member's value.
		/// </summary>
		/// <param name="member">the member</param>
		/// <returns>the type</returns>
		public static Type GetTypeOfValue(this MemberInfo member)
		{
			Contract.Requires<ArgumentNullException>(member != null);
			switch (member.MemberType)
			{
				case MemberTypes.Event:
					return ((EventInfo) member).EventHandlerType;
				case MemberTypes.Field:
					return ((FieldInfo) member).FieldType;
				case MemberTypes.Property:
					return ((PropertyInfo) member).PropertyType;
				default:
					throw new NotSupportedException();
			}
		}
	}
}