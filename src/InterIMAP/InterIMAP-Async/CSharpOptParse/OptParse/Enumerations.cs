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
using System.Reflection;

namespace CommandLine.OptParse
{
	#region UnknownOptHandleType enum
	/// <summary>
	/// Define the behavior of the parser if an option is given that is not reconginzed
	/// </summary>
	public enum UnknownOptHandleType
	{
		/// <summary>
		/// Do not do anything, the option will be considered an argument
		/// </summary>
		NoAction,

		/// <summary>
		/// A warning will be sent back to the caller of the parser (see 
		/// the <see cref="Parser.OptionWarning"/> event)
		/// </summary>
		Warning,
		
		/// <summary>
		/// Stop parsing, throwing a <see cref="ParseException"/>
		/// </summary>
		Error
	}
	#endregion UnknownOptHandleType enum

	#region DupOptHandleType enum
	/// <summary>
	/// Define the behavior of the parser if an option is duplicated
	/// </summary>
	/// <remarks>
	/// If the option is not defined as a option that allows multiple values, then
	/// this enumeration specifies the action to take when a duplicate option is given
	/// at the command-line.
	/// </remarks>
	public enum DupOptHandleType
	{
		/// <summary>
		/// Allow the duplicate declaration, using the last declaration to specify
		/// the value
		/// </summary>
		Allow,

		/// <summary>
		/// Allow the duplicate declaration, using the last declaration to specify
		/// the value, but send a warning back to the caller of the parser (see 
		/// the <see cref="Parser.OptionWarning"/> event)
		/// </summary>
		Warning,
		
		/// <summary>
		/// Deny the duplicate declaration, throwing a <see cref="ParseException"/>
		/// </summary>
		Error
	}
	#endregion DupOptHandleType enum

	#region OptStyle enum
	/// <summary>
	/// The platform style of option
	/// </summary>
	public enum OptStyle
	{
		/// <summary>
		/// Windows style of options (/opt)
		/// </summary>
		/// <remarks>
		/// Use options in the standard Windows format.
		/// Example:
		/// <code>
		/// program.exe /opt1:"Option 1 Value" /opt2:Opt2Value /opt3
		/// </code>
		/// <para>
		/// Windows arguments do not differentiate between short an long formats like Unix
		/// arguments.
		/// </para>
		/// </remarks>
		Windows,

		/// <summary>
		/// Unix style of options (--opt, -o)
		/// </summary>
		/// <remarks>
		/// Use options in the standard Unix format.
		/// Example:
		/// <code>
		/// program --opt1="Option 1 Value" --opt2 Opt2Value -o
		/// </code>
		/// <para>
		/// Long argument values may be separated from the option by a space or by an
		/// equal sign. Short option values are separated from the option by a space
		/// </para>
		/// </remarks>
		Unix
	}
	#endregion OptStyle enum

	#region UnixShortOption enum
	/// <summary>
	/// Specify how short options may be given for Unix-style arguments
	/// </summary>
	public enum UnixShortOption
	{
		/// <summary>
		/// Allow short arguments to be collapsed.
		/// </summary>
		/// <remarks>
		/// Example:
		/// <code>
		/// Program -abcd 0 "This is d's value"
		/// </code>
		/// <para>
		/// If the options require a value (See <see cref="OptValType"/> enum), then
		/// the the values after the arguments will be applied in order. Optional
		/// arguments that precede required arguments become required, meaning that
		/// if 3 values are given for 4 values (i.e. <c>program -abcd val1 val2 val3</c>), 
		/// and the first two arguments have optional values, and the 3rd is required, and the fourth
		/// takes an optional value, then per the example, the value of 'val1' will be
		/// assigned to 'a', 'val2' to 'b', and 'val3' to 'c' with 'd' not having a value.
		/// </para>
		/// </remarks>
		CollapseShort,

		/// <summary>
		/// Short options must be separated, and values do not need a space after the name
		/// </summary>
		/// <remarks>
		/// Example: <c>program -a -b -c0 -d"This is d's value"</c>
		/// </remarks>
		ShortSeparated
	}
	#endregion UnixShortOption enum

