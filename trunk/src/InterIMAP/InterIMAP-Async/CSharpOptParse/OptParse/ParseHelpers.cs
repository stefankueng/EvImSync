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
using System.Reflection;

namespace CommandLine.OptParse
{
	#region DictionaryParserHelper class
	/// <summary>
	/// Class that parses option results into a dictionary interface
	/// <seealso cref="Parser"/>
	/// <seealso cref="ParserFactory"/>
	/// </summary>
	/// <remarks>
	/// The results are populated as {Key:[OptionDefinition] Value:[OptionResult]} pairs
	/// </remarks>
	public class DictionaryParserHelper : IOptionResults
	{
		#region Members
		private Hashtable   _optionsByName;
		private Hashtable   _optionsByLowerName;
		private IDictionary _results;
		#endregion Members

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="optDefs">The supported options</param>
		/// <param name="valuesDictionary">The dictionary that will be used to store
		/// the values of the options</param>
		public DictionaryParserHelper(OptionDefinition[] optDefs, 
			IDictionary valuesDictionary)
		{
			_optionsByName      = new Hashtable();
			_optionsByLowerName = new Hashtable();

			foreach (OptionDefinition optDef in optDefs)
			{
				if (optDef.LongNames != null)
				{
					foreach (string name in optDef.LongNames)
					{
						_optionsByName.Add(name, optDef);
						// allow duplicate definitions of lower case names (don't use add)
						_optionsByLowerName[name.ToLower()] = optDef;
					}
				}
				
				if (optDef.ShortNames != null)
				{
					foreach (char name in optDef.ShortNames)
					{
						_optionsByName.Add(name, optDef);
						// allow duplicate definitions of lower case names (don't use add)
						_optionsByLowerName[char.ToLower(name)] = optDef;
					}
				}
			}

			_results = valuesDictionary;
		}
		#endregion Constructors

		#region IOptionResults Members
		/// <summary>
		/// See <see cref="IOptionContainer.GetOptions()"/>
		/// </summary>
		public OptionDefinition[] GetOptions()
		{
			OptionDefinition[] options = new OptionDefinition[_results.Count];
			_results.Keys.CopyTo(options, 0);
			return options;
		}


		/// <summary>
		/// See <see cref="P:IOptionResults.Item(OptionDefinition)"/>
		/// </summary>
		public OptionResult this[OptionDefinition def] 
		{ 
			get { return (OptionResult)_results[def]; }
			set { _results[def] = value; }
		}


		/// <summary>
		/// See <see cref="IOptionResults.GetOptionDefinition(char, bool)"/>
		/// </summary>
		public OptionDefinition GetOptionDefinition(char optionName, bool caseSensitive)
		{
			if (caseSensitive)
				return (OptionDefinition)_optionsByName[optionName];
			else
				return (OptionDefinition)_optionsByLowerName[char.ToLower(optionName)];
		}


		/// <summary>
		/// See <see cref="IOptionResults.GetOptionDefinition(string, bool)"/>
		/// </summary>
		public OptionDefinition GetOptionDefinition(string optionName, bool caseSensitive)
		{
			if (caseSensitive)
				return (OptionDefinition)_optionsByName[optionName];
			else
				return (OptionDefinition)_optionsByLowerName[optionName.ToLower()];
		}
		#endregion
	}
	#endregion DictionaryParserHelper class

	#region PropertyFieldParserHelper class
	/// <summary>
	/// Helper class for the parser that stores values into properties and fields
	/// of a class.
	/// <seealso cref="Parser"/>
	/// <seealso cref="ParserFactory"/>
	/// <seealso cref="OptDefAttribute"/>
	/// <seealso cref="UseNameAsLongOptionAttribute"/>
	/// <seealso cref="LongOptionNameAttribute"/>
	/// <seealso cref="ShortOptionNameAttribute"/>
	/// <seealso cref="DefaultValueAttribute"/>
	/// <seealso cref="CategoryAttribute"/>
	/// <seealso cref="DescriptionAttribute"/>
	/// </summary>
	/// <remarks>
	/// Used to create option definitions from properties and fields of an object.
	/// Options are defined using <see cref="OptDefAttribute"/>, 
	/// <see cref="UseNameAsLongOptionAttribute"/>,
	/// <see cref="LongOptionNameAttribute"/>, 
	/// <see cref="ShortOptionNameAttribute"/>,
	/// <see cref="DefaultValueAttribute"/>,
	/// <see cref="CategoryAttribute"/>,
	/// and <see cref="DescriptionAttribute"/> attributes.
	/// </remarks>
	/// <include file="ExternalDocs.xml" 
	///		path="externalDocs/doc[@name='ExamplePropClass']/*"/>
	public class PropertyFieldParserHelper : IOptionResults
	{
		#region Members
		private Hashtable _memberInfoObjs;
		private Hashtable _optionDefinitions;
		private Hashtable _optionDefinitionsLowerName;
		private Type      _valueType;
		private object    _value;
		#endregion Members

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="value">The object with the properties and fields defined as
		/// options</param>
		public PropertyFieldParserHelper(object value)
		{
			_memberInfoObjs             = new Hashtable();
			_optionDefinitions          = new Hashtable();
			_optionDefinitionsLowerName = new Hashtable();
			_value                      = value;
			_valueType                  = value.GetType();

			// populate the hashtable
			foreach (FieldInfo info in _valueType.GetFields())
				AddMemberDefinition(info);

			foreach (PropertyInfo info in _valueType.GetProperties())
				AddMemberDefinition(info);
		}
		#endregion Constructor

