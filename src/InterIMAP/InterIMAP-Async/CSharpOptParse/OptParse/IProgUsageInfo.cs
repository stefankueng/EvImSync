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

namespace CommandLine.OptParse
{
	/// <summary>
	/// Interface for the parser to use to be able to print usage information
	/// for the given program.
	/// </summary>
	/// <remarks>
	/// The usage structure is similar to the Perl POD structure, of headers with
	/// description blocks. The implementation however is not as robust.
	/// </remarks>
	[Obsolete("Use UsageBuilder instead")]
	public interface IProgUsageInfo
	{
		/// <summary>
		/// Get a list of all the "topic" headers for the usage
		/// </summary>
		/// <remarks>
		/// Each header is similar to a header in a Unix man page. The header is a title
		/// block for the contents to follow. The usage information is broken up
		/// into sections, each started with a header. Typical headers are 
		/// "Description", "Synopsis", "Options", "Arguments"
		/// </remarks>
		string[] Headers { get; }
		
		/// <summary>
		/// Check if a given header should contain the usage of the options
		/// </summary>
		/// <param name="header">The header to check</param>
		/// <returns>True if the contents of this header is the options description
		/// </returns>
		bool IsOptionHeader(string header);
		
		/// <summary>
		/// Get the contents of a non-option header
		/// </summary>
		/// <param name="header">The header to get the contents for</param>
		/// <returns>The contents for the header</returns>
        string GetContents(string header);
	}
}
