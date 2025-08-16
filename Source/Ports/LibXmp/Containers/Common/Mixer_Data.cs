/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class Mixer_Data
	{
		/// <summary>
		/// Sampling rate
		/// </summary>
		public c_int Freq { get; set; }

		/// <summary>
		/// Sample format
		/// </summary>
		public Xmp_Format Format { get; set; }

		/// <summary>
		/// Amplification multiplier
		/// </summary>
		public c_int Amplify { get; set; }

		/// <summary>
		/// Percentage of channel separation
		/// </summary>
		public c_int Mix { get; set; }

		/// <summary>
		/// Interpolation type
		/// </summary>
		public Xmp_Interp Interp { get; set; }

		/// <summary>
		/// DSP effect flags
		/// </summary>
		public Xmp_Dsp Dsp { get; set; }

		/// <summary>
		/// Output buffer
		/// </summary>
		public ref CPointer<int8> Buffer => ref _Buffer;
		private CPointer<int8> _Buffer;

		/// <summary>
		/// Temporary buffer for 32 bit samples
		/// </summary>
		public ref CPointer<int32> Buf32 => ref _Buf32;
		private CPointer<int32> _Buf32;

		/// <summary>
		/// Output buffer for rear speakers
		/// </summary>
		public ref CPointer<int8> BufferRear => ref _BufferRear;
		private CPointer<int8> _BufferRear;

		/// <summary>
		/// Temporary buffer for 32 bit samples for real speakers
		/// </summary>
		public ref CPointer<int32> Buf32Rear => ref _Buf32Rear;
		private CPointer<int32> _Buf32Rear;

		/// <summary>
		/// Default softmixer voices number
		/// </summary>
		public c_int NumVoc { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public c_int TickSize { get; set; }

		/// <summary>
		/// Anticlick control, right channel
		/// </summary>
		public c_int DtRight { get; set; }

		/// <summary>
		/// Anticlick control, left channel
		/// </summary>
		public c_int DtLeft { get; set; }

		/// <summary>
		/// Adjustment for IT bidirectional loops
		/// </summary>
		public c_int BiDir_Adjust { get; set; }

		/// <summary>
		/// Period base
		/// </summary>
		public c_double PBase { get; set; }

		/// <summary>
		/// Indicate if surround should be handled or not
		/// </summary>
		public Surround EnableSurround { get; set; }
	}
}
