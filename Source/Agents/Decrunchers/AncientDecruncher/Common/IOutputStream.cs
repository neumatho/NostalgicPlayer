/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common
{
	/// <summary>
	/// Different output streams implements this interface
	/// </summary>
	internal interface IOutputStream
	{
		/// <summary>
		/// Indicate if the buffer has been filled or not
		/// </summary>
		bool Eof { get; }

		/// <summary>
		/// Return the current position
		/// </summary>
		uint GetOffset();

		/// <summary>
		/// Write a single byte to the output
		/// </summary>
		void WriteByte(uint value);

		/// <summary>
		/// Copy a block of already written data in the buffer
		/// </summary>
		byte Copy(uint distance, uint count);
	}
}
