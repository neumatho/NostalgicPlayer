/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvTxContext
	{
		// Fields the root transform and subtransforms use or may use

		/// <summary>
		/// Length of the transform
		/// </summary>
		public c_int Len;

		/// <summary>
		/// If transform is inverse
		/// </summary>
		public c_int Inv;

		/// <summary>
		/// Lookup table(s)
		/// </summary>
		public CPointer<c_int> Map;

		/// <summary>
		/// Any non-pre-baked multiplication factors,
		/// or extra temporary buffer
		/// </summary>
		public IPointer Exp;		// Is a CPointer<TXComplex>

		/// <summary>
		/// Temporary buffer, if needed
		/// </summary>
		public IPointer Tmp;		// Is a CPointer<TXComplex>

		/// <summary>
		/// Subtransform context(s), if needed
		/// </summary>
		public CPointer<AvTxContext> Sub;

		/// <summary>
		/// Function(s) for the subtransforms
		/// </summary>
		public readonly UtilFunc.Av_Tx_Fn[] Fn = new UtilFunc.Av_Tx_Fn[UtilConstants.Tx_Max_Sub];

		/// <summary>
		/// Number of subtransforms.
		/// The reason all of these are set here
		/// rather than in each separate context
		/// is to eliminate extra pointer
		/// dereferences
		/// </summary>
		public c_int Nb_Sub;

		// Fields mainly useul/applicable for the root transform or initialization

		/// <summary>
		/// Subtransform codelets
		/// </summary>
		public readonly FFTxCodelet[] Cd = new FFTxCodelet[UtilConstants.Tx_Max_Sub];

		/// <summary>
		/// Codelet for the current context
		/// </summary>
		public FFTxCodelet Cd_Self;

		/// <summary>
		/// Type of transform
		/// </summary>
		public AvTxType Type;

		/// <summary>
		/// A combination of AVTXFlags and
		/// codelet flags used when creating
		/// </summary>
		public AvTxFlags Flags;

		/// <summary>
		/// Direction of AVTXContext.Map
		/// </summary>
		public FFTxMapDirection Map_Dir;

		/// <summary>
		/// 
		/// </summary>
		public c_float Scale_F;

		/// <summary>
		/// 
		/// </summary>
		public c_double Scale_D;

		/// <summary>
		/// Free to use by implementations
		/// </summary>
		public IOpaque Opaque;
	}
}
