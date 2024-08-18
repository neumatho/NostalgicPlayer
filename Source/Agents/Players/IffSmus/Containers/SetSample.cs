/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Containers
{
	/// <summary>
	/// Indicate what to do in the next Play() call
	/// </summary>
	internal enum SetSample : ushort
	{
		Nothing = 0,
		SetLoop,
		StartSample
	}
}
