/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// C like console methods
	/// </summary>
	public static class CConsole
	{
		/********************************************************************/
		/// <summary>
		/// Do printf style formatting to the console
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void printf(string fmt, params object[] args)
		{
			printf(Console.Out, fmt.ToCharPointer(), args);
		}



		/********************************************************************/
		/// <summary>
		/// Do printf style formatting to the console
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void printf(CPointer<char> fmt, params object[] args)
		{
			printf(Console.Out, fmt, args);
		}



		/********************************************************************/
		/// <summary>
		/// Do printf style formatting to the console
		/// </summary>
		/********************************************************************/
		public static void printf(TextWriter stream, string fmt, params object[] args)
		{
			printf(stream, fmt.ToCharPointer(), args);
		}



		/********************************************************************/
		/// <summary>
		/// Do printf style formatting to the console
		/// </summary>
		/********************************************************************/
		public static void printf(TextWriter stream, CPointer<char> fmt, params object[] args)
		{
			CPointer<char> buf = new CPointer<char>(200);

			c_int r = CString.snprintf(buf, (size_t)buf.Length, fmt, args);
			if (r > buf.Length)
			{
				buf = new CPointer<char>(r + 1);
				CString.snprintf(buf, (size_t)buf.Length, fmt, args);
			}

			stream.Write(buf.ToString());
		}
	}
}
