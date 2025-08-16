/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using System.Text;

namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// Extension methods for strings
	/// </summary>
	public static class CStringExtension
	{
		/********************************************************************/
		/// <summary>
		/// Convert the string to a C pointer
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<uint8_t> ToPointer(this string str)
		{
			return str.ToPointer(Encoding.Latin1);
		}



		/********************************************************************/
		/// <summary>
		/// Convert the string to a C pointer using the given encoder
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<uint8_t> ToPointer(this string str, Encoding encoder)
		{
			int byteCount = encoder.GetByteCount(str);
			uint8_t[] bytesWithNull = new uint8_t[byteCount + 1];

			encoder.GetBytes(str, 0, str.Length, bytesWithNull, 0);
			bytesWithNull[byteCount] = 0;	// Null-terminate the string

			return new CPointer<uint8_t>(bytesWithNull);
		}
	}
}
