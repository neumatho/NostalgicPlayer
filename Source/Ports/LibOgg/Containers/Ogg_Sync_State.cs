/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOgg.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Ogg_Sync_State
	{
		public byte[] Data;
		public c_int Storage;
		public c_int Fill;
		public c_int Returned;

		public bool Unsynced;
		public c_int HeaderBytes;
		public c_int BodyBytes;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Data = null;
			Storage = 0;
			Fill = 0;
			Returned = 0;

			Unsynced = false;
			HeaderBytes = 0;
			BodyBytes = 0;
		}
	}
}
