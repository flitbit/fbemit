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
	/// Base helper class for working with methods in the IL stream.
	/// </summary>
	public abstract class EmittedMethodBase : EmittedMember
	{
		List<Action<EmittedMethodBase, ILGenerator>> _gen;
		ILGenerator _generatorDuringCompile;
		List<EmittedLocal> _locals = new List<EmittedLocal>();
		List<EmittedParameter> _parameters = new List<EmittedParameter>();
		MethodAttributes _attributes;
		CallingConventions _callingConvention;

		/// <summary>
		/// Indicates whether the method has a builder.
		/// </summary>
		protected abstract bool HasBuilder { get; }

		/// <summary>
		/// Creates a new instance
		/// </summary>
		/// <param name="type">the emitted type, owner</param>
		/// <param name="name">the method's name</param>
		public EmittedMethodBase(EmittedClass type, string name)
			: base(type, name)
		{
		}

		/// <summary>
		/// Gets the method's attributes.
		/// </summary>
		public MethodAttributes Attributes 
		{
			get { return _attributes; }
			private set
			{
				Contract.Assert(!HasBuilder, "Attributes must be set before the Builder is created or accessed");

				_attributes = value;
				this.IsStatic = value.HasFlag(MethodAttributes.Static);
			}
		}
		/// <summary>
		/// Gets the method's calling convetions.
		/// </summary>
		public CallingConventions CallingConvention
		{
			get { return _callingConvention; }
			set
			{
				Contract.Assert(!HasBuilder, "CallingConvention must be set before the Builder is created or accessed");
				_callingConvention = value;
			}
		}
		/// <summary>
		/// Gets the method's parameter types.
		/// </summary>
		public virtual Type[] ParameterTypes
		{
			get
			{
				return (from prop in _parameters
								select prop.ParameterType.Target).ToArray<Type>();
			}
		}

		/// <summary>
		/// Clears the method's attributes.
		/// </summary>
		public void ClearAttributes()
		{
			Contract.Assert(!HasBuilder, "Attributes must be set before the Builder is created or accessed");
			Attributes = (MethodAttributes)0;
		}

		/// <summary>
		/// Defines a local variable.
		/// </summary>
		/// <param name="name">the local's name</param>
		/// <param name="type">the local's type</param>
		/// <returns>the emitted local</returns>
		public EmittedLocal DefineLocal(string name, Type type)
		{
			return DefineLocal(name, new TypeRef(type));
		}

		/// <summary>
		/// Defines a local variable.
		/// </summary>
		/// <param name="name">the local's name</param>
		/// <param name="type">the local's type (ref)</param>
		/// <returns>the emitted local</returns>
		public EmittedLocal DefineLocal(string name, TypeRef type)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			if ((from l in _locals
					 where String.Equals(name, l.Name)
					 select l).SingleOrDefault() != null)
				throw new InvalidOperationException(String.Format(
					@"Method already contains a local by the same name: name = {0} unfinished method = {1}", name, this.UnfinishedSignature()));

			EmittedLocal result;
			_locals.Add(result = new EmittedLocal(name, _locals.Count, type));
			if (_generatorDuringCompile != null)
			{
				result.Compile(_generatorDuringCompile);
			}
			return result;
		}

		/// <summary>
		/// Contributes instructions for the method.
		/// </summary>
		/// <param name="gen">an action that provides instructions for the method</param>
		/// <remarks>
		/// The <paramref name="gen"/> actions provided to the method will be called in the
		/// order they are provided. It is the caller's responsibility that the generators
		/// are registered in the proper order and that dependencies are satisfield before
		/// each generator is called.
		/// </remarks>
		public void ContributeInstructions(Action<EmittedMethodBase, ILGenerator> gen)
		{
			if (_gen == null) _gen = new List<Action<EmittedMethodBase, ILGenerator>>();
			_gen.Add(gen);
		}

		/// <summary>
		/// Defines a parameter.
		/// </summary>
		/// <param name="name">the parameter's name</param>
		/// <param name="typeRef">the parameter's type (ref)</param>
		/// <returns>the emitted parameter</returns>
		public EmittedParameter DefineParameter(string name, TypeRef typeRef)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);

			if ((from p in _parameters
					 where String.Equals(name, p.Name)
					 select p).SingleOrDefault() != null)
				throw new InvalidOperationException(String.Format(
			@"Method already contains a parameter by the same name:
			name = {0}
			unfinished method = {1}", name, this.UnfinishedSignature()));

			EmittedParameter result = new EmittedParameter(this, _parameters.Count, name, typeRef);
			_parameters.Add(result);
			return result;
		}

		/// <summary>
		/// Defines a parameter.
		/// </summary>
		/// <param name="name">the parameter's name</param>
		/// <param name="type">the parameter's type</param>
		/// <returns>the emitted parameter</returns>
		public EmittedParameter DefineParameter(string name, Type type)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentNullException>(name.Length > 0);
			Contract.Requires<ArgumentNullException>(type != null);

			return DefineParameter(name, new TypeRef(type));
		}

		/// <summary>
		/// Adds a parameter introduced by subclasses.
		/// </summary>
		/// <param name="parameter">the parameter</param>
		protected void AddParameter(EmittedParameter parameter)
		{
			_parameters.Add(parameter);
		}

		/// <summary>
		/// Gets the method's defined parameters.
		/// </summary>
		public IEnumerable<EmittedParameter> Parameters { get { return _parameters.ToArray(); } }

		/// <summary>
		/// Emits instructions for calling the method.
		/// </summary>
		/// <param name="il">IL</param>
		public abstract void EmitCall(ILGenerator il);

		/// <summary>
		/// Excludes the attributes given.
		/// </summary>
		/// <param name="attr">attributes to be excluded</param>
		public void ExcludeAttributes(MethodAttributes attr)
		{
			Contract.Assert(!HasBuilder, "Attributes must be set before the Builder is created or accessed");
			Attributes &= (~attr);
			if (Attributes.HasFlag(MethodAttributes.Static))
			{
				this.CallingConvention &= (~CallingConventions.HasThis);
			}
			else this.CallingConvention |= CallingConventions.HasThis;
		}

		/// <summary>
		/// Includes the attributes given.
		/// </summary>
		/// <param name="attr">the attributes to be included</param>
		public void IncludeAttributes(MethodAttributes attr)
		{
			Contract.Assert(!HasBuilder, "Attributes must be set before the Builder is created or accessed");
			Attributes |= attr;
			if (Attributes.HasFlag(MethodAttributes.Static))
			{
				this.CallingConvention &= (~CallingConventions.HasThis);
			}
			else this.CallingConvention |= CallingConventions.HasThis;
		}

		/// <summary>
		/// Compiles the method's locals.
		/// </summary>
		/// <param name="il">IL</param>
		protected void CompileLocals(ILGenerator il)
		{
			foreach (EmittedLocal l in _locals)
			{
				l.Compile(il);
			}
		}

		/// <summary>
		/// Compiles the method's parmeters.
		/// </summary>
		/// <param name="m">the method</param>
		protected void CompileParameters(MethodBuilder m)
		{
			foreach (EmittedParameter p in _parameters)
			{
				p.Compile(m);
			}
		}

		/// <summary>
		/// Compiles the construtor's parameters.
		/// </summary>
		/// <param name="c">the construtor builder</param>
		protected void CompileParameters(ConstructorBuilder c)
		{
			foreach (EmittedParameter p in _parameters)
			{
				p.Compile(c);
			}
		}

		/// <summary>
		/// Compiles the method's contributed instructions.
		/// </summary>
		/// <param name="il">IL</param>
		protected virtual void EmitInstructions(ILGenerator il)
		{
			if (_gen != null)
			{
				foreach (Action<EmittedMethodBase, ILGenerator> gen in _gen)
				{
					gen(this, il);
				}
			}
		}

		/// <summary>
		/// Sets the IL generator used when emitting. If subclasses utilize the 
		/// compilation helper methods, the ILGenerator must be set prior to
		/// calling these helpers.
		/// </summary>
		/// <param name="il">IL</param>
		/// <returns>the given IL</returns>
		protected ILGenerator SetILGenerator(ILGenerator il)
		{
			return _generatorDuringCompile = il;
		}

		private string UnfinishedSignature()
		{
			// TODO: Output the parameters as they are defined at the time of the call.
			return String.Concat(this.TargetClass.Builder.FullName, '.', this.Name, "(...)");
		}    
	}
}
