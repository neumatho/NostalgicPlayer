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
	/// An instrument
	/// </summary>
	internal class DB3ModuleInstrument
	{
		public string Name;
		public uint16_t Volume;					// 0 to 64 (including)
		public int16_t Panning;					// -128 full left, +128 full right
		public InstrumentType Type;
		public uint16_t VolumeEnvelope;			// Index of volume envelope, 0xffff if none
		public uint16_t PanningEnvelope;		// Index of volume envelope, 0xffff if none
	}
}
