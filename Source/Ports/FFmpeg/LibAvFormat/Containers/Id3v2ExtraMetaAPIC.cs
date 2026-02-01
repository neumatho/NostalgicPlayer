/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class Id3v2ExtraMetaAPIC : IExtraMetadata
	{
		/// <summary>
		/// 
		/// </summary>
		public AvBufferRef Buf;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Type;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Description;

		/// <summary>
		/// 
		/// </summary>
		public AvCodecId Id;
	}
}
