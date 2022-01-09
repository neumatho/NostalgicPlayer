/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Holds information about a single position
	/// </summary>
	public class PositionInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor (used by module players)
		/// </summary>
		/********************************************************************/
		public PositionInfo(byte speed, ushort bpm, TimeSpan time, object extra = null)
		{
			Speed = speed;
			Bpm = bpm;
			Time = time;
			ExtraInfo = extra;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor (used by sample players)
		/// </summary>
		/********************************************************************/
		public PositionInfo(TimeSpan time, object extra = null)
		{
			Time = time;
			ExtraInfo = extra;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the speed at the current position
		/// </summary>
		/********************************************************************/
		public byte Speed
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the BPM at the current position
		/// </summary>
		/********************************************************************/
		public ushort Bpm
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the time at the current position
		/// </summary>
		/********************************************************************/
		public TimeSpan Time
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds some extra information if needed
		/// </summary>
		/********************************************************************/
		public object ExtraInfo
		{
			get;
		}
	}
}
