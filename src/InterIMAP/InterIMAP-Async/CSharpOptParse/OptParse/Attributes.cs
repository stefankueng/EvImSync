/* This file is part of the CSharpOptParse .NET C# library
 *
 * The library is hosted at http://csharpoptparse.sf.net
 *
 * Copyright (C) 2005 by Andrew Robinson
 *
 * This source code is open source, protected under the GNU GPL Version 2, June 1991
 * Please see http://opensource.org/licenses/gpl-license.php for information and
 * specifics on this license.
 */
using System;
using System.ComponentModel;

namespace CommandLine.OptParse
{
	#region OptDefAttribute
	/// <summary>
	/// Define that a property or field can be given as an option
	/// <seealso cref="Parser"/>
	/// </summary>
	/// <remarks>
	/// For <see cref="OptValType.Flag"/> the property or field must have be type of bool 
	/// <para>
	/// For <see cref="OptValType.IncrementalFlag"/> the property or field must be a integer
	/// </para>
	/// <para>
	/// For <see cref="OptValType.ValueReq"/> or <see cref="OptValType.ValueOpt"/> the
	/// property or field must be the type of the <see cref="ValueType"/> property.
	/// </para>
	/// <para>
	/// For <see cref="OptValType.MultValue"/> the property or field must be 
	/// an <see cref="System.Collections.IList"/> of values of the type defined by the
	/// <see cref="ValueType"/> property (IList cannot be null).
	/// </para>
	/// </remarks>
	/// <include file="ExternalDocs.xml" 
	///		path="externalDocs/doc[@name='ExamplePropClass']/*"/>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
		 AllowMultiple=false, Inherited=true)]
	public class OptDefAttribute : Attribute
	{
		#region Members
		private OptValType _optionValueType;
		private Type       _valueType = null;
		#endregion Members

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="optionValueType">The type of value the option takes or null to use
		/// the type of the property/field</param>
		public OptDefAttribute(OptValType optionValueType)
		{
			_optionValueType = optionValueType;
		}
		#endregion Constructor

		#region Properties
		/// <summary>
		/// Get or set the type of value to support
		/// </summary>
		/// <remarks>
		/// For most properties and fields, this is optional (can be left null), but for
		/// IList types of properties that take multiple values, this property needs to be
		/// set to know how to convert the command-line values to the type the list expects
		/// </remarks>
		public Type ValueType 
		{
			get { return _valueType; }
			set { _valueType = value; }
		}


		/// <summary>
		/// Get the type of value the option takes
		/// </summary>
		public OptValType OptionValueType 
		{
			get { return _optionValueType; }
		}
		#endregion Properties
	}
	#endregion OptDefAttribute

	#region UseNameAsLongOptionAttribute
	/// <summary>
	/// Gives the ability to stop the name of a field or property being used
	/// to be used as an option name
	/// </summary>
	/// <remarks>
	/// By default, the name of a property or field is used as a option name
	/// when defined as an option (see <see cref="OptDefAttribute"/>). This
	/// attribute allows only names to be given using <see cref="LongOptionNameAttribute"/>
	/// and <see cref="ShortOptionNameAttribute"/> attributes. If this attribute value
	/// is false, one of the above attributes must be given for the property
	/// </remarks>
	/// <include file="ExternalDocs.xml" 
	///		path="externalDocs/doc[@name='ExamplePropClass']/*"/>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
		 AllowMultiple=false, Inherited=true)]
	public class UseNameAsLongOptionAttribute : Attribute
	{
		#region Members
		private bool _useName;
		#endregion Members

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="useName">Set to false to not use this field or property as
		/// a possible option name</param>
		public UseNameAsLongOptionAttribute(bool useName)
		{
			_useName = useName;
		}
		#endregion Constructor

		#region Properties
		/// <summary>
		/// Get if the name should be used as an option
		/// </summary>
		public bool UseName 
		{
			get { return _useName; }
		}
		#endregion Properties
	}
	#endregion UseNameAsLongOptionAttribute

	#region LongOptionNameAttribute
	/// <summary>
	/// Defines a long option for a field or property
	/// </summary>
	/// <remarks>This is only applicable if the field or property is marked as an option
	/// using <see cref="OptDefAttribute"/></remarks>
	/// <include file="ExternalDocs.xml" 
	///		path="externalDocs/doc[@name='ExamplePropClass']/*"/>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
		 AllowMultiple=true, Inherited=true)]
	public class LongOptionNameAttribute : Attribute
	{
		#region Members
		private string _name;
		#endregion Members

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name of the option</param>
		public LongOptionNameAttribute(string name)
		{
			_name = name;
		}
		#endregion Constructor

		#region Properties
		/// <summary>
		/// Get the name of the option
		/// </summary>
		public string Name 
		{
			get { return _name; }
		}
		#endregion Properties
	}
	#endregion LongOptionNameAttribute

	#region ShortOptionNameAttribute
	/// <summary>
	/// Defines a short option name for a field or property
	/// </summary>
	/// <remarks>This is only applicable if the field or property is marked as an option
	/// using <see cref="OptDefAttribute"/></remarks>
	/// <include file="ExternalDocs.xml" 
	///		path="externalDocs/doc[@name='ExamplePropClass']/*"/>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
		 AllowMultiple=true, Inherited=true)]
	public class ShortOptionNameAttribute : Attribute
	{
		#region Members
		private char _name;
		#endregion Members

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name of the option</param>
		public ShortOptionNameAttribute(char name)
		{
			_name = name;
		}
		#endregion Constructor

		#region Properties
		/// <summary>
		/// Get the name of the option
		/// </summary>
		public char Name 
		{
			get { return _name; }
		}
		#endregion Properties
	}
	#endregion ShortOptionNameAttribute
}
