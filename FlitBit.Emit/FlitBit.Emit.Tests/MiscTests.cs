using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Emit.Tests
{
	[TestClass]
	public class MiscTests
	{
		[TestMethod]
		public void FlattenTypeHierarchy()
		{
			var types = new Queue<Type>(typeof(I).GetTypeHierarchyInDeclarationOrder());
			Assert.IsNotNull(types);
			Assert.AreEqual(2, types.Count());
			Assert.AreEqual(typeof(IDisposable), types.Dequeue());
			Assert.AreEqual(typeof(I), types.Dequeue());

			types = new Queue<Type>(typeof(II).GetTypeHierarchyInDeclarationOrder());
			Assert.IsNotNull(types);
			Assert.AreEqual(3, types.Count());
			Assert.AreEqual(typeof(IDisposable), types.Dequeue());
			Assert.AreEqual(typeof(I), types.Dequeue());
			Assert.AreEqual(typeof(II), types.Dequeue());

			types = new Queue<Type>(typeof(C).GetTypeHierarchyInDeclarationOrder());
			Assert.IsNotNull(types);
			Assert.AreEqual(4, types.Count());
			Assert.AreEqual(typeof(Object), types.Dequeue());
			Assert.AreEqual(typeof(IDisposable), types.Dequeue());
			Assert.AreEqual(typeof(I), types.Dequeue());
			Assert.AreEqual(typeof(C), types.Dequeue());

			types = new Queue<Type>(typeof(D).GetTypeHierarchyInDeclarationOrder());
			Assert.IsNotNull(types);
			Assert.AreEqual(5, types.Count());
			Assert.AreEqual(typeof(Object), types.Dequeue());
			Assert.AreEqual(typeof(IDisposable), types.Dequeue());
			Assert.AreEqual(typeof(I), types.Dequeue());
			Assert.AreEqual(typeof(II), types.Dequeue());
			Assert.AreEqual(typeof(D), types.Dequeue());

			types = new Queue<Type>(typeof(E).GetTypeHierarchyInDeclarationOrder());
			Assert.IsNotNull(types);
			Assert.AreEqual(6, types.Count());
			Assert.AreEqual(typeof(Object), types.Dequeue());
			Assert.AreEqual(typeof(IDisposable), types.Dequeue());
			Assert.AreEqual(typeof(I), types.Dequeue());
			Assert.AreEqual(typeof(C), types.Dequeue());
			Assert.AreEqual(typeof(II), types.Dequeue());
			Assert.AreEqual(typeof(E), types.Dequeue());
		}

		class C : I, IDisposable
		{
			#region I Members

			public void Dispose() { throw new NotImplementedException(); }

			#endregion
		}

		class D : I, II, IDisposable
		{
			#region I Members

			public void Dispose() { throw new NotImplementedException(); }

			#endregion
		}

		class E : C, II
		{}

		interface I : IDisposable
		{}

		interface II : I
		{}
	}
}