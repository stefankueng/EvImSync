using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace CommandLine.OptParse
{
	#region ListType enum
	/// <summary>
	/// Type of list to create in the usage
	/// </summary>
	public enum ListType
	{
		/// <summary>
		/// Numbered/ordered list
		/// </summary>
		Ordered,

		/// <summary>
		/// Unordered list
		/// </summary>
		Unordered
	}
	#endregion ListType enum

	#region UsageBuilder class
	/// <summary>
	/// Class to assist the building of program usage information
	/// </summary>
	/// <remarks>
	/// The API of this class is similar to that of an 
	/// <see cref="System.Xml.XmlTextWriter"/>. The output of the usage
	/// is an <see cref="XmlDocument"/>. The using the 
	/// <see cref="ToHtml"/>, <see cref="ToText"/> and <see cref="Transform"/> methods
	/// the XML can be transformed into the desired output format for the usage.
	/// </remarks>
	/// <include file='ExternalDocs.xml' 
	///		path='externalDocs/doc[@name="ExampleUsageCode"]/*'/>
	public class UsageBuilder
	{
		#region Members
		private XmlDocument _usage;
		private XmlNode     _currentNode;
		private bool        _groupOptionsByCategory;
		private string      _defaultOptionCategory = "Uncategorized";
		#endregion Members

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public UsageBuilder()
		{
			_usage = new XmlDocument();
			_currentNode = _usage.AppendChild(_usage.CreateElement("usage"));
		}
		#endregion Constructor

		#region Properties
		/// <summary>
		/// Get the <see cref="XmlDocument"/> containing the usage content
		/// </summary>
		public XmlDocument Usage
		{
			get { return _usage; }
		}


		/// <summary>
		/// Get or set the category to use for options when an option is missing a category
		/// <seealso cref="GroupOptionsByCategory"/>
		/// </summary>
		/// <remarks>Cannot be set to null or an empty string, if so, the default value will
		/// be used</remarks>
		[DefaultValue("Uncategorized")]
		public string DefaultOptionCategory 
		{
			get { return _defaultOptionCategory; }
			set 
			{
				if (value == null || value.Length == 0)
					value = "Uncategorized";

				_defaultOptionCategory = value; 
			}
		}


		/// <summary>
		/// Get or set to group options by their categories
		/// <seealso cref="DefaultOptionCategory"/>
		/// </summary>
		public bool GroupOptionsByCategory 
		{
			get { return _groupOptionsByCategory; }
			set { _groupOptionsByCategory = value; }
		}
		#endregion Properties

		#region Printing methods
		/// <summary>
		/// Convenience method for transforming the XML using a custom XSLT
		/// </summary>
		/// <param name="writer">TextWriter to write the output to</param>
		/// <param name="xsltSource">Xslt content to use to transform with</param>
		/// <param name="arguments">Xslt arguments to pass to the <see cref="XslTransform"/></param>
		public void Transform(TextWriter writer, XmlReader xsltSource, XsltArgumentList arguments)
		{
			XslTransform trans = new XslTransform();
			trans.Load(xsltSource, null, Assembly.GetExecutingAssembly().Evidence);
			trans.Transform(this.Usage, arguments, writer, null);
		}


		/// <summary>
		/// Convert the usage to HTML
		/// </summary>
		/// <param name="writer">TextWriter to write the output to</param>
		/// <param name="optStyle">The style to use when printing possible option names</param>
		/// <param name="cssStyleSheet">Stylesheet URI to apply to the HTML content</param>
		/// <param name="includeDefaultValues">True to include the default option values in the output</param>
		/// <param name="title">Title to use for the HTML page</param>
		public void ToHtml(TextWriter writer, OptStyle optStyle, Uri cssStyleSheet, bool includeDefaultValues, string title)
		{
			XsltArgumentList argList = new XsltArgumentList();
			XslTransform trans = new XslTransform();

			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
					   "CommandLine.OptParse.Xslt.Html.xslt"))
			{
				trans.Load(new XmlTextReader(stream), null, Assembly.GetExecutingAssembly().Evidence);
			}

			argList.AddParam("shortOptPrefix", string.Empty, (optStyle == OptStyle.Unix) ? "-" : "/");
			argList.AddParam("longOptPrefix", string.Empty, (optStyle == OptStyle.Unix) ? "--" : "/");
			argList.AddParam("includeDefaultValues", string.Empty, includeDefaultValues);
				
			if (cssStyleSheet != null)
				argList.AddParam("cssStyleSheet", string.Empty, cssStyleSheet.AbsoluteUri);
				
			argList.AddParam("title", string.Empty, title);

			trans.Transform(this.Usage, argList, writer, null);
		}


		/// <summary>
		/// Convert the usage to Text
		/// </summary>
		/// <param name="writer">TextWriter to write the output to</param>
		/// <param name="optStyle">The style to use when printing possible option names</param>
		/// <param name="includeDefaultValues">True to include the default option values in the output</param>
		public void ToText(TextWriter writer, OptStyle optStyle, bool includeDefaultValues)
		{
			ToText(writer, optStyle, includeDefaultValues, -1);
		}


		/// <summary>
		/// Convert the usage to Text
		/// </summary>
		/// <remarks>if <paramref name="maxColumns"/> is -1, the width of the console will be used
		/// on windows machines, and the text will not be wrapped on non-windows machines</remarks>
		/// <param name="writer">TextWriter to write the output to</param>
		/// <param name="optStyle">The style to use when printing possible option names</param>
		/// <param name="includeDefaultValues">True to include the default option values in the output</param>
		/// <param name="maxColumns">Wrap text at the given column (attempts to wrap at '-' or ' ')</param>
		public void ToText(TextWriter writer, OptStyle optStyle, bool includeDefaultValues, int maxColumns)
		{
			XsltArgumentList argList = new XsltArgumentList();
			XslTransform     trans = new XslTransform();

			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
					   "CommandLine.OptParse.Xslt.Text.xslt"))
			{
				trans.Load(new XmlTextReader(stream), null, Assembly.GetExecutingAssembly().Evidence);
			}

			argList.AddParam("shortOptPrefix", string.Empty, (optStyle == OptStyle.Unix) ? "-" : "/");
			argList.AddParam("longOptPrefix", string.Empty, (optStyle == OptStyle.Unix) ? "--" : "/");
			argList.AddParam("includeDefaultValues", string.Empty, includeDefaultValues);
			argList.AddParam("newline", string.Empty, Environment.NewLine);
			argList.AddExtensionObject("extObj", new TextTransformHelper(maxColumns));

			if (maxColumns <= 0)
				maxColumns = -1;

			argList.AddParam("maxColumns", string.Empty, maxColumns);

			trans.Transform(this.Usage, argList, writer, null);
		}
		#endregion Printing methods

		#region Usage building functions
		/// <summary>
		/// Begin a new section tag
		/// </summary>
		/// <remarks>
		/// May only be under a "usage" or "section" tag. <see cref="EndSection"/> must be called
		/// to close this tag
		/// </remarks>
		/// <param name="header">Name of the section</param>
		public void BeginSection(string header)
		{
			ValidateCurrentNode("section", "usage");
			_currentNode = _currentNode.AppendChild(_usage.CreateElement("section"));
			_currentNode.Attributes.Append(_usage.CreateAttribute("name")).Value = header;
		}


		/// <summary>
		/// Close the section
		/// </summary>
		/// <remarks>Section tag must be active</remarks>
		public void EndSection()
		{
			ValidateCurrentNode("section");
			_currentNode = _currentNode.ParentNode;
		}


		/// <summary>
		/// Add a paragraph to the usage
		/// </summary>
		/// <remarks>
		/// May only be inside of "section" or "description" tags
		/// </remarks>
		/// <param name="body">The body of the paragraph</param>
		public void AddParagraph(string body)
		{
			ValidateCurrentNode("section", "description");
			_currentNode.AppendChild(_usage.CreateElement("para")).InnerText = body;
		}


		/// <summary>
		/// Add a paragraph to the usage
		/// </summary>
		/// <remarks>
		/// May only be inside of "section" or "description" tags. <see cref="EndParagraph"/> must be called
		/// to close this tag. Use <see cref="AddParagraphContent"/> to add text to the paragraph body.
		/// </remarks>
		public void BeginParagraph()
		{
			ValidateCurrentNode("section", "description");
			_currentNode.AppendChild(_usage.CreateElement("para"));
		}


		/// <summary>
		/// Add content to the current paragraph node
		/// </summary>
		/// <remarks>May only be called when a "para" tag is active.</remarks>
		/// <param name="text">Text to add</param>
		public void AddParagraphContent(string text)
		{
			ValidateCurrentNode("para");
			_currentNode.InnerText += text;
		}


		/// <summary>
		/// Close the active "para" tag
		/// </summary>
		/// <remarks>May only be called when a "para" tag is active.</remarks>
		public void EndParagraph()
		{
			ValidateCurrentNode("para");
			_currentNode = _currentNode.ParentNode;
		}


		/// <summary>
		/// Open a list tag
		/// </summary>
		/// <remarks>Sets the active tag to a new "list" tag. May only be called when
		/// one of the following tags are active: "section", "paragraph", "list", "description".
		/// <see cref="EndList"/> must be called to close this tag</remarks>
		/// <param name="type">The type of list</param>
		public void BeginList(ListType type)
		{
			ValidateCurrentNode("section", "paragraph", "list", "description");
			_currentNode = _currentNode.AppendChild(_usage.CreateElement("list"));
			_currentNode.Attributes.Append(_usage.CreateAttribute("type")).Value = type.ToString().ToLower();
		}

		
		/// <summary>
		/// Add a list item tag to the current list
		/// </summary>
		/// <remarks>May only be called when a "list" tag is active.</remarks>
		/// <param name="body">Body of the list item to add</param>
		public void AddListItem(string body)
		{
			ValidateCurrentNode("list");
			_currentNode.AppendChild(_usage.CreateElement("item")).InnerText = body;
		}


		/// <summary>
		/// Close the active "list" tag
		/// </summary>
		/// <remarks>May only be called when a "list" tag is active.</remarks>
		public void EndList()
		{
			ValidateCurrentNode("list");
			_currentNode = _currentNode.ParentNode;
		}

		
		/// <summary>
		/// Begin an active "options" tag
		/// <seealso cref="GroupOptionsByCategory"/>
		/// <seealso cref="DefaultOptionCategory"/>
		/// </summary>
		/// <remarks>May only be called when a "header" tag is active.</remarks>
		public void BeginOptions()
		{
			ValidateCurrentNode("section");
			_currentNode = _currentNode.AppendChild(_usage.CreateElement("options"));
		}


		/// <summary>
		/// Close the active "options" tag
		/// </summary>
		/// <remarks>May only be called when a "options" tag is active.</remarks>
		public void EndOptions()
		{
			ValidateCurrentNode("options");
			_currentNode = _currentNode.ParentNode;
		}


		/// <summary>
		/// Method to start adding an option to the usage.
		/// <seealso cref="AddOption"/>
		/// </summary>
		/// <remarks>Leaves the "description" tag as the active tag. Allows paragraph
		/// and list content to be added to the description of an option beyond
		/// the normal description. Does not include the description body from
		/// the <paramref name="opt"/> (must be added manually). Call 
		/// <see cref="EndOption"/> to close this tag.</remarks>
		/// <param name="opt">Option to start</param>
		public void BeginOption(OptionDefinition opt)
		{
			ValidateCurrentNode("options", "category");

			if (_groupOptionsByCategory)
			{
				XmlNode node;
				string  cat = opt.Category;
				
				if (cat == null)
					cat = _defaultOptionCategory;

				node = _currentNode.SelectSingleNode("category[@name='" + cat + "']");
				if (node == null)
				{
					_currentNode = _currentNode.AppendChild(_usage.CreateElement("category"));
					_currentNode.Attributes.Append(_usage.CreateAttribute("name")).Value = cat;
				}
				else
					_currentNode = node;
			}
			
			_currentNode = _currentNode.AppendChild(_usage.CreateElement("option"));
			_currentNode.Attributes.Append(_usage.CreateAttribute("type")).Value = 
				EnumDescriptorReader.GetEnumFieldDescription(opt.Type);
			
			// names
			_currentNode = _currentNode.AppendChild(_usage.CreateElement("names"));
			foreach (char name in opt.ShortNames)
			{
				_currentNode = _currentNode.AppendChild(_usage.CreateElement("name"));
				_currentNode.Attributes.Append(_usage.CreateAttribute("value")).Value = name.ToString();
				_currentNode.Attributes.Append(_usage.CreateAttribute("type")).Value = "short";
				_currentNode = _currentNode.ParentNode;
			}
			foreach (string name in opt.LongNames)
			{
				_currentNode = _currentNode.AppendChild(_usage.CreateElement("name"));
				_currentNode.Attributes.Append(_usage.CreateAttribute("value")).Value = name;
				_currentNode.Attributes.Append(_usage.CreateAttribute("type")).Value = "long";
				_currentNode = _currentNode.ParentNode;
			}

			_currentNode = _currentNode.ParentNode;

			if (opt.ValueType != null)
			{
				if (typeof(ValueType).IsAssignableFrom(opt.ValueType) || opt.ValueType.Namespace == "System")
					_currentNode.AppendChild(_usage.CreateElement("valueType")).InnerText = opt.ValueType.Name;
				else
					_currentNode.AppendChild(_usage.CreateElement("valueType")).InnerText = opt.ValueType.ToString();
			}

			if (opt.DefaultValue != null)
				_currentNode.AppendChild(_usage.CreateElement("defaultValue")).InnerText = 
					opt.DefaultValue.ToString();

			_currentNode = _currentNode.AppendChild(_usage.CreateElement("description"));
		}


		/// <summary>
		/// Close the active "description" tag and "option" tags
		/// </summary>
		/// <remarks>May only be called when a "description" tag is active below an
		/// "option" tag.</remarks>
		public void EndOption()
		{
			ValidateCurrentNode("description");
			_currentNode = _currentNode.ParentNode;
			_currentNode = _currentNode.ParentNode;

			if (_currentNode.Name == "category")
				_currentNode = _currentNode.ParentNode;
		}


		/// <summary>
		/// Add multiple options to the output
		/// </summary>
		/// <remarks>Can only be called if the active tag is "section"</remarks>
		/// <param name="optionContainer">Container with all the options 
		/// for the program to document</param>
		public void AddOptions(IOptionContainer optionContainer)
		{
			BeginOptions();
			foreach (OptionDefinition opt in optionContainer.GetOptions())
				AddOption(opt);
			EndOptions();
		}


		/// <summary>
		/// Add multiple options to the output
		/// </summary>
		/// <remarks>Can only be called if the active tag is "section"</remarks>
		/// <param name="options">All the options for the program</param>
		public void AddOptions(OptionDefinition[] options)
		{
			BeginOptions();
			foreach (OptionDefinition opt in options)
				AddOption(opt);
			EndOptions();
		}


		/// <summary>
		/// Adds an option to the output
		/// </summary>
		/// <remarks>Like <see cref="BeginOption"/>, but uses the desription from the
		/// option as the body of the "description" tag. Does not open a new active tag</remarks>
		/// <param name="opt">Option to add</param>
		public void AddOption(OptionDefinition opt)
		{
			BeginOption(opt);
			_currentNode.InnerText = opt.Description;
			EndOption();
		}

		
		/// <summary>
		/// Begin an active "arguments" tag
		/// </summary>
		/// <remarks>May only be called when a "header" tag is active.</remarks>
		public void BeginArguments()
		{
			ValidateCurrentNode("section");
			_currentNode = _currentNode.AppendChild(_usage.CreateElement("arguments"));
		}


		/// <summary>
		/// Close the active "arguments" tag
		/// </summary>
		/// <remarks>May only be called when an "arguments" tag is active.</remarks>
		public void EndArguments()
		{
			ValidateCurrentNode("arguments");
			_currentNode = _currentNode.ParentNode;
		}


		
		/// <summary>
		/// Add an argument overview to the usage
		/// </summary>
		/// <remarks>Can only be called when a "section" tag is active. Does not leave a new
		/// active tag open.</remarks>
		/// <param name="name">Short name description of the argument</param>
		/// <param name="description">Description of the argument</param>
		/// <param name="type">Supported data type for the argument (<c>typeof(string)</c> is
		/// usually the best choice).</param>
		/// <param name="optional">True if the argument should be marked as optional</param>
		public void AddArgument(string name, string description, Type type, bool optional)
		{
			BeginArgument(name, type, optional);
			_currentNode.InnerText = description;
			EndArgument();
		}


		/// <summary>
		/// Add an argument overview to the usage
		/// </summary>
		/// <remarks>Can only be called when an "arguments" tag is active. Leaves the "description" tag
		/// active under the "argument" tag.</remarks>
		/// <param name="name">Short name description of the argument</param>
		/// <param name="type">Supported data type for the argument (<c>typeof(string)</c> is
		/// usually the best choice).</param>
		/// <param name="optional">True if the argument should be marked as optional</param>
		public void BeginArgument(string name, Type type, bool optional)
		{
			ValidateCurrentNode("arguments");
			
			_currentNode = _currentNode.AppendChild(_usage.CreateElement("argument"));

			if (typeof(ValueType).IsAssignableFrom(type) || type.Namespace == "System")
				_currentNode.Attributes.Append(_usage.CreateAttribute("type")).Value = type.Name;
			else
				_currentNode.Attributes.Append(_usage.CreateAttribute("type")).Value = type.ToString();
			
			_currentNode.Attributes.Append(_usage.CreateAttribute("name")).Value = name;
			_currentNode.Attributes.Append(_usage.CreateAttribute("optional")).Value = optional.ToString();
			_currentNode = _currentNode.AppendChild(_usage.CreateElement("description"));
		}


		/// <summary>
		/// Close the active "description" tag and "argument" tags
		/// </summary>
		/// <remarks>May only be called when a "description" tag is active below an
		/// "argument" tag.</remarks>
		public void EndArgument()
		{
			EndOption();
		}
		#endregion Usage building functions

		#region Validation methods
		void ValidateCurrentNode(params string[] expectedTypes)
		{
			foreach (string type in expectedTypes)
				if (_currentNode.Name == type)
					return;

			throw new InvalidOperationException("Invaild command, must be inside of one of: " + 
				string.Join(", ", expectedTypes));
		}
		#endregion Validation methods

		#region TextTransformHelper class
		/// <summary>
		/// Class to help with Text transformation
		/// </summary>
		private class TextTransformHelper
		{
			#region Members
			private readonly char[] _splitChars = new char[] { '-', ' ' };

			private int _maxColumns;
			#endregion Members

			#region Constructor
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="maxColumns">Column to wrap at</param>
			public TextTransformHelper(int maxColumns)
			{
				_maxColumns = maxColumns;

				if (_maxColumns == -1)
				{
					// try to determine console width
					string os = Environment.GetEnvironmentVariable("OS");

					if (os != null && os.StartsWith("Win"))
					{
						ConsoleUtils.ConsoleHelper ch = new ConsoleUtils.ConsoleHelper();
						_maxColumns = ch.GetScreenInfo().Size.X;
					}
				}
			}
			#endregion Constructor

			#region Functions
			/// <summary>
			/// Create a string of spaces
			/// </summary>
			/// <param name="length">Length of the string to create</param>
			/// <returns>The string</returns>
			public string CreateSpaces(int length)
			{
				return new string(' ', length);
			}


			/// <summary>
			/// Formats text, adding indent and wrapping lines
			/// </summary>
			/// <param name="text">Text to format</param>
			/// <param name="indent">Indent to add to each line</param>
			/// <param name="hangingIndent">Additional indent to add to lines after the first</param>
			/// <returns>Formatted text</returns>
			public string FormatText(string text, int indent, int hangingIndent)
			{
				StringBuilder output = new StringBuilder();
				bool          first = true;
				int           firstColumns = _maxColumns - indent - 1;
				int           subseqColumns = _maxColumns - indent - hangingIndent - 1;
				string        handingIndentStr = new string(' ', indent + hangingIndent);
				string        indentStr = new string(' ', indent);
				string[]      lines;
				
				text = text.Replace("\r\n", "\n");
				text = text.Replace("\r", "\n");

				lines = text.Split('\n');
				foreach (string line in lines)
				{
					if (!first)
						output.Append(Environment.NewLine);

					if (_maxColumns == -1)
					{
						output.Append((first) ? indentStr : handingIndentStr);
						output.Append(line);
						output.Append(Environment.NewLine);
						first = false;
						continue;
					}

					for (int start = 0; start < line.Length;)
					{
						int    cols = (first) ? firstColumns : subseqColumns;
						int    len;
						string istr = (first) ? indentStr : handingIndentStr;

						len = Math.Min(line.Length - start, cols);

						if (len < cols)
						{
							output.Append(istr);
							output.Append(line.Substring(start));
							first = false;
							break;
						}
						else
						{
							int splitLoc = line.LastIndexOfAny(_splitChars, start + len - 1, len);

							if (splitLoc > 0)
							{
								output.Append(istr);
								output.Append(line.Substring(start, splitLoc - start));
								output.Append(Environment.NewLine);

								start = splitLoc;
								if (line[start] == ' ')
									start++;
							}
							else
							{
								output.Append(istr);
								output.Append(line.Substring(start, cols));
								output.Append(Environment.NewLine);
								start += cols;
							}
						}
						first = false;
					}
				}

				output.Append(Environment.NewLine);

				return output.ToString();
			}
			#endregion Functions
		}
		#endregion TextTransformHelper class
	}
	#endregion UsageBuilder class
}
