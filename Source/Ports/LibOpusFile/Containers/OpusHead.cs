/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpusFile.Containers
{
	/// <summary>
	/// Header information
	/// </summary>
	public class OpusHead
	{
		/// <summary>
		/// The Ogg Opus format version, in the range 0...255.
		/// The top 4 bits represent a "major" version, and the bottom four
		/// bits represent backwards-compatible "minor" revisions.
		/// The current specification describes version 1.
		/// This library will recognize versions up through 15 as backwards
		/// compatible with the current specification.
		/// An earlier draft of the specification described a version 0, but
		/// the only difference between version 1 and version 0 is that
		/// version 0 did not specify the semantics for handling the version
		/// field
		/// </summary>
		public c_int Version;

		/// <summary>
		/// The number of channels, in the range 1...255
		/// </summary>
		public c_int Channel_Count;

		/// <summary>
		/// The number of samples that should be discarded from the beginning
		/// of the stream
		/// </summary>
		public c_uint Pre_Skip;

		/// <summary>
		/// The sampling rate of the original input.
		/// All Opus audio is coded at 48 kHz, and should also be decoded
		/// at 48 kHz for playback (unless the target hardware does not
		/// support this sampling rate).
		/// However, this field may be used to resample the audio back to
		/// the original sampling rate, for example, when saving the output
		/// to a file
		/// </summary>
		public opus_uint32 Input_Sample_Rate;

		/// <summary>
		/// The gain to apply to the decoded output, in dB, as a Q8 value in
		/// the range -32768...32767.
		/// The libopusfile API will automatically apply this gain to the
		/// decoded output before returning it, scaling it by
		/// pow(10,output_gain/(20.0*256)).
		/// You can adjust this behavior with op_set_gain_offset()
		/// </summary>
		public c_int Output_Gain;

		/// <summary>
		/// The channel mapping family, in the range 0...255.
		/// Channel mapping family 0 covers mono or stereo in a single
		/// stream. Channel mapping family 1 covers 1 to 8 channels in one
		/// or more streams, using the Vorbis speaker assignments.
		/// Channel mapping family 255 covers 1 to 255 channels in one or
		/// more streams, but without any defined speaker assignment
		/// </summary>
		public c_int Mapping_Family;

		/// <summary>
		/// The number of Opus streams in each Ogg packet, in the range
		/// 1...255
		/// </summary>
		public c_int Stream_Count;

		/// <summary>
		/// The number of coupled Opus streams in each Ogg packet, in the
		/// range 0...127.
		/// This must satisfy 0 &lt;= coupled_count &lt;= stream_count and
		/// coupled_count + stream_count &lt;= 255.
		/// The coupled streams appear first, before all uncoupled streams,
		/// in an Ogg Opus packet
		/// </summary>
		public c_int Coupled_Count;

		/// <summary>
		/// The mapping from coded stream channels to output channels.
		/// Let index=mapping[k] be the value for channel k.
		/// If index&lt;2*coupled_count, then it refers to the left channel
		/// from stream (index/2) if even, and the right channel from
		/// stream (index/2) if odd.
		/// Otherwise, it refers to the output of the uncoupled stream
		/// (index-coupled_count)
		/// </summary>
		public byte[] Mapping = new byte[Constants.Opus_Channel_Count_Max];
	}
}
