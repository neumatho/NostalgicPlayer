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
	/// EnvPr structure
	/// </summary>
	public struct EnvPr
	{
		public EnvelopeFlag Flg;				// Envelope flag
		public byte Pts;						// Number of envelope points
		public byte SusBeg;						// Envelope sustain index begin
		public byte SusEnd;						// Envelope sustain index end
		public bool SusActive;					// Indicate if sustain is active (no interpolation)
		public byte Beg;						// Envelope loop begin
		public byte End;						// Envelope loop end
		public short P;							// Current envelope counter
		public ushort Index;					// Envelope index for the point after the current one
		public short LastValue;					// The last calculated value
		public EnvPt[] Env;						// Envelope points
	}
#pragma warning restore 1591
}
