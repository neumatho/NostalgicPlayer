/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class EnvelopeInterpolation
	{
		public uint16_t Index;					// Index to envelopes table, set in MSynth_Instrument(), -1 if no envelope
		public uint16_t TickCounter;			// Ticks left in section
		public uint16_t Section;				// Current section
		public uint16_t SustainA;				// Set by trigger, cleared to 0xffff with keyoff
		public uint16_t SustainB;				// Set by trigger, cleared to 0xffff with keyoff
		public uint16_t LoopEnd;				// Set by trigger, cleared to 0xffff with keyoff
	}
}
