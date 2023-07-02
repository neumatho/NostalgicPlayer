/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.Ancient.Common
{
	/// <summary>
	/// Different helper methods
	/// </summary>
	internal static class Common
	{
		private static readonly uint8_t[] rotateNibble =
		{
			0x0, 0x8, 0x4, 0xc,
			0x2, 0xa, 0x6, 0xe,
			0x1, 0x9, 0x5, 0xd,
			0x3, 0xb, 0x7, 0xf
		};

		/********************************************************************/
		/// <summary>
		/// Convert a 4 letter string to a 32-bit number
		/// </summary>
		/********************************************************************/
		public static uint32_t FourCC(string cc)
		{
			return (uint32_t)((cc[0] << 24) | (cc[1] << 16) | (cc[2] << 8) | cc[3]);
		}



		/********************************************************************/
		/// <summary>
		/// Rotate the bits in the value given to the right count number of
		/// times
		/// </summary>
		/********************************************************************/
		public static uint32_t RotateBits(uint32_t value, uint32_t count)
		{
			uint32_t ret = 0;

			for (uint32_t i = 0; i < count; i += 4)
			{
				ret = (ret << 4) | rotateNibble[value & 0xf];
				value >>= 4;
			}

			ret >>= (int)((4 - count) & 3);

			return ret;
		}
	}
}
