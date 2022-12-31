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
	/// Envelope (used for both panning and volume envelopes)
	/// </summary>
	internal class DB3ModuleEnvelope
	{
		public uint16_t InstrumentNumber;		// Number of instrument (from 0)
		public uint16_t NumberOfSections;
		public uint16_t LoopFirst;				// Point number
		public uint16_t LoopLast;				// Point number, loop disabled if 0xffff
		public uint16_t SustainA;				// Point number, disabled if 0xffff
		public uint16_t SustainB;				// Point number, disabled if 0xffff
		public DB3ModuleEnvelopePoint[] Points = new DB3ModuleEnvelopePoint[Constants.Envelope_Max_Points];
	}
}
