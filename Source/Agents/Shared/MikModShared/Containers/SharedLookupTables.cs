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
	/// <summary>
	/// Holds different lookup tables
	/// </summary>
	public static class SharedLookupTables
	{
		/// <summary>
		/// Is an array over how many bytes each operand use
		/// </summary>
		public static readonly ushort[] UniOperands = new ushort[(int)Command.UniLast]
		{
			0,			// Not used
			1,			// UniNote
			1,			// UniInstrument
			1,			// UniPtEffect0
			1,			// UniPtEffect1
			1,			// UniPtEffect2
			1,			// UniPtEffect3
			1,			// UniPtEffect4
			1,			// UniPtEffect5
			1,			// UniPtEffect6
			1,			// UniPtEffect7
			1,			// UniPtEffect8
			1,			// UniPtEffect9
			1,			// UniPtEffectA
			1,			// UniPtEffectB
			1,			// UniPtEffectC
			1,			// UniPtEffectD
			1,			// UniPtEffectE
			1,			// UniPtEffectF
			1,			// UniS3MEffectA
			1,			// UniS3MEffectD
			1,			// UniS3MEffectE
			1,			// UniS3MEffectF
			1,			// UniS3MEffectI
			1,			// UniS3MEffectQ
			1,			// UniS3MEffectR
			1,			// UniS3MEffectT
			1,			// UniS3MEffectU
			0,			// UniKeyOff
			1,			// UniKeyFade
			2,			// UniVolEffects
			1,			// UniXmEffect4
			1,			// UniXmEffect6
			1,			// UniXmEffectA
			1,			// UniXmEffectE1
			1,			// UniXmEffectE2
			1,			// UniXmEffectEA
			1,			// UniXmEffectEB
			1,			// UniXmEffectG
			1,			// UniXmEffectH
			1,			// UniXmEffectL
			1,			// UniXmEffectP
			1,			// UniXmEffectX1
			1,			// UniXmEffectX2
			1,			// UniItEffectG
			1,			// UniItEffectH
			1,			// UniItEffectI
			1,			// UniItEffectM
			1,			// UniItEffectN
			1,			// UniItEffectP
			1,			// UniItEffectT
			1,			// UniItEffectU
			1,			// UniItEffectW
			1,			// UniItEffectY
			2,			// UniItEffectZ
			1,			// UniItEffectS0
			2,			// UniUltEffect9
			2,			// UniMedSpeed
			0,			// UniMedEffectF1
			0,			// UniMedEffectF2
			0,			// UniMedEffectF3
			2,			// UniOktArp
			0,			// Not used
			1,			// UniS3MEffectH
			1,			// UniItEffectH_Old
			1,			// UniItEffectU_Old
			1,			// UniGdmEffect4
			1,			// UniGdmEffect7
			1,			// UniGdmEffect14
			2,			// UniMedEffectVib
			0,			// UniMedEffectFD
			1,			// UniMedEffect16
			1,			// UniMedEffect18
			1,			// UniMedEffect1E
			1			// UniMedEffect1F
		};
	}
}
