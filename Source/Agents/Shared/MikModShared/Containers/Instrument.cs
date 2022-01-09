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
	/// Instrument structure
	/// </summary>
	public class Instrument
	{
		public string InsName;

		public InstrumentFlag Flags;

		public readonly ushort[] SampleNumber = new ushort[SharedConstant.InstNotes];
		public readonly byte[] SampleNote = new byte[SharedConstant.InstNotes];

		public Nna NnaType;
		public Dca Dca;							// Duplicate check action
		public Dct Dct;							// Duplicate check type
		public byte GlobVol;
		public ushort VolFade;
		public short Panning;					// Instrument-based panning var

		public byte PitPanSep;					// Pitch pan separation (0 to 255)
		public byte PitPanCenter;				// Pitch pan center (0 to 119)
		public byte RVolVar;					// Random volume variations (0 - 100%)
		public byte RPanVar;					// Random panning variations (0 - 100%)

		// Volume envelope
		public EnvelopeFlag VolFlg;				// Bit 0: On 1: Sustain 2: Loop
		public byte VolPts;
		public byte VolSusBeg;
		public byte VolSusEnd;
		public byte VolBeg;
		public byte VolEnd;
		public readonly EnvPt[] VolEnv = new EnvPt[SharedConstant.EnvPoints];

		// Panning envelope
		public EnvelopeFlag PanFlg;				// Bit 0: On 1: Sustain 2: Loop
		public byte PanPts;
		public byte PanSusBeg;
		public byte PanSusEnd;
		public byte PanBeg;
		public byte PanEnd;
		public readonly EnvPt[] PanEnv = new EnvPt[SharedConstant.EnvPoints];

		// Pitch envelope
		public EnvelopeFlag PitFlg;				// Bit 0: On 1: Sustain 2: Loop
		public byte PitPts;
		public byte PitSusBeg;
		public byte PitSusEnd;
		public byte PitBeg;
		public byte PitEnd;
		public readonly EnvPt[] PitEnv = new EnvPt[SharedConstant.EnvPoints];
	}
#pragma warning restore 1591
}
