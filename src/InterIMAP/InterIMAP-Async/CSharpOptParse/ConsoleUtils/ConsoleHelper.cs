/* This file is part of the CSharpOptParse .NET C# library
 *
 * The library is hosted at http://csharpoptparse.sf.net
 */
using System;
using System.Runtime.InteropServices;

namespace CommandLine.ConsoleUtils
{
	#region Structs
	/// <summary>
	/// Position in the console
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct ConsolePoint
	{
		/// <summary>
		/// X coordinate
		/// </summary>
		public short X;

		/// <summary>
		/// Y coordinate
		/// </summary>
		public short Y;

		/// <summary>
		/// Constructor
		/// </summary>
		public ConsolePoint(short x, short y)
		{
			X = x;
			Y = y;
		}
	}


	/// <summary>
	/// Rectangle in the console
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct ConsoleRect
	{
		/// <summary>
		/// Left edge
		/// </summary>
		public short Left;

		/// <summary>
		/// Top edge
		/// </summary>
		public short Top;

		/// <summary>
		/// Right edge
		/// </summary>
		public short Right;

		/// <summary>
		/// Bottom edge
		/// </summary>
		public short Bottom;
	}


	/// <summary>
	/// Information on the current screen buffer
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct ConsoleScreenBufferInfo
	{
		/// <summary>
		/// Size of the screen buffer
		/// </summary>
		public ConsolePoint Size;

		/// <summary>
		/// Position of the cursor on the screen
		/// </summary>
		public ConsolePoint CursorPosition;

		/// <summary>
		/// Attributes
		/// </summary>
		public int          Attributes;

		/// <summary>
		/// Bounds of the window
		/// </summary>
		public ConsoleRect  Window;

		/// <summary>
		/// Maximum window size
		/// </summary>
		public ConsolePoint MaximumWindowSize;
	}
	#endregion Structs

	#region ConsoleHelper class
	/// <summary>
	/// Class to help with more advanced console functions
	/// </summary>
	public class ConsoleHelper
	{
		private const int _stdOutputHandle = -11;
		private int _consoleHandle;

		[DllImport("kernel32.dll", 
			 EntryPoint="GetStdHandle", 
			 SetLastError=true, 
			 CharSet=CharSet.Auto, 
			 CallingConvention=CallingConvention.StdCall)]
		private static extern int GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll", 
			 EntryPoint="GetConsoleScreenBufferInfo", 
			 SetLastError=true,
			 CharSet=CharSet.Auto, 
			 CallingConvention=CallingConvention.StdCall)]
		private static extern int GetConsoleScreenBufferInfo(int hConsoleOutput, 
			out ConsoleScreenBufferInfo lpConsoleScreenBufferInfo);

		[DllImport("kernel32.dll", 
			 EntryPoint="SetConsoleCursorPosition", 
			 SetLastError=true, 
			 CharSet=CharSet.Auto, 
			 CallingConvention=CallingConvention.StdCall)]
		private static extern int SetConsoleCursorPosition(int hConsoleOutput, 
			ConsolePoint dwCursorPosition);

		/// <summary>Cosntructor</summary>
		public ConsoleHelper()
		{
			_consoleHandle = GetStdHandle(_stdOutputHandle);
		}


		/// <summary>
		/// Set the cursor position
		/// </summary>
		public void SetCursorPos(short x, short y)
		{
			SetConsoleCursorPosition(_consoleHandle, new ConsolePoint(x, y));
		}


		/// <summary>
		/// Get the current screen information
		/// </summary>
		public ConsoleScreenBufferInfo GetScreenInfo()
		{
			ConsoleScreenBufferInfo res;
			GetConsoleScreenBufferInfo(_consoleHandle, out res);
			return res;
		}


		/// <summary>
		/// Get the cursor position
		/// </summary>
		public ConsolePoint GetCursorPos()
		{
			ConsoleScreenBufferInfo res;
			GetConsoleScreenBufferInfo(_consoleHandle, out res);
			return res.CursorPosition;
		}
	}
	#endregion ConsoleHelper class
}
