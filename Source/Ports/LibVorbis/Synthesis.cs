/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibOgg.Containers;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Internal;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis
{
	/// <summary>
	/// Single-block PCM synthesis
	/// </summary>
	public static class Synthesis
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Vorbis_Synthesis(VorbisBlock vb, Ogg_Packet op)
		{
			VorbisDspState vd = vb != null ? vb.vd : null;
			PrivateState b = vd != null ? (PrivateState)vd.backend_state : null;
			VorbisInfo vi = vd != null ? vd.vi : null;
			CodecSetupInfo ci = vi != null ? (CodecSetupInfo)vi.codec_setup : null;
			OggPack opb = vb != null ? vb.opb : null;

			if ((vd == null) || (b == null) || (vi == null) || (ci == null))// || (opb == null))
				return (c_int)VorbisError.BadPacket;

			// First things first. Make sure decode is ready
			Block.Vorbis_Block_Ripcord(vb);
			OggPack.ReadInit(out opb, op.Packet, (c_int)op.Bytes);
			vb.opb = opb;

			// Check the packet type
			if (opb.Read(1) != 0)
			{
				// Oops. This is not an audio data packet
				return (c_int)VorbisError.NotAudio;
			}

			// Read our mode and pre/post windowsize
			c_int mode = (c_int)opb.Read(b.modebits);
			if (mode == -1)
				return (c_int)VorbisError.BadPacket;

			vb.mode = mode;

			if (ci.mode_param[mode] == null)
				return (c_int)VorbisError.BadPacket;

			vb.W = ci.mode_param[mode].blockflag;

			if (vb.W != 0)
			{
				// This doesn't get mapped through mode selection as it's used
				// only for window selection
				vb.lW = opb.Read(1);
				vb.nW = opb.Read(1);

				if (vb.nW == -1)
					return (c_int)VorbisError.BadPacket;
			}
			else
			{
				vb.lW = 0;
				vb.nW = 0;
			}

			// More setup
			vb.granulepos = op.GranulePos;
			vb.sequence = op.PacketNo;
			vb.eofflag = op.Eos;

			// Alloc pcm passback storage
			vb.pcmend = (c_int)ci.blocksizes[vb.W];
			vb.pcm = new CPointer<c_float>[vi.channels];

			for (c_int i = 0; i < vi.channels; i++)
				vb.pcm[i] = Block.Vorbis_Block_Alloc<c_float>(vb, vb.pcmend);

			// Unpack header enforces range checking
			c_int type = ci.map_type[ci.mode_param[mode].mapping];

			return Registry.mapping_P[type].Inverse(vb, ci.map_param[ci.mode_param[mode].mapping]);
		}



		/********************************************************************/
		/// <summary>
		/// Used to track pcm position without actually performing decode.
		/// Useful for sequential 'fast forward'
		/// </summary>
		/********************************************************************/
		public static c_int Vorbis_Synthesis_Trackonly(VorbisBlock vb, Ogg_Packet op)
		{
			VorbisDspState vd = vb.vd;
			PrivateState b = (PrivateState)vd.backend_state;
			VorbisInfo vi = vd.vi;
			CodecSetupInfo ci = (CodecSetupInfo)vi.codec_setup;

			// First things first. Make sure decode is ready
			Block.Vorbis_Block_Ripcord(vb);
			OggPack.ReadInit(out OggPack opb, op.Packet, (c_int)op.Bytes);
			vb.opb = opb;

			// Check the packet type
			if (opb.Read(1) != 0)
			{
				// Oops. This is not an audio data packet
				return (c_int)VorbisError.NotAudio;
			}

			// Read our mode and pre/post windowsize
			c_int mode = (c_int)opb.Read(b.modebits);
			if (mode == -1)
				return (c_int)VorbisError.BadPacket;

			vb.mode = mode;

			if (ci.mode_param[mode] == null)
				return (c_int)VorbisError.BadPacket;

			vb.W = ci.mode_param[mode].blockflag;

			if (vb.W != 0)
			{
				vb.lW = opb.Read(1);
				vb.nW = opb.Read(1);

				if (vb.nW == -1)
					return (c_int)VorbisError.BadPacket;
			}
			else
			{
				vb.lW = 0;
				vb.nW = 0;
			}

			// More setup
			vb.granulepos = op.GranulePos;
			vb.sequence = op.PacketNo;
			vb.eofflag = op.Eos;

			// No pcm
			vb.pcmend = 0;
			vb.pcm = null;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_long Vorbis_Packet_Blocksize(VorbisInfo vi, Ogg_Packet op)
		{
			CodecSetupInfo ci = vi.codec_setup as CodecSetupInfo;

			if ((ci == null) || (ci.modes <= 0))
			{
				// Codec setup not properly initialized
				return (c_long)VorbisError.Fault;
			}

			OggPack.ReadInit(out OggPack opb, op.Packet, (c_int)op.Bytes);

			// Check the packet type
			if (opb.Read(1) != 0)
			{
				// Oops. This is not an audio data packet
				return (c_long)VorbisError.NotAudio;
			}

			// Read our mode and pre/post windowsize
			c_int mode = (c_int)opb.Read(Sharedbook.Ov_ILog((ogg_uint32_t)ci.modes - 1));
			if ((mode == -1) || (ci.mode_param[mode] == null))
				return (c_long)VorbisError.BadPacket;

			return ci.blocksizes[ci.mode_param[mode].blockflag];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Vorbis_Synthesis_Halfrate_P(VorbisInfo vi)
		{
			CodecSetupInfo ci = (CodecSetupInfo)vi.codec_setup;

			return ci.halfrate_flag;
		}
	}
}
