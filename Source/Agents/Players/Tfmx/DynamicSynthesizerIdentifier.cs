/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Dns;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx
{
	/// <summary>
	/// Helper class to identify the different formats
	/// </summary>
	internal static class DynamicSynthesizerIdentifier
	{
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public static readonly string[] FileExtensions = [ "dns" ];



		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is.
		///
		/// I still use my original detection routine, since the real
		/// detection is merged into the init functionality in
		/// LibTfmxAudioDecoder
		/// </summary>
		/********************************************************************/
		public static (ModuleType moduleType, bool singleFile) TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 256)
				return (ModuleType.Unknown, false);

			// Read the first part of the file
			byte[] buffer = new byte[256];

			moduleStream.Seek(0, SeekOrigin.Begin);
			if (moduleStream.Read(buffer, 0, 256) != 256)
				return (ModuleType.Unknown, false);

			DnsDecoder decoder = new DnsDecoder();
			if (decoder.Detect(buffer.ToPointer(), (uint)buffer.Length))
				return (ModuleType.DynamicSynthesizer, false);

			return (ModuleType.Unknown, false);
		}
	}
}
