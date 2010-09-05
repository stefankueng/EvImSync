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
	/// Interface that describes an object that contains option definitions
	/// </summary>
	public interface IOptionContainer
	{
		/// <summary>
		/// Get all of the options
		/// </summary>
		/// <returns>Array of option definitions</returns>
		OptionDefinition[] GetOptions();
	}
}
