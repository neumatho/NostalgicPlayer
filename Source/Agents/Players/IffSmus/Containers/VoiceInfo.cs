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
		public VoiceStatus Status { get; set; }

		public InstrumentSetup InstrumentSetupSequence { get; set; }
		public IInstrumentFormat InstrumentFormat { get; set; }
		public short InstrumentNumber { get; set; }

		public byte Note { get; set; }
		public ushort Volume { get; set; }

		public SetSample SetSampleSequence { get; set; }
		public sbyte[] SampleData { get; set; }
		public uint SampleStartOffset { get; set; }
		public ushort SampleLengthInWords { get; set; }
		public ushort Period { get; set; }
		public ushort FinalVolume { get; set; }

		public sbyte[] CalculatedSynthesisSample { get; set; }

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
