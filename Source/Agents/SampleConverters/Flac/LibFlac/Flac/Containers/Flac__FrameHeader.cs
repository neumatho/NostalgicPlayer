/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// FLAC frame header structure
	/// </summary>
	internal class Flac__FrameHeader
	{
		/// <summary>
		/// The number of samples per subframe
		/// </summary>
		public uint32_t BlockSize;

		/// <summary>
		/// The sample rate in Hz
		/// </summary>
		public uint32_t Sample_Rate;

		/// <summary>
		/// The number of channels (== number of subframes)
		/// </summary>
		public uint32_t Channels;

		/// <summary>
		/// The channel assignment for the frame
		/// </summary>
		public Flac__ChannelAssignment Channel_Assignment;

		/// <summary>
		/// The sample resolution
		/// </summary>
		public uint32_t Bits_Per_Sample;

		/// <summary>
		/// The numbering scheme used for the frame. As a convenience, the
		/// decoder will always convert a frame number to a sample number because
		/// the rules are complex
		/// </summary>
		public Flac__FrameNumberType Number_Type;

		/// <summary>
		/// The frame number or sample number of first sample in frame;
		/// use the Number_Type value to determine which to use
		/// </summary>
		public Flac__uint32 Frame_Number;
		public Flac__uint64 Sample_Number;

		/// <summary>
		/// CRC-8 (polynomial = x^8 + x^2 + x^1 + x^0, initialized with 0)
		/// of the raw frame header bytes, meaning everything before the CRC byte
		/// including the sync code
		/// </summary>
		public Flac__uint8 Crc;
	}
}
