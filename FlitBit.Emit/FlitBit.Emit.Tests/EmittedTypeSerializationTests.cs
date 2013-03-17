using System;
using System.Collections.Generic;
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
			ImplementSpecializedEquals<ITestSamplingOfTypes>(builder);
			ImplementSpecializedGetHashCode(builder, chashCodeSeed);

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

		static EmittedMethod ImplementSpecializedEquals<T>(EmittedClass builder)
		{
			var equatable = typeof(IEquatable<>).MakeGenericType(builder.Builder);
			builder.AddInterfaceImplementation(equatable);

			var specializedEquals = builder.DefineMethod("Equals");
			specializedEquals.ClearAttributes();
			specializedEquals.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
				MethodAttributes.Virtual | MethodAttributes.Final);
			specializedEquals.ReturnType = TypeRef.FromType<bool>();
			var other = specializedEquals.DefineParameter("other", builder.Ref);

			specializedEquals.ContributeInstructions((m, il) =>
			{
				il.DeclareLocal(typeof(bool));
				var exitFalse = il.DefineLabel();
				il.Nop();

				var fields =
					new List<EmittedField>(builder.Fields.Where(f => f.IsStatic == false));
				for (var i = 0; i < fields.Count; i++)
				{
					var field = fields[i];
					var fieldType = field.FieldType.Target;
					if (fieldType.IsArray)
					{
						LoadFieldFromThisAndParam(il, field, other);
						il.Call(typeof(Core.Extensions).GetMethod("EqualsOrItemsEqual", BindingFlags.Static | BindingFlags.Public)
																					.MakeGenericMethod(fieldType));
					}
					else if (fieldType.IsClass)
					{
						var opEquality = fieldType.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);
						if (opEquality != null)
						{
							LoadFieldFromThisAndParam(il, field, other);
							il.Call(opEquality);
						}
						else
						{
							il.Call(typeof(EqualityComparer<>).MakeGenericType(fieldType)
																								.GetMethod("get_Default", BindingFlags.Static | BindingFlags.Public));
							LoadFieldFromThisAndParam(il, field, other);
							il.CallVirtual(typeof(IEqualityComparer<>).MakeGenericType(fieldType)
																												.GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance,
																																	null,
																																	new[] {fieldType, fieldType},
																																	null
															));
						}
					}
					else
					{
						LoadFieldFromThisAndParam(il, field, other);
						il.CompareEquality(fieldType);
					}
					if (i < fields.Count - 1)
					{
						il.BranchIfFalse(exitFalse);
					}
				}
				var exit = il.DefineLabel();
				il.Branch(exit);
				il.MarkLabel(exitFalse);
				il.Load_I4_0();
				il.MarkLabel(exit);
				il.StoreLocal_0();
				var fin = il.DefineLabel();
				il.Branch(fin);
				il.MarkLabel(fin);
				il.LoadLocal_0();
			});

			var contributedEquals = new Action<EmittedMethodBase, ILGenerator>((m, il) =>
			{
				var exitFalse2 = il.DefineLabel();
				var exit = il.DefineLabel();
				il.DeclareLocal(typeof(bool));
				il.Nop();
				il.LoadArg_1();
				il.IsInstance(builder.Builder);
				il.BranchIfFalse(exitFalse2);
				il.LoadArg_0();
				il.LoadArg_1();
				il.CastClass(builder.Builder);
				il.Call(specializedEquals);
				il.Branch(exit);
				il.MarkLabel(exitFalse2);
				il.LoadValue(false);
				il.MarkLabel(exit);
				il.StoreLocal_0();
				var fin = il.DefineLabel();
				il.Branch(fin);
				il.MarkLabel(fin);
				il.LoadLocal_0();
			});

			var equatableT = typeof(IEquatable<>).MakeGenericType(typeof(T));
			builder.AddInterfaceImplementation(equatableT);
			var equalsT = builder.DefineMethod("Equals");
			equalsT.ClearAttributes();
			equalsT.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
				MethodAttributes.Virtual | MethodAttributes.Final);
			equalsT.ReturnType = TypeRef.FromType<bool>();
			equalsT.DefineParameter("other", typeof(T));
			equalsT.ContributeInstructions(contributedEquals);

			builder.DefineOverrideMethod(typeof(Object).GetMethod("Equals", BindingFlags.Instance | BindingFlags.Public, null,
																														new[] {typeof(object)}, null))
						.ContributeInstructions(contributedEquals);

			return specializedEquals;
		}

		static void ImplementSpecializedGetHashCode(EmittedClass builder, EmittedField chashCodeSeed)
		{
			Contract.Requires<ArgumentNullException>(builder != null);

			var method = builder.DefineMethod("GetHashCode");
			method.ClearAttributes();
			method.IncludeAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual);
			method.ReturnType = TypeRef.FromType<int>();
			method.ContributeInstructions((m, il) =>
			{
				var prime = il.DeclareLocal(typeof(Int32));
				var result = il.DeclareLocal(typeof(Int32));
				il.DeclareLocal(typeof(Int32));
				il.DeclareLocal(typeof(bool));
				il.Nop();
				il.LoadValue(Constants.NotSoRandomPrime);
				il.StoreLocal(prime);
				il.LoadValue(chashCodeSeed);
				il.LoadLocal(prime);
				il.Multiply();
				il.StoreLocal(result);
				var exit = il.DefineLabel();
				var fields =
					new List<EmittedField>(builder.Fields.Where(f => f.IsStatic == false));
				foreach (var field in fields)
				{
					var fieldType = field.FieldType.Target;
					var tc = Type.GetTypeCode(fieldType);
					switch (tc)
					{
						case TypeCode.Boolean:
						case TypeCode.Byte:
						case TypeCode.Char:
						case TypeCode.Int16:
						case TypeCode.Int32:
						case TypeCode.SByte:
						case TypeCode.Single:
						case TypeCode.UInt16:
						case TypeCode.UInt32:
							il.LoadLocal(result);
							il.LoadLocal(prime);
							il.LoadArg_0();
							il.LoadField(field);
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							break;
						case TypeCode.DateTime:
							il.LoadLocal(result);
							il.LoadLocal(prime);
							il.LoadArg_0();
							il.LoadFieldAddress(field);
							il.Constrained(typeof(DateTime));
							il.CallVirtual<object>("GetHashCode");
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							break;
						case TypeCode.Decimal:
							il.LoadLocal(result);
							il.LoadLocal(prime);
							il.LoadArg_0();
							il.LoadFieldAddress(field);
							il.Call<Decimal>("GetHashCode");
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							break;
						case TypeCode.Double:
							il.LoadLocal(result);
							il.LoadLocal(prime);
							il.LoadArg_0();
							il.LoadFieldAddress(field);
							il.Call<Double>("GetHashCode");
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							break;
						case TypeCode.Int64:
							il.LoadLocal(result);
							il.LoadLocal(prime);
							il.LoadArg_0();
							il.LoadFieldAddress(field);
							il.Constrained(typeof(Int64));
							il.CallVirtual<object>("GetHashCode");
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							break;
						case TypeCode.Object:
							if (typeof(Guid).IsAssignableFrom(fieldType))
							{
								il.LoadLocal(result);
								il.LoadLocal(prime);
								il.LoadArg_0();
								il.LoadFieldAddress(field);
								il.Constrained(typeof(Guid));
								il.CallVirtual<object>("GetHashCode");
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
							}
							else if (fieldType.IsArray)
							{
								var elmType = fieldType.GetElementType();
								il.LoadLocal(result);
								il.LoadLocal(prime);
								il.LoadArg_0();
								il.LoadField(field);
								il.LoadLocal(result);
								il.Call(typeof(Core.Extensions).GetMethod("CalculateCombinedHashcode", BindingFlags.Public | BindingFlags.Static)
																							.MakeGenericMethod(elmType));
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
							}
							else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
							{
								il.LoadLocal(result);
								il.LoadLocal(prime);
								il.LoadArg_0();
								il.LoadFieldAddress(field);
								il.Constrained(fieldType);
								il.CallVirtual<object>("GetHashCode");
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
							}
							else
							{
								il.LoadLocal(result);
								il.LoadLocal(prime);
								il.LoadArg_0();
								if (fieldType.IsValueType)
								{
									il.LoadFieldAddress(field);
									il.Constrained(fieldType);
								}
								else
								{
									il.LoadField(field);
								}
								il.CallVirtual<object>("GetHashCode");
								il.Multiply();
								il.Xor();
								il.StoreLocal(result);
							}
							break;
						case TypeCode.String:
							il.LoadArg_0();
							il.LoadField(field);
							il.LoadNull();
							il.CompareEqual();
							il.StoreLocal_2();
							il.LoadLocal_2();
							var lbl = il.DefineLabel();
							il.BranchIfTrue_ShortForm(lbl);
							il.Nop();
							il.LoadLocal(result);
							il.LoadLocal(prime);
							il.LoadArg_0();
							il.LoadField(field);
							il.CallVirtual<object>("GetHashCode");
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							il.MarkLabel(lbl);
							break;
						case TypeCode.UInt64:
							il.LoadLocal(result);
							il.LoadLocal(prime);
							il.LoadArg_0();
							il.LoadFieldAddress(field);
							il.Constrained(typeof(UInt64));
							il.CallVirtual<object>("GetHashCode");
							il.Multiply();
							il.Xor();
							il.StoreLocal(result);
							break;
						default:
							throw new InvalidOperationException(String.Concat("Unable to produce hashcode for type: ",
																																fieldType.GetReadableFullName()));
					}
				}
				il.LoadLocal(result);
				il.StoreLocal_2();
				il.Branch(exit);
				il.MarkLabel(exit);
				il.LoadLocal_2();
			});
		}

		static void LoadFieldFromThisAndParam(ILGenerator il, EmittedField field, EmittedParameter parm)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(field != null);
			il.LoadArg_0();
			il.LoadField(field);
			il.LoadArg(parm);
			il.LoadField(field);
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