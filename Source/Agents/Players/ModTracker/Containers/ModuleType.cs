/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers
{
	/// <summary>
	/// Holds the different module types
	/// </summary>
	internal enum ModuleType
	{
		Unknown = 0,

		// Old SoundTracker compatibilities of different kinds
		UltimateSoundTracker10,
		UltimateSoundTracker18,
		SoundTrackerII,
		SoundTrackerVI,
		SoundTrackerIX,
		MasterSoundTracker10,
		SoundTracker2x,
		SoundTracker26,
		IceTracker,

		// NoiseTracker compatibility
		NoiseTracker,
		HisMastersNoise,
		StarTrekker,
		StarTrekker8,
		AudioSculpture,

		// ProTracker compatibility
		ProTracker
	}
}
