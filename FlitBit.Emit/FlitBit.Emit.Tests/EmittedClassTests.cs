using System;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Emit.Tests
{
	[TestClass]
	public class EmittedClassTests
	{
		int _classCount = 0;

		EmittedAssembly _assembly;
		EmittedModule _module;

		string NextClassName()
		{
			return String.Concat("Class_", Interlocked.Increment(ref _classCount));
		}

		[TestInitialize]
		public void Init()
		{			
			_assembly = new EmittedAssembly(typeof(EmittedClassTests).Name, typeof(EmittedClassTests).Namespace);
			_module = _assembly.BaseModule;
		}

		[TestCleanup]
		public void Cleanup()
		{
			// output the assembly so we can eyeball the classes, etc.
			_assembly.Compile();
			_assembly.Save(); 
		}

		[TestMethod]
		public void EmittedClass_CanConstructAndCompileEmptyClass()
		{
			var cls = new EmittedClass(_module.Builder, NextClassName());
			Assert.IsFalse(cls.IsCompiled);
			
			cls.Compile();
			
			Assert.IsTrue(cls.IsCompiled);
			var generatedType = cls.Ref.Target;
			Assert.IsNotNull(generatedType);			

			var obj = Activator.CreateInstance(generatedType);
			Assert.IsNotNull(obj);
		}
		
		[TestMethod]
		public void EmittedClass_CanInitStaticField()
		{
			var cls = new EmittedClass(_module.Builder, NextClassName());
			Assert.IsFalse(cls.IsCompiled);

			var field = cls.DefineField<int>("__field").WithInit(new ValueRef<int>(13));
			cls.DefineCtor();
			cls.Compile();

			Assert.IsTrue(cls.IsCompiled);

			var obj = Activator.CreateInstance(cls.Ref.Target);

			Assert.IsNotNull(obj);
			
			FieldInfo __field = obj.GetType().GetField("__field", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.AreEqual(13, (int)__field.GetValue(obj));
		}
	}
}
