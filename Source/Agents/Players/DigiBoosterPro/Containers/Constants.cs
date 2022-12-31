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
	/// Different constants
	/// </summary>
	internal static class Constants
	{
		public const uint32_t Envelope_Max_Points = 32;
		public const uint16_t Envelope_Disabled = 0xffff;
		public const uint16_t Envelope_Loop_Disabled = 0xffff;
		public const uint16_t Envelope_Sustain_Disabled = 0xffff;

		public const uint16_t Creator_DigiBooster_2 = 2;
		public const uint16_t Creator_DigiBooster_3 = 3;

		public const uint32_t Dsp_Mask_Echo = 0x00000001;
	}
}