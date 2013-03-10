#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Reflection.Emit;
using FlitBit.Core;

namespace FlitBit.Emit
{
	/// <summary>
	/// Helper class for working with IL.
	/// </summary>
	public static class ILGeneratorExtensions
	{
		/// <summary>
		/// Adds two values on the stack and pushes the result onto the stack.
		/// </summary>
		/// <param name="il">il generator</param>
		public static void Add(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Add);
		}

		/// <summary>
		/// Adds two unsigned integers on the stack, performs an overflow check and pushes the result onto the stack.
		/// </summary>		
		/// <param name="il">il generator</param>
		public static void AddUnsignedWithOverflowCheck(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Add_Ovf_Un);
		}

		/// <summary>
		/// Adds two integers on the stack, performs an overflow check and pushes the result onto the stack.
		/// </summary>
		/// <param name="il">il generator</param>
		public static void AddWithOverflowCheck(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Add_Ovf);
		}

		/// <summary>
		/// Pushes an unmanaged pointer to the argument list of the current method onto the stack.
		/// </summary>
		/// <param name="il">il generator</param>
		public static void ArgListPointer(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Arglist);
		}

		/// <summary>
		/// Starts a new scope.
		/// </summary>
		/// <param name="il">il generator</param>
		public static void BeginScope(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.BeginScope();
		}

		/// <summary>
		/// Computes the bitwise AND of two values on the stack and pushes the result onto the stack.
		/// </summary>
		/// <param name="il">il generator</param>
		public static void BitwiseAnd(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.And);
		}

		/// <summary>
		/// Converts a ValueType to an object reference.
		/// </summary>
		/// <param name="valueType">the value type</param>
		/// <param name="il">il generator</param>
		public static void Box(this ILGenerator il, Type valueType)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(valueType != null);

			il.Emit(OpCodes.Box, valueType);
		}

		/// <summary>
		/// Transfers control to a target label.
		/// </summary>
		/// <param name="label"></param>
		/// <param name="il">il generator</param>
		public static void Branch(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Br, label);
		}

		/// <summary>
		/// Transfers control to a target label if two values are equal.
		/// </summary>
		/// <param name="label">A target label.</param>
		/// <param name="il">il generator</param>
		public static void BranchIfEqual(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Beq, label);
		}

		/// <summary>
		/// Transfers control to a target label if two values are not equal.
		/// </summary>
		/// <param name="label">A target label.</param>
		/// <param name="il">il generator</param>
		public static void BranchIfNotEqual_Un(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Bne_Un, label);
		}

		/// <summary>
		/// Transfers control to a target label if two values are not equal.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="type">type of values being compared</param>
		/// <param name="label">target label</param>
		public static void BranchIfNotEqual(this ILGenerator il, TypeRef type, Label label)
		{
			BranchIfNotEqual(il, type.Target, label);
		}

		/// <summary>
		/// Transfers control to a target label if two values are not equal.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="type">type of values being compared</param>
		/// <param name="label">target label</param>		
		public static void BranchIfNotEqual(this ILGenerator il, Type type, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(type != null);

			var typeCode = Type.GetTypeCode(type);
			switch (typeCode)
			{
				case TypeCode.Boolean: BranchIfNotEqual_Un(il, label); break;
				case TypeCode.Byte: BranchIfNotEqual_Un(il, label); break;
				case TypeCode.Char: BranchIfNotEqual_Un(il, label); break;
				case TypeCode.DateTime:
					il.Call<DateTime>("op_Equality", BindingFlags.Public | BindingFlags.Static);
					il.BranchIfFalse(label);
					break;
				case TypeCode.Decimal:
					il.Call<DateTime>("op_Equality", BindingFlags.Public | BindingFlags.Static);
					il.BranchIfFalse(label);
					break;
				case TypeCode.Double:
					il.Call<DateTime>("op_Equality", BindingFlags.Public | BindingFlags.Static);
					il.BranchIfFalse(label);
					break;
				case TypeCode.Int16: BranchIfNotEqual_Un(il, label); break;
				case TypeCode.Int32: BranchIfNotEqual_Un(il, label); break;
				case TypeCode.Int64: BranchIfNotEqual_Un(il, label); break;
				case TypeCode.SByte: BranchIfNotEqual_Un(il, label); break;
				case TypeCode.Single: BranchIfNotEqual_Un(il, label); break;
				case TypeCode.String:
					il.Call<DateTime>("op_Equality", BindingFlags.Public | BindingFlags.Static);
					il.BranchIfFalse(label);
					break;
				case TypeCode.UInt16: BranchIfNotEqual_Un(il, label); break;
				case TypeCode.UInt32: BranchIfNotEqual_Un(il, label); break;
				case TypeCode.UInt64: BranchIfNotEqual_Un(il, label); break;
				default:
					if (type.IsEnum)
					{
						BranchIfNotEqual(il, Enum.GetUnderlyingType(type), label);
					}
					else
					{
						var op_Equality = type.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);
						if (op_Equality != null)
						{
							il.Call(op_Equality);
							il.BranchIfFalse(label);
						}
						else
						{
							// compare the object reference
							BranchIfNotEqual_Un(il, label); break;
						}
					}
					break;
			}
		}

		/// <summary>
		/// Transfers control to a target label if two values are equal.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfEqual_ShortForm(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Beq_S, label);
		}

		/// <summary>
		/// Transfers control to a target label if the value on the stack is false, null, or zero.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfFalse(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Brfalse, label);
		}

		/// <summary>
		/// Transfers control to a target label if the value on the stack is false, null, or zero.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfFalse_ShortForm(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Brfalse_S, label);
		}

		/// <summary>
		/// Transfers control to a target label if the first value on the stack 
		/// is greater than the second value on the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfGreaterThan(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Bgt, label);
		}

		/// <summary>
		/// Transfers control to a target label if the first value on the stack 
		/// is greater than or equal to the second value on the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfGreaterThanOrEqual(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Bge, label);
		}

		/// <summary>
		/// Transfers control to a target label if the first value on the stack 
		/// is greater than or equal to the second value on the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfGreaterThanOrEqual_ShortForm(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Bge_S, label);
		}

		/// <summary>
		/// Transfers control to a target label if the first value on the stack 
		/// is greater than or equal to the second value on the stack when
		/// comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfGreaterThanOrEqual_Unsigned(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Bge_Un, label);
		}

		/// <summary>
		/// Transfers control to a target label if the first value on the stack 
		/// is greater than or equal to the second value on the stack when
		/// comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfGreaterThanOrEqual_Unsigned_ShortForm(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Bge_Un_S, label);
		}

		/// <summary>
		/// Transfers control to a target label the first value on the stack 
		/// is greater than the second value on the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfGreaterThan_ShortForm(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Bgt_S, label);
		}

		/// <summary>
		/// Transfers control to a target label the first value on the stack 
		/// is greater than the second value on the stack when
		/// comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfGreaterThan_Unsigned(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Bgt_Un, label);
		}

		/// <summary>
		/// Transfers control to a target label if the first value on the stack 
		/// is greater than the second value on the stack when
		/// comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfGreaterThan_Unsigned_ShortForm(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Bgt_Un_S, label);
		}

		/// <summary>
		/// Transfers control to a target label the first value on the stack 
		/// is less than the second value on the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfLessThan(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Blt, label);
		}

		/// <summary>
		/// Transfers control to a target label if the first value on the stack 
		/// is less than or equal to the second value on the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfLessThanOrEqual(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ble, label);
		}

		/// <summary>
		/// Transfers control to a target label if the first value on the stack 
		/// is less than or equal to the second value on the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfLessThanOrEqual_ShortForm(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ble_S, label);
		}

		/// <summary>
		/// Transfers control to a target label if the first value on the stack 
		/// is less than or equal to the second value on the stack when
		/// comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfLessThanOrEqual_Unsigned(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ble_Un, label);
		}

		/// <summary>
		/// Transfers control to a target label if the first value on the stack 
		/// is less than or equal to the second value on the stack when
		/// comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfLessThanOrEqual_Unsigned_ShortForm(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ble_Un_S, label);
		}

		/// <summary>
		/// Transfers control to a target label if the first value on the stack 
		/// is less than the second value on the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfLessThan_ShortForm(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Blt_S, label);
		}

		/// <summary>
		/// Transfers control to a target label if the first value on the stack 
		/// is less than the second value on the stack when
		/// comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfLessThan_Unsigned(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Blt_Un, label);
		}

		/// <summary>
		/// Transfers control to a target label if the first value on the stack 
		/// is less than the second value on the stack when
		/// comparing unsigned integer values or unordered float values.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfLessThan_Unsigned_ShortForm(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Blt_Un_S, label);
		}

		/// <summary>
		/// Transfers control to a target label if the value on the stack is true, not null, or non zero.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfTrue(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Brtrue, label);
		}

		/// <summary>
		/// Transfers control to a target label if the value on the stack is true, not null, or non zero.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void BranchIfTrue_ShortForm(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Brtrue_S, label);
		}

		/// <summary>
		/// Transfers control to a target label.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="label">A target label.</param>
		public static void Branch_ShortForm(this ILGenerator il, Label label)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Br_S, label);
		}

		/// <summary>
		/// Signals the CLI to inform the debugger that a breakpoint has been tripped.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void Break(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Break);
		}

		/// <summary>
		/// Calls the method indicated by the method descriptor.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="method">MethodInfo for the method to call.</param>
		public static void Call(this ILGenerator il, MethodInfo method)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(method != null, "method cannot be null");

			il.EmitCall(OpCodes.Call, method, null);
		}

		/// <summary>
		/// Emits instructions to call the method given.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="method">the method</param>
		public static void Call(this ILGenerator il, EmittedMethodBase method)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(method != null, "method cannot be null");

			method.EmitCall(il);
		}

		/// <summary>
		/// Emits instructions to call a method by name on type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="name">name of the method to call</param>
		public static void Call<T>(this ILGenerator il, string name)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			var method = typeof(T).GetMethod(name);
			Contract.Assert(method != null, "method lookup failed");

			il.EmitCall(OpCodes.Call, method, null);
		}

		/// <summary>
		/// Emits instructions to call a method by name on type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="name">name of the method to call</param>
		/// <param name="bindingAttr">method binding flags used to lookup the method</param>
		public static void Call<T>(this ILGenerator il, string name, BindingFlags bindingAttr)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			var method = typeof(T).GetMethod(name, bindingAttr);
			Contract.Assert(method != null, "method lookup failed");

			il.EmitCall(OpCodes.Call, method, null);
		}

		/// <summary>
		/// Emits instructions to call a method by name on type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="name">name of the method to call</param>
		/// <param name="types">parameters that differentiate the method to call</param>
		public static void Call<T>(this ILGenerator il, string name, params Type[] types)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			var method = typeof(T).GetMethod(name, types);
			Contract.Assert(method != null, "method lookup failed");

			il.EmitCall(OpCodes.Call, method, null);
		}

		/// <summary>
		/// Emits instructions to call a method by name on type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="name">name of the method to call</param>
		/// <param name="bindingAttr">method binding flags used to lookup the method</param>
		/// <param name="types">parameters that differentiate the method to call</param>
		public static void Call<T>(this ILGenerator il, string name, BindingFlags bindingAttr, params Type[] types)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			var method = typeof(T).GetMethod(name, bindingAttr, null, types, null);
			Contract.Assert(method != null, "method lookup failed");

			il.EmitCall(OpCodes.Call, method, null);
		}

		/// <summary>
		/// Calls the constructor indicated by the constructor descriptor.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="ctor">ConstructorInfo for the constructor to call.</param>
		public static void Call(this ILGenerator il, ConstructorInfo ctor)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(ctor != null, "ctor cannot be null");

			il.Emit(OpCodes.Call, ctor);
		}

		/// <summary>
		/// Calls the method indicated on the evaluation stack (as a pointer to an entry point).
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="method">MethodInfo for the method to call.</param>
		/// <param name="callingConventions">The managed calling conventions to be used.</param>
		/// <param name="returnType">The return type of the method if it returns a result; otherwise null.</param>
		/// <param name="parameterTypes">The types of parameters for the call.</param>
		/// <param name="optionalParameterTypes">The types of optional parameters for the call if the method accepts optional parameters; otherwise null.</param>
		public static void CallIndirectManaged(this ILGenerator il, MethodInfo method, CallingConventions callingConventions, Type returnType
			, Type[] parameterTypes, Type[] optionalParameterTypes)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(method != null, "method cannot be null");

			il.EmitCalli(OpCodes.Calli, callingConventions, returnType, parameterTypes, optionalParameterTypes);
		}

		/// <summary>
		/// Calls the method indicated on the evaluation stack (as a pointer to an entry point).
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="method">MethodInfo for the method to call.</param>
		/// <param name="callingConventions">The unmanaged calling conventions to be used.</param>
		/// <param name="returnType">The return type of the method if it returns a result; otherwise null.</param>
		/// <param name="parameterTypes">The types of parameters for the call.</param>
		public static void CallIndirectUnanaged(this ILGenerator il, MethodInfo method, System.Runtime.InteropServices.CallingConvention callingConventions, Type returnType
			, Type[] parameterTypes)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(method != null, "method cannot be null");

			il.EmitCalli(OpCodes.Calli, callingConventions, returnType, parameterTypes);
		}

		/// <summary>
		/// Calls the varargs method indicated by the method descriptor.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="method">MethodInfo for the method to call.</param>
		/// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method; otherwise null.</param>
		public static void CallVarArgs(this ILGenerator il, MethodInfo method, Type[] optionalParameterTypes)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(method != null, "method cannot be null");

			il.EmitCall(OpCodes.Call, method, optionalParameterTypes);
		}

		/// <summary>
		/// Calls a late-bound method on an object, pushing the result object onto the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="method">MethodInfo for the method to call.</param>
		/// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method; otherwise null.</param>
		public static void CallVarArgsVirtual(this ILGenerator il, MethodInfo method, Type[] optionalParameterTypes)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(method != null, "method cannot be null");

			il.EmitCall(OpCodes.Callvirt, method, optionalParameterTypes);
		}

		/// <summary>
		/// Calls a late-bound method on an object, pushing the result object onto the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="method">MethodInfo for the method to call.</param>
		public static void CallVirtual(this ILGenerator il, MethodInfo method)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(method != null, "method cannot be null");

			il.EmitCall(OpCodes.Callvirt, method, null);
		}

		/// /// <summary>
		/// Emits instructions to call a virtual method by name on type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="name">the target method's name</param>
		public static void CallVirtual<T>(this ILGenerator il, string name)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			var method = typeof(T).GetMethod(name);
			Contract.Assert(method != null, "method lookup failed");

			il.EmitCall(OpCodes.Callvirt, method, null);
		}

		/// /// <summary>
		/// Emits instructions to call a virtual method by name on type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="name">the target method's name</param>
		/// <param name="parameterTypes">parameters that differentiate the method to call</param>
		public static void CallVirtual<T>(this ILGenerator il, string name, params Type[] parameterTypes)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			var method = typeof(T).GetMethod(name, parameterTypes);
			Contract.Assert(method != null, "method lookup failed");

			il.EmitCall(OpCodes.Callvirt, method, null);
		}

		/// /// <summary>
		/// Emits instructions to call a virtual method by name on type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="name">the target method's name</param>
		/// <param name="binding">binding flags used to lookup the method</param>
		/// <param name="parameterTypes">parameters that differentiate the method to call</param>
		public static void CallVirtual<T>(this ILGenerator il, string name, BindingFlags binding, params Type[] parameterTypes)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			var method = typeof(T).GetMethod(name, binding, null, parameterTypes, null);
			Contract.Assert(method != null, "method lookup failed");

			il.EmitCall(OpCodes.Callvirt, method, null);
		}

		/// <summary>
		/// Casts to the target type.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="targetType">the target type</param>
		public static void CastClass(this ILGenerator il, Type targetType)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(targetType != null, "targetType cannot be null");

			il.Emit(OpCodes.Castclass, targetType);
		}

		/// <summary>
		/// Throws a System.ArithmeticException if the value on the stack is not a finite number.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void CheckFinite(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ckfinite);
		}

		/// <summary>
		/// Compares two values on the stack and if they are equal, the integer value 1 is placed on the stack; otherwise the value 0 is placed on the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void CompareEqual(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ceq);
		}

		/// <summary>
		/// Compares the values on the stack for equality.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="type">the values' type</param>
		public static void CompareEquality(this ILGenerator il, Type type)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(type != null);

			var typeCode = Type.GetTypeCode(type);
			switch (typeCode)
			{
				case TypeCode.Boolean: CompareEqual(il); break;
				case TypeCode.Byte: CompareEqual(il); break;
				case TypeCode.Char: CompareEqual(il); break;
				case TypeCode.DateTime: il.Call<DateTime>("op_Equality", BindingFlags.Public | BindingFlags.Static); break;
				case TypeCode.Decimal: il.Call<Decimal>("op_Equality", BindingFlags.Public | BindingFlags.Static); break;
				case TypeCode.Double: il.Call<Double>("op_Equality", BindingFlags.Public | BindingFlags.Static); break;
				case TypeCode.Int16: CompareEqual(il); break;
				case TypeCode.Int32: CompareEqual(il); break;
				case TypeCode.Int64: CompareEqual(il); break;
				case TypeCode.SByte: CompareEqual(il); break;
				case TypeCode.Single: CompareEqual(il); break;
				case TypeCode.String: il.Call<string>("op_Equality", BindingFlags.Public | BindingFlags.Static); break;
				case TypeCode.UInt16: CompareEqual(il); break;
				case TypeCode.UInt32: CompareEqual(il); break;
				case TypeCode.UInt64: CompareEqual(il); break;
				default:
					if (type.IsEnum)
					{
						CompareEquality(il, Enum.GetUnderlyingType(type));
					}
					if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
					{
						var argType = type.GetGenericArguments()[0];
						il.Call(typeof(Nullable).GetGenericMethod("Equals", BindingFlags.Static | BindingFlags.Public, 2, 1).MakeGenericMethod(argType));
					}
					else
					{
						var op_Equality = type.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);
						if (op_Equality == null)
						{
							op_Equality = typeof(Object).GetMethod("Equals", BindingFlags.Public | BindingFlags.Static);
						}
						il.Call(op_Equality);
					}
					break;
			}
		}

		/// <summary>
		/// Compares the two values on top of the stack for inequality.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="type">the values' type</param>
		/// <param name="lookingForInequality">indicates whether inequality is desired; reverses the logic</param>
		public static void CompareEquality(this ILGenerator il, Type type, bool lookingForInequality)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(type != null);

			var typeCode = Type.GetTypeCode(type);
			switch (typeCode)
			{
				case TypeCode.Boolean: CompareEqual(il); break;
				case TypeCode.Byte: CompareEqual(il); break;
				case TypeCode.Char: CompareEqual(il); break;
				case TypeCode.DateTime:
					if (lookingForInequality)
					{
						il.Call<DateTime>("op_Inequality", BindingFlags.Public | BindingFlags.Static);
						il.Load_I4_0();
						il.CompareEqual();
					}
					else
					{
						il.Call<DateTime>("op_Equality", BindingFlags.Public | BindingFlags.Static);
					}
					break;
				case TypeCode.Decimal:
					if (lookingForInequality)
					{
						il.Call<Decimal>("op_Inequality", BindingFlags.Public | BindingFlags.Static);
						il.Load_I4_0();
						il.CompareEqual();
					}
					else
					{
						il.Call<Decimal>("op_Equality", BindingFlags.Public | BindingFlags.Static);
					}
					break;
				case TypeCode.Double:
					if (lookingForInequality)
					{
						il.Call<Double>("op_Inequality", BindingFlags.Public | BindingFlags.Static);
						il.Load_I4_0();
						il.CompareEqual();
					}
					else
					{
						il.Call<Double>("op_Equality", BindingFlags.Public | BindingFlags.Static);
					}
					break;
				case TypeCode.Int16: CompareEqual(il); break;
				case TypeCode.Int32: CompareEqual(il); break;
				case TypeCode.Int64: CompareEqual(il); break;
				case TypeCode.SByte: CompareEqual(il); break;
				case TypeCode.Single: CompareEqual(il); break;
				case TypeCode.String:
					if (lookingForInequality)
					{
						il.Call<string>("op_Inequality", BindingFlags.Public | BindingFlags.Static);
						il.Load_I4_0();
						il.CompareEqual();
					}
					else
					{
						il.Call<string>("op_Equality", BindingFlags.Public | BindingFlags.Static);
					}
					break;
				case TypeCode.UInt16: CompareEqual(il); break;
				case TypeCode.UInt32: CompareEqual(il); break;
				case TypeCode.UInt64: CompareEqual(il); break;
				default:
					if (type.IsEnum)
					{
						CompareEquality(il, Enum.GetUnderlyingType(type));
					}
					else if (lookingForInequality)
					{
						var op_Inequality = type.GetMethod("op_Inequality", BindingFlags.Public | BindingFlags.Static);
						if (op_Inequality == null)
						{
							il.Call<Object>("Equals", BindingFlags.Public | BindingFlags.Static);
							il.Load_I4_1();
							il.CompareEqual();
						}
						else
						{
							il.Call(op_Inequality);
							il.Load_I4_0();
							il.CompareEqual();
						}
					}
					if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
					{
						var argType = type.GetGenericArguments()[0];
						il.Call(typeof(Nullable).GetGenericMethod("Equals", BindingFlags.Static | BindingFlags.Public, 2, 1).MakeGenericMethod(argType));
						il.Load_I4_1();
						il.CompareEqual();
					}
					else
					{
						var op_Equality = type.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);
						if (op_Equality == null)
						{
							op_Equality = typeof(Object).GetMethod("Equals", BindingFlags.Public | BindingFlags.Static);
						}
						il.Call(op_Equality);
					}
					break;
			}
		}

		/// <summary>
		/// Compares the two values placed on the stack via callback methods for equality.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="loadLeftOperand">action that pushes the left hand operand onto the stack</param>
		/// <param name="loadRightOperand">action that pushes the right hand operand onto the stack</param>
		/// <param name="type">the values' type</param>
		/// <param name="lookingForInequality">indicates whether inequality is desired; reverses the logic</param>
		public static void CompareEquality(this ILGenerator il, Action<ILGenerator> loadLeftOperand,
			Action<ILGenerator> loadRightOperand, Type type, bool lookingForInequality)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(type != null);

			var typeCode = Type.GetTypeCode(type);
			switch (typeCode)
			{
				case TypeCode.Boolean:
					loadLeftOperand(il);
					loadRightOperand(il);
					CompareEqual(il);
					break;
				case TypeCode.Byte:
					loadLeftOperand(il);
					loadRightOperand(il);
					CompareEqual(il); break;
				case TypeCode.Char:
					loadLeftOperand(il);
					loadRightOperand(il);
					CompareEqual(il); break;
				case TypeCode.DateTime:
					loadLeftOperand(il);
					loadRightOperand(il);
					if (lookingForInequality)
					{
						il.Call<DateTime>("op_Inequality", BindingFlags.Public | BindingFlags.Static);
						il.Load_I4_0();
						il.CompareEqual();
					}
					else
					{
						il.Call<DateTime>("op_Equality", BindingFlags.Public | BindingFlags.Static);
					}
					break;
				case TypeCode.Decimal:
					loadLeftOperand(il);
					loadRightOperand(il);
					if (lookingForInequality)
					{
						il.Call<Decimal>("op_Inequality", BindingFlags.Public | BindingFlags.Static);
						il.Load_I4_0();
						il.CompareEqual();
					}
					else
					{
						il.Call<Decimal>("op_Equality", BindingFlags.Public | BindingFlags.Static);
					}
					break;
				case TypeCode.Double:
					loadLeftOperand(il);
					loadRightOperand(il);
					if (lookingForInequality)
					{
						il.Call<Double>("op_Inequality", BindingFlags.Public | BindingFlags.Static);
						il.Load_I4_0();
						il.CompareEqual();
					}
					else
					{
						il.Call<Double>("op_Equality", BindingFlags.Public | BindingFlags.Static);
					}
					break;
				case TypeCode.Int16:
					loadLeftOperand(il);
					loadRightOperand(il);
					CompareEqual(il); break;
				case TypeCode.Int32:
					loadLeftOperand(il);
					loadRightOperand(il);
					CompareEqual(il); break;
				case TypeCode.Int64:
					loadLeftOperand(il);
					loadRightOperand(il);
					CompareEqual(il); break;
				case TypeCode.SByte:
					loadLeftOperand(il);
					loadRightOperand(il);
					CompareEqual(il); break;
				case TypeCode.Single:
					loadLeftOperand(il);
					loadRightOperand(il);
					CompareEqual(il); break;
				case TypeCode.String:
					loadLeftOperand(il);
					loadRightOperand(il);
					if (lookingForInequality)
					{
						il.Call<string>("op_Inequality", BindingFlags.Public | BindingFlags.Static);
						il.Load_I4_0();
						il.CompareEqual();
					}
					else
					{
						il.Call<string>("op_Equality", BindingFlags.Public | BindingFlags.Static);
					}
					break;
				case TypeCode.UInt16:
					loadLeftOperand(il);
					loadRightOperand(il);
					CompareEqual(il); break;
				case TypeCode.UInt32:
					loadLeftOperand(il);
					loadRightOperand(il);
					CompareEqual(il); break;
				case TypeCode.UInt64:
					loadLeftOperand(il);
					loadRightOperand(il);
					CompareEqual(il); break;
				default:
					if (type.IsEnum)
					{
						loadLeftOperand(il);
						loadRightOperand(il);
						CompareEquality(il, Enum.GetUnderlyingType(type));
					}
					else if (lookingForInequality)
					{
						var op_Inequality = type.GetMethod("op_Inequality", BindingFlags.Public | BindingFlags.Static);
						if (op_Inequality == null)
						{
							il.Call(typeof(EqualityComparer<>).MakeGenericType(type).GetMethod("get_Default", BindingFlags.Static | BindingFlags.Public));
							loadLeftOperand(il);
							loadRightOperand(il);
							il.CallVirtual(typeof(IEqualityComparer<>).MakeGenericType(type).GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance,
								null,
								new Type[] { type, type },
								null
								));
							il.Load_I4_1();
							il.CompareEqual();
						}
						else
						{
							loadLeftOperand(il);
							loadRightOperand(il);
							il.Call(op_Inequality);
							il.Load_I4_0();
							il.CompareEqual();
						}
					}
					else
					{
						var op_Equality = type.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);
						if (op_Equality == null)
						{
							il.Call(typeof(EqualityComparer<>).MakeGenericType(type).GetMethod("get_Default", BindingFlags.Static | BindingFlags.Public));
							loadLeftOperand(il);
							loadRightOperand(il);
							il.CallVirtual(typeof(IEqualityComparer<>).MakeGenericType(type).GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance,
								null,
								new Type[] { type, type },
								null
								));
						}
						else
						{
							loadLeftOperand(il);
							loadRightOperand(il);
							il.Call(op_Equality);
						}
					}
					break;
			}
		}

		/// <summary>
		/// Loads the default value for a type; similar to C#'s default keyword.
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		public static void LoadDefaultValue(this ILGenerator il, Type type)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(type != null);

			var typeCode = Type.GetTypeCode(type);
			switch (typeCode)
			{
				case TypeCode.Boolean:
					il.Load_I4_0();
					break;
				case TypeCode.Byte:
					il.Load_I4_0();
					break;
				case TypeCode.Char:
					il.Load_I4_0();
					break;
				case TypeCode.DateTime:
					var localDateTime = il.DeclareLocal(type);
					il.LoadLocalAddress(localDateTime);
					il.InitObject(type);
					il.LoadLocal(localDateTime);
					break;
				case TypeCode.Decimal:
					il.LoadValue(0M);
					break;
				case TypeCode.Double:
					il.LoadValue(0d);
					break;
				case TypeCode.Int16:
					il.Load_I4_0();
					break;
				case TypeCode.Int32:
					il.Load_I4_0();
					break;
				case TypeCode.Int64:
					il.LoadValue(0L);
					break;
				case TypeCode.Object:
					if (type.IsEnum)
					{
						LoadDefaultValue(il, Enum.GetUnderlyingType(type));
					}
					else if (type.IsValueType)
					{
						var localStruct = il.DeclareLocal(type);
						il.LoadLocalAddress(localStruct);
						il.InitObject(type);
						il.LoadLocal(localStruct);					
					}
					else
					{
						il.LoadNull();
					}
					break;
				case TypeCode.SByte:
					il.Load_I4_0();
					break;
				case TypeCode.Single:
					il.LoadValue((float)0d);
					break;
				case TypeCode.String:
					il.LoadNull();
					break;
				case TypeCode.UInt16:
					il.Load_I4_0();
					break;
				case TypeCode.UInt32:
					break;
				case TypeCode.UInt64:
					il.LoadValue(0L);
					break;
				default:
					throw new InvalidOperationException(String.Concat("Not default value for type: ", type.GetReadableFullName()));
			}
		}

		/// <summary>
		/// Compares two values on the stack and if the first value is greater than the second, the integer value 1 is placed on the stack; otherwise the value 0 is placed on the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void CompareGreaterThan(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Cgt);
		}

		/// <summary>
		/// Compares two values on the stack and if the first value is greater than the second, the integer value 1 is placed on the stack; otherwise the value 0 is placed on the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void CompareGreaterThan_Unsigned(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Cgt_Un);
		}

		/// <summary>
		/// Compares two values on the stack and if the first value is less than the second, the integer value 1 is placed on the stack; otherwise the value 0 is placed on the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void CompareLessThan(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Clt);
		}

		/// <summary>
		/// Compares two values on the stack and if the first value is less than the second, the integer value 1 is placed on the stack; otherwise the value 0 is placed on the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void CompareLessThan_Unsigned(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Clt_Un);
		}

		/// <summary>
		/// Emits the constrained op code.
		/// </summary>
		/// <param name="il"></param>
		/// <param name="t"></param>
		public static void Constrained(this ILGenerator il, Type t)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(t != null);
			il.Emit(OpCodes.Constrained, t);
		}

		/// <summary>
		/// Converts the value on the top of the stack to a float32.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToFloat32(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_R4);
		}

		/// <summary>
		/// Converts the unsigned value on the top of the stack to a float32.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToFloat32WithOverflow_Unsigned(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_R_Un);
		}

		/// <summary>
		/// Converts the value on the top of the stack to a float64.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToFloat64(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_R8);
		}

		/// <summary>
		/// Converts the value on the top of the stack to an int16 and pads it to an int.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToInt16(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_I2);
		}

		/// <summary>
		/// Converts the value on the top of the stack to an int16 and pads it to an int.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToInt16WithOverflow(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_I2);
		}

		/// <summary>
		/// Converts the unsigned value on the top of the stack to an int16 and pads it to an int.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToInt16WithOverflow_Unsigned(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_I2_Un);
		}

		/// <summary>
		/// Converts the value on the top of the stack to an int32.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToInt32(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_I4);
		}

		/// <summary>
		/// Converts the value on the top of the stack to an int32.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToInt32WithOverflow(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_I4);
		}

		/// <summary>
		/// Converts the unsigned value on the top of the stack to an int32.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToInt32WithOverflow_Unsigned(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_I4_Un);
		}

		/// <summary>
		/// Converts the value on the top of the stack to an int64.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToInt64(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_I8);
		}

		/// <summary>
		/// Converts the value on the top of the stack to an int64.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToInt64WithOverflow(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_I8);
		}

		/// <summary>
		/// Converts the unsigned value on the top of the stack to an int64.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToInt64WithOverflow_Unsigned(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_I8_Un);
		}

		/// <summary>
		/// Converts the value on the top of the stack to an int8 and pads it to an int.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToInt8(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_I1);
		}

		/// <summary>
		/// Converts the value on the top of the stack to an int8 and pads it to an int.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToInt8WithOverflow(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_I1);
		}

		/// <summary>
		/// Converts the unsigned value on the top of the stack to an int8 and pads it to an int.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToInt8WithOverflow_Unsigned(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_I1_Un);
		}

		/// <summary>
		/// Converts the value on the top of the stack to a natural int.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToNaturalInt(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_I);
		}

		/// <summary>
		/// Converts the value on the top of the stack to a natural int.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToNaturalIntWithOverflow(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_I);
		}

		/// <summary>
		/// Converts the unsigned value on the top of the stack to a natural int.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToNaturalIntWithOverflow_Unsigned(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_I_Un);
		}

		/// <summary>
		/// Converts the value on the top of the stack to an unsigned int16 and pads it to an int.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUInt16(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_U2);
		}

		/// <summary>
		/// Converts the value on the top of the stack to an unsigned int32.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUInt32(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_U4);
		}

		/// <summary>
		/// Converts the value on the top of the stack to an unsigned int64.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUInt64(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_U8);
		}

		/// <summary>
		/// Converts the value on the top of the stack to an unsigned int8 and pads it to an int.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUInt8(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_U1);
		}

		/// <summary>
		/// Converts the signed value on the top of the stack to a unsigned int16 and pads it to an int32.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUnsignedInt16WithOverflow(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_U2);
		}

		/// <summary>
		/// Converts the unsigned value on the top of the stack to an unsigned int16 and pads it to int32.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUnsignedInt16WithOverflow_Unsigned(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_U2_Un);
		}

		/// <summary>
		/// Converts the signed value on the top of the stack to a unsigned int32.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUnsignedInt32WithOverflow(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_U4);
		}

		/// <summary>
		/// Converts the unsigned value on the top of the stack to an unsigned int32.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUnsignedInt32WithOverflow_Unsigned(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_U4_Un);
		}

		/// <summary>
		/// Converts the signed value on the top of the stack to a unsigned int64.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUnsignedInt64WithOverflow(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_U8);
		}

		/// <summary>
		/// Converts the unsigned value on the top of the stack to an unsigned int64.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUnsignedInt64WithOverflow_Unsigned(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_U8_Un);
		}

		/// <summary>
		/// Converts the signed value on the top of the stack to a unsigned int8 and pads it to an int32.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUnsignedInt8WithOverflow(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_U1);
		}

		/// <summary>
		/// Converts the unsigned value on the top of the stack to an unsigned int8 and pads it to int32.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUnsignedInt8WithOverflow_Unsigned(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_U1_Un);
		}

		/// <summary>
		/// Converts the value on the top of the stack to a unsigned natural int.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUnsignedNaturalInt(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_U);
		}

		/// <summary>
		/// Converts the signed value on the top of the stack to a natural unsigned int.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUnsignedNaturalIntWithOverflow(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_U);
		}

		/// <summary>
		/// Converts the unsigned value on the top of the stack to an unsigned natural int.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void ConvertToUnsignedNaturalIntWithOverflow_Unsigned(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Conv_Ovf_U_Un);
		}

		/// <summary>
		/// Copies a specified number of bytes from a source address to a destination address.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void CopyBlock(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Cpblk);
		}

		/// <summary>
		/// Copies the value type located at an address to another address (type &amp;, *, or natural int).
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void CopyObject(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Cpobj);
		}

		/// <summary>
		/// Declares a local variable.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="localType">the local's type</param>
		/// <returns>a local builder</returns>
		public static LocalBuilder DeclareLocal(this ILGenerator il, Type localType)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			return il.DeclareLocal(localType);
		}

		/// <summary>
		/// Declares a local variable.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="localType">the local's type</param>
		/// <param name="pinned">indicates whether the local should be pinned</param>
		/// <returns>a local builder</returns>
		public static LocalBuilder DeclareLocal(this ILGenerator il, Type localType, bool pinned)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			return il.DeclareLocal(localType, pinned);
		}

		/// <summary>
		/// Defines and marks a label.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <returns>returns the label</returns>
		public static Label DefineAndMarkLabel(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Label result = il.DefineLabel();
			il.MarkLabel(result);
			return result;
		}

		/// <summary>
		/// Defines a label.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <returns>returns the label</returns>
		public static Label DefineLabel(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			return il.DefineLabel();
		}

		/// <summary>
		/// Divides two values and pushes the result as a floating-point or quotient onto the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void Divide(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Div);
		}

		/// <summary>
		/// Divides two unsigned integer values and pushes the result (int32) onto the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void Divide_Unsigned(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Div_Un);
		}

		/// <summary>
		/// Copies the topmost value on the evaluation stack and pushes the copy onto the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void Duplicate(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Dup);
		}

		/// <summary>
		/// Transfers control back from the filter clause of an exception block back to the CLI exception handler.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void EndFilter(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Endfilter);
		}

		/// <summary>
		/// Transfers control back from the fault or finally clause of an exception block back to the CLI exception handler.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void EndFinally(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Endfinally);
		}

		/// <summary>
		/// Ends a scope.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		public static void EndScope(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.EndScope();
		}

		/// <summary>
		/// Initializes each field of the value type at a specified address to a null reference or a 0 of the appropriate primitive type.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="type">the type</param>
		public static void InitObject(this ILGenerator il, Type @type)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(@type != null);

			il.Emit(OpCodes.Initobj, @type);
		}

		/// <summary>
		/// Copies a value type object pointed to by an address to the top of the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="type">the type being copied</param>
		public static void LoadValueType(this ILGenerator il, Type type)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(type != null && type.IsValueType);			

			il.Emit(OpCodes.Ldobj, type);
		}

		/// <summary>
		/// Emits an instruction to test whether an object reference
		/// (type O) is an instance of a particular class.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="type">the type</param>
		public static void IsInstance(this ILGenerator il, Type type)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(type != null);
			il.Emit(OpCodes.Isinst, type);
		}

		/// <summary>
		/// Emits instructions to load an argument (referenced by a specified index value) onto the stack.
		/// </summary>
		/// <param name="il">an ILGenerator where instructions are emitted</param>
		/// <param name="index">the arg's index</param>
		public static void LoadArg(this ILGenerator il, int index)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			switch (index)
			{
				case 0: il.Emit(OpCodes.Ldarg_0); break;
				case 1: il.Emit(OpCodes.Ldarg_1); break;
				case 2: il.Emit(OpCodes.Ldarg_2); break;
				case 3: il.Emit(OpCodes.Ldarg_3); break;
				default: il.Emit(OpCodes.Ldarg, index); break;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void LoadElementRef(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldelem_Ref);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void LoadArg_0(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldarg_0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void LoadArg_1(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldarg_1);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void LoadArg_2(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldarg_2);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void LoadArg_3(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldarg_3);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="a"></param>
		public static void LoadArg_ShortForm(this ILGenerator il, short a)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldarg_S, a);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="a"></param>
		public static void LoadArgAddress(this ILGenerator il, int a)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldarga, a);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="parameter"></param>
		public static void LoadArgAddress(this ILGenerator il, EmittedParameter parameter)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(parameter != null, "Parameter cannot be null");
			int ofs = (parameter.Method.IsStatic) ? 0 : 1;
			il.Emit(OpCodes.Ldarga, parameter.Index + ofs);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="a"></param>
		public static void LoadArgAddress_ShortForm(this ILGenerator il, int a)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldarga_S, a);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="parameter"></param>
		public static void LoadArgAddress_ShortForm(this ILGenerator il, EmittedParameter parameter)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(parameter != null, "Parameter cannot be null");
			int ofs = (parameter.Method.IsStatic) ? 0 : 1;
			il.Emit(OpCodes.Ldarga_S, parameter.Index + ofs);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="field"></param>
		public static void LoadField(this ILGenerator il, IFieldRef field)
		{
			Contract.Requires<ArgumentNullException>(field != null);
			LoadField(il, field.FieldInfo);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="field"></param>
		public static void LoadField(this ILGenerator il, FieldInfo field)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(field != null);

			if (field.IsStatic) il.Emit(OpCodes.Ldsfld, field);
			else il.Emit(OpCodes.Ldfld, field);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="field"></param>
		public static void LoadFieldAddress(this ILGenerator il, FieldInfo field)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(field != null);
			if (field.IsStatic) il.Emit(OpCodes.Ldsflda, field);
			else il.Emit(OpCodes.Ldflda, field);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="field"></param>
		public static void LoadFieldAddress(this ILGenerator il, EmittedField field)
		{
			Contract.Requires<ArgumentNullException>(field != null);
			LoadFieldAddress(il, field.Builder);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="local"></param>
		public static void LoadLocal(this ILGenerator il, LocalBuilder @local)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(@local != null, "@local cannot be null");

			il.LoadLocal(@local.LocalIndex);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="lcl"></param>
		public static void LoadLocal(this ILGenerator il, int lcl)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			switch (lcl)
			{
				case 0: il.Emit(OpCodes.Ldloc_0); break;
				case 1: il.Emit(OpCodes.Ldloc_1); break;
				case 2: il.Emit(OpCodes.Ldloc_2); break;
				case 3: il.Emit(OpCodes.Ldloc_3); break;
				default: il.Emit(OpCodes.Ldloc, lcl); break;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="local"></param>
		public static void LoadLocalAddress(this ILGenerator il, LocalBuilder local)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(local != null, "@local cannot be null");

			il.LoadLocalAddress(local.LocalIndex);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="lcl"></param>
		public static void LoadLocalAddress(this ILGenerator il, int lcl)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldloca, lcl);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="local"></param>
		public static void LoadLocalAddressShort(this ILGenerator il, LocalBuilder local)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(local != null, "@local cannot be null");

			il.LoadLocalAddressShort(local.LocalIndex);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="lcl"></param>
		public static void LoadLocalAddressShort(this ILGenerator il, int lcl)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldloca_S, lcl);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void LoadLocal_0(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldloc_0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void LoadLocal_1(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldloc_1);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void LoadLocal_2(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldloc_2);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void LoadLocal_3(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldloc_3);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void LoadNull(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldnull);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void LoadObjectRef(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldind_Ref);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="p"></param>
		/// <param name="nonPublic"></param>
		public static void LoadProperty(this ILGenerator il, PropertyInfo p, bool nonPublic)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(p != null, "p cannot be null");

			MethodInfo m = p.GetGetMethod(nonPublic);
			if (m == null) throw new InvalidOperationException(
				String.Format("Get method inaccessible for property: {0}", p.Name));
			il.Call(m);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		public static void LoadToken(this ILGenerator il, Type @type)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(@type != null);
			il.Emit(OpCodes.Ldtoken, @type);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		public static void LoadType(this ILGenerator il, Type @type)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(@type != null);
			il.Emit(OpCodes.Ldtoken, @type);
			il.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"), Type.EmptyTypes);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="value"></param>
		public static void LoadValue(this ILGenerator il, bool value)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(((value) ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="value"></param>
		public static void LoadValue(this ILGenerator il, int value)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			switch (value)
			{
				case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
				case 0: il.Emit(OpCodes.Ldc_I4_0); break;
				case 1: il.Emit(OpCodes.Ldc_I4_1); break;
				case 2: il.Emit(OpCodes.Ldc_I4_2); break;
				case 3: il.Emit(OpCodes.Ldc_I4_3); break;
				case 4: il.Emit(OpCodes.Ldc_I4_4); break;
				case 5: il.Emit(OpCodes.Ldc_I4_5); break;
				case 6: il.Emit(OpCodes.Ldc_I4_6); break;
				case 7: il.Emit(OpCodes.Ldc_I4_7); break;
				case 8: il.Emit(OpCodes.Ldc_I4_8); break;
				default:
					il.Emit(OpCodes.Ldc_I4, value);
					break;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="value"></param>
		public static void LoadValue(this ILGenerator il, long value)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldc_I8, value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="value"></param>
		public static void LoadValue(this ILGenerator il, float value)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldc_R4, value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="value"></param>
		public static void LoadValue(this ILGenerator il, double value)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldc_R8, value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="value"></param>
		public static void LoadValue(this ILGenerator il, decimal value)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			int[] v = decimal.GetBits((decimal)value);
			LocalBuilder lcl = il.DeclareLocal(typeof(int[]));
			il.NewArr(typeof(int), 3);
			il.StoreElement(lcl, 0, v[0]);
			il.StoreElement(lcl, 1, v[1]);
			il.StoreElement(lcl, 2, v[2]);
			il.LoadLocal(lcl.LocalIndex);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="value"></param>
		public static void LoadValue(this ILGenerator il, IValueRef value)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(value != null);

			value.LoadValue(il);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="value"></param>
		public static void LoadValue(this ILGenerator il, string value)
		{
			il.Emit(OpCodes.Ldstr, value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="value"></param>
		public static void LoadValue(this ILGenerator il, FieldInfo value)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(value != null);

			il.Emit(OpCodes.Ldfld, value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="value"></param>
		public static void LoadValue(this ILGenerator il, object value)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			if (value == null) il.Emit(OpCodes.Ldnull);
			else
			{
				switch (Type.GetTypeCode(value.GetType()))
				{
					case TypeCode.Boolean:
						il.LoadValue((bool)value);
						break;
					case TypeCode.Byte:
					case TypeCode.Char:
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.SByte:
					case TypeCode.UInt16:
					case TypeCode.UInt32:
						il.LoadValue(Convert.ToInt32(value));
						break;
					case TypeCode.DateTime:
						il.LoadValue((long)((DateTime)value).Ticks);
						il.NewObj(typeof(DateTime).GetConstructor(new Type[] { typeof(long) }));
						break;
					case TypeCode.Decimal:
						il.LoadValue(Convert.ToDecimal(value));
						break;
					case TypeCode.Double:
						il.LoadValue(Convert.ToDouble(value));
						break;
					case TypeCode.Empty:
						il.LoadNull();
						break;
					case TypeCode.Int64:
					case TypeCode.UInt64:
						il.LoadValue(Convert.ToInt64(value));
						break;
					case TypeCode.Single:
						il.LoadValue(Convert.ToSingle(value));
						break;
					case TypeCode.String:
						il.LoadValue(Convert.ToString(value));
						break;
					case TypeCode.Object:
					default:
						throw new InvalidOperationException();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="a"></param>
		public static void Load_I4(this ILGenerator il, int a)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			switch (a)
			{
				case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
				case 0: il.Emit(OpCodes.Ldc_I4_0); break;
				case 1: il.Emit(OpCodes.Ldc_I4_1); break;
				case 2: il.Emit(OpCodes.Ldc_I4_2); break;
				case 3: il.Emit(OpCodes.Ldc_I4_3); break;
				case 4: il.Emit(OpCodes.Ldc_I4_4); break;
				case 5: il.Emit(OpCodes.Ldc_I4_5); break;
				case 6: il.Emit(OpCodes.Ldc_I4_6); break;
				case 7: il.Emit(OpCodes.Ldc_I4_7); break;
				case 8: il.Emit(OpCodes.Ldc_I4_8); break;
				default: il.Emit(OpCodes.Ldc_I4, a); break;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Load_I4_0(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldc_I4_0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Load_I4_1(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldc_I4_1);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Load_I4_2(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldc_I4_2);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Load_I4_3(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldc_I4_3);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Load_I4_4(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldc_I4_4);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Load_I4_5(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldc_I4_5);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Load_I4_6(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldc_I4_6);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Load_I4_7(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldc_I4_7);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Load_I4_8(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldc_I4_8);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Load_I4_M1(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldc_I4_M1);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="a"></param>
		public static void Load_I4_ShortForm(this ILGenerator il, short a)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ldc_I4_S, a);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="lbl"></param>
		public static void MarkLabel(this ILGenerator il, Label lbl)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.MarkLabel(lbl);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Multiply(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Mul);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Xor(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Xor);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="il"></param>
		/// <param name="bindingAttr"></param>
		/// <param name="types"></param>
		public static void New<T>(this ILGenerator il, BindingFlags bindingAttr, params Type[] types)
		{
			Contract.Requires<ArgumentNullException>(il != null);

			var ctor = typeof(T).GetConstructor(bindingAttr, null, types, null);
			Contract.Assert(ctor != null, "constructor lookup failed");

			il.Emit(OpCodes.Newobj, ctor);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="il"></param>
		/// <param name="types"></param>
		public static void New<T>(this ILGenerator il, params Type[] types)
		{
			Contract.Requires<ArgumentNullException>(il != null);

			var ctor = typeof(T).GetConstructor(types);
			Contract.Assert(ctor != null, "constructor lookup failed");

			il.Emit(OpCodes.Newobj, ctor);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="ctor"></param>
		public static void NewObj(this ILGenerator il, ConstructorInfo ctor)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(ctor != null, "ctor cannot be null");
			il.Emit(OpCodes.Newobj, ctor);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Nop(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Nop);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Pop(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Pop);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Return(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Ret);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="local"></param>
		/// <param name="index"></param>
		/// <param name="value"></param>
		public static void StoreElement(this ILGenerator il, LocalBuilder local, int index, int value)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(local != null, "local cannot be null");

			il.LoadLocal(local.LocalIndex);
			il.LoadValue(index);
			il.LoadValue(value);
			il.Emit(OpCodes.Stelem_I4);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void StoreElement(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Stelem);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void StoreElementRef(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Stelem_Ref);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="field"></param>
		public static void StoreField(this ILGenerator il, FieldInfo field)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(field != null);
			if (field.IsStatic)
			{
				il.Emit(OpCodes.Stsfld, field);
			}
			else
			{
				il.Emit(OpCodes.Stfld, field);
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="field"></param>
		public static void StoreField(this ILGenerator il, IFieldRef field)
		{
			Contract.Requires<ArgumentNullException>(field != null);

			StoreField(il, field.FieldInfo);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="local"></param>
		public static void StoreLocal(this ILGenerator il, LocalBuilder local)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(local != null, "local cannot be null");

			il.StoreLocal(local.LocalIndex);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="localIndex"></param>
		public static void StoreLocal(this ILGenerator il, int localIndex)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			switch (localIndex)
			{
				case 0: il.Emit(OpCodes.Stloc_0); break;
				case 1: il.Emit(OpCodes.Stloc_1); break;
				case 2: il.Emit(OpCodes.Stloc_2); break;
				case 3: il.Emit(OpCodes.Stloc_3); break;
				default: il.Emit(OpCodes.Stloc, localIndex); break;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="lcl"></param>
		public static void StoreLocalShortForm(this ILGenerator il, int lcl)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Stloc_S, lcl);
		}
		/// <summary>
		/// Emits IL to store a parameter's value.
		/// </summary>
		/// <param name="il"></param>
		/// <param name="index">the parameter's index</param>
		public static void StoreArg(this ILGenerator il, int index)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Starg, index);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void StoreLocal_0(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Stloc_0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void StoreLocal_1(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Stloc_1);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void StoreLocal_2(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Stloc_2);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void StoreLocal_3(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Stloc_3);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="p"></param>
		/// <param name="nonPublic"></param>
		public static void StoreProperty(this ILGenerator il, PropertyInfo p, bool nonPublic)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(p != null, "p cannot be null");

			MethodInfo m = p.GetSetMethod(nonPublic);
			if (m == null) throw new InvalidOperationException(String.Concat("Set method inaccessible for property: ", p.Name));
			il.Call(m);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="value"></param>
		public static void StoreValue(this ILGenerator il, IValueRef value)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(value != null);

			value.StoreValue(il);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Subtract(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Sub);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="cases"></param>
		public static void Switch(this ILGenerator il, Label[] cases)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Switch, cases);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void Throw(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.Emit(OpCodes.Throw);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="exception"></param>
		public static void Throw(this ILGenerator il, Type exception)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(exception != null);
			Contract.Requires<ArgumentNullException>(typeof(Exception).IsAssignableFrom(exception));
			il.ThrowException(exception);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		public static void UnboxAny(this ILGenerator il, Type @type)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(@type != null);

			il.Emit(OpCodes.Unbox_Any, @type);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void BeginExceptionBlock(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.BeginExceptionBlock();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void BeginFinallyBlock(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.BeginFinallyBlock();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		public static void EndExceptionBlock(this ILGenerator il)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			il.EndExceptionBlock();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		/// <param name="elmCount"></param>
		public static void NewArr(this ILGenerator il, Type @type, int elmCount)
		{
			Contract.Requires<ArgumentNullException>(il != null);
			Contract.Requires<ArgumentNullException>(@type != null);

			il.LoadValue(elmCount);
			il.Emit(OpCodes.Newarr, @type);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="exceptionType"></param>
		public static void BeginCatchBlock(this ILGenerator il, Type exceptionType)
		{
			il.BeginCatchBlock(exceptionType);
		}
	}
}