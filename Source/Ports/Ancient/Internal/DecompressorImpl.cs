/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Ports.Ancient.Common.Buffers;

namespace Polycode.NostalgicPlayer.Ports.Ancient.Internal
{
    /// <summary>
    /// Wrapper for decompression and detection class
    /// </summary>
    internal class DecompressorImpl
    {
        private readonly Buffer buffer;
        internal readonly Decompressor decompressor;

        /********************************************************************/
        /// <summary>
        /// Constructor
        /// </summary>
        /********************************************************************/
        public DecompressorImpl(Stream crunchedDataStream)
        {
            buffer = new StreamBuffer(crunchedDataStream);
            decompressor = Decompressor.Create(buffer);
        }
    }
}
