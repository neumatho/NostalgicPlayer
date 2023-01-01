/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using SharpCompress.Compressors;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats.Streams
{
	/// <summary>
	/// Wrapper class to the SharpCompress BZip2Stream
	/// </summary>
	internal class BZip2Stream : NoLengthStream
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public BZip2Stream(string agentName, Stream wrapperStream) : base(agentName, wrapperStream, false)
		{
		}

		#region NoLengthStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the stream holding the crunched data
		/// </summary>
		/********************************************************************/
		protected override Stream OpenCrunchedDataStream()
		{
			return new SharpCompress.Compressors.BZip2.BZip2Stream(wrapperStream, CompressionMode.Decompress, false);
		}
		#endregion
	}
}
