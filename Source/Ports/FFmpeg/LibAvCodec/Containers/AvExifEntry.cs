/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvExifEntry
	{
		/// <summary>
		/// 
		/// </summary>
		public uint16_t Id;

		/// <summary>
		/// 
		/// </summary>
		public AvTiffDataType Type;

		/// <summary>
		/// 
		/// </summary>
		public uint32_t Count;

		/// <summary>
		/// These are for IFD-style MakerNote
		/// entries which occur after a fixed
		/// offset rather than at the start of
		/// the entry. The ifd_lead field contains
		/// the leading bytes which typically
		/// identify the type of MakerNote
		/// </summary>
		public uint32_t Ifd_Offset;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<uint8_t> Ifd_Lead;

		/// <summary>
		/// An array of entries of size count
		/// Unless it's an IFD, in which case
		/// it's not an array and count = 1
		/// </summary>
		public (
			CPointer<int64_t> SInt,
			CPointer<uint64_t> UInt,
			CPointer<c_double> Dbl,
			CPointer<char> Str,
			CPointer<uint8_t> UBytes,
			CPointer<int8_t> SBytes,
			CPointer<AvRational> Rat,
			AvExifMetadata Ifd
		) Value;
	}
}
