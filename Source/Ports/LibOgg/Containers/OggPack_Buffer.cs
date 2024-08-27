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
	internal class OggPack_Buffer
	{
		/// <summary></summary>
		public c_long EndByte;
		/// <summary></summary>
		public c_int EndBit;

		/// <summary></summary>
		public byte[] Buffer;
		/// <summary></summary>
		public int Ptr;
		/// <summary></summary>
		public c_long Storage;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			EndByte = 0;
			EndBit = 0;

			Buffer = null;
			Ptr = 0;
			Storage = 0;
		}
	}
}
