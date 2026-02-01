/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class Id3v2ExtraMetaCHAP : IExtraMetadata
	{
		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Element_Id;

		/// <summary>
		/// 
		/// </summary>
		public uint32_t Start;

		/// <summary>
		/// 
		/// </summary>
		public uint32_t End;

		/// <summary>
		/// 
		/// </summary>
		public AvDictionary Meta;
	}
}
