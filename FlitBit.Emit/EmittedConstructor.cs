﻿#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace FlitBit.Emit
{
	/// <summary>
	///   Helper class for working with constructors in the IL stream.
	/// </summary>
	public class EmittedConstructor : EmittedMethodBase
	{
		ConstructorBuilder _builder;

		/// <summary>
		///   Creates a new instance
		/// </summary>
		/// <param name="type">the emitted type; owner</param>
		/// <param name="name">the constructor's name</param>
		public EmittedConstructor(EmittedClass type, string name)
			: base(type, name)
		{
			this.IncludeAttributes(MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.HideBySig |
				MethodAttributes.Public);
			this.CallingConvention = CallingConventions.Standard;
		}

		/// <summary>
		///   Gets the constructor's builder.
		/// </summary>
		public ConstructorBuilder Builder
		{
			get
			{
				return this._builder ?? (this._builder = this.TargetClass.Builder.DefineConstructor(this.Attributes,
																																														this.CallingConvention,
																																														this.ParameterTypes
					));
			}
		}

		/// <summary>
		///   Indicates whether the constructor has a builder.
		/// </summary>
		protected override bool HasBuilder
		{
			get { return this._builder != null; }
		}

		/// <summary>
		///   Emits a call to the underlying constructor.
		/// </summary>
		/// <param name="il"></param>
		public override void EmitCall(ILGenerator il)
		{
			if (!IsCompiled)
			{
				Compile();
			}
			il.Call(Builder);
		}

		/// <summary>
		///   Compiles the constructor.
		/// </summary>
		protected internal override void OnCompile()
		{
			var cls = TargetClass;
			var il = SetILGenerator(Builder.GetILGenerator());
			try
			{
				CompileParameters(Builder);
				foreach (var f in cls.Fields.Where(f => f.IsStatic == IsStatic))
				{
					f.EmitInit(this, il);
				}
				CompileLocals(il);

				base.EmitInstructions(il);
				il.Return();
			}
			finally
			{
				SetILGenerator(il);
			}
		}
	}
}