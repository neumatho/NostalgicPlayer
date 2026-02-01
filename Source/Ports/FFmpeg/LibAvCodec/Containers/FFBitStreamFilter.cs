/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class FFBitStreamFilter : AvBitStreamFilter
	{
		/// <summary>
		/// The public AVBitStreamFilter. See bsf.h for it
		/// </summary>
		public AvBitStreamFilter P => this;

		/// <summary>
		/// 
		/// </summary>
//		public c_int Priv_Data_Size;
		public CodecFunc.Private_Data_Alloc_Delegate Priv_Data_Alloc;

		/// <summary>
		/// 
		/// </summary>
		public CodecFunc.Init_Bsf_Delegate Init;

		/// <summary>
		/// 
		/// </summary>
		public CodecFunc.Filter_Bsf_Delegate Filter;

		/// <summary>
		/// 
		/// </summary>
		public CodecFunc.Close_Bsf_Delegate Close;

		/// <summary>
		/// 
		/// </summary>
		public CodecFunc.Flush_Bsf_Delegate Flush;
	}
}
