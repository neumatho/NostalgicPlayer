/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibReSidFp
{
	/// <summary>
	/// 
	/// </summary>
	internal abstract class Integrator
	{
		protected int vx;
		protected int vc;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Integrator()
		{
			vx = 0;
			vc = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract int Solve(int vi);
	}
}