		#region Private methods
		private void AddMemberDefinition(MemberInfo info)
		{
			OptDefAttribute  defAttr;
			OptionDefinition optDef;

			defAttr = (OptDefAttribute)GetAttribute(typeof(OptDefAttribute), info);

			// check if an attribute
			if (defAttr == null)
				return;
         
			optDef = BuildOptDef(defAttr, info);

			_memberInfoObjs.Add(optDef, info);

			foreach (char shortName in optDef.ShortNames)
			{
				_optionDefinitions.Add(shortName, optDef);
				// allow duplicate definitions of lower case names (don't use add)
				_optionDefinitionsLowerName[char.ToLower(shortName)] = optDef;
			}

			foreach (string longName in optDef.LongNames)
			{
				_optionDefinitions.Add(longName, optDef);
				// allow duplicate definitions of lower case names (don't use add)
				_optionDefinitionsLowerName[longName.ToLower()] = optDef;
			}
		}


		private OptionDefinition BuildOptDef(OptDefAttribute defAttr, MemberInfo info)
		{
			ArrayList                    nameDefinitions;
			Attribute[]                  attrs;
			CategoryAttribute            catAttr;
			DefaultValueAttribute        defValAttr;
			DescriptionAttribute         descAttr;
			OptionDefinition             def;
			UseNameAsLongOptionAttribute useNameAttr;
			Type                         type;
			bool                         useName = false;
			char[]                       shortNames = null;
			string                       category;
			string                       description;
			string[]                     longNames = null;
			
			// Get the description
			descAttr = (DescriptionAttribute)GetAttribute(typeof(DescriptionAttribute), info);
			description = (descAttr == null) ? info.Name : descAttr.Description;

			// Get the category
			catAttr = (CategoryAttribute)GetAttribute(typeof(CategoryAttribute), info);
			category = (catAttr == null) ? null : catAttr.Category;

			// use the name as a value?
			useNameAttr = (UseNameAsLongOptionAttribute)GetAttribute(
				typeof(UseNameAsLongOptionAttribute), info);
			useName = (useNameAttr == null) ? true : useNameAttr.UseName;

			nameDefinitions = new ArrayList(); 

			attrs = (Attribute[])info.GetCustomAttributes(typeof(ShortOptionNameAttribute), true);
			if (attrs != null)
			{
				int counter = 0;

				shortNames = new char[attrs.Length];

				foreach (ShortOptionNameAttribute attr in attrs)
				{
					shortNames[counter] = attr.Name;
					counter++;
				}                    
			}
			
			attrs = (Attribute[])info.GetCustomAttributes(typeof(LongOptionNameAttribute), true);
			if (attrs != null)
			{
				int count = (useName) ? attrs.Length + 1 : attrs.Length;
				int counter = 0;

				longNames = new string[count];

				foreach (LongOptionNameAttribute attr in attrs)
				{
					longNames[counter] = attr.Name;
					counter++;
				}
         
				if (useName)
					longNames[longNames.Length - 1] = info.Name;
			}

			if (defAttr.ValueType == null)
			{
				if (defAttr.OptionValueType == OptValType.MultValue)
					throw new ParseException("Multiple value options must have the " +
						"ValueType property set in the OptDefAttribute declaration");

				defAttr.ValueType = (info is FieldInfo) ?
					((FieldInfo)info).FieldType : ((PropertyInfo)info).PropertyType;
			}

			if (defAttr.ValueType == null)
			{
				if (info is PropertyInfo)
					type = ((PropertyInfo)info).PropertyType;
				else
					type = ((FieldInfo)info).FieldType;
			}
			else
				type =  defAttr.ValueType;

			def = new OptionDefinition(info.Name,
				defAttr.OptionValueType, type,
				category, description, shortNames, longNames);
			
			// Get the default value
			defValAttr = (DefaultValueAttribute)GetAttribute(typeof(DefaultValueAttribute), info);
			if (defValAttr != null)
				def.DefaultValue = defValAttr.Value;

			return def;
		}


