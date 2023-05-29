/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal enum Io
	{
		None,
		Fd,								// Wrapping over callbacks operation on streams
		Handle							// Wrapping over custom handle callbacks
	}
}
