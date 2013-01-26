#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System.Reflection;

namespace FlitBit.Emit
{
	/// <summary>
	/// Interface for field references.
	/// </summary>
	public interface IFieldRef : IValueRef
	{
		/// <summary>
		/// Gets the FieldInfo for the target field.
		/// </summary>
		/// <returns>FieldInfo metadata for the underlying field.</returns>
		FieldInfo FieldInfo { get; }
	}
}