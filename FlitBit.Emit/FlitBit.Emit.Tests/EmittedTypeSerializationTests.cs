using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FlitBit.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Emit.Tests
{
	public enum MyByteEnum : byte
	{
		Zero = 0,
		One,
		Two
	}

	public enum MyInt16Enum : short
	{
		Zero = 0,
		One,
		Two,
		Three,
		Four
	}

	public enum MyInt32Enum : short
	{
		Zero = 0,
		One,
		Two,
		Three,
		Four,
		Five,
		Six,
		Seven,
		Eight,
		Nine
	}

	public interface ITestSamplingOfTypes
	{
		bool Boolean { get; set; }
		byte Byte { get; set; }
		char Char { get; set; }
		DateTime DateTime { get; set; }
		DateTimeOffset DateTimeOffeset { get; set; }
		decimal Decimal { get; set; }
		double Double { get; set; }
		float Float { get; set; }
		Guid Guid { get; set; }
		short Int16 { get; set; }
		int Int32 { get; set; }
		int[] Int32Arr { get; set; }
		long Int64 { get; set; }
		MyByteEnum MyByteEnum { get; set; }
		MyInt16Enum MyInt16Enum { get; set; }
		MyInt32Enum MyInt32Enum { get; set; }

		bool? NullableBoolean { get; set; }
		byte? NullableByte { get; set; }
		char? NullableChar { get; set; }
		DateTime? NullableDateTime { get; set; }
		DateTimeOffset? NullableDateTimeOffeset { get; set; }
		decimal? NullableDecimal { get; set; }
		double? NullableDouble { get; set; }
		float? NullableFloat { get; set; }
		Guid? NullableGuid { get; set; }
		short? NullableInt16 { get; set; }
		int? NullableInt32 { get; set; }
		long? NullableInt64 { get; set; }
		MyByteEnum? NullableMyByteEnum { get; set; }
		MyInt16Enum? NullableMyInt16Enum { get; set; }
		MyInt32Enum? NullableMyInt32Enum { get; set; }
		sbyte? NullableSByte { get; set; }
		ushort? NullableUInt16 { get; set; }
		uint? NullableUInt32 { get; set; }
		ulong? NullableUInt64 { get; set; }
		sbyte SByte { get; set; }
		string String { get; set; }
		ushort UInt16 { get; set; }
		uint UInt32 { get; set; }
		ulong UInt64 { get; set; }
	}

	[TestClass]
	public class EmittedTypeSerializationTests
	{
		Type _type;

		[TestMethod]
		public void CanRoundTripSerializeEmittedType()
		{
			const int iterations = 10000;
			var gen = new DataGenerator();
			for (var i = 0; i < iterations; i++)
			{
				var it = (ITestSamplingOfTypes) Activator.CreateInstance(_type);
				Assert.IsNotNull(it);
				it.Boolean = gen.GetBoolean();
				it.Byte = gen.GetByte();
				it.Char = gen.GetChar();
				it.DateTime = gen.GetDateTime();
				it.DateTimeOffeset = gen.GetDateTimeOffset();
				it.Decimal = gen.GetDecimal();
				it.Double = gen.GetDouble();
				it.Float = gen.GetSingle();
				it.Guid = gen.GetGuid();
				it.Int16 = gen.GetInt16();
				it.Int32 = gen.GetInt32();
				it.Int64 = gen.GetInt64();
				it.MyByteEnum = gen.GetEnum<MyByteEnum>();
				it.MyInt16Enum = gen.GetEnum<MyInt16Enum>();
				it.MyInt32Enum = gen.GetEnum<MyInt32Enum>();
				it.NullableBoolean = gen.GetBoolean() ? gen.GetBoolean() : (bool?) null;
				it.NullableByte = gen.GetBoolean() ? gen.GetByte() : (byte?) null;
				it.NullableChar = gen.GetBoolean() ? gen.GetChar() : (char?) null;
				it.NullableDateTime = gen.GetBoolean() ? gen.GetDateTime() : (DateTime?) null;
				it.NullableDateTimeOffeset = gen.GetBoolean() ? gen.GetDateTimeOffset() : (DateTimeOffset?) null;
				it.NullableDecimal = gen.GetBoolean() ? gen.GetDecimal() : (decimal?) null;
				it.NullableDouble = gen.GetBoolean() ? gen.GetDouble() : (double?) null;
				it.NullableFloat = gen.GetBoolean() ? gen.GetSingle() : (float?) null;
				it.NullableGuid = gen.GetBoolean() ? gen.GetGuid() : (Guid?) null;
				it.NullableInt16 = gen.GetBoolean() ? gen.GetInt16() : (short?) null;
				it.NullableInt32 = gen.GetBoolean() ? gen.GetInt32() : (int?) null;
				it.NullableInt64 = gen.GetBoolean() ? gen.GetInt64() : (long?) null;
				it.NullableMyByteEnum = gen.GetBoolean() ? gen.GetEnum<MyByteEnum>() : (MyByteEnum?) null;
				it.NullableMyInt16Enum = gen.GetBoolean() ? gen.GetEnum<MyInt16Enum>() : (MyInt16Enum?) null;
				it.NullableMyInt32Enum = gen.GetBoolean() ? gen.GetEnum<MyInt32Enum>() : (MyInt32Enum?) null;
				it.NullableSByte = gen.GetBoolean() ? gen.GetSByte() : (sbyte?) null;
				it.SByte = gen.GetSByte();
				it.String = gen.GetString(Math.Max(gen.GetInt32() % 256, 1));
				it.UInt16 = gen.GetUInt16();
				it.UInt32 = gen.GetUInt32();
				it.UInt64 = gen.GetUInt64();
				it.NullableUInt16 = gen.GetBoolean() ? gen.GetUInt16() : (ushort?) null;
				it.NullableUInt32 = gen.GetBoolean() ? gen.GetUInt32() : (uint?) null;
				it.NullableUInt64 = gen.GetBoolean() ? gen.GetUInt64() : (ulong?) null;

				using (var serialized = SerializeToStream(it))
				{
					var deserialized = DeserializeFromStream<ITestSamplingOfTypes>(serialized);
					Assert.AreEqual(it.Boolean, deserialized.Boolean);
					Assert.AreEqual(it.Byte, deserialized.Byte);
					Assert.AreEqual(it.Char, deserialized.Char);
					Assert.AreEqual(it.DateTime, deserialized.DateTime);
					Assert.AreEqual(it.DateTimeOffeset, deserialized.DateTimeOffeset);
					Assert.AreEqual(it.Decimal, deserialized.Decimal);
					Assert.AreEqual(it.Double, deserialized.Double);
					Assert.AreEqual(it.Float, deserialized.Float);
					Assert.AreEqual(it.Guid, deserialized.Guid);
					Assert.AreEqual(it.Int16, deserialized.Int16);
					Assert.AreEqual(it.Int32, deserialized.Int32);
					Assert.AreEqual(it.Int64, deserialized.Int64);
					Assert.AreEqual(it.MyByteEnum, deserialized.MyByteEnum);
					Assert.AreEqual(it.MyInt16Enum, deserialized.MyInt16Enum);
					Assert.AreEqual(it.MyInt32Enum, deserialized.MyInt32Enum);
					Assert.AreEqual(it.NullableBoolean, deserialized.NullableBoolean);
					Assert.AreEqual(it.NullableByte, deserialized.NullableByte);
					Assert.AreEqual(it.NullableChar, deserialized.NullableChar);
					Assert.AreEqual(it.NullableDateTime, deserialized.NullableDateTime);
					Assert.AreEqual(it.NullableDateTimeOffeset, deserialized.NullableDateTimeOffeset);
					Assert.AreEqual(it.NullableDecimal, deserialized.NullableDecimal);
					Assert.AreEqual(it.NullableDouble, deserialized.NullableDouble);
					Assert.AreEqual(it.NullableFloat, deserialized.NullableFloat);
					Assert.AreEqual(it.NullableGuid, deserialized.NullableGuid);
					Assert.AreEqual(it.NullableInt16, deserialized.NullableInt16);
					Assert.AreEqual(it.NullableInt32, deserialized.NullableInt32);
					Assert.AreEqual(it.NullableInt64, deserialized.NullableInt64);
					Assert.AreEqual(it.NullableMyByteEnum, deserialized.NullableMyByteEnum);
					Assert.AreEqual(it.NullableMyInt16Enum, deserialized.NullableMyInt16Enum);
					Assert.AreEqual(it.NullableMyInt32Enum, deserialized.NullableMyInt32Enum);
					Assert.AreEqual(it.NullableSByte, deserialized.NullableSByte);
					Assert.AreEqual(it.NullableUInt16, deserialized.NullableUInt16);
					Assert.AreEqual(it.NullableUInt32, deserialized.NullableUInt32);
					Assert.AreEqual(it.NullableUInt64, deserialized.NullableUInt64);
					Assert.AreEqual(it.SByte, deserialized.SByte);
					Assert.AreEqual(it.String, deserialized.String);
					Assert.AreEqual(it.UInt16, deserialized.UInt16);
					Assert.AreEqual(it.UInt32, deserialized.UInt32);
					Assert.AreEqual(it.UInt64, deserialized.UInt64);

					// And after all that, ensure the specialized equality works...
					Assert.IsTrue(it.Equals(deserialized));
					Assert.AreEqual(it.GetHashCode(), deserialized.GetHashCode());
				}
			}
		}

		[TestInitialize]
		public void Init()
		{
			RuntimeAssemblies.WriteDynamicAssemblyOnExit = true;
			var module = RuntimeAssemblies.DynamicAssembly.DefineModule("Tests", null);
			var tt = typeof(ITestSamplingOfTypes);
			var typeName = RuntimeAssemblies.PrepareTypeName(tt, "Test");
			var builder = module.DefineClass(typeName, EmittedClass.DefaultTypeAttributes, typeof(Object), null);
			builder.Attributes = TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

			// mark the emitted class as serializable...
			builder.SetCustomAttribute<SerializableAttribute>();

			builder.DefineDefaultCtor();

			var chashCodeSeed = builder.DefineField<int>("CHashCodeSeed");
			chashCodeSeed.IncludeAttributes(FieldAttributes.Static | FieldAttributes.Private | FieldAttributes.InitOnly);
			var cctor = builder.DefineCCtor();
			cctor.ContributeInstructions((m, il) =>
			{
				il.LoadType(builder.Builder);
				il.CallVirtual(typeof(Type).GetProperty("AssemblyQualifiedName").GetGetMethod());
				il.CallVirtual<object>("GetHashCode");
				il.StoreField(chashCodeSeed);
			});

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
			Func<EmittedField, bool> filter = f => f.FieldType.Target == typeof(PropertyChangedEventHandler);
			builder.SpecializeEquals(new Type[] {typeof(ITestSamplingOfTypes)}, filter, null);
			builder.SpecializeGetHashCode(chashCodeSeed, f => f.FieldType.Target == typeof(PropertyChangedEventHandler), null);
			
			builder.Compile();
			_type = builder.Ref.Target;
		}

		static T DeserializeFromStream<T>(MemoryStream stream)
		{
			IFormatter formatter = new BinaryFormatter();
			stream.Seek(0, SeekOrigin.Begin);
			var o = (T) formatter.Deserialize(stream);
			return o;
		}

		static MemoryStream SerializeToStream<T>(T o)
		{
			var stream = new MemoryStream();
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, o);
			return stream;
		}
	}
}