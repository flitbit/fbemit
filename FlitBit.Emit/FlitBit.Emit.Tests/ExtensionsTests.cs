using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Emit.Tests
{
	public class UsedForTesting<T>
	{
		public T NoArgs() { return default(T); }
		public T OneArg(string s) { return default(T); }
		public T OneArg<TArg>(TArg arg) where TArg : class { return default(T); }
		public TResult OneArg<TResult>(string name) { return default(TResult); }
		public void OneArgOneOtherArg<TArg>(TArg arg, string another) {  }
		public TResult OneArg<TResult>(params String[] p) where TResult : struct { return default(TResult); }
	}

	[TestClass]
	public class ExtensionsTests
	{
		[TestMethod]
		public void GetGenericMethod_MatchesCompatible()
		{
			var m = typeof(UsedForTesting<string>).MatchGenericMethod("OneArg", 1, typeof(string), typeof(string));
			Assert.IsNotNull(m);
			Assert.IsNotNull(m.MakeGenericMethod(typeof(string)));
		}

		[TestMethod]
		public void GetGenericMethod_MatchesCompatibleVoidReturnType()
		{
			var m = typeof(UsedForTesting<string>).MatchGenericMethod("OneArgOneOtherArg", 1, typeof(void), typeof(int), typeof(string));
			Assert.IsNotNull(m);
			Assert.IsNotNull(m.MakeGenericMethod(typeof(int)));

		}

		[TestMethod]
		public void GetGenericMethod_FailsMatchWhenIncompatable()
		{
			var m = typeof(UsedForTesting<string>).MatchGenericMethod("OneArg", 1, typeof(string), typeof(DateTime));
			Assert.IsNull(m);
		}

		[TestMethod]
		public void GetGenericMethod_MatchesCompatibleWhenArgIsParams()
		{
			var m = typeof(UsedForTesting<string>).MatchGenericMethod("OneArg", 1, typeof(DateTime), typeof(string[]));
			Assert.IsNotNull(m);
			Assert.IsNotNull(m.MakeGenericMethod(typeof(DateTime)));
		}

	}
}
