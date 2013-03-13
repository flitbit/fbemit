#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FlitBit.Emit.Properties;

namespace FlitBit.Emit
{
	/// <summary>
	///   Helper class for working with generic arguments in the IL stream.
	/// </summary>
	public class EmittedGenericArgument
	{
		readonly List<TypeRef> _interfaceConstraints = new List<TypeRef>();
		TypeRef _baseTypeConstraint;

		/// <summary>
		///   Gets the argument's attributes.
		/// </summary>
		public GenericParameterAttributes Attributes { get; set; }

		/// <summary>
		///   Gets the argument's name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///   Gets the argument's position
		/// </summary>
		public int Position { get; set; }

		/// <summary>
		///   Adds a type constraint to the generic argument.
		/// </summary>
		/// <param name="type">the constraining type</param>
		public void AddBaseTypeConstraint(Type @type)
		{
			Contract.Requires<ArgumentNullException>(@type != null);
			if (_baseTypeConstraint == null)
			{
				throw new InvalidOperationException(Resources.Err_BaseTypeConstraintNotSet);
			}

			_baseTypeConstraint = new TypeRef(@type);
		}

		/// <summary>
		///   Adds a type constraint to the generic argument.
		/// </summary>
		/// <param name="typeRef">a reference to the constraining type</param>
		public void AddBaseTypeConstraint(TypeRef typeRef)
		{
			Contract.Requires<ArgumentNullException>(typeRef != null);
			if (_baseTypeConstraint == null)
			{
				throw new InvalidOperationException(Resources.Err_BaseTypeConstraintNotSet);
			}

			_baseTypeConstraint = typeRef;
		}

		/// <summary>
		///   Adds an interface constraint to the generic argument.
		/// </summary>
		/// <param name="type">the constraining type</param>
		public void AddInterfaceConstraint(Type @type)
		{
			Contract.Requires<ArgumentNullException>(@type != null);

			_interfaceConstraints.Add(new TypeRef(@type));
		}

		/// <summary>
		///   Adds an interface constraint to the generic argument.
		/// </summary>
		/// <param name="typeref">a reference to the constraining type</param>
		public void AddInterfaceConstraint(TypeRef typeref)
		{
			Contract.Requires<ArgumentNullException>(typeref != null);
			if (_baseTypeConstraint == null)
			{
				throw new InvalidOperationException(Resources.Err_BaseTypeConstraintNotSet);
			}

			_interfaceConstraints.Add(typeref);
		}

		internal void FinishDefinition(GenericTypeParameterBuilder arg)
		{
			Contract.Requires<ArgumentNullException>(arg != null);

			arg.SetGenericParameterAttributes(Attributes);
			if (_baseTypeConstraint != null)
			{
				arg.SetBaseTypeConstraint(_baseTypeConstraint.Target);
			}
			if (_interfaceConstraints.Count > 0)
			{
				arg.SetInterfaceConstraints((from i in _interfaceConstraints
																		select i.Target).ToArray());
			}
		}
	}
}