﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundFx.Containers
{
	/// <summary>
	/// Information about a single playing channel
	/// </summary>
	internal class Channel : IDeepCloneable<Channel>
	{
		public uint PatternData;
		public short SampleNumber;
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

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Channel MakeDeepClone()
		{
			return (Channel)MemberwiseClone();
		}
	}
}
