/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DigitalSoundStudio.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public byte Sample { get; set; }
		public ushort Period { get; set; }
		public Effect Effect { get; set; }
		public byte EffectArg { get; set; }

		public byte FineTune { get; set; }
		public byte Volume { get; set; }

		public byte PlayingSampleNumber { get; set; }
		public sbyte[] SampleData { get; set; }
		public uint SampleStartOffset { get; set; }
		public ushort SampleLength { get; set; }
		public uint LoopStart { get; set; }
		public ushort LoopLength { get; set; }
		public ushort SampleOffset { get; set; }

		public ushort PitchPeriod { get; set; }
		public bool PortamentoDirection { get; set; }
		public byte PortamentoSpeed { get; set; }
		public ushort PortamentoEndPeriod { get; set; }

		public bool UseTonePortamentoForSlideEffects { get; set; }
		public bool UseTonePortamentoForPortamentoEffects { get; set; }

		public short LoopRow { get; set; }
		public ushort LoopCounter { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			return (VoiceInfo)MemberwiseClone();
		}
	}
}
