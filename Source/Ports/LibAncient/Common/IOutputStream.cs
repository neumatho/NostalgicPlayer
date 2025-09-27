/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibAncient.Common
{
	/// <summary>
	/// Different output streams implements this interface
	/// </summary>
	internal interface IOutputStream
	{
		/// <summary>
		/// Indicate if the buffer has been filled or not
		/// </summary>
		bool Eof { get; }

		/// <summary>
		/// Return the current position
		/// </summary>
		size_t GetOffset();

		/// <summary>
		/// Write a single byte to the output
		/// </summary>
		void WriteByte(uint8_t value);

		/// <summary>
		/// Copy a block of already written data in the buffer
		/// </summary>
		uint8_t Copy(size_t distance, size_t count);
	}
}
