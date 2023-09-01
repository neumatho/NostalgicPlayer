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
		public c_int Vol;

		/// <summary>
		/// Global volume
		/// </summary>
		public c_int Gvl;

		/// <summary>
		/// Pan
		/// </summary>
		public c_int Pan;

		/// <summary>
		/// Transpose
		/// </summary>
		public c_int Xpo;

		/// <summary>
		/// Finetune
		/// </summary>
		public c_int Fin;

		/// <summary>
		/// Vibrato waveform
		/// </summary>
		public c_int Vwf;

		/// <summary>
		/// Vibrato depth
		/// </summary>
		public c_int Vde;

		/// <summary>
		/// Vibrato rate
		/// </summary>
		public c_int Vra;

		/// <summary>
		/// Vibrato sweep
		/// </summary>
		public c_int Vsw;

		/// <summary>
		/// Random volume/pan variation (IT)
		/// </summary>
		public c_int Rvv;

		/// <summary>
		/// Sample number
		/// </summary>
		public c_int Sid;

		/// <summary>
		/// New note action
		/// </summary>
		public Xmp_Inst_Nna Nna;

		/// <summary>
		/// Duplicate check type
		/// </summary>
		public Xmp_Inst_Dct Dct;

		/// <summary>
		/// Duplicate check action
		/// </summary>
		public Xmp_Inst_Dca Dca;

		/// <summary>
		/// Initial filter cutoff
		/// </summary>
		public c_int Ifc;

		/// <summary>
		/// Initial filter resonance
		/// </summary>
		public c_int Ifr;
	}
}
