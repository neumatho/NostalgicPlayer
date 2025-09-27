/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.LibAncient.Common.Buffers;

namespace Polycode.NostalgicPlayer.Ports.LibAncient.Internal.Decompressors
{
	/// <summary>
	/// Base class for all XPK decompressors
	/// </summary>
	internal abstract class XpkDecompressor
	{
		/// <summary>
		/// State class
		/// </summary>
		public abstract class State
		{
		}

		/********************************************************************/
		/// <summary>
		/// Actual decompression
		/// </summary>
		/********************************************************************/
		public abstract void DecompressImpl(Buffer rawData, List<Buffer> previousBuffers);
	}
}
