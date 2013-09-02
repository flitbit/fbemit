#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Reflection.Emit;

namespace FlitBit.Emit
{
	/// <summary>
	///   Helper class for working with events in the IL stream.
	/// </summary>
	public class EmittedEvent : EmittedMember
	{
		private EventBuilder _builder;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="type">the event's type</param>
		/// <param name="name">the event's name</param>
		/// <param name="eventType">the event's type</param>
		/// <param name="isStatic">whether the property is a static property</param>
		public EmittedEvent(EmittedClass type, string name, Type eventType, bool isStatic)
			: base(type, name)
		{
			Contract.Requires<ArgumentNullException>(eventType != null);

			EventType = TypeRef.FromType(eventType);
			IsStatic = isStatic;
		}

		/// <summary>
		///   Gets the event's attributes.
		/// </summary>
		public EventAttributes Attributes { get; protected set; }

		/// <summary>
		///   Gets the event's builder.
		/// </summary>
		public EventBuilder Builder
		{
			get
			{
				if (_builder == null)
				{
					_builder = TargetClass.Builder.DefineEvent(Name
						, Attributes
						, EventType.Target);
				}
				return _builder;
			}
		}

		/// <summary>
		///   Gets the event's type.
		/// </summary>
		public TypeRef EventType { get; private set; }

		/// <summary>
		///   Compiles the property.
		/// </summary>
		protected internal override void OnCompile()
		{
			base.OnCompile();
		}
	}
}