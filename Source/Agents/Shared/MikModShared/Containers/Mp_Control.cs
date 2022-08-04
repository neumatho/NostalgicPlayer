/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers
{
#pragma warning disable 1591
	/// <summary>
	/// Mp_Control structure
	/// </summary>
	public class Mp_Control
	{
		public Mp_Channel Main;

		public Mp_Voice Slave;					// Audio slave of current effects control channel

		public byte SlaveChn;					// Audio slave of current effects control channel
		public bool Muted;						// If set, channel not played
		public ushort UltOffset;				// Fine sample offset memory
		public byte ANote;						// The note that indexes the audible
		public byte OldNote;
		public bool OwnPer;
		public bool OwnVol;
		public Dca Dca;							// Duplicate check action
		public Dct Dct;							// Duplicate check type
		public byte[] Row;						// Row currently playing on this channel
		public int RowOffset;					// Current offset into the row above
		public sbyte Retrig;					// Retrig value (0 means don't retrig)
		public uint Speed;						// What fine tune to use
		public short Volume;					// Amiga volume (0 t/m 64) to play the sample at

		public short TmpVolume;					// Tmp volume
		public ushort TmpPeriod;				// Tmp period
		public ushort WantedPeriod;				// Period to slide to (with effect 3 or 5)

		public byte ArpMem;						// Arpeggio command memory
		public byte PanSSpd;					// Pan slide speed
		public ushort SlideSpeed;
		public ushort PortSpeed;				// Note slide speed (tone portamento)

		public byte S3MTremor;					// S3M tremor (effect I) counter
		public byte S3MTrOnOf;					// S3M tremor on time/off time
		public byte S3MVolSlide;				// Last used vol slide
		public sbyte Sliding;
		public byte S3MRtgSpeed;				// Last used retrig speed
		public byte S3MRtgSlide;				// Last used retrig slide

		public bool FarTonePortaRunning;		// FAR tone porta (effect 3) is a little bit different than other effects. It should keep running when the effect has first started, even if it is not given on subsequently rows
		public int FarTonePortaSpeed;			// FAR tone porta increment value
		public int FarCurrentValue;				// Because we're using fixing points as speed and the current period is an integer, we need to store the current value here for next round
		public byte FarRetrigCount;				// Number of retrigs to do

		// These variables are only stored on the first control instance and therefore used globally.
		// The reason they are stored here, is to minimize the number of global variables
		public byte FarCurTempo;				// Farandole current speed
		public short FarTempoBend;				// Used by the Farandole fine tempo effects and store the current bend value

		public byte Glissando;					// Glissando (0 means off)
		public byte WaveControl;

		public sbyte VibPos;					// Current vibrato position
		public byte VibSpd;						// "" speed
		public byte VibDepth;					// "" depth

		public sbyte TrmPos;					// Current tremolo position
		public byte TrmSpd;						// "" speed
		public byte TrmDepth;					// "" depth

		public byte FSlideUpSpd;
		public byte FSlideDnSpd;
		public byte FPortUpSpd;					// fx E1 (extra fine portamento up) data
		public byte FPortDnSpd;					// fx E2 (extra fine portamento down) data
		public byte FFPortUpSpd;				// fx X1 (extra fine portamento up) data
		public byte FFPortDnSpd;				// fx X2 (extra fine portamento down) data

		public uint HiOffset;					// Last used high order of sample offset
		public ushort SOffset;					// Last used low order of sample offset (effect 9)

		public SsCommand SsEffect;				// Last used Sxx effect
		public byte SsData;						// Last used Sxx data info
		public byte ChanVolSlide;				// Last used channel volume slide

		public byte PanbWave;					// Current panbrello waveform
		public byte PanbPos;					// Current panbrello position
		public sbyte PanbSpd;					// "" speed
		public byte PanbDepth;					// "" depth

		public byte NewNote;					// Set to 1 if the current row contains a note
		public byte NewSamp;					// Set to 1 upon sample / inst change
		public VolEffect VolEffect;				// Volume Column Effect Memory as used by IT
		public byte VolData;					// Volume Column Data Memory

		public short Pat_RepPos;				// PatternLoop position
		public ushort Pat_RepCnt;				// Times to loop
	}
#pragma warning restore 1591
}
