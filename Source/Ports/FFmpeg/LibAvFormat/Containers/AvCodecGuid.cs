/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Demuxer.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvCodecGuid
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AvCodecGuid(AvCodecId id, FF_Asf_Guid guid)
		{
			Id = id;
			Guid = guid;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public AvCodecId Id { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public FF_Asf_Guid Guid { get; }
	}
}