	#region OptValType enum
	/// <summary>
	/// Type of value that the option permits
	/// </summary>
	public enum OptValType
	{
		/// <summary>
		/// The option does not accept a value
		/// </summary>
		[Description("Flag")]
		Flag,

		/// <summary>
		/// The option does not accept a value, but may be declared multiple times
		/// </summary>
		/// <remarks>
		/// This type of argument supports multiple declaration. For example,
		/// <c>--verbose --verbose --verbose</c>, could be used to allow a 3rd level
		/// of verbose information to be printed by a program.
		/// </remarks>
		[Description("Incremental flag")]
		IncrementalFlag,

		/// <summary>
		/// The option requires a value. If a value is not supplied, an error will be thrown
		/// </summary>
		[Description("Value required")]
		ValueReq,

		/// <summary>
		/// The option optional takes a value
		/// </summary>
		[Description("Value optional")]
		ValueOpt,


		/// <summary>
		/// The option allows 0 to many values
		/// </summary>
		[Description("0 to many values accepted")]
		MultValue
	}
	#endregion OptValType enum
		
	#region EnumDescriptorReader class
	/// <summary>
	/// Class that assists with reading <see cref="DescriptionAttribute"/> values for enumeration values
	/// </summary>
	public sealed class EnumDescriptorReader 
	{
		#region Constructor
		private EnumDescriptorReader() {}
		#endregion Constructor

		#region Methods
		/// <summary>
		/// Get the field description for the enumeration value
		/// </summary>
		/// <example>
		/// Enumeration:
		/// <code>
		/// public enum ExampleEnum
		/// {
		///	[EnumDescription("This is the first value")]
		///	FirstValue = 1,
		///	[EnumDescription("This is the second value")]
		///	SecondValue = 2
		/// }
		/// </code>
		/// Usage:
		/// <code>
		/// string desc = EnumDescriptorReader.GetEnumFieldDescription(ExampleEnum.FirstValue);
		/// </code>
		/// </example>
		/// <param name="val">The enumeration value to get the description of</param>
		/// <returns>The description, or if it has none, the name</returns>
		public static string GetEnumFieldDescription(Enum val)
		{
			FieldInfo[]          enumFields = val.GetType().GetFields();
			DescriptionAttribute enumFieldDescriptor;
			FieldInfo            field;
			string               itemName;

			itemName = Enum.GetName(val.GetType(), val);

			for (int i = 0; i < enumFields.Length - 1; i++) 
			{
				field = enumFields[i + 1];
				if (field.Name.Equals(itemName))
				{
					enumFieldDescriptor = Attribute.GetCustomAttribute(field, 
						typeof(DescriptionAttribute)) as DescriptionAttribute;

					if (enumFieldDescriptor != null) 
						return enumFieldDescriptor.Description;
					else if (field.Name.Equals(itemName)) 
						return field.Name;
				}
			}

			return "";
		}

		
		/// <summary>
		/// Gets all of the descriptions for the values in the given enumeration type.
		/// </summary>
		/// <remarks>
		/// If a description attribute is not found, the enumeration field name will
		/// be returned as the description instead
		/// </remarks>
		/// <param name="enumType">The enumeration type</param>
		/// <returns>An array of the descriptions.</returns>
		public static string[] GetEnumFieldDescriptions(Type enumType) 
		{
			DescriptionAttribute enumFieldDescriptor;
			FieldInfo            field;
			FieldInfo[]          enumFields = enumType.GetFields();
			string[]             enumDescriptions = new string[enumFields.Length - 1];

			for (int i = 0; i < enumFields.Length - 1; i++) 
			{
				field = enumFields[i + 1];

				enumFieldDescriptor = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
				if (enumFieldDescriptor != null) 
					enumDescriptions[i] = enumFieldDescriptor.Description;
				else 
					enumDescriptions[i] = field.Name;
			}

			return enumDescriptions;
		}
		#endregion Methods
	}
	#endregion EnumDescriptorReader class
}
