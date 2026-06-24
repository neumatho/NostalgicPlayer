/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibReSidFp.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Params
	{
		/// <summary></summary>
		public SamplingMethod Method;

		/// <summary></summary>
		public double ClockFrequency;

		/// <summary></summary>
		public double SamplingFrequency;

		/// <summary></summary>
		public double FilterCurve6581;

		/// <summary></summary>
		public double FilterRange6581;

		/// <summary></summary>
		public double FilterCurve8580;

		/// <summary></summary>
		public bool Old6581Caps;
	}
}
