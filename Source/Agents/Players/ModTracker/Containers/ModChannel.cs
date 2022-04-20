/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/

using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers
{
	/// <summary>
	/// Channel structure
	/// </summary>
	internal class ModChannel
	{
		public TrackLine TrackLine = new TrackLine();
		public sbyte[] SampleData;
		public ushort Offset;
		public ushort Length;
		public ushort LoopStart;
		public ushort LoopLength;
		public ushort StartOffset;
		public ushort Period;
		public byte FineTune;
		public sbyte FineTuneHmn;
		public sbyte Volume;			// Volume set from sample data or changed by effects
		public ushort Panning;
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
		public byte PattPos;
		public byte LoopCount;
		public byte FunkOffset;
		public ushort WaveStart;
		public bool AutoSlide;
		public byte AutoSlideArg;

		public bool SynthSample;		// True if synth sample, false if normal

		// For StarTrekker synths
		public AmToDo AmToDo;			// Switch number
		public ushort SampleNum;		// Current sample number
		public ushort VibDegree;		// Vibrato degree
		public short SustainCounter;	// Sustain time counter
		public short StarVolume;		// Calculated volume from synth samples (0-256)

		// For His Master's Noise synths
		public byte DataCounter;
		public short HmnVolume;			// Calculated volume from synth samples (0-64)
		public HmnSynthData SynthData;

		// Visual information
		public VisualInfo VisualInfo = new VisualInfo();
	}
}
