#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace FlitBit.Emit
{
	/// <summary>
	///   Helper class for working with parameters in the IL stream. These helpers make it easier
	///   to deal with method parameters and parameter info, including generics.
	/// </summary>
	public static class ParameterHelper
	{
		/// <summary>
		///   Extracts a parameter list from a method.
		/// </summary>
		/// <param name="method">the method</param>
		/// <param name="parameters">additional parameter info</param>
		/// <returns>a parameter list</returns>
		public static Type[] GetParameterTypes(MethodInfo method, ParameterInfo[] parameters)
		{
			Contract.Requires<ArgumentNullException>(method != null);
			Contract.Requires<ArgumentNullException>(parameters != null);

			Type[] result = Type.EmptyTypes;
			if (parameters.Length > 0)
			{
				result = CreateParametersList(parameters);
			}
			return result;
		}

		/// <summary>
		///   Gets the parameter count.
		/// </summary>
		/// <param name="method">The method info.</param>
		public static int ParameterCount(MethodInfo method)
		{
			Contract.Requires<ArgumentNullException>(method != null);
			return method.GetParameters().Length;
		}

		/// <summary>
		///   Set up parameter constraints.
		/// </summary>
		/// <param name="parameterTypes"></param>
		/// <param name="genericTypeParameterBuilders"></param>
		public static void SetUpParameterConstraints(Type[] parameterTypes,
			GenericTypeParameterBuilder[] genericTypeParameterBuilders)
		{
			Contract.Requires<ArgumentNullException>(parameterTypes != null);
			foreach (Type parameter in parameterTypes)
			{
				if (parameter.IsGenericParameter)
				{
					AddGenericParameterConstraints(genericTypeParameterBuilders, parameter);
				}
			}
		}

		/// <summary>
		///   Defines parameters on the given method builder according to the parameter types and info given.
		/// </summary>
		/// <param name="parameterTypes">array of parameter types</param>
		/// <param name="parameters">additional array of parameter info</param>
		/// <param name="method">the method builder on which to define the parameters</param>
		/// <returns></returns>
		public static ParameterBuilder[] SetUpParameters(Type[] parameterTypes, ParameterInfo[] parameters,
			MethodBuilder method)
		{
			Contract.Requires<ArgumentNullException>(method != null);
			Contract.Requires<ArgumentNullException>(parameters != null);
			if (parameters.Length > 0)
			{
				method.SetParameters(parameterTypes);
				return DefineParameters(parameters, method);
			}
			return new ParameterBuilder[0];
		}

		private static void AddGenericParameterConstraints(
			IEnumerable<GenericTypeParameterBuilder> genericTypeParameterBuilders,
			Type parameter)
		{
			Contract.Requires<ArgumentNullException>(parameter != null);
			Contract.Requires<ArgumentException>(parameter.IsGenericParameter);
			Contract.Requires<ArgumentNullException>(genericTypeParameterBuilders != null);

			Type[] constraints = parameter.GetGenericParameterConstraints();
			GenericTypeParameterBuilder genericTypeParameterBuilder =
				genericTypeParameterBuilders.First(x => x.Name == parameter.Name);

			genericTypeParameterBuilder.SetGenericParameterAttributes(parameter.GenericParameterAttributes);

			var interfaceConstraints = new List<Type>();
			foreach (Type constraint in constraints)
			{
				if (constraint.IsInterface)
				{
					interfaceConstraints.Add(constraint);
				}
				if (constraint.IsClass)
				{
					genericTypeParameterBuilder.SetBaseTypeConstraint(constraint);
				}
			}
			genericTypeParameterBuilder.SetInterfaceConstraints(interfaceConstraints.ToArray());
		}

		private static Type[] CreateParametersList(IEnumerable<ParameterInfo> parameters)
		{
			Contract.Requires<ArgumentNullException>(parameters != null);
			return (from p in parameters
				select p.ParameterType).ToArray();
		}

		private static ParameterBuilder[] DefineParameters(IEnumerable<ParameterInfo> parameters, MethodBuilder method)
		{
			Contract.Requires<ArgumentNullException>(parameters != null);
			Contract.Requires<ArgumentNullException>(method != null);

			int i = (method.IsStatic) ? 0 : 1;
			return
				(from p in parameters let attributes = p.Attributes select method.DefineParameter(i++, p.Attributes, p.Name))
					.ToArray();
		}
	}
}