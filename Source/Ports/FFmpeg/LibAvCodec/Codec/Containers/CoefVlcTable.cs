/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class CoefVlcTable
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CoefVlcTable(c_int n, c_int max_Level, uint32_t[] huffCodes, uint8_t[] huffBits, uint16_t[] levels)
		{
			N = n;
			Max_Level = max_Level;
			HuffCodes = huffCodes;
			HuffBits = huffBits;
			Levels = levels;
		}



		/********************************************************************/
		/// <summary>
		/// Total number of codes
		/// </summary>
		/********************************************************************/
		public c_int N { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Max_Level { get; }



		/********************************************************************/
		/// <summary>
		/// VLC bit values
		/// </summary>
		/********************************************************************/
		public CPointer<uint32_t> HuffCodes { get; }



		/********************************************************************/
		/// <summary>
		/// VLC bit size
		/// </summary>
		/********************************************************************/
		public CPointer<uint8_t> HuffBits { get; }



		/********************************************************************/
		/// <summary>
		/// Table to build run/level tables
		/// </summary>
		/********************************************************************/
		public CPointer<uint16_t> Levels { get; }
	}
}
