/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class Id3v2ExtraMetaGEOB : IExtraMetadata
	{
		/// <summary>
		/// 
		/// </summary>
		public uint32_t DataSize;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Mime_Type;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> File_Name;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Description;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Data;
	}
}
