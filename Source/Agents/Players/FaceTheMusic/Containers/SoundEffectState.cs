/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.FaceTheMusic.Containers
{
	/// <summary>
	/// Holds the current state of the sound effect script per voice
	/// </summary>
	internal class SoundEffectState : IDeepCloneable<SoundEffectState>
	{
		public SoundEffectScript EffectScript;
		public int ScriptPosition;

		public ushort WaitCounter;
		public ushort LoopCounter;

		public VoiceInfo VoiceInfo;

		public ushort NewPitchGotoLineNumber;
		public ushort NewVolumeGotoLineNumber;
		public ushort NewSampleGotoLineNumber;
		public ushort ReleaseGotoLineNumber;
		public ushort PortamentoGotoLineNumber;
		public ushort VolumeDownGotoLineNumber;

		public ushort InterruptLineNumber;

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
