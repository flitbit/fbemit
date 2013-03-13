using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FlitBit.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Emit.Tests
{
	public interface ISerializedType
	{
		int ID { get; set; }
		string Name { get; set; }
	}

	[TestClass]
	public class EmittedTypeSerializationTests
	{
		Type _type;

		[TestInitialize]
		public void Init()
		{
			RuntimeAssemblies.WriteDynamicAssemblyOnExit = true;
			var module = RuntimeAssemblies.DynamicAssembly.DefineModule("Tests", null);
			var tt = typeof(ISerializedType);
			string typeName = RuntimeAssemblies.PrepareTypeName(tt, "Test");
			var builder = module.DefineClass(typeName, EmittedClass.DefaultTypeAttributes, typeof(Object), null);
			builder.Attributes = TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

			// mark the emitted class as serializable...
			builder.SetCustomAttribute<SerializableAttribute>();
			
			foreach (var intf in from type in tt.GetTypeHierarchyInDeclarationOrder()
													 where type.IsInterface
													 select type)
			{
				builder.AddInterfaceImplementation(intf);
				var properties = intf.GetProperties();
				foreach (var p in properties)
				{
					var property = p;
					builder.DefinePropertyWithBackingField(property.Name, property.PropertyType);
				}
				builder.StubMethodsForInterface(intf, true, true);
			}
			builder.Compile();
			_type = builder.Ref.Target;
		}

		[TestMethod]
		public void CanRoundTripSerializeEmittedType()
		{
			var gen = new DataGenerator();
			var it = (ISerializedType)Activator.CreateInstance(_type);
			Assert.IsNotNull(it);
			it.ID = gen.GetInt32();
			it.Name = gen.GetString(60);

			using (var serialized = SerializeToStream(it))
			{
				var deserialized = DeserializeFromStream<ISerializedType>(serialized);
				Assert.AreEqual(it.ID, deserialized.ID);
				Assert.AreEqual(it.Name, deserialized.Name);
			}
		}

		static MemoryStream SerializeToStream<T>(T o)
		{
			var stream = new MemoryStream();
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, o);
			return stream;
		}

		static T DeserializeFromStream<T>(MemoryStream stream)
		{
			IFormatter formatter = new BinaryFormatter();
			stream.Seek(0, SeekOrigin.Begin);
			var o = (T)formatter.Deserialize(stream);
			return o;
		}
	}
}
