/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// FLAC frame structure
	/// </summary>
	public class Flac__Frame
	{
		/// <summary></summary>
		public Flac__FrameHeader Header = new Flac__FrameHeader();
		/// <summary></summary>
		public Flac__SubFrame[] SubFrames = ArrayHelper.InitializeArray<Flac__SubFrame>((int)Constants.Flac__Max_Channels);
		/// <summary></summary>
		public Flac__FrameFooter Footer = new Flac__FrameFooter();
	}
}
