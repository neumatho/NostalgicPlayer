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
		public byte Sample;
		public ushort Period;
		public Effect Effect;
		public byte EffectArg;

		public byte FineTune;
		public byte Volume;

		public byte PlayingSampleNumber;
		public sbyte[] SampleData;
		public uint SampleStartOffset;
		public ushort SampleLength;
		public uint LoopStart;
		public ushort LoopLength;
		public ushort SampleOffset;

		public ushort PitchPeriod;
		public bool PortamentoDirection;
		public byte PortamentoSpeed;
		public ushort PortamentoEndPeriod;

		public bool UseTonePortamentoForSlideEffects;
		public bool UseTonePortamentoForPortamentoEffects;

		public short LoopRow;
		public ushort LoopCounter;

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
