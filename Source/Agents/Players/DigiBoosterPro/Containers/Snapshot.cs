/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Implementation;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Holds all the information about the player state at a specific time
	/// </summary>
	internal class Snapshot : ISnapshot
	{
		public ModuleSynth ModuleSynth;
		public EffectMaster EffectMaster;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Snapshot(ModuleSynth moduleSynth, EffectMaster effectMaster)
		{
			ModuleSynth = moduleSynth.MakeDeepClone();
			EffectMaster = effectMaster.MakeDeepClone();
		}
	}
}
