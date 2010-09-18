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

namespace CommandLine.OptParse
{
	/// <summary>
	/// Dictionary of option results
	/// <seealso cref="DictionaryParserHelper"/>
	/// <seealso cref="ParserFactory.BuildParser(OptionDefinition[], 
	///		OptionResultsDictionary)"/>
	/// </summary>
	/// <remarks>
	/// When using the <see cref="DictionaryParserHelper"/>, the helper will
	/// use this class to store the results of option parsing in an instance
	/// of this class.
	/// </remarks>
	public class OptionResultsDictionary : DictionaryBase
	{
		#region Members
		private Hashtable _indexedByID = new Hashtable();
		#endregion Members

		#region Methods
		/// <summary>
		/// Add a result
		/// </summary>
		/// <param name="key">Option definition</param>
		/// <param name="value">Result value</param>
		public void Add(OptionDefinition key, OptionResult value)
		{
			base.Dictionary.Add(key, value);
		}
		#endregion Methods
		
		#region Properties
		/// <summary>
		/// Get or set result by definition
		/// </summary>
		public OptionResult this[OptionDefinition key]
		{
			get { return (OptionResult)base.Dictionary[key]; }
			set { base.Dictionary[key] = value; }
		}


		/// <summary>
		/// Get the result by the ID of a definition
		/// </summary>
		public OptionResult this[object ID]
		{
			get { return (OptionResult)_indexedByID[ID]; }
		}
		#endregion Properties

		#region Overrides
		/// <summary>
		/// Validate the type of the key and value
		/// </summary>
		protected override void OnValidate(object key, object value)
		{
			if ((key is OptionDefinition) == false)
				throw new ArgumentException("Key must be OptionDefinition");
			if ((value is OptionResult) == false)
				throw new ArgumentException("Value must be OptionResult");

			base.OnValidate (key, value);
		}


		/// <summary>
		/// Update the inner index hashtables
		/// </summary>
		protected override void OnClearComplete()
		{
			_indexedByID.Clear();
			base.OnClearComplete ();
		}


		/// <summary>
		/// Update the inner index hashtables
		/// </summary>
		protected override void OnInsertComplete(object key, object value)
		{
			base.OnInsert (key, value);
			_indexedByID[((OptionDefinition)key).ID] = value;
		}


		/// <summary>
		/// Update the inner index hashtables
		/// </summary>
		protected override void OnRemoveComplete(object key, object value)
		{
			base.OnRemoveComplete (key, value);
			_indexedByID[((OptionDefinition)key).ID] = value;
		}


		/// <summary>
		/// Update the inner index hashtables
		/// </summary>
		protected override void OnSetComplete(object key, object oldValue, object newValue)
		{
			base.OnSetComplete (key, oldValue, newValue);
			_indexedByID[((OptionDefinition)key).ID] = newValue;
		}
		#endregion Overrides
	}
}
