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
	/// UniMod structure
	/// </summary>
	public class Module
	{
		// General module information
		public string SongName;					// Name of the song
//		public string ModType;					// String type of module loaded
		public string Comment = string.Empty;	// Module comments

		public ModuleFlag Flags;				// Module flags
		public byte NumChn;						// Number of module channels
		public byte NumVoices;					// Max # voices used for full NNA playback
		public ushort NumPos;					// Number of positions in this song
		public ushort NumPat;					// Number of patterns in this song
		public ushort NumIns;					// Number of instruments
		public ushort NumSmp;					// Number of samples
		public Instrument[] Instruments;		// All instruments
		public Sample[] Samples;				// All samples
		public byte RealChn;					// Real number of channels used
		public byte TotalChn;					// Total number of channels used (incl NNAs)

		// Playback settings
		public ushort RepPos;					// Restart position
		public byte InitSpeed;					// Initial song speed
		public ushort InitTempo;				// Initial song tempo
		public byte InitVolume;					// Initial global volume (0 - 128)
		public readonly ushort[] Panning = new ushort[SharedConstant.UF_MaxChan];	// Panning positions
		public readonly byte[] ChanVol = new byte[SharedConstant.UF_MaxChan];		// Channel positions
		public ushort Bpm;						// Current beats-per-minute speed
		public ushort SngSpd;					// Current song speed
		public short Volume;					// Song volume (0-128) (or user volume)

		public bool ExtSpd;						// Extended speed flag (default enabled)
		public bool PanFlag;					// Panning flag (default enabled)
		public bool Wrap;						// Wrap module? (default disabled)
		public bool Loop;						// Allow module to loop? (default enabled)
		public bool FadeOut;					// Volume fade out during last pattern

		public ushort PatPos;					// Current row number
		public short SngPos;					// Current song position
		public uint SngTime;					// Current song time in 2^-10 seconds

		public byte FarCurTempo;				// Farandole current speed
		public short FarTempoBend;				// Used by the Farandole fine tempo effects and store the current bend value

		public short RelSpd;					// Relative speed factor

		// Internal module representation
		public ushort NumTrk;					// Number of tracks
		public byte[][] Tracks;					// Array of NumTrk pointers to tracks
		public ushort[] Patterns;				// Array of Patterns
		public ushort[] PattRows;				// Array of number of rows for each pattern
		public ushort[] Positions;				// All positions

//		public bool Forbid;						// If true, no player update!
		public ushort NumRow;					// Number of rows in current pattern
		public ushort VbTick;					// Tick counter (counts from 0 to SngSpd)
		public ushort SngRemainder;				// Used for song computation

		public Mp_Control[] Control;			// Effect Channel information
		public Mp_Voice[] Voice;				// Audio Voice information

		public byte GlobalSlide;				// Global volume slide rate
		public bool Pat_RepCrazy;				// Module has just looped to position -1
		public ushort PatBrk;					// Position where to start a new pattern
		public byte PatDly;						// Pattern delay counter (command memory)
		public byte PatDly2;					// Pattern delay counter (real one)
		public short PosJmp;					// Flag to indicate a position jump is needed...
		public ushort BpmLimit;					// Threshold to detect BPM or speed values
	}
#pragma warning restore 1591
}
