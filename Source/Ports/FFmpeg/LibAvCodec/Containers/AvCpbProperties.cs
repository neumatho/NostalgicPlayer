/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// This structure describes the bitrate properties of an encoded bitstream. It
	/// roughly corresponds to a subset the VBV parameters for MPEG-2 or HRD
	/// parameters for H.264/HEVC
	/// </summary>
	public class AvCpbProperties : IDataContext
	{
		/// <summary>
		/// Maximum bitrate of the stream, in bits per second.
		/// Zero if unknown or unspecified
		/// </summary>
		public int64_t Max_BitRate;

		/// <summary>
		/// Minimum bitrate of the stream, in bits per second.
		/// Zero if unknown or unspecified
		/// </summary>
		public int64_t Min_BitRate;

		/// <summary>
		/// Average bitrate of the stream, in bits per second.
		/// Zero if unknown or unspecified
		/// </summary>
		public int64_t Avg_BitRate;

		/// <summary>
		/// The size of the buffer to which the ratecontrol is applied, in bits.
		/// Zero if unknown or unspecified
		/// </summary>
		public int64_t Buffer_Size;

		/// <summary>
		/// The delay between the time the packet this structure is associated with
		/// is received and the time when it should be decoded, in periods of a 27MHz
		/// clock.
		///
		/// UINT64_MAX when unknown or unspecified
		/// </summary>
		public uint64_t Vbv_Delay;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IDataContext MakeDeepClone()
		{
			return (AvCpbProperties)MemberwiseClone();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyTo(IDataContext destination)
		{
			AvCpbProperties dst = (AvCpbProperties)destination;

			dst.Max_BitRate = Max_BitRate;
			dst.Min_BitRate = Min_BitRate;
			dst.Avg_BitRate = Avg_BitRate;
			dst.Buffer_Size = Buffer_Size;
			dst.Vbv_Delay = Vbv_Delay;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public UtilFunc.Alloc_DataContext_Delegate Allocator => CreateInstance;

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Allocate a new instance of this type
		/// </summary>
		/********************************************************************/
		private static IDataContext CreateInstance()
		{
			return new AvCpbProperties();
		}
		#endregion
	}
}
