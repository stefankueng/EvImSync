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
	#region WarningEventHandler delegate
	/// <summary>
	/// Delegate to receive warning events from a parser
	/// </summary>
	public delegate void WarningEventHandler(Parser sender, OptionWarningEventArgs e);
	#endregion WarningEventHandler delegate
}
