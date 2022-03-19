/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// Tempo class
	/// </summary>
	internal class Tempo
	{
		public readonly LimVar<ushort> tempo = new(1, 240);
		public readonly LimVar<ushort> ticksPerLine = new(1, 32);
		public readonly LimVar<ushort> linesPerBeat = new(1, 32);
		public bool bpm;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Tempo()
		{
			tempo.Value = 125;
			ticksPerLine.Value = 6;
			linesPerBeat.Value = 4;
			bpm = true;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Tempo(Tempo newTempo)
		{
			tempo.Value = newTempo.tempo.Value;
			ticksPerLine.Value = newTempo.ticksPerLine.Value;
			linesPerBeat.Value = newTempo.linesPerBeat.Value;
			bpm = newTempo.bpm;
		}
	}
}
