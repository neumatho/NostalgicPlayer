/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Kiss_Twiddle_Cpx
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Kiss_Twiddle_Cpx(kiss_twiddle_scalar _r, kiss_twiddle_scalar _i)
		{
			r = _r;
			i = _i;
		}

		public readonly kiss_twiddle_scalar r;
		public readonly kiss_twiddle_scalar i;
	}
}
