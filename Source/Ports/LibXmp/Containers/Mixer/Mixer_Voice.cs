/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Virt;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer
{
	/// <summary>
	/// 
	/// </summary>
	internal class Mixer_Voice : IDeepCloneable<Mixer_Voice>
	{
		/// <summary>
		/// Channel number
		/// </summary>
		public c_int Chn { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public c_int Root { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public c_int Note { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public c_int Pan { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public c_int Vol { get; set; }

		/// <summary>
		/// Current period
		/// </summary>
		public c_double Period { get; set; }

		/// <summary>
		/// Position in sample
		/// </summary>
		public c_double Pos { get; set; }

		/// <summary>
		/// Position in sample before mixing
		/// </summary>
		public c_int Pos0 { get; set; }

		/// <summary>
		/// Mixer function index
		/// </summary>
		public Mixer_Index_Flag FIdx { get; set; }

		/// <summary>
		/// Instrument number
		/// </summary>
		public c_int Ins { get; set; }

		/// <summary>
		/// Sample number
		/// </summary>
		public c_int Smp { get; set; }

		/// <summary>
		/// Loop start
		/// </summary>
		public c_int Start { get; set; }

		/// <summary>
		/// Loop end
		/// </summary>
		public c_int End { get; set; }

		/// <summary>
		/// NNA info and status of voice
		/// </summary>
		public Virt_Action Act { get; set; }

		/// <summary>
		/// Key for DCA note check
		/// </summary>
		public c_int Key { get; set; }

		/// <summary>
		/// Previous volume, left channel
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public c_int Old_VL { get; set; }

		/// <summary>
		/// Previous volume, right channel
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public c_int Old_VR { get; set; }

		/// <summary>
		/// Last left sample output, in 32bit
		/// </summary>
		public c_int SLeft { get; set; }

		/// <summary>
		/// Last right sample output, in 32bit
		/// </summary>
		public c_int SRight { get; set; }

		/// <summary>
		/// Flags
		/// </summary>
		public Mixer_Flag Flags { get; set; }

		/// <summary>
		/// Sample pointer
		/// </summary>
		public ref CPointer<byte> SPtr => ref _SPtr;
		private CPointer<byte> _SPtr;

		// ReSharper disable once InconsistentNaming
		public (
			c_int Smp,
			c_int Dummy
		) Queued;

		/// <summary>
		/// Filter variables
		/// </summary>
		public (
			c_int R1,
			c_int R2,
			c_int L1,
			c_int L2,
			c_int A0,
			c_int B0,
			c_int B1,
			c_int CutOff,
			c_int Resonance
		// ReSharper disable once InconsistentNaming
		) Filter;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Mixer_Voice MakeDeepClone()
		{
			return (Mixer_Voice)MemberwiseClone();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Chn = 0;
			Root = 0;
			Note = 0;
			Pan = 0;
			Vol = 0;
			Period = 0;
			Pos = 0;
			Pos0 = 0;
			FIdx = 0;
			Ins = 0;
			Smp = 0;
			Start = 0;
			End = 0;
			Act = 0;
			Key = 0;
			Old_VL = 0;
			Old_VR = 0;
			SLeft = 0;
			SRight = 0;
			Flags = 0;
			SPtr.SetToNull();
			Filter.R1 = 0;
			Filter.R2 = 0;
			Filter.L1 = 0;
			Filter.L2 = 0;
			Filter.A0 = 0;
			Filter.B0 = 0;
			Filter.B1 = 0;
			Filter.CutOff = 0;
			Filter.Resonance = 0;
		}
	}
}
