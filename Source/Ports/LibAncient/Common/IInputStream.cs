/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibAncient.Common
{
	/// <summary>
	/// Different input streams implements this interface
	/// </summary>
	internal interface IInputStream
	{
		/// <summary>
		/// Return the current position
		/// </summary>
		size_t GetOffset();

		/// <summary>
		/// Read a single byte
		/// </summary>
		uint8_t ReadByte();

		/// <summary>
		/// Read a 16-bit integer in big-endian format
		/// </summary>
		uint16_t ReadBE16();

		/// <summary>
		/// Read a 32-bit integer in big-endian format
		/// </summary>
		uint32_t ReadBE32();

		/// <summary>
		/// Read a 16-bit integer in little-endian format
		/// </summary>
		uint16_t ReadLE16();

		/// <summary>
		/// Read a 32-bit integer in little-endian format
		/// </summary>
		uint32_t ReadLE32();
	}
}
