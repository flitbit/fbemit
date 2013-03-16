using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

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

	public interface IBase<T> { }
	public interface IDerived<T> : IBase<T> { }
	public interface IUseBase<T>
	{
		void TryAndMatchThisGeneric<TT>(IBase<T> one, IBase<T> two) where TT : T;
	}

	[TestClass]
	public class ExtensionsTests
	{
		[TestMethod]
		public void MatchGenericMethod_MatchesCompatible()
		{
			var m = typeof(UsedForTesting<string>).MatchGenericMethod("OneArg", 1, typeof(string), typeof(string));
			Assert.IsNotNull(m);
			Assert.IsNotNull(m.MakeGenericMethod(typeof(string)));
		}

		[TestMethod]
		public void MatchGenericMethod_MatchesCompatibleVoidReturnType()
		{
			var m = typeof(UsedForTesting<string>).MatchGenericMethod("OneArgOneOtherArg", 1, typeof(void), typeof(int), typeof(string));
			Assert.IsNotNull(m);
			Assert.IsNotNull(m.MakeGenericMethod(typeof(int)));

		}

		[TestMethod]
		public void MatchGenericMethod_FailsMatchWhenIncompatable()
		{
			var m = typeof(UsedForTesting<string>).MatchGenericMethod("OneArg", 1, typeof(string), typeof(DateTime));
			Assert.IsNull(m);
		}

		[TestMethod]
		public void MatchGenericMethod_MatchesCompatibleWhenArgIsParams()
		{
			var m = typeof(UsedForTesting<string>).MatchGenericMethod("OneArg", 1, typeof(DateTime), typeof(string[]));
			Assert.IsNotNull(m);
			Assert.IsNotNull(m.MakeGenericMethod(typeof(DateTime)));
		}

		[TestMethod]
		public void MatchGenericMethod_MatchesCompatibleCandidateHasAssignableGenericArguments()
		{
			
			var m = typeof(Nullable).MatchGenericMethod("Equals", BindingFlags.Static | BindingFlags.Public, 1, typeof(bool), typeof(int?), typeof(int?));
			Assert.IsNotNull(m);
			Assert.IsNotNull(m.MakeGenericMethod(typeof(int)));
		}

		[TestMethod]
		public void MatchGenericMethod_MatchesCompatibleCandidateHasAssignableGenericArguments_VariousCandidateOpenGenerics()
		{
			var m = typeof(IUseBase<WebRequest>).MatchGenericMethod("TryAndMatchThisGeneric", 1, typeof(void), typeof(IBase<HttpWebRequest>), typeof(IDerived<WebRequest>));
			Assert.IsNotNull(m);
			Assert.IsNotNull(m.MakeGenericMethod(typeof(HttpWebRequest)));
		}

	}
}
