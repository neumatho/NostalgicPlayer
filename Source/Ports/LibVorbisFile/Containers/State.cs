/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibVorbisFile.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal enum State
	{
		/// <summary></summary>
		NotOpen = 0,
		/// <summary></summary>
		PartOpen = 1,
		/// <summary></summary>
		Opened = 2,
		/// <summary></summary>
		StreamSet = 3,
		/// <summary></summary>
		InitSet = 4
	}
}
