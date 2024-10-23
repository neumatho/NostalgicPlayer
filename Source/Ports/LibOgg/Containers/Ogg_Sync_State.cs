/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOgg.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class Ogg_Sync_State
	{
		/// <summary></summary>
		public Pointer<byte> Data;
		/// <summary></summary>
		public c_int Storage;
		/// <summary></summary>
		public c_int Fill;
		/// <summary></summary>
		public c_int Returned;

		/// <summary></summary>
		public bool Unsynced;
		/// <summary></summary>
		public c_int HeaderBytes;
		/// <summary></summary>
		public c_int BodyBytes;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal void Clear()
		{
			Data.SetToNull();
			Storage = 0;
			Fill = 0;
			Returned = 0;

			Unsynced = false;
			HeaderBytes = 0;
			BodyBytes = 0;
		}
	}
}
