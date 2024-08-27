/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOgg.Containers
{
	/// <summary>
	/// Is used to encapsulate the data in one Ogg bitstream page
	/// </summary>
	public class Ogg_Page
	{
		/// <summary></summary>
		public byte[] Header;
		/// <summary></summary>
		public int HeaderOffset;
		/// <summary></summary>
		public c_long HeaderLen;
		/// <summary></summary>
		public byte[] Body;
		/// <summary></summary>
		public int BodyOffset;
		/// <summary></summary>
		public c_long BodyLen;
	}
}
