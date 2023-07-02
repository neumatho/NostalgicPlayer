/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Common
{
	/// <summary>
	/// Helper class for delta decoding
	/// </summary>
	internal static class DltaDecode
	{
		/********************************************************************/
		/// <summary>
		/// Delta decoder
		/// </summary>
		/********************************************************************/
		public static void Decode(Buffer bufferDest, Buffer bufferSrc, size_t offset, size_t size)
		{
			uint8_t ctr = 0;
			for (size_t i = 0; i < size; i++)
			{
				ctr += bufferSrc[offset + i];
				bufferDest[offset + i] = ctr;
			}
		}
	}
}
