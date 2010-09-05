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
using System.IO;
using System.Runtime.Serialization;

namespace CommandLine.OptParse
{
	#region ParseException
	/// <summary>
	/// Exception thrown if the parser encounters an error
	/// </summary>
	public class ParseException : ApplicationException
	{
		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="errMsg">Description of the error that occurred while parsing</param>
		public ParseException(string errMsg) : base(errMsg) {}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="rootCause">Exception that was thrown during parsing</param>
		public ParseException(Exception rootCause) : base(rootCause.Message, rootCause) {}

        
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="errMsg">Description of the error that occurred while parsing</param>
		/// <param name="rootCause">Exception that was thrown during parsing</param>
		public ParseException(string errMsg, Exception rootCause) : base(errMsg, rootCause) {}


		/// <summary>
		/// Constructor for serialization. See 
		/// <see cref="ApplicationException(SerializationInfo, StreamingContext)"/>
		/// </summary>
		protected ParseException(SerializationInfo info, StreamingContext ctx) : 
			base(info, ctx) {}
		#endregion Constructors
	}
	#endregion ParseException

	#region InvalidValueException
	/// <summary>
	/// Parsing exception that is thrown if a value is given that is not of the expected type
	/// </summary>
	public class InvalidValueException : ParseException
	{
		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="errMsg">Description of the error that occurred while parsing</param>
		public InvalidValueException(string errMsg) : base(errMsg) {}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="rootCause">Exception that was thrown during parsing</param>
		public InvalidValueException(Exception rootCause) : base(rootCause.Message, rootCause) {}

        
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="errMsg">Description of the error that occurred while parsing</param>
		/// <param name="rootCause">Exception that was thrown during parsing</param>
		public InvalidValueException(string errMsg, Exception rootCause) : base(errMsg, rootCause) {}


		/// <summary>
		/// Constructor for serialization. See 
		/// <see cref="ApplicationException(SerializationInfo, StreamingContext)"/>
		/// </summary>
		protected InvalidValueException(SerializationInfo info, StreamingContext ctx) : 
			base(info, ctx) {}
		#endregion Constructors
	}
	#endregion InvalidValueException
}
