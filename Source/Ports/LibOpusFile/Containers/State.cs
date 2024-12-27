/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpusFile.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal enum State
	{
		/// <summary>
		/// Initial state
		/// </summary>
		NotOpen = 0,

		/// <summary>
		/// We've found the first Opus stream in the first link
		/// </summary>
		PartOpen = 1,

		/// <summary>
		/// 
		/// </summary>
		Opened = 2,

		/// <summary>
		/// We've found the first Opus stream in the current link
		/// </summary>
		StreamSet = 3,

		/// <summary>
		/// We've initialized the decoder for the chosen Opus stream in the
		/// current link
		/// </summary>
		InitSet = 4
	}
}
