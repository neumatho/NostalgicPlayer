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
		public TrackLine TrackLine = new TrackLine();

		public short SampleNumber;		// Current sample number
		public sbyte[] SampleData;
		public uint Offset;
		public ushort Length;
		public uint LoopStart;
		public ushort LoopLength;
		public uint StartOffset;
		public ushort Period;
		public byte FineTune;
		public sbyte FineTuneHmn;
		public sbyte Volume;			// Volume set from sample data or changed by effects
		public byte TonePortDirec;
		public byte TonePortSpeed;
		public ushort WantedPeriod;
		public byte VibratoCmd;
		public sbyte VibratoPos;
		public byte TremoloCmd;
		public sbyte TremoloPos;
		public byte WaveControl;
		public byte GlissFunk;
		public byte SampleOffset;
		public sbyte PattPos;
		public byte LoopCount;
		public byte FunkOffset;
		public uint WaveStart;
		public bool AutoSlide;
		public byte AutoSlideArg;

		public bool SynthSample;		// True if synth sample, false if normal

		// For StarTrekker synths
		public AmToDo AmToDo;			// Switch number
		public ushort VibDegree;		// Vibrato degree
		public short SustainCounter;	// Sustain time counter
		public short StarVolume;		// Calculated volume from synth samples (0-256)

		// For His Master's Noise synths
		public byte DataCounter;
		public short HmnVolume;			// Calculated volume from synth samples (0-64)
		public HmnSynthData SynthData;

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
