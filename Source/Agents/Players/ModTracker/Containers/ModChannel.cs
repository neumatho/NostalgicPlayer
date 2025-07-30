/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers
{
	/// <summary>
	/// Channel structure
	/// </summary>
	internal class ModChannel : IDeepCloneable<ModChannel>
	{
		public TrackLine TrackLine { get; set; } = new TrackLine();

		public short SampleNumber { get; set; }		// Current sample number
		public sbyte[] SampleData { get; set; }
		public uint Offset { get; set; }
		public ushort Length { get; set; }
		public uint LoopStart { get; set; }
		public ushort LoopLength { get; set; }
		public uint StartOffset { get; set; }
		public ushort Period { get; set; }
		public byte FineTune { get; set; }
		public sbyte FineTuneHmn { get; set; }
		public sbyte Volume { get; set; }			// Volume set from sample data or changed by effects
		public byte TonePortDirec { get; set; }
		public byte TonePortSpeed { get; set; }
		public ushort WantedPeriod { get; set; }
		public byte VibratoCmd { get; set; }
		public sbyte VibratoPos { get; set; }
		public byte TremoloCmd { get; set; }
		public sbyte TremoloPos { get; set; }
		public byte WaveControl { get; set; }
		public byte GlissFunk { get; set; }
		public byte SampleOffset { get; set; }
		public sbyte PattPos { get; set; }
		public byte LoopCount { get; set; }
		public byte FunkOffset { get; set; }
		public uint WaveStart { get; set; }
		public bool AutoSlide { get; set; }
		public byte AutoSlideArg { get; set; }

		public bool SynthSample { get; set; }		// True if synth sample, false if normal

		// For StarTrekker synths
		public AmToDo AmToDo { get; set; }			// Switch number
		public ushort VibDegree { get; set; }		// Vibrato degree
		public short SustainCounter { get; set; }	// Sustain time counter
		public short StarVolume { get; set; }		// Calculated volume from synth samples (0-256)

		// For His Master's Noise synths
		public byte DataCounter { get; set; }
		public short HmnVolume { get; set; }			// Calculated volume from synth samples (0-64)
		public HmnSynthData SynthData { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public ModChannel MakeDeepClone()
		{
			ModChannel clone = (ModChannel)MemberwiseClone();
			clone.TrackLine = TrackLine.MakeDeepClone();

			return clone;
		}
	}
}
