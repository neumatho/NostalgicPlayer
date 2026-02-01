/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	internal class Tx_Double : Tx_Template<c_double, c_double, c_double, c_double, AvComplexDouble>
	{
		private static readonly Tx_Double mySelf = new Tx_Double();

		/// <summary>
		/// 
		/// </summary>
		public static readonly FFTxCodelet[] ff_Tx_Codelet_List_Double_C;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		static Tx_Double()
		{
			ff_Tx_Codelet_List_Double_C = mySelf.Build_Codelet_List();
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Tx_Double() : base("Double")
		{
		}
	}
}
