/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using System.Text;

namespace Polycode.NostalgicPlayer.CKit
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
			return str.ToPointer(Encoding.ASCII);
		}



		/********************************************************************/
		/// <summary>
		/// Convert the string to a C pointer using the given encoder
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CPointer<uint8_t> ToPointer(this string str, Encoding encoder)
		{
			uint8_t[] bytes = encoder.GetBytes(str);

			return new CPointer<uint8_t>(bytes);
		}
	}
}
