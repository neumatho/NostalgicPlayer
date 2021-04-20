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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	/// <summary>
	/// All the different commands
	/// </summary>
	public enum Command : byte
	{
		// Simple note
		UniNote = 1,

		// Instrument change
		UniInstrument,

		// ProTracker effects
		UniPtEffect0,				// Arpeggio
		UniPtEffect1,				// Porta up
		UniPtEffect2,				// Porta down
		UniPtEffect3,				// Porta to note
		UniPtEffect4,				// Vibrato
		UniPtEffect5,				// Dual effect 3+A
		UniPtEffect6,				// Dual effect 4+A
		UniPtEffect7,				// Tremolo
		UniPtEffect8,				// Pan
		UniPtEffect9,				// Sample offset
		UniPtEffectA,				// Volume slide
		UniPtEffectB,				// Pattern jump
		UniPtEffectC,				// Set volume
		UniPtEffectD,				// Pattern break
		UniPtEffectE,				// Extended effects
		UniPtEffectF,				// Set speed

		// Scream Tracker effects
		UniS3MEffectA,				// Set speed
		UniS3MEffectD,				// Volume slide
		UniS3MEffectE,				// Porta down
		UniS3MEffectF,				// Porta up
		UniS3MEffectI,				// Tremor
		UniS3MEffectQ,				// Retrig
		UniS3MEffectR,				// Tremolo
		UniS3MEffectT,				// Set tempo
		UniS3MEffectU,				// Fine vibrato
		UniKeyOff,					// Note off

		// Fast Tracker effects
		UniKeyFade,					// Note fade
		UniVolEffects,				// Volume column effects
		UniXmEffect4,				// Vibrato
		UniXmEffect6,				// Dual effect 4+A
		UniXmEffectA,				// Volume slide
		UniXmEffectE1,				// Fine porta up
		UniXmEffectE2,				// Fine porta down
		UniXmEffectEA,				// Fine volume slide up
		UniXmEffectEB,				// Fine volume slide down
		UniXmEffectG,				// Set global volume
		UniXmEffectH,				// Global volume slide
		UniXmEffectL,				// Set envelope position
		UniXmEffectP,				// Pan slide
		UniXmEffectX1,				// Extra fine porta up
		UniXmEffectX2,				// Extra fine porta down

		// Impulse Tracker effects
		UniItEffectG,				// Porta to note
		UniItEffectH,				// Vibrato
		UniItEffectI,				// Tremor (xy not incremented)
		UniItEffectM,				// Set channel volume
		UniItEffectN,				// Slide / fine slide channel volume
		UniItEffectP,				// Slide / fine slide channel panning
		UniItEffectT,				// Slide tempo
		UniItEffectU,				// Fine vibrato
		UniItEffectW,				// Slide / fine slide global volume
		UniItEffectY,				// Panbrello
		UniItEffectZ,				// Resonant filters
		UniItEffectS0,

		// UltraTracker effects
		UniUltEffect9,				// Sample fine offset

		// OctaMED effects
		UniMedSpeed,
		UniMedEffectF1,				// Play note twice
		UniMedEffectF2,				// Delay note
		UniMedEffectF3,				// Play note three times

		// Oktalyzer effects
		UniOktArp,					// Arpeggio

		UniLast
	}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
