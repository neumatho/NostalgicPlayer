/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.FaceTheMusic.Containers
{
	/// <summary>
	/// Holds the current state of the sound effect script per voice
	/// </summary>
	internal class SoundEffectState : IDeepCloneable<SoundEffectState>
	{
		public SoundEffectScript EffectScript { get; set; }
		public int ScriptPosition { get; set; }

		public ushort WaitCounter { get; set; }
		public ushort LoopCounter { get; set; }

		public VoiceInfo VoiceInfo { get; set; }

		public ushort NewPitchGotoLineNumber { get; set; }
		public ushort NewVolumeGotoLineNumber { get; set; }
		public ushort NewSampleGotoLineNumber { get; set; }
		public ushort ReleaseGotoLineNumber { get; set; }
		public ushort PortamentoGotoLineNumber { get; set; }
		public ushort VolumeDownGotoLineNumber { get; set; }

		public ushort InterruptLineNumber { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public SoundEffectState MakeDeepClone()
		{
			return (SoundEffectState)MemberwiseClone();
		}
	}
}
