/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AudioConvert
	{
		/// <summary>
		/// 
		/// </summary>
		public c_int Channels;

		/// <summary>
		/// 
		/// </summary>
		public SwrFunc.Conv_Func_Type_Delegate Conv_F;

		/// <summary>
		/// 
		/// </summary>
		public SwrFunc.Simd_Func_Type_Delegate Simd_F;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<c_int> Ch_Map;

		/// <summary>
		/// Silence input sample
		/// </summary>
		public readonly uint8_t[] Silence = new uint8_t[8];
	}
}
