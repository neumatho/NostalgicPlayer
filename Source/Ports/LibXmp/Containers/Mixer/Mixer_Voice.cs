/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Kit.Interfaces;
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
		public c_int Chn;

		/// <summary>
		/// 
		/// </summary>
		public c_int Root;

		/// <summary>
		/// 
		/// </summary>
		public c_int Note;

		/// <summary>
		/// 
		/// </summary>
		public c_int Pan;

		/// <summary>
		/// 
		/// </summary>
		public c_int Vol;

		/// <summary>
		/// Current period
		/// </summary>
		public c_double Period;

		/// <summary>
		/// Position in sample
		/// </summary>
		public c_double Pos;

		/// <summary>
		/// Position in sample before mixing
		/// </summary>
		public c_int Pos0;

		/// <summary>
		/// Mixer function index
		/// </summary>
		public Mixer_Index_Flag FIdx;

		/// <summary>
		/// Instrument number
		/// </summary>
		public c_int Ins;

		/// <summary>
		/// Sample number
		/// </summary>
		public c_int Smp;

		/// <summary>
		/// Loop start
		/// </summary>
		public c_int Start;

		/// <summary>
		/// Loop end
		/// </summary>
		public c_int End;

		/// <summary>
		/// NNA info and status of voice
		/// </summary>
		public Virt_Action Act;

		/// <summary>
		/// Key for DCA note check
		/// </summary>
		public c_int Key;

		/// <summary>
		/// Previous volume, left channel
		/// </summary>
		public c_int Old_VL;

		/// <summary>
		/// Previous volume, right channel
		/// </summary>
		public c_int Old_VR;

		/// <summary>
		/// Last left sample output, in 32bit
		/// </summary>
		public c_int SLeft;

		/// <summary>
		/// Last right sample output, in 32bit
		/// </summary>
		public c_int SRight;

		/// <summary>
		/// Flags
		/// </summary>
		public Mixer_Flag Flags;

		/// <summary>
		/// Sample pointer
		/// </summary>
		public CPointer<byte> SPtr;

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
