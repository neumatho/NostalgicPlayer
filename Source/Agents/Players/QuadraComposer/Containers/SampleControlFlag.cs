﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.QuadraComposer.Containers
{
	/// <summary>
	/// Different control flag that can be used on a sample
	/// </summary>
	[Flags]
	internal enum SampleControlFlag : byte
	{
		None = 0,
		Loop = 0x1
	}
}
