/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Calculate Schism Tracker version field for IT / S3M header based on specified release date
	/// Date calculation derived from https://alcor.concordia.ca/~gpkatch/gdate-algorithm.html
	/// </summary>
	internal class SchismVersionFromDate
	{
		private readonly int32 d;
		private readonly int32 mm;
		private readonly int32 yy;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SchismVersionFromDate(int32 y, int32 m, int32 d)
		{
			this.d = d;

			mm = (m + 9) % 12;
			yy = y - (mm / 10);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int32 Date => (yy * 365) + (yy / 4) - (yy / 100) + (yy / 400) + (((mm * 306) + 5) / 10) + (d - 1);
	}
}
