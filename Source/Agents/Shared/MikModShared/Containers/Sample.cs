/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers
{
#pragma warning disable 1591
	/// <summary>
	/// Sample structure
	/// </summary>
	public class Sample
	{
		public short Panning;					// Panning (0-255 or PAN_SURROUND)
		public uint Speed;						// Base playing speed/frequency of note
		public byte Volume;						// Volume 0-64
//		public SampleFlag InFlags;				// Sample format on disk
		public SampleFlag Flags;				// Sample format in memory
		public uint Length;						// Length of sample (in samples!)
		public uint LoopStart;					// Repeat position (relative to start, in samples)
		public uint LoopEnd;					// Repeat end
		public uint SusBegin;					// Sustain loop begin (in samples)   \ Not supported
		public uint SusEnd;						// Sustain loop end                  / yet!

		// Variables used by the module player only!
		public byte GlobVol;					// Global volume
		public VibratoFlag VibFlags;			// Auto vibration flag stuffs
		public byte VibType;					// Vibratos moved from Instruments to Sample
		public byte VibSweep;
		public byte VibDepth;
		public byte VibRate;
		public string SampleName;				// Name of sample

		// Values used internally only (not saved in disk formats)
		public ushort AVibPos;					// Auto vibrato pos [player use]
//		public byte DivFactor;					// For sample scaling, maintains proper period slides
		public uint SeekPos;					// Seek position in file
		public sbyte[] Handle;					// Sample handle. Points to the sample in memory
	}
#pragma warning restore 1591
}
