/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers.Chunks
{
	/// <summary>
	/// Holds data from the VERS chunk
	/// </summary>
	internal class VersChunk : IChunk
	{
		public ushort Cwtv;					// Created with tracker
		public ushort Cmwt;					// Compatible with tracker
		public byte[] CreatedWithTracker;	// Name of tracker (XM only)
	}
}
