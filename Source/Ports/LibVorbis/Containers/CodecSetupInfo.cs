/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// CodecSetupInfo contains all the setup information for a specific to the
	/// specific compression/decompression mode in progress (eg,
	/// psychoacoustic settings, channel setup, options, codebook etc).
	/// </summary>
	internal class CodecSetupInfo : ICodecSetup
	{
		// Vorbis supports only short and long blocks, but allows the
		// encoder to choose the sizes
		public readonly c_long[] blocksizes = new c_long[2];

		// Modes are the primary means of supporting on-the-fly different
		// blocksizes, different channel mappings (LR or M/A),
		// different residue backends, etc. Each mode consists of a
		// blocksize flag and a mapping (along with the mapping setup)
		public c_int modes;
		public c_int maps;
		public c_int floors;
		public c_int residues;
		public c_int books;

		public readonly VorbisInfoMode[] mode_param = new VorbisInfoMode[64];
		public readonly c_int[] map_type = new c_int[64];
		public readonly IVorbisInfoMapping[] map_param = new IVorbisInfoMapping[64];
		public readonly c_int[] floor_type = new c_int[64];
		public readonly IVorbisInfoFloor[] floor_param = new IVorbisInfoFloor[64];
		public readonly c_int[] residue_type = new c_int[64];
		public readonly IVorbisInfoResidue[] residue_param = new IVorbisInfoResidue[64];
		public readonly StaticCodebook[] book_param = new StaticCodebook[256];
		public Codebook[] fullbooks;

		public c_int halfrate_flag = 0;         // Painless downsample for decode
	}
}
