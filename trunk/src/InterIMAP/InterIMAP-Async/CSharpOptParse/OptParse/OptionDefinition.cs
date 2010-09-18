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
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace CommandLine.OptParse
{
	/// <summary>
	/// Definition of an option
	/// </summary>
	/// <remarks>
	/// Use this class to define possible options for a command-line. This is an alternative
	/// to using a class with fields and properties defined as options.
	/// </remarks>
	public class OptionDefinition
	{
		#region Members
		private OptValType _type;
		private Type       _valueType;
		private char[]     _shortNames;
		private object     _defaultValue;
		private object     _ID;
		private string     _category;
		private string     _description;
		private string[]   _longNames;
		#endregion Members

		#region Static methods
		/// <summary>
		/// Helper method to create <c>OptionDefinition</c> instances 
		/// for each <c>string</c> in <c>definitions</c>
		/// </summary>
		/// <param name="definitions">Definitions (See <see cref="OptionDefinition(string)"/>)</param>
		/// <returns><c>OptionDefinition</c> instances</returns>
		public static OptionDefinition[] CreateDefinitions(string[] definitions)
		{
			OptionDefinition[] defs = new OptionDefinition[definitions.Length];

			for (int i = 0; i < definitions.Length; i++)
				defs[i] = new OptionDefinition(definitions[i]);

			return defs;
		}
		#endregion Static methods

		#region Constructors
		/// <summary>
		/// Build an option from a Perl-like string syntax
		/// </summary>
		/// <remarks>
		/// The definition must match the following regular expression syntax:
		/// <c>^[\w|]+([:=+][sifd]?)?$</c>.
		/// <para>
		/// Explanation:
		/// <list type="table">
		///		<listheader>
		///			<term>Part</term>
		///			<description>Description</description>
		///		</listheader>
		///		<item>
		///			<term>[\w|]+</term>
		///			<description>A series of one or more word or character names separated
		///			by pipe ('|') characters. 
		///			Example: <c>help|h|?</c>. In this example, for Unix-style options, the valid
		///			option variations would be: <c>--help, -h or -?</c>
		///			</description>
		///		</item>
		///		<item>
		///			<term>[:=+]</term>
		///			<description>A character to define the type of value expected. This is optional,
		///			and if not given, the option will be considered a flag. So a definition of 
		///			<c>help</c> can be given at the command-line like: <c>Program --help</c>.
		///			The ':' and '=' requires the type of value (see below). The '+' can be used
		///			in two ways. First, it can mean that a flag can be given multiple times (to
		///			be counted for example). This usage: <c>verbose+</c> can be given like:
		///			<c>Program --verbose --verbose --verbose</c> to result in a verbosity of 3.
		///			With a type of value (see below), it means that the option can have many values.
		///			A value of ':' means the option may take a value, and '=' means that
		///			a value is required for this option.
		///			</description>
		///		</item>
		///		<item>
		///			<term>[sifd]?</term>
		///			<description>A character to define the type of data to accept. This is 
		///			optional if the preceding character is '+' (see above). If given, the
		///			values map to the following .NET types: s - String, i - Int32, d - Double and 
		///			f - Float</description>
		///		</item>
		/// </list>
		/// </para>
		/// Examples:
		/// <code>
		/// Flag:
		///		Example show help: help|h|?
		///	Flag that gets counted:
		///		Example to set level of verbosity:	verbose|v+
		///	Option with a required string value (Example: set an output directory):
		///		Example to set an output directory: directory|dir|d=s
		///	Option with an optional integer value:
		///		Example to count value that defaults to 0: count|c:i
		///	Option that accepts multiple string values:
		///		Example to specify include patters: include|i+s
		/// </code>
		/// </remarks>
		/// <param name="definition">The option definition</param>
		public OptionDefinition(string definition)
		{
			ArrayList shorts;
			ArrayList longs;
			Match     m;
			string    val;
			string[]  names;
			
			m     = Regex.Match(definition, @"^(?<names>[\w|]+)(?<val>[:=+][sidf]?)?$");
			names = m.Groups["names"].Value.Split('|');
			val   = m.Groups["val"].Value;

			if (names == null || names.Length == 0)
				throw new ArgumentException("Invalid option definition: " + definition);

			if (val != null && val.Length > 1)
			{
				if (val.StartsWith(":"))
					_type = OptValType.ValueOpt;
				else if (val.StartsWith("="))
					_type = OptValType.ValueReq;
				else //if (val.StartsWith("+"))
					_type = OptValType.MultValue;

				switch (val[1])
				{
					case 's': _valueType = typeof(string); break;
					case 'i': _valueType = typeof(int);    break;
					case 'f': _valueType = typeof(float);  break;
					case 'd': _valueType = typeof(double);  break;
				}
			}
			else if (val != null)
			{
				if (val.StartsWith("+"))
				{
					_type = OptValType.IncrementalFlag;
					_valueType = typeof(int);
				}
				else
					throw new ArgumentException("Invalid option definition: " + definition);
			}
			else
			{
				_type = OptValType.Flag;
				_valueType = typeof(bool);
			}

			shorts = new ArrayList();
			longs  = new ArrayList();

			foreach (string name in names)
			{
				if (name.Length == 0)
					throw new ArgumentException("Invalid option definition: " + definition);
				else if (name.Length == 1)
					shorts.Add(name[0]);
				else
					longs.Add(name);
			}

			_shortNames = (char[])shorts.ToArray(typeof(char));
			_longNames  = (string[])longs.ToArray(typeof(string));
		}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ID">ID of the option</param>
		/// <param name="type">Option value type</param>
		/// <param name="description">User-friendly description of the option</param>
		/// <param name="shortName">The short name to map to this option</param>
		/// <param name="valueType">the type of value that the option supports 
		/// (if values are supported, ignored otherwise)</param>
		public OptionDefinition(object ID, OptValType type, Type valueType, string description,
			char shortName) : 
			this (ID, type, valueType, description, new char[] { shortName }, null) {}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ID">ID of the option</param>
		/// <param name="type">Option value type</param>
		/// <param name="description">User-friendly description of the option</param>
		/// <param name="shortNames">The short names to map to this option</param>
		/// <param name="valueType">the type of value that the option supports 
		/// (if values are supported, ignored otherwise)</param>
		public OptionDefinition(object ID, OptValType type, Type valueType, string description,
			char[] shortNames) : this (ID, type, valueType, description, shortNames, null) {}

		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ID">ID of the option</param>
		/// <param name="type">Option value type</param>
		/// <param name="valueType">the type of value that the option supports 
		/// (if values are supported, ignored otherwise)</param>
		/// <param name="description">User-friendly description of the option</param>
		/// <param name="longName">The long name to map to this option</param>
		public OptionDefinition(object ID, OptValType type, Type valueType, string description,
			string longName) : 
			this (ID, type, valueType, description, null, new string[] { longName }) {}
		

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ID">ID of the option</param>
		/// <param name="type">Option value type</param>
		/// <param name="valueType">the type of value that the option supports 
		/// (if values are supported, ignored otherwise)</param>
		/// <param name="description">User-friendly description of the option</param>
		/// <param name="longNames">The long names to map to this option</param>
		public OptionDefinition(object ID, OptValType type, Type valueType, string description,
			string[] longNames) : this (ID, type, valueType, description, null, longNames) {}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ID">ID of the option</param>
		/// <param name="type">Option value type</param>
		/// <param name="valueType">the type of value that the option supports 
		/// (if values are supported, ignored otherwise)</param>
		/// <param name="description">User-friendly description of the option</param>
		/// <param name="longNames">The long names to map to this option</param>
		/// <param name="shortNames">The short names to map to this option</param>
		public OptionDefinition(object ID, OptValType type, Type valueType, string description,
			char[] shortNames, string[] longNames) : 
			this(ID, type, valueType, null, description, shortNames, longNames) {}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ID">ID of the option</param>
		/// <param name="type">Option value type</param>
		/// <param name="valueType">the type of value that the option supports 
		/// (if values are supported, ignored otherwise)</param>
		/// <param name="category">Category to use to group options when printing</param>
		/// <param name="description">User-friendly description of the option</param>
		/// <param name="longNames">The long names to map to this option</param>
		/// <param name="shortNames">The short names to map to this option</param>
		public OptionDefinition(object ID, OptValType type, Type valueType, 
			string category, string description, char[] shortNames, string[] longNames)
		{
			_ID          = ID;
			_category    = category;
			_type        = type;
			_description = description;
			_shortNames  = shortNames;
			_longNames   = longNames;
			_valueType   = valueType;

			// Set the correct type for flags and incremental flags
			switch (type)
			{
				case OptValType.IncrementalFlag: _valueType = typeof(int);  break;
				case OptValType.Flag:            _valueType = typeof(bool); break;
			}

			if ((_shortNames == null || _shortNames.Length == 0) &&
				(_longNames == null || _longNames.Length == 0))
				throw new ArgumentException("Option must have at least one name");
		}
		#endregion Constructors

		#region Properties
		/// <summary>
		/// Default value of the option
		/// </summary>
		public object DefaultValue 
		{
			get { return _defaultValue; }
			set { _defaultValue = value; }
		}
		
		
		/// <summary>
		/// Get or set the category of the option
		/// </summary>
		/// <remarks>
		/// Categories can be used to group the options when the usage is printed
		/// using the <see cref="UsageBuilder"/>
		/// </remarks>
		public string Category 
		{
			get { return _category; }
			set { _category = value; }
		}


		/// <summary>
		/// Get the type of value that the option supports (if values are supported)
		/// </summary>
		public Type ValueType 
		{
			get { return _valueType; }
		}


		/// <summary>
		/// Get the type of value the option takes
		/// </summary>
		public OptValType Type 
		{
			get { return _type; }
		}


		/// <summary>
		/// Get the short names for this option
		/// </summary>
		/// <remarks>
		/// Null if short names are not supported by this option
		/// </remarks>
		public char[] ShortNames 
		{
			get { return _shortNames; }
		}


		/// <summary>
		/// Get the ID for this option
		/// </summary>
		/// <remarks>
		/// Allows an option to be identified if multiple names are used to define this option
		/// </remarks>
		public object ID 
		{
			get { return _ID; }
		}


		/// <summary>
		/// Get the user-friendly description of this option
		/// </summary>
		public string Description 
		{
			get { return _description; }
		}


		/// <summary>
		/// Get the long names for this option
		/// </summary>
		/// <remarks>
		/// Null if long names are not supported by this option
		/// </remarks>
		public string[] LongNames 
		{
			get { return _longNames; }
		}
		#endregion Properties

		#region Methods
		/// <summary>
		/// Try to convert a given value to the value that this option supports
		/// </summary>
		/// <param name="value">The value given</param>
		/// <exception cref="InvalidValueException">Thrown if the value could not 
		/// be converted</exception>
		/// <returns>The converted value or null if the value could not be converted</returns>
		public object ConvertValue(string value)
		{
			try
			{
				return Convert.ChangeType(value, _valueType);
			}
			catch (Exception ex)
			{
				throw new InvalidValueException("Invalue value: " + value, ex);
			}
		}


		private char ConvertTypeToChar()
		{
			if (_valueType == null)
				return '?';
			else if (typeof(int) == _valueType)
				return 's';
			else if (typeof(float) == _valueType)
				return 'f';
			else if (typeof(double) == _valueType)
				return 'd';
			else if (typeof(string) == _valueType)
				return 's';
			else
				return '?';
		}
		#endregion Methods

		#region Overrides
		/// <summary>
		/// See <see cref="object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			OptionDefinition opt;

			if (obj == this)
				return true;

			opt = obj as OptionDefinition;

			if (opt == null)
				return false;

			return _ID.Equals(opt.ID);
		}


		/// <summary>
		/// See <see cref="object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			return _ID.GetHashCode();
		}

        
		/// <summary>
		/// Produce a string version of this variable
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(string.Join("|", _longNames));

			foreach (char shortName in _shortNames)
			{
				if (sb.Length > 0)
					sb.Append("|");
				sb.Append(shortName);
			}

			switch (_type)
			{
				case OptValType.Flag:
					break;
				case OptValType.IncrementalFlag:
					sb.Append("+");
					break;
				case OptValType.MultValue:
					sb.Append("+");
					sb.Append(ConvertTypeToChar());
					break;
				case OptValType.ValueOpt:
					sb.Append(":");
					sb.Append(ConvertTypeToChar());
					break;
				case OptValType.ValueReq:
					sb.Append("=");
					sb.Append(ConvertTypeToChar());
					break;
			}

			return sb.ToString();
		}
		#endregion Overrides
	}
}
