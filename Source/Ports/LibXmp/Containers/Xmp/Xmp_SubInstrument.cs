/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// 
	/// </summary>
	public class Xmp_SubInstrument
	{
		/// <summary>
		/// Default volume
		/// </summary>
		public c_int Vol { get; internal set; }

		/// <summary>
		/// Global volume
		/// </summary>
		public c_int Gvl { get; internal set; }

		/// <summary>
		/// Pan
		/// </summary>
		public c_int Pan { get; internal set; }

		/// <summary>
		/// Transpose
		/// </summary>
		public ref c_int Xpo => ref _Xpo;
		private c_int _Xpo;

		/// <summary>
		/// Finetune
		/// </summary>
		public ref c_int Fin => ref _Fin;
		private c_int _Fin;

		/// <summary>
		/// Vibrato waveform
		/// </summary>
		public c_int Vwf { get; internal set; }

		/// <summary>
		/// Vibrato depth
		/// </summary>
		public c_int Vde { get; internal set; }

		/// <summary>
		/// Vibrato rate
		/// </summary>
		public c_int Vra { get; internal set; }

		/// <summary>
		/// Vibrato sweep
		/// </summary>
		public c_int Vsw { get; internal set; }

		/// <summary>
		/// Random volume/pan variation (IT)
		/// </summary>
		public c_int Rvv { get; internal set; }

		/// <summary>
		/// Sample number
		/// </summary>
		public c_int Sid { get; internal set; }

		/// <summary>
		/// New note action
		/// </summary>
		public Xmp_Inst_Nna Nna { get; internal set; }

		/// <summary>
		/// Duplicate check type
		/// </summary>
		public Xmp_Inst_Dct Dct { get; internal set; }

		/// <summary>
		/// Duplicate check action
		/// </summary>
		public Xmp_Inst_Dca Dca { get; internal set; }

		/// <summary>
		/// Initial filter cutoff
		/// </summary>
		public c_int Ifc { get; internal set; }

		/// <summary>
		/// Initial filter resonance
		/// </summary>
		public c_int Ifr { get; internal set; }
	}
}
