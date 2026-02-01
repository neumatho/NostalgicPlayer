/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// This structure stores auxiliary information for decoding, presenting, or
	/// otherwise processing the coded stream. It is typically exported by demuxers
	/// and encoders and can be fed to decoders and muxers either in a per packet
	/// basis, or as global side data (applying to the entire coded stream).
	///
	/// Global side data is handled as follows:
	///  - During demuxing, it may be exported through
	///    AVCodecParameters.coded_side_data "AVStream's codec parameters", which can
	///    then be passed as input to decoders through the
	///    AVCodecContext.coded_side_data "decoder context's side data", for
	///    initialization.
	///  - For muxing, it can be fed through AVCodecParameters.coded_side_data
	///    "AVStream's codec parameters", typically  the output of encoders through
	///    the AVCodecContext.coded_side_data "encoder context's side data", for
	///    initialization.
	///
	/// Packet specific side data is handled as follows:
	///  - During demuxing, it may be exported through AVPacket.side_data
	///    "AVPacket's side data", which can then be passed as input to decoders.
	///  - For muxing, it can be fed through AVPacket.side_data "AVPacket's
	///    side data", typically the output of encoders.
	///
	/// Different modules may accept or export different types of side data
	/// depending on media type and codec. Refer to AVPacketSideDataType for a
	/// list of defined types and where they may be found or used
	/// </summary>
	public class AvPacketSideData
	{
		/// <summary>
		/// 
		/// </summary>
		public IDataContext Data;

		/// <summary>
		/// 
		/// </summary>
		public AvPacketSideDataType Type;
	}
}
