/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Demuxer.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public struct FF_Asf_Guid
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FF_Asf_Guid(uint8_t[] data)
		{
			if (data.Length != 16)
				throw new ArgumentException("data not in right size");

			Data = ArrayHelper.CloneArray(data);
		}



		/********************************************************************/
		/// <summary>
		/// The GUID data
		/// </summary>
		/********************************************************************/
		public readonly CPointer<uint8_t> Data;
	}
}
