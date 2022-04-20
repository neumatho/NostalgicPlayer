/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Extra information that visuals can use
	/// </summary>
	public class VisualInfo
	{
		/********************************************************************/
		/// <summary>
		/// The sample number used to play the note, starting from 0
		/// </summary>
		/********************************************************************/
		public ushort SampleNumber
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// The note being played from 0 to 119
		/// </summary>
		/********************************************************************/
		public byte NoteNumber
		{
			get; set;
		}
	}
}
