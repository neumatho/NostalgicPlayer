/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.Containers
{
	/// <summary>
	/// A single frame information
	/// </summary>
	internal class Frame
	{
		public int Stereo;
		public int Lsf;
		public bool Mpeg25;
		public int Lay;
		public int ErrorProtection;
		public int BitRateIndex;
		public int SamplingFrequency;
		public int Padding;
		public int Extension;
		public Mode Mode;
		public int ModeExt;
		public int Copyright;
		public int Original;
		public int Emphasis;
		public int FrameSize;
		public int PadSize;
		public int SideInfoSize;
	}
}
