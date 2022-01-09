/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundFx.Containers
{
	/// <summary>
	/// Information about a single playing channel
	/// </summary>
	internal class Channel
	{
		public uint PatternData;
		public sbyte[] Sample;
		public uint SampleLen;
		public uint LoopStart;
		public uint LoopLength;
		public ushort CurrentNote;
		public ushort Volume;
		public short StepValue;
		public ushort StepNote;
		public ushort StepEndNote;
		public ushort SlideControl;
		public bool SlideDirection;
		public ushort SlideParam;
		public ushort SlidePeriod;
		public ushort SlideSpeed;
	}
}
