/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Common
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
		/// Consume the given number of bytes
		/// </summary>
		Span<uint8_t> Consume(size_t bytes, uint8_t[] buffer);
	}
}
