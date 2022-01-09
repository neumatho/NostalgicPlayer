/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers
{
	/// <summary>
	/// TrackLine structure
	/// </summary>
	internal class TrackLine
	{
		public byte Note;		// The note to play
		public byte Sample;		// The sample
		public Effect Effect;	// The effect to use
		public byte EffectArg;	// Effect argument
	}
}
