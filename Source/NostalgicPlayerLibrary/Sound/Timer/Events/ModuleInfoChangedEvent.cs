/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Timer.Events
{
	/// <summary>
	/// Event for module information changes
	/// </summary>
	internal class ModuleInfoChangedEvent : ITimedEvent
	{
		private readonly SoundBase soundBase;
		private readonly ModuleInfoChanged changes;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoChangedEvent(SoundBase soundBase, ModuleInfoChanged moduleInfoChanged)
		{
			this.soundBase = soundBase;
			changes = moduleInfoChanged;
		}



		/********************************************************************/
		/// <summary>
		/// Do whatever this event want to do
		/// </summary>
		/********************************************************************/
		public void Execute(int differenceTime)
		{
			soundBase.OnModuleInfoChanged(new ModuleInfoChangedEventArgs(changes.Line, changes.Value));
		}
	}
}