		private Attribute GetAttribute(Type attrType, MemberInfo info)
		{
			Attribute[] attrs;

			attrs = (Attribute[])info.GetCustomAttributes(attrType, true);

			if (attrs == null || attrs.Length == 0)
				return null;

			return attrs[0];
		}


		private OptionResult GetResult(OptionDefinition def, MemberInfo info)
		{
			OptionResult result = new OptionResult(def);
			object       val;

			if (info is FieldInfo)
				val = ((FieldInfo)info).GetValue(_value);
			else
				val = ((PropertyInfo)info).GetValue(_value, null);

			switch (result.Defintion.Type)
			{
				case OptValType.Flag:
					bool isSet;

					if (val is bool)
						isSet = (bool)val;
					else
						isSet = Convert.ToBoolean(val);

					result.NumDefinitions = 1;
					result.Value          = isSet;
					break;

				case OptValType.IncrementalFlag:
					int numSet;

					numSet = Convert.ToInt32(val);

					result.NumDefinitions = numSet;
					result.Value          = numSet;

					break;

				case OptValType.MultValue:
					
					foreach (object value in (ICollection)val)
					{
						result.NumDefinitions++;
						result.AddValue(value);
					}

					break;
				case OptValType.ValueOpt:
				case OptValType.ValueReq:

					if (val == null)
					{
						result.NumDefinitions = 0;
						result.Value = null;
					}
					else
					{
						result.NumDefinitions = 1;
						result.Value = val;
					}
						
					break;
			}

			return result;
		}


		private void UpdateResult(MemberInfo info, OptionResult result)
		{
			object value;

			// Multiple value types are handled differently than the rest
			if (result.Defintion.Type == OptValType.MultValue)
			{
				IList list;
				if (info is FieldInfo)
					list = (IList)((FieldInfo)info).GetValue(_value);
				else
					list = (IList)((PropertyInfo)info).GetValue(_value, null);

				// will need to be improved later, but this will ensure only the
				// options are stored in the list
				list.Clear();
				foreach (string resultValue in result.Values)
					list.Add(resultValue);

				return;
			}

			switch (result.Defintion.Type)
			{
				case OptValType.Flag:
					value = result.IsDefined;
					break;
				case OptValType.IncrementalFlag:
					value = result.NumDefinitions;
					break;
				case OptValType.ValueOpt:
				case OptValType.ValueReq:
				default:
					value = result.Value;
					break;
			}

			if (info is FieldInfo)
				((FieldInfo)info).SetValue(_value, value);
			else
				((PropertyInfo)info).SetValue(_value, value, null);
		}
		#endregion Private methods

		#region IOptionResults Members
		/// <summary>
		/// See <see cref="IOptionContainer.GetOptions()"/>
		/// </summary>
		public OptionDefinition[] GetOptions()
		{
			OptionDefinition[] options = new OptionDefinition[_memberInfoObjs.Count];
			_memberInfoObjs.Keys.CopyTo(options, 0);
			return options;
		}


		/// <summary>
		/// See <see cref="P:IOptionResults.Item(OptionDefinition)"/>
		/// </summary>
		public OptionResult this[OptionDefinition def] 
		{ 
			get 
			{
				MemberInfo info = (MemberInfo)_memberInfoObjs[def];

				if (info == null)
					return null;

				return GetResult(def, info);
			}
			
			set 
			{ 
				MemberInfo info = (MemberInfo)_memberInfoObjs[def];
				
				if (info == null)
					throw new ArgumentException("No such option defintion found", "def");

				UpdateResult(info, value);
			}
		}


		/// <summary>
		/// See <see cref="IOptionResults.GetOptionDefinition(char, bool)"/>
		/// </summary>
		public OptionDefinition GetOptionDefinition(char optionName, bool caseSensitive)
		{
			if (caseSensitive)
				return (OptionDefinition)_optionDefinitions[optionName];
			else
				return (OptionDefinition)_optionDefinitionsLowerName[char.ToLower(optionName)];
		}


		/// <summary>
		/// See <see cref="IOptionResults.GetOptionDefinition(string, bool)"/>
		/// </summary>
		public OptionDefinition GetOptionDefinition(string optionName, bool caseSensitive)
		{
			if (caseSensitive)
				return (OptionDefinition)_optionDefinitions[optionName];
			else
				return (OptionDefinition)_optionDefinitionsLowerName[optionName.ToLower()];
		}
		#endregion
	}
	#endregion PropertyFieldParserHelper class
}
