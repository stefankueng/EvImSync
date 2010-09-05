using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace CommandLine.OptParse
{
	/// <summary>
	/// A simple, default implementation of <see cref="IProgUsageInfo"/> that
	/// is fairly easy to use
	/// </summary>
	[Obsolete("Use UsageBuilder instead")]
	public class DefaultUsageInfo : IProgUsageInfo
	{
		#region Members
		private NameValueCollection _info = new NameValueCollection();
		#endregion Members
 
		#region Constructors
		/// <summary>
		/// Constructor to make argument description construction easier
		/// </summary>
		/// <remarks>
		/// The <c>argumentDescriptions</c> array should be an even length. The
		/// event indexes (0, 2, 4, etc.) should be the argument names, and
		/// the odd indexes are the descriptions of the names (1, 3, etc. where 1 is
		/// the description of 0, 3 of 2, etc.).
		/// <para>Example:
		/// <code>
		/// new DefaultUsageInfo("HelloWorld.exe", "Says hello world with the arguments",
		/// "Additional text", "Text to print after saying hello world");
		/// </code>
		/// </para>
		/// </remarks>
		/// <param name="programName">The name of the program (something.exe)</param>
		/// <param name="description">Description of the program</param>
		/// <param name="upperCase">True to use all-upper case headers, or 
		/// false for title cased headers</param>
		/// <param name="argumentDescription">list of descriptions in the format of
		/// { "Argument", "Description" [, "Argument", "Description"] ...}</param>
		public DefaultUsageInfo(string programName, string description, bool upperCase,
			params string[] argumentDescription)
		{
			if (argumentDescription == null || argumentDescription.Length == 0)
				Init(programName, description, upperCase, null);
			else
			{
				NameValueCollection argDesc = new NameValueCollection();

				for (int i = 0; i < argumentDescription.Length; i += 2)
				{
					if (i + 1 == argumentDescription.Length)
						argDesc.Add(argumentDescription[i], string.Empty);
					else
						argDesc.Add(argumentDescription[i], argumentDescription[i + 1]);
				}

				Init(programName, description, upperCase, argDesc);
			}
		}
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="programName">The name of the program (something.exe)</param>
		/// <param name="description">Description of the program</param>
		/// <param name="upperCase">True to use all-upper case headers, or 
		/// false for title cased headers</param>
		/// <param name="argumentDescription">Argument name, argument description
		/// pairs to describe the arguments the program takes or null if none</param>
		public DefaultUsageInfo(string programName, string description, bool upperCase,
			NameValueCollection argumentDescription)
		{
			Init(programName, description, upperCase, argumentDescription);
		}

		#endregion Constructors
		
		#region Private methods
		void Init(string programName, string description, bool upperCase,
			NameValueCollection argumentDescription)
		{
			_info.Add(upperCase ? "NAME" : "Name", programName);
			_info.Add(upperCase ? "DESCRIPTION" : "Description", description);

			if (argumentDescription == null)
				_info.Add(upperCase ? "SYNOPSIS" : "Synopsis", 
					string.Format("{0} [OPTIONS]", programName));
			else
				_info.Add(upperCase ? "SYNOPSIS" : "Synopsis", 
					string.Format("{0} [Options] [Arguments]", programName));

			_info.Add(upperCase ? "OPTIONS" : "Options", string.Empty);
            
			if (argumentDescription != null)
			{
				StringBuilder desc = new StringBuilder();

				for (int i = 0; i < argumentDescription.Count; i++)
				{
					desc.Append(argumentDescription.GetKey(i));
					desc.Append(Environment.NewLine);
					desc.Append("\t");
					desc.Append(argumentDescription[i]);
					desc.Append(Environment.NewLine);
				}

				_info.Add(upperCase ? "ARGUMENTS" : "Arguments", desc.ToString());
			}
		}
		#endregion Private methods

		#region IProgUsageInfo Members
		/// <summary>
		/// Get the headers for the usage
		/// </summary>
		public string[] Headers
		{
			get { return _info.AllKeys; }
		}


		/// <summary>
		/// Get if the given header should hold the option description
		/// </summary>
		/// <param name="header">The header</param>
		/// <returns>True if the options header</returns>
		public bool IsOptionHeader(string header)
		{
			return (header == "OPTIONS" || header == "Options");
		}


		/// <summary>
		/// Get the contents of the given header
		/// </summary>
		public string GetContents(string header)
		{
			return _info[header];
		}
		#endregion
	}
}
