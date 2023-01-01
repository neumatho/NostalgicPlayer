/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common
{
	/// <summary>
	/// Different input streams implements this interface
	/// </summary>
	internal interface IInputStream
	{
		/// <summary>
		/// Read a single byte
		/// </summary>
		byte ReadByte();

		/// <summary>
		/// Consume the given number of bytes
		/// </summary>
		byte[] Consume(uint bytes, byte[] buffer);
	}
}
