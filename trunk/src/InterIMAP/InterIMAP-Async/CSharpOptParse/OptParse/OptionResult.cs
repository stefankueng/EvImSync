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
using System.ComponentModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace CommandLine.OptParse
{
	/// <summary>
	/// Stores the information on the result of parsing an option
	/// <seealso cref="Parser"/>
	/// <seealso cref="IOptionResults"/>
	/// <seealso cref="OptionResultsDictionary"/>
	/// </summary>
	public class OptionResult 
	{
		#region Members
		private ArrayList        _values;
		private OptionDefinition _defintion;
		private int              _numDefinitions;
		#endregion Members

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="def">Definition of the option this result if for</param>
		public OptionResult(OptionDefinition def)
		{
			_defintion = def;
		}
		#endregion Constructors

		#region Properties
		/// <summary>
		/// Get the definition for this result
		/// </summary>
		public OptionDefinition Defintion 
		{
			get { return _defintion; }
		}


		/// <summary>
		/// Get or set the number of times this option was defined
		/// </summary>
		/// <remarks>
		/// If the type of the option supports multiple definitions or multiple values, then
		/// this property specifies how many times the option was defined
		/// </remarks>
		public int NumDefinitions 
		{
			get { return _numDefinitions; }
			set { _numDefinitions = value; }
		}


		/// <summary>
		/// Get if this property has been defined
		/// </summary>
		public bool IsDefined
		{
			get { return _numDefinitions > 0; }
		}


		/// <summary>
		/// Get or set the value of the option
		/// </summary>
		/// <remarks>
		/// This value will always be null for <see cref="OptValType.Flag"/> style arguments and
		/// may be null for <see cref="OptValType.ValueOpt"/> and 
		/// <see cref="OptValType.MultValue"/> options.
		/// <para>
		/// If the option is defined multiple times, this will get or set the first value given
		/// </para>
		/// </remarks>
		public object Value 
		{
			get 
			{ 
				if (_values == null || _values.Count == 0)
					return null;
				else
					return _values[0]; 
			}

			set 
			{ 
				CheckType(value);

				if (_values == null)
					AddValue(value);
				else
					_values[0] = value; 
			}
		}


		/// <summary>
		/// Get or set all the values given for this option
		/// <seealso cref="AddValue(object)"/>
		/// </summary>
		/// <remarks>
		/// This property allows access to all the values of the option for use with 
		/// <see cref="OptValType.MultValue"/> type of options
		/// </remarks>
		public ArrayList Values
		{
			get { return _values; }
		}
		#endregion Properties

		#region Methods
		/// <summary>
		/// Add a value
		/// </summary>
		/// <remarks>The value should already be converted to the correct type</remarks>
		/// <param name="value">The value to add</param>
		public void AddValue(object value)
		{
			CheckType(value);
			if (_values == null)
				_values = new ArrayList();
			
			_values.Add(value);
		}

		
		private void CheckType(object value)
		{
			if (value == null && typeof(ValueType).IsAssignableFrom(_defintion.ValueType))
				throw new InvalidValueException("Null is not supported for this type");
			else if (value == null)
				return;
			else if (_defintion.ValueType.IsAssignableFrom(value.GetType()) == false)
				throw new InvalidValueException("Value is not of the correct type");
		}
		#endregion Methods
	}
}
