/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;

namespace Polycode.NostalgicPlayer.Ports.LibOgg.Containers
{
	/// <summary>
	/// Is used to encapsulate the data in one Ogg bitstream page
	/// </summary>
	public class Ogg_Page
	{
		/// <summary></summary>
		public CPointer<byte> Header;
		/// <summary></summary>
		public c_long HeaderLen;
		/// <summary></summary>
		public CPointer<byte> Body;
		/// <summary></summary>
		public c_long BodyLen;
	}
}
