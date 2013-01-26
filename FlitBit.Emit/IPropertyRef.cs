#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Reflection;

namespace FlitBit.Emit
{

	internal interface IPropertyRef : IValueRef
	{
		PropertyInfo GetPropertyInfo();
	}

	
}