/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// Simulate the data union in Id3v2ExtraMeta
	/// </summary>
	public class Id3v2ExtraMetadataUnion
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Id3v2ExtraMetadataUnion(IExtraMetadata metadata)
		{
			Data = metadata;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IExtraMetadata Data;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Id3v2ExtraMetaAPIC APic => (Id3v2ExtraMetaAPIC)Data;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Id3v2ExtraMetaCHAP Chap => (Id3v2ExtraMetaCHAP)Data;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Id3v2ExtraMetaGEOB Geob => (Id3v2ExtraMetaGEOB)Data;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Id3v2ExtraMetaPRIV Priv => (Id3v2ExtraMetaPRIV)Data;
	}
}
