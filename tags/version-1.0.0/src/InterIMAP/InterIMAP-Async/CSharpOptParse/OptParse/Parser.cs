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
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CommandLine.OptParse
{
	#region Parser class
	/// <summary>
	/// Class that parses the command line options. Use the <see cref="ParserFactory"/>
	/// to construct an instance of a parser
	/// <seealso cref="UsageBuilder"/>
	/// <seealso cref="PropertyFieldParserHelper"/>
	/// <seealso cref="DictionaryParserHelper"/>
	/// <seealso cref="IOptionResults"/>
	/// <seealso cref="ParserFactory"/>
	/// </summary>
	/// <remarks>
	/// Class that
	/// </remarks>
	/// <include file="ExternalDocs.xml" 
	///		path="externalDocs/doc[@name='ExamplePropClass']/*"/>
	public class Parser : IOptionContainer
	{
		#region Events
		/// <summary>
		/// Event fired when a warning event occurs
		/// </summary>
		public event WarningEventHandler OptionWarning;
		#endregion Events

		#region Members
		private DupOptHandleType     _dupOptHandleType = DupOptHandleType.Warning;
		private IOptionResults       _resultsHandler;
		private OptStyle             _optStyle = OptStyle.Unix;
		private UnknownOptHandleType _unknownOptHandleType = UnknownOptHandleType.Warning;
		private UnixShortOption      _unixShortOption = UnixShortOption.ShortSeparated;
		private bool                 _caseSensitive = true;
		private bool                 _searchEnvironment = false;
		#endregion Members
		
		#region Constructor
		/// <summary>
		/// Constructor
		/// <seealso cref="ParserFactory"/>
		/// </summary>
		/// <remarks>
		/// <para>The <paramref name="resultsHandler" /> object is resposible for
		/// maintaining the option definitions and stores the results of parsing.
		/// </para>
		/// <para>The <see cref="ParserFactory"/> class can assist with constructing
		/// instances of the parser (handles <see cref="IOptionResults"/> object
		/// creating). If the factory is not used, the 
		/// <see cref="PropertyFieldParserHelper"/> and
		/// <see cref="DictionaryParserHelper"/> classes can be used.</para>
		/// </remarks>
		/// <param name="resultsHandler">The interface containing the option
		/// definitions and handles the results of parsing.</param>
		public Parser(IOptionResults resultsHandler)
		{
			_resultsHandler = resultsHandler;
		}
		#endregion Constructor

		#region Properties
		/// <summary>
		/// Get or set to search the environment names for values to the options
		/// </summary>
		/// <remarks>
		/// If set to true, environment variable values will be used if found
		/// and the options are not given on the command line. The prefixes
		/// (--, -, or /) are not used, only the names of the options when
		/// searching the environment.
		/// </remarks>
		[DefaultValue(false)]
		public bool SearchEnvironment 
		{
			get { return _searchEnvironment; }
			set { _searchEnvironment = value; }
		}


		/// <summary>
		/// Get or set if the parsing of options should consider the case of the options
		/// </summary>
		[DefaultValue(true)]
		public bool CaseSensitive
		{
			get { return _caseSensitive; }
			set { _caseSensitive = value; }
		}


		/// <summary>
		/// Get or set how the parser should handle un-expected duplicate option declarations
		/// </summary>
		[DefaultValue(DupOptHandleType.Warning)]
		public DupOptHandleType DupOptHandleType 
		{
			get { return _dupOptHandleType; }
			set { _dupOptHandleType = value; }
		}


		/// <summary>
		/// Get the handler for the parser
		/// </summary>
		public IOptionResults ResultsHandler 
		{
			get { return _resultsHandler; }
		}


		/// <summary>
		/// Get or set the option style to parse
		/// </summary>
		public OptStyle OptStyle 
		{
			get { return _optStyle; }
			set { _optStyle = value; }
		}


		/// <summary>
		/// Get or set how to handle unknown options
		/// </summary>
		[DefaultValue(UnknownOptHandleType.Warning)]
		public UnknownOptHandleType UnknownOptHandleType 
		{
			get { return _unknownOptHandleType; }
			set { _unknownOptHandleType = value; }
		}


		/// <summary>
		/// Get how to handle short unix options (with or without bundling)
		/// </summary>
		[DefaultValue(UnixShortOption.ShortSeparated)]
		public UnixShortOption UnixShortOption 
		{
			get { return _unixShortOption; }
			set { _unixShortOption = value; }
		}
		#endregion Properties

		#region Public methods
		/// <summary>
		/// Get all of the option definitions that have been declared
		/// </summary>
		/// <returns>Array of option definitions</returns>
        public OptionDefinition[] GetOptionDefinitions()
		{
			return _resultsHandler.GetOptions();
		}


		/// <summary>
		/// Get a collection of option value, option description pairs
		/// </summary>
		/// <remarks>
		/// Used to help print the usage of the application. This function concatinates
		/// the options together into one string that is used as the key of the
		/// collection, and then sets the value as the description of the option.
		/// <para>
		/// Example result:
		/// <code>
		/// Key  : -h, --help
		///	Value: Print the usage of this application
		/// </code>
		/// Here, the key is a comma concatination of the options and the value
		/// is the description of that option.
		/// </para>
		/// <para>
		/// Short names will precede the long options in the key.
		/// </para>
		/// </remarks>
		/// <param name="style">The style of options. During concatination, the
		/// appropriate prefix ('/', '-' or '--') will be added to each option
		/// name</param>
		/// <returns>Collection of the options and their descriptions</returns>
		[Obsolete("Use UsageBuilder instead")]
		public NameValueCollection GetOptionDescriptions(OptStyle style)
		{
			NameValueCollection coll = new NameValueCollection();
			OptionDefinition[]  opts = _resultsHandler.GetOptions();
			StringBuilder       key  = new StringBuilder();

			foreach (OptionDefinition opt in opts)
			{
				key.Length = 0;

				if (opt.ShortNames != null && opt.ShortNames.Length > 0)
				{
					foreach (char optName in opt.ShortNames)
					{
						if (key.Length > 0) 
							key.Append((style == OptStyle.Unix) ? ", -" : ", /");
						else
							key.Append((style == OptStyle.Unix) ? "-" : "/");
						key.Append(optName);
					}

					if (opt.LongNames != null && opt.LongNames.Length > 0)
						key.Append(", ");
				}

				if (opt.LongNames != null && opt.LongNames.Length > 0)
				{
					key.Append((style == OptStyle.Unix) ? "--" : "/");
					key.Append(string.Join((style == OptStyle.Unix) ? ", --" : ", /",
						opt.LongNames));
				}

				coll.Add(key.ToString(), opt.Description);
			}

			return coll;
		}

		
		/// <summary>
		/// Prints out the usage of this program to the given text writer
		/// <seealso cref="IProgUsageInfo"/>
		/// <seealso cref="DefaultUsageInfo"/>
		/// <seealso cref="GetOptionDescriptions"/>
		/// </summary>
		/// <remarks>
		/// Prints the usage to the standard output. See <see cref="IProgUsageInfo"/>
		/// and <see cref="GetOptionDescriptions"/>
		/// for more information on how the usage is printed. 
		/// <para>At this time, wrapping is not done at word boundries</para>
		/// </remarks>
		/// <param name="optStyle">The style of the options to have printed
		/// in the usage</param>
		/// <param name="usageInfo"></param>
		/// <param name="columns">The number of columns to print before wrapping
		/// the text. Used to correctly format the indenting</param>
		[Obsolete("Use UsageBuilder instead")]
		public void PrintUsage(OptStyle optStyle, IProgUsageInfo usageInfo, 
			int columns)
		{
			PrintUsage(optStyle, usageInfo, Console.Out, columns);
		}


		/// <summary>
		/// Prints out the usage of this program to the given text writer
		/// <seealso cref="IProgUsageInfo"/>
		/// <seealso cref="DefaultUsageInfo"/>
		/// <seealso cref="GetOptionDescriptions"/>
		/// </summary>
		/// <remarks>
		/// Prints the usage to the given writer. See <see cref="IProgUsageInfo"/>
		/// and <see cref="GetOptionDescriptions"/>
		/// for more information on how the usage is printed. 
		/// <para>At this time, wrapping is not done at word boundries</para>
		/// </remarks>
		/// <param name="optStyle">The style of the options to have printed
		/// in the usage</param>
		/// <param name="writer">The writer to print the usage to</param>
		/// <param name="usageInfo"></param>
		/// <param name="columns">The number of columns to print before wrapping
		/// the text. Used to correctly format the indenting</param>
		[Obsolete("Use UsageBuilder instead")]
		public void PrintUsage(OptStyle optStyle, IProgUsageInfo usageInfo, 
			TextWriter writer, int columns)
		{
			foreach (string header in usageInfo.Headers)
			{
				writer.WriteLine(header);
				if (usageInfo.IsOptionHeader(header))
				{
					NameValueCollection coll = GetOptionDescriptions(optStyle);

					for (int i = 0; i < coll.Count; i++)
					{
						PrintWithIndent(4, coll.GetKey(i), columns, writer);
						PrintWithIndent(8, coll[i], columns, writer);
					}
				}
				else
					PrintWithIndent(4, usageInfo.GetContents(header), columns, writer);
			}
		}
		#endregion Public methods

		#region Parse Methods
		/// <summary>
		/// Parse the <see cref="Environment.GetCommandLineArgs"/> 
		/// for options using the parser settings
		/// </summary>
		/// <returns>Arguments from the command-line that were not options</returns>
		public string[] Parse()
		{
			return Parse(this.OptStyle, this.UnixShortOption, this.DupOptHandleType,
				this.UnknownOptHandleType, this.CaseSensitive, Environment.GetCommandLineArgs());
		}


		/// <summary>
		/// Parse the arguments for options using the parser settings
		/// </summary>
		/// <param name="args">The command-line arguments to parse</param>
		/// <returns>Arguments from the command-line that were not options</returns>
		public string[] Parse(string[] args)
		{
			return Parse(this.OptStyle, this.UnixShortOption, this.DupOptHandleType,
				this.UnknownOptHandleType, this.CaseSensitive, args);
		}


		/// <summary>
		/// Parse the arguments for options using the given settings
		/// </summary>
		/// <param name="optStyle">What type of options to parse</param>
		/// <param name="unixShortOption">How to parse unix short options (ignored
		/// if not unix style parsing)</param>
		/// <param name="dupOptHandleType">How to handle un-expected duplicate option
		/// definitions</param>
		/// <param name="unknownOptHandleType">How to handle options that were not
		/// defined</param>
		/// <param name="caseSesitive">If the parsing of options 
		/// should consider the case of the options</param>
		/// <param name="args">The command-line arguments to parse</param>
		/// <returns>Arguments from the command-line that were not options</returns>
		public string[] Parse(OptStyle optStyle, UnixShortOption unixShortOption, 
			DupOptHandleType dupOptHandleType, UnknownOptHandleType unknownOptHandleType,
			bool caseSesitive, string[] args)
		{
			ArrayList        foundDefinitions = new ArrayList();
			Group            grp;
			Match            m;
			Regex            re;
			StringCollection arguments;
			bool             isShort;
			bool             unknown;
			string           name;
			string           value;
			string[]         results;

			if (args == null || args.Length == 0)
			{
				if (this.SearchEnvironment)
					ParseEnvironment(foundDefinitions, caseSesitive);
				return new string[0];
			}

			arguments = new StringCollection();
			
			re = BuildRegEx(optStyle, unixShortOption);

			for (int i = 0; i < args.Length; i++)
			{
				m = re.Match(args[i]);

				if (m.Success)
				{
					// see if there is a value
					grp = m.Groups["shortname"];

					if (grp != null && grp.Value != string.Empty)
					{
						isShort = true;
						name = grp.Value;
						value = m.Groups["shortvalue"].Value;
					}
					else
					{
						isShort = false;
						name = m.Groups["longname"].Value;
						value = m.Groups["longvalue"].Value;

						// remove the '=' or ':' at the beginning of the value
						if (value != null && value.Length > 0)
							value = value.Substring(1);
					}

					if (optStyle == OptStyle.Unix)
					{
						if (isShort == false)
							ParseLongOption(args, name, value, caseSesitive, 
								ref i, dupOptHandleType, unknownOptHandleType, foundDefinitions,
								out unknown);
						// see if the value is just concatinated short options
						else if (unixShortOption == UnixShortOption.CollapseShort)
						{
							// name is the first option
							ParseShortOption(args, name[0], null, caseSesitive, 
								ref i, dupOptHandleType, unknownOptHandleType, foundDefinitions,
								out unknown);

							// parse the rest of the options that lie in the 'value' variable
							if (value != null)
							{
								char[] unknownOptions;

								ParseShortOptions(args, value.ToLower().ToCharArray(), 
									caseSesitive, ref i, dupOptHandleType, unknownOptHandleType,
									foundDefinitions, out unknownOptions);

								// don't do anything with unknown concatinated short options,
								// they should not be considered options or arguments
							}
						}
						else
							// name is the first option
							ParseShortOption(args, name[0], value, caseSesitive,
								ref i, dupOptHandleType, unknownOptHandleType, foundDefinitions,
								out unknown);
					}
					else
					{
						if (name.Length == 1)
							ParseShortOption(args, name[0], value, caseSesitive,
								ref i, dupOptHandleType, unknownOptHandleType, foundDefinitions,
								out unknown);
						else
							ParseLongOption(args, name, value, caseSesitive, ref i, 
								dupOptHandleType, unknownOptHandleType, foundDefinitions,
								out unknown);
					}

					// consider unknown options to be arguments
					if (unknown)
						arguments.Add(args[i]);
				}
				else
					arguments.Add(args[i]);
			}

			
			// parse the environment
			if (this.SearchEnvironment)
				ParseEnvironment(foundDefinitions, caseSesitive);

			results = new string[arguments.Count];
			arguments.CopyTo(results, 0);
			return results;
		}
		#endregion Parse Methods

		#region Private methods
		private void ParseEnvironment(ArrayList foundDefinitions, bool caseSensitive)
		{
			OptionDefinition opt;
			OptionResult     result;

			foreach (string key in Environment.GetEnvironmentVariables().Keys)
			{
				string value;

				if (key.Length == 1)
                    opt = _resultsHandler.GetOptionDefinition(key[0], caseSensitive);
				else
					opt = _resultsHandler.GetOptionDefinition(key, caseSensitive);

				if (opt != null)
				{
					// don't not overwrite the command-line values
					if (foundDefinitions.Contains(opt))
						continue;

					value  = Environment.GetEnvironmentVariable(key);
					result = _resultsHandler[opt];

					if (result == null) result = new OptionResult(opt);

					switch (opt.Type)
					{
						case OptValType.Flag:
							if (value.ToLower() != bool.TrueString.ToLower()) continue;
							result.NumDefinitions = 1;
							_resultsHandler[opt] = result;
							break;

						case OptValType.IncrementalFlag:
							if (value.ToLower() != bool.TrueString.ToLower()) continue;
							result.NumDefinitions++;
							result.Value = result.NumDefinitions;
							_resultsHandler[opt] = result;
							break;

						case OptValType.MultValue:
							if (value == null) continue;
							try { result.AddValue(opt.ConvertValue(value)); }
							catch { continue; }
							result.NumDefinitions++;
							_resultsHandler[opt] = result;
							break;

						case OptValType.ValueOpt:
							if (value == null) continue;
							try { result.Value = opt.ConvertValue(value); }
							catch { continue; }
							result.NumDefinitions = 1;
							_resultsHandler[opt] = result;
							break;

						case OptValType.ValueReq:
							if (value == null) continue;
							try { result.Value = opt.ConvertValue(value); }
							catch { continue; }
							result.NumDefinitions = 1;
							_resultsHandler[opt] = result;
							break;
					}
				}
			}
		}


		/// <summary>
		/// Prints text to the given writer with the given indent
		/// </summary>
		/// <remarks>At this time wrapping is not done at a word boundry</remarks>
		/// <param name="indent">The indent to add to the text in terms of number
		/// of spaces</param>
		/// <param name="text">The text to print</param>
		/// <param name="maxColumns">The number of columns to wrap at</param>
		/// <param name="writer">The writer to print to</param>
		private void PrintWithIndent(int indent, string text, int maxColumns,
			TextWriter writer)
		{
			string   indentStr = new string(' ', indent);
			string[] lines;

			text = text.Replace("\t", "    ");
			text = text.Replace("\r\n", "\n");
			text = text.Replace("\r", "\n");

			lines = text.Split('\n');
			maxColumns = maxColumns - indent;

			foreach (string line in lines)
			{
				if (text.Length <= maxColumns)
				{
					writer.Write(indentStr);
					writer.WriteLine(line);
				}
				else
				{
					for (int currPos = 0; currPos < line.Length;)
					{
						int    i = Math.Min(maxColumns, line.Length - currPos);
						string str = line.Substring(currPos, i);

						writer.Write(indentStr);
						writer.WriteLine(str);

						currPos += i;
					}
				}
			}
		}


		/// <summary>
		/// Parse a concatinated list of short options
		/// </summary>
		/// <param name="args">All the arguments</param>
		/// <param name="options">The options found</param>
		/// <param name="index">The current index in the arguments which can be incremented
		/// by this function</param>
		/// <param name="dupOptHandleType">How to handle un-expected duplicate option
		/// definitions</param>
		/// <param name="unknownOptHandleType">How to handle options that were not
		/// defined</param>
		/// <param name="caseSesitive">True if parsing is case-sensitive</param>
		/// <param name="foundDefinitions">The option definitions already found</param>
		/// <param name="unknownChars">An array of all unknown option characters or null
		/// if all were known</param>
		private void ParseShortOptions(string[] args, char[] options, bool caseSesitive, ref int index,
			DupOptHandleType dupOptHandleType, UnknownOptHandleType unknownOptHandleType,
			ArrayList foundDefinitions, out char[] unknownChars)
		{
			ArrayList unknownCharsList = new ArrayList();
			bool      wasUnknown;

			foreach (char option in options)
			{
				ParseShortOption(args, option, null, caseSesitive, ref index, dupOptHandleType,
					unknownOptHandleType, foundDefinitions, out wasUnknown);

				if (wasUnknown)
					unknownCharsList.Add(option);
			}

			if (unknownCharsList.Count == 0)
				unknownChars = null;
			else
				unknownChars = (char[])unknownCharsList.ToArray(typeof(char));
		}


		/// <summary>
		/// Parse a short option
		/// </summary>
		/// <param name="args">All the arguments</param>
		/// <param name="option">The option name found</param>
		/// <param name="value">The value immediately following the option (no space)</param>
		/// <param name="index">The current index in the arguments which can be incremented
		/// by this function</param>
		/// <param name="dupOptHandleType">How to handle un-expected duplicate option
		/// definitions</param>
		/// <param name="unknownOptHandleType">How to handle options that were not
		/// defined</param>
		/// <param name="caseSesitive">True if parsing is case-sensitive</param>
		/// <param name="foundDefinitions">The option definitions already found</param>
		/// <param name="wasUnknown">If the option was unknown and the handle type was not
		/// error, this will be true so that the "option" can be added to the arguments</param>
		private void ParseShortOption(string[] args, char option, string value, bool caseSesitive, 
			ref int index, DupOptHandleType dupOptHandleType, 
			UnknownOptHandleType unknownOptHandleType, ArrayList foundDefinitions, out bool wasUnknown)
		{
			ParseOption(args, option.ToString(), 
				_resultsHandler.GetOptionDefinition(option, caseSesitive), 
				value, ref index, dupOptHandleType, unknownOptHandleType, foundDefinitions,
				out wasUnknown);
		}


		/// <summary>
		/// Parse a long option
		/// </summary>
		/// <param name="args">All the arguments</param>
		/// <param name="option">The option name found</param>
		/// <param name="value">The value immediately following the option ([=:]value syntax)</param>
		/// <param name="index">The current index in the arguments which can be incremented
		/// by this function</param>
		/// <param name="dupOptHandleType">How to handle un-expected duplicate option
		/// definitions</param>
		/// <param name="unknownOptHandleType">How to handle options that were not
		/// defined</param>
		/// <param name="caseSesitive">True if parsing is case-sensitive</param>
		/// <param name="foundDefinitions">The option definitions already found</param>
		/// <param name="wasUnknown">If the option was unknown and the handle type was not
		/// error, this will be true so that the "option" can be added to the arguments</param>
		private void ParseLongOption(string[] args, string option, string value, bool caseSesitive, 
			ref int index, DupOptHandleType dupOptHandleType, 
			UnknownOptHandleType unknownOptHandleType, ArrayList foundDefinitions, out bool wasUnknown)
		{
			ParseOption(args, option, _resultsHandler.GetOptionDefinition(option, caseSesitive), 
				value, ref index, dupOptHandleType, unknownOptHandleType, foundDefinitions,
				out wasUnknown);
		}


		/// <summary>
		/// Parse an option
		/// </summary>
		/// <param name="args">All the arguments</param>
		/// <param name="optName">The char or string that the option was identified with</param>
		/// <param name="optDef">The option found</param>
		/// <param name="value">The value immediately following the option ([=:]value syntax)</param>
		/// <param name="index">The current index in the arguments which can be incremented
		/// by this function</param>
		/// <param name="dupOptHandleType">How to handle un-expected duplicate option
		/// definitions</param>
		/// <param name="unknownOptHandleType">How to handle options that were not
		/// defined</param>
		/// <param name="foundDefinitions">The option definitions already found</param>
		/// <param name="wasUnknown">If the option was unknown and the handle type was not
		/// error, this will be true so that the "option" can be added to the arguments</param>
		private void ParseOption(string[] args, object optName,
			OptionDefinition optDef, string value, 
			ref int index, DupOptHandleType dupOptHandleType, 
			UnknownOptHandleType unknownOptHandleType, ArrayList foundDefinitions, out bool wasUnknown)
		{
			OptionResult result;

			// make sure the option is found
			if (optDef == null)
			{
				switch (unknownOptHandleType)
				{
					case UnknownOptHandleType.Error:
						throw new ParseException(
							string.Format("Option {0} is not recoginzed", optName));
					case UnknownOptHandleType.Warning:
						if (OptionWarning != null)
							OptionWarning(this, new OptionWarningEventArgs(
								string.Format("Option {0} is not recoginzed", optName)));
						break;
				}

				wasUnknown = true;
				return;
			}
			else
				wasUnknown = false;

			// make sure value is not given went it should not be given
			CheckIfValueless(optDef, value);

			result = _resultsHandler[optDef];

			// make sure value is not duplicate when it should not be
			CheckIfDup(optDef, foundDefinitions, dupOptHandleType);
			foundDefinitions.Add(optDef);

			switch (optDef.Type)
			{
				case OptValType.Flag:
					if (result == null)
						result = new OptionResult(optDef);

					result.NumDefinitions = 1;
					result.Value = true;
					_resultsHandler[optDef] = result;
					break;

				case OptValType.IncrementalFlag:
					if (result == null)
						result = new OptionResult(optDef);

					result.NumDefinitions++;
					result.Value = result.NumDefinitions;
					_resultsHandler[optDef] = result;
					break;

				case OptValType.MultValue:
					value = GetValue(args, value, ref index);

					if (value == null)
						throw new ParseException(
							string.Format("Option {0} requires a value", optDef.ID));

					if (result == null)
						result = new OptionResult(optDef);

					result.NumDefinitions++;
					result.AddValue(optDef.ConvertValue(value));
					_resultsHandler[optDef] = result;
					break;

				case OptValType.ValueOpt:
					value = GetValue(args, value, ref index);

					if (result == null)
						result = new OptionResult(optDef);

					result.NumDefinitions = 1;
					result.Value = optDef.ConvertValue(value);
					_resultsHandler[optDef] = result;
					break;

				case OptValType.ValueReq:
					value = GetValue(args, value, ref index);

					if (value == null)
						throw new ParseException(
							string.Format("Option {0} requires a value", optDef.ID));
					
					if (result == null)
						result = new OptionResult(optDef);

					result.NumDefinitions = 1;
					result.Value = optDef.ConvertValue(value);
					_resultsHandler[optDef] = result;
					break;
			}
		}


		private string GetValue(string[] args, string value, ref int index)
		{
			if (value == null || value == string.Empty)
			{
				if (index + 1 >= args.Length)
					return null;
                
				value = args[index + 1];

				// make sure an option is not next
				if (value.StartsWith("-") || value.StartsWith("/"))
					return null;

				value = args[++index];
			}

			return value;
		}


		/// <summary>
		/// Check if the option is a duplicate (2nd, 3rd, etc. declaration) and the option
		/// does not support multiple values/declarations
		/// </summary>
		/// <param name="optDef">The option found</param>
		/// <param name="foundDefinitions">All of the found definitions so far</param>
		/// <param name="dupOptHandleType">How to handle duplicates</param>
		/// <returns>True if the option has already been found, and the type
		/// does not support duplicates</returns>
		private bool CheckIfDup(OptionDefinition optDef, ArrayList foundDefinitions, 
			DupOptHandleType dupOptHandleType)
		{
			if (foundDefinitions.IndexOf(optDef) == -1) return false;

			switch (optDef.Type)
			{
					// types that allow duplicates:
				case OptValType.MultValue:
				case OptValType.IncrementalFlag:
					return false;
				default:
					switch (dupOptHandleType)
					{
						case DupOptHandleType.Error:	
							throw new ParseException(
								string.Format("Option {0} does not support duplicate values",
								optDef.ID));
						case DupOptHandleType.Warning:
							if (OptionWarning != null)
								OptionWarning(this, new OptionWarningEventArgs(
									string.Format("Option {0} does not support duplicate values",
								optDef.ID)));
							break;
					}
					return true;
			}
		}



		/// <summary>
		/// Check if the option definition does not allow a value and if a value was given
		/// </summary>
		/// <param name="optDef">The option definition</param>
		/// <param name="value">The value given or null if one was not given</param>
		/// <exception cref="ParseException">Thrown if value was given to a type
		/// that doesn't support a value</exception>
		private void CheckIfValueless(OptionDefinition optDef, string value)
		{
			// make sure value is not null (or empty)
			if (value == null || value == string.Empty)
				return;

			switch (optDef.Type)
			{
					// types that allow values:
				case OptValType.MultValue:
				case OptValType.ValueOpt:
				case OptValType.ValueReq:
					return;
				default:
					throw new ParseException(
						string.Format("Value given to option {0} that does not support a value",
						optDef.ID));
			}
		}


		private Regex BuildRegEx(OptStyle optStyle, UnixShortOption unixShortOption)
		{
			if (optStyle == OptStyle.Unix)
				return new Regex(@"^
					(?:
						--(?<longname>[^\s=]+)
						(?<longvalue>=.+)?
						|
						-(?<shortname>\S)
						(?<shortvalue>.+)?
					)$", RegexOptions.IgnorePatternWhitespace);
			else
				return new Regex(@"^/(?<longname>[^\s:]+)(?<longvalue>:.+)?$");
		}
		#endregion Private methods

		#region IOptionContainer Members
		/// <summary>
		/// Get the options defined for this parser
		/// </summary>
		/// <returns>All of the defined options</returns>
		public OptionDefinition[] GetOptions()
		{
			return _resultsHandler.GetOptions();
		}
		#endregion
	}
	#endregion Parser class

	#region OptionWarningEventArgs class
	/// <summary>
	/// Event args for parse warnings
	/// </summary>
	public class OptionWarningEventArgs : EventArgs
	{
		#region Members
		private string _warningMessage;
		#endregion Members

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="warningMessage">The warning message</param>
		public OptionWarningEventArgs(string warningMessage)
		{
			_warningMessage = warningMessage;
		}
		#endregion Constructor

		#region Properties
		/// <summary>
		/// Get the warning message
		/// </summary>
		public string WarningMessage 
		{
			get { return _warningMessage; }
		}
		#endregion Properties
	}
	#endregion OptionWarningEventArgs class

	#region ParserFactory class
	/// <summary>
	/// Factory to build parser instances
	/// <seealso cref="Parser"/>
	/// <seealso cref="ParserFactory"/>
	/// <seealso cref="UsageBuilder"/>
	/// </summary>
	public sealed class ParserFactory
	{
		#region Constructor
		private ParserFactory() {}
		#endregion Constructor

		#region Factory methods
		/// <summary>
		/// Build a parser
		/// </summary>
		/// <param name="propertiesObject">Object containing fields and properties
		/// that represent option definitions</param>
		/// <returns>Parser instance</returns>
		public static Parser BuildParser(object propertiesObject)
		{
			return new Parser(new PropertyFieldParserHelper(propertiesObject));
		}

	
		/// <summary>
		/// Build a parser
		/// </summary>
		/// <param name="optDefs">The supported options</param>
		/// <param name="valuesDictionary">Dictionary to recieve parsed option values</param>
		/// <returns>Parser instance</returns>
		public static Parser BuildParser(OptionDefinition[] optDefs, IDictionary valuesDictionary)
		{
			return new Parser(new DictionaryParserHelper(optDefs, valuesDictionary));
		}


		/// <summary>
		/// Build a parser
		/// </summary>
		/// <param name="optDefs">The supported options</param>
		/// <param name="valuesDictionary">The dictionary to store the option results in</param>
		/// <returns>Parser instance</returns>
		public static Parser BuildParser(OptionDefinition[] optDefs, 
			OptionResultsDictionary valuesDictionary)
		{
			return new Parser(new DictionaryParserHelper(optDefs, valuesDictionary));
		}


		/// <summary>
		/// Build a parser using a custom <see cref="IOptionResults"/> handler.
		/// </summary>
		/// <param name="resultsHandler">Custom handler</param>
		/// <returns>Parser instance</returns>
		public static Parser BuildParser(IOptionResults resultsHandler)
		{
			return new Parser(resultsHandler);
		}
		#endregion Factory methods
	}
	#endregion ParserFactory class
}
