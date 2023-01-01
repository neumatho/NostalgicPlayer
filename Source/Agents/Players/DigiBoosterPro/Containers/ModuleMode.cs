/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Sequencer modes
	/// </summary>
	internal enum ModuleMode
	{
		Halted,									// Sequence is halted, no row fetches
		Pattern,
		Song,
		Manual,									// Play single row the go to MMODE_HALTED
		PatternBack,							// Play pattern with sequencer reversed
		ManualBack,								// Play single row with sequencer reversed, then go to MMODE_HALTED
		Song_Once								// Play song one time then wait forever
	}
}
