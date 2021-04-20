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
	/// Mp_Channel structure
	/// </summary>
	public struct Mp_Channel
	{
		public Instrument I;
		public Sample S;
		public byte Sample;						// Which sample number
		public byte Note;						// The audible note (as heard, direct rep of period)
		public short OutVolume;					// Output volume (Vol + SampVol + InstVol)
		public byte ChanVol;					// Channel's "global" volume
		public ushort FadeVol;					// Fading volume rate
		public short Panning;					// Panning position
		public Kick Kick;						// If true = sample has to be restarted
		public bool Kick_Flag;					// Kick has been true
		public ushort Period;					// Period to play the sample at
		public Nna Nna;							// New Note Action type + master/slave flags

		public EnvelopeFlag VolFlg;				// Volume envelope settings
		public EnvelopeFlag PanFlg;				// Panning envelope settings
		public EnvelopeFlag PitFlg;				// Pitch envelope settings

		public KeyFlag KeyOff;					// If true = fade out and stuff
		public sbyte[] Handle;					// Which sample-handle
		public byte NoteDelay;					// (Used for note delay)
		public int Start;						// The starting byte index in the sample
	}
#pragma warning restore 1591
}
