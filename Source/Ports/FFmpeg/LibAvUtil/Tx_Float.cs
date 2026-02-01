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
	internal class Tx_Float : Tx_Template<c_float, c_float, c_float, c_float, AvComplexFloat>
	{
		private static readonly Tx_Float mySelf = new Tx_Float();

		/// <summary>
		/// 
		/// </summary>
		public static readonly FFTxCodelet[] ff_Tx_Codelet_List_Float_C;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		static Tx_Float()
		{
			ff_Tx_Codelet_List_Float_C = mySelf.Build_Codelet_List();
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Tx_Float() : base("Float")
		{
		}
	}
}
