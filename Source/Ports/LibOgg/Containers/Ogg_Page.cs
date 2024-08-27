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
		public byte[] Header;
		public int HeaderOffset;
		public c_long HeaderLen;
		public byte[] Body;
		public int BodyOffset;
		public c_long BodyLen;
	}
}
