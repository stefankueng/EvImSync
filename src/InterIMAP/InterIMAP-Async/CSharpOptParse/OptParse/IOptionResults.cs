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
	/// Interface for interacting with the results of parsing the options
	/// </summary>
	/// <remarks>
	/// This interface allows for storing option results in different formats. The 
	/// <see cref="Parser"/> class uses this interface.
	/// <para>
	/// The implementation of this interface should be able to index the option definitions
	/// in both a case sensitive and case insensitive format. The parser's settings will
	/// determine the case sesitivity that will be used.
	/// </para>
	/// </remarks>
	public interface IOptionResults : IOptionContainer
	{
		/// <summary>
		/// Get or set the result of an option by its definition
		/// </summary>
		OptionResult this[OptionDefinition def] { get; set; }


		/// <summary>
		/// Get the ID of the option for the provided short name
		/// </summary>
		/// <param name="optionName">The short name of the option to get the ID of</param>
		/// <param name="caseSensitive">If the option name's case should be considered</param>
		/// <returns>The definition of the option or null if there is no option defined for the
		/// given name</returns>
		OptionDefinition GetOptionDefinition(char optionName, bool caseSensitive);


		/// <summary>
		/// Get the ID of the option for the provided name
		/// </summary>
		/// <param name="optionName">The long name of the option to get the ID of</param>
		/// <param name="caseSensitive">If the option name's case should be considered</param>
		/// <returns>The definition of the option or null if there is no option defined for the
		/// given name</returns>
		OptionDefinition GetOptionDefinition(string optionName, bool caseSensitive);
	}
}
