/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public VoiceStatus Status;

		public InstrumentSetup InstrumentSetupSequence;
		public IInstrumentFormat InstrumentFormat;
		public short InstrumentNumber;

		public byte Note;
		public ushort Volume;

		public SetSample SetSampleSequence;
		public sbyte[] SampleData;
		public uint SampleStartOffset;
		public ushort SampleLengthInWords;
		public ushort Period;
		public ushort FinalVolume;

		public sbyte[] CalculatedSynthesisSample;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			VoiceInfo clone = (VoiceInfo)MemberwiseClone();

			if (CalculatedSynthesisSample != null)
				clone.CalculatedSynthesisSample = ArrayHelper.CloneArray(CalculatedSynthesisSample);

			return clone;
		}
	}
}
