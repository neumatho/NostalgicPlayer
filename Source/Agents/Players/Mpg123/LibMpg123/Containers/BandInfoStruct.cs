/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class BandInfoStruct
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public BandInfoStruct(c_ushort[] longIdx, c_uchar[] longDiff, c_ushort[] shortIdx, c_uchar[] shortDiff)
		{
			LongIdx = longIdx;
			LongDiff = longDiff;
			ShortIdx = shortIdx;
			ShortDiff = shortDiff;
		}

		public c_ushort[] LongIdx;
		public c_uchar[] LongDiff;
		public c_ushort[] ShortIdx;
		public c_uchar[] ShortDiff;
	}
}
