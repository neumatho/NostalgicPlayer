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
	/// 
	/// </summary>
	public static class Info
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void V_ReadString(OggPack o, CPointer<byte> buf, c_int bytes)
		{
			while (bytes-- != 0)
				buf[0, 1] = (byte)o.Read(8);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Vorbis_Comment_Init(VorbisComment vc)
		{
			vc.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Vorbis_Comment_Clear(VorbisComment vc)
		{
			if (vc != null)
			{
				if (vc.user_comments.IsNotNull)
				{
					for (c_long i = 0; i < vc.comments; i++)
					{
						if (vc.user_comments[i].IsNotNull)
							Memory.Ogg_Free(vc.user_comments[i]);
					}

					Memory.Ogg_Free(vc.user_comments);
				}

				if (vc.comment_lengths.IsNotNull)
					Memory.Ogg_Free(vc.comment_lengths);

				if (vc.vendor.IsNotNull)
					Memory.Ogg_Free(vc.vendor);

				vc.Clear();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Blocksize 0 is guaranteed to be short, 1 is guaranteed to be
		/// long. They may be equal, but short will never be greater than
		/// long
		/// </summary>
		/********************************************************************/
		public static c_int Vorbis_Info_Blocksize(VorbisInfo vi, c_int zo)
		{
			CodecSetupInfo ci = vi.codec_setup as CodecSetupInfo;

			return ci != null ? (c_int)ci.blocksizes[zo] : -1;
		}



		/********************************************************************/
		/// <summary>
		/// Used by synthesis, which has a full, allocated vi
		/// </summary>
		/********************************************************************/
		public static void Vorbis_Info_Init(VorbisInfo vi)
		{
			vi.Clear();

			vi.codec_setup = new CodecSetupInfo();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Vorbis_Info_Clear(VorbisInfo vi)
		{
			CodecSetupInfo ci = vi.codec_setup as CodecSetupInfo;

			if (ci != null)
			{
				for (c_int i = 0; i < ci.modes; i++)
				{
					if (ci.mode_param[i] != null)
						ci.mode_param[i] = null;
				}

				for (c_int i = 0; i < ci.maps; i++)			// Unpack does the range checking
				{
					if (ci.map_param[i] != null)			// This may be cleaning up an aborted unpack, in which case the below type cannot be trusted
						Registry.mapping_P[ci.map_type[i]].Free_Info(ci.map_param[i]);
				}

				for (c_int i = 0; i < ci.floors; i++)		// Unpack does the range checking
				{
					if (ci.floor_param[i] != null)			// This may be cleaning up an aborted unpack, in which case the below type cannot be trusted
						Registry.floor_P[ci.floor_type[i]].Free_Info(ci.floor_param[i]);
				}

				for (c_int i = 0; i < ci.residues; i++)		// Unpack does the range checking
				{
					if (ci.residue_param[i] != null)		// This may be cleaning up an aborted unpack, in which case the below type cannot be trusted
						Registry.residue_P[ci.residue_type[i]].Free_Info(ci.residue_param[i]);
				}

				for (c_int i = 0; i < ci.books; i++)
				{
					if (ci.book_param[i] != null)
					{
						// Knows if the book was not allocated
						Sharedbook.Vorbis_Staticbook_Destroy(ci.book_param[i]);
					}

					if (ci.fullbooks != null)
						Sharedbook.Vorbis_Book_Clear(ci.fullbooks[i]);
				}

				if (ci.fullbooks != null)
					ci.fullbooks = null;
			}

			vi.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Vorbis_Unpack_Info(VorbisInfo vi, OggPack opb)
		{
			CodecSetupInfo ci = vi.codec_setup as CodecSetupInfo;

			if (ci == null)
				return (c_int)VorbisError.Fault;

			vi.version = (c_int)opb.Read(32);
			if (vi.version != 0)
				return (c_int)VorbisError.Version;

			vi.channels = (c_int)opb.Read(8);
			vi.rate = opb.Read(32);

			vi.bitrate_upper = opb.Read(32);
			vi.bitrate_nominal = opb.Read(32);
			vi.bitrate_lower = opb.Read(32);

			c_int bs = (c_int)opb.Read(4);
			if (bs < 0)
				goto Err_Out;

			ci.blocksizes[0] = 1 << bs;

			bs = (c_int)opb.Read(4);
			if (bs < 0)
				goto Err_Out;

			ci.blocksizes[1] = 1 << bs;

			if (vi.rate < 1)
				goto Err_Out;

			if (vi.channels < 1)
				goto Err_Out;

			if (ci.blocksizes[0] < 64)
				goto Err_Out;

			if (ci.blocksizes[1] < ci.blocksizes[0])
				goto Err_Out;

			if (ci.blocksizes[1] > 8192)
				goto Err_Out;

			if (opb.Read(1) != 1)	// EOP check
				goto Err_Out;

			return 0;

			Err_Out:
			Vorbis_Info_Clear(vi);

			return (c_int)VorbisError.BadHeader;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Vorbis_Unpack_Comment(VorbisComment vc, OggPack opb)
		{
			c_int vendorlen = (c_int)opb.Read(32);
			if (vendorlen < 0)
				goto Err_Out;

			if (vendorlen > (opb.Buffer.Storage - 8))
				goto Err_Out;

			vc.vendor = Memory.Ogg_CAlloc<byte>((size_t)vendorlen + 1);
			V_ReadString(opb, vc.vendor, vendorlen);

			c_int i = (c_int)opb.Read(32);
			if (i < 0)
				goto Err_Out;

			if (i > ((opb.Buffer.Storage - opb.Bytes()) >> 2))
				goto Err_Out;

			vc.comments = i;
			vc.user_comments = Memory.Ogg_CAlloc<CPointer<byte>>((size_t)vc.comments + 1);
			vc.comment_lengths = Memory.Ogg_CAlloc<c_int>((size_t)vc.comments + 1);

			for (i = 0; i < vc.comments; i++)
			{
				c_int len = (c_int)opb.Read(32);
				if (len < 0)
					goto Err_Out;

				if (len > (opb.Buffer.Storage - opb.Bytes()))
					goto Err_Out;

				vc.comment_lengths[i] = len;
				vc.user_comments[i] = Memory.Ogg_CAlloc<byte>((size_t)len + 1);
				V_ReadString(opb, vc.user_comments[i], len);
			}

			if (opb.Read(1) != 1)	// EOP check
				goto Err_Out;

			return 0;

			Err_Out:
			Vorbis_Comment_Clear(vc);

			return (c_int)VorbisError.BadHeader;
		}



		/********************************************************************/
		/// <summary>
		/// All of the real encoding details are here. The modes, books,
		/// everything
		/// </summary>
		/********************************************************************/
		private static c_int Vorbis_Unpack_Books(VorbisInfo vi, OggPack opb)
		{
			CodecSetupInfo ci = (CodecSetupInfo)vi.codec_setup;

			// Codebooks
			ci.books = (c_int)opb.Read(8) + 1;
			if (ci.books <= 0)
				goto Err_Out;

			for (c_int i = 0; i < ci.books; i++)
			{
				ci.book_param[i] = CodebookImpl.Vorbis_Staticbook_Unpack(opb);
				if (ci.book_param[i] == null)
					goto Err_Out;
			}

			// Time backend settings; hooks are unused
			{
				c_int times = (c_int)opb.Read(6) + 1;
				if (times <= 0)
					goto Err_Out;

				for (c_int i = 0; i < times; i++)
				{
					c_int test = (c_int)opb.Read(16);
					if ((test < 0) || (test >= Constants.Vi_TimeB))
						goto Err_Out;
				}
			}

			// Floor backend settings
			ci.floors = (c_int)opb.Read(6) + 1;
			if (ci.floors <= 0)
				goto Err_Out;

			for (c_int i = 0; i < ci.floors; i++)
			{
				ci.floor_type[i] = (c_int)opb.Read(16);
				if ((ci.floor_type[i] < 0) || (ci.floor_type[i] >= Constants.Vi_FloorB))
					goto Err_Out;

				ci.floor_param[i] = Registry.floor_P[ci.floor_type[i]].Unpack(vi, opb);
				if (ci.floor_param[i] == null)
					goto Err_Out;
			}

			// Residue backend settings
			ci.residues = (c_int)opb.Read(6) + 1;
			if (ci.residues <= 0)
				goto Err_Out;

			for (c_int i = 0; i < ci.residues; i++)
			{
				ci.residue_type[i] = (c_int)opb.Read(16);
				if ((ci.residue_type[i] < 0) || (ci.residue_type[i] >= Constants.Vi_ResB))
					goto Err_Out;

				ci.residue_param[i] = Registry.residue_P[ci.residue_type[i]].Unpack(vi, opb);
				if (ci.residue_param[i] == null)
					goto Err_Out;
			}

			// Map backend settings
			ci.maps = (c_int)opb.Read(6) + 1;
			if (ci.maps <= 0)
				goto Err_Out;

			for (c_int i = 0; i < ci.maps; i++)
			{
				ci.map_type[i] = (c_int)opb.Read(16);
				if ((ci.map_type[i] < 0) || (ci.map_type[i] >= Constants.Vi_MapB))
					goto Err_Out;

				ci.map_param[i] = Registry.mapping_P[ci.map_type[i]].Unpack(vi, opb);
				if (ci.map_param[i] == null)
					goto Err_Out;
			}

			// Mode settings
			ci.modes = (c_int)opb.Read(6) + 1;
			if (ci.modes <= 0)
				goto Err_Out;

			for (c_int i = 0; i < ci.modes; i++)
			{
				ci.mode_param[i] = new VorbisInfoMode();
				ci.mode_param[i].blockflag = (c_int)opb.Read(1);
				ci.mode_param[i].windowtype = (c_int)opb.Read(16);
				ci.mode_param[i].transformtype = (c_int)opb.Read(16);
				ci.mode_param[i].mapping = (c_int)opb.Read(8);

				if (ci.mode_param[i].windowtype >= Constants.Vi_WindowB)
					goto Err_Out;

				if (ci.mode_param[i].transformtype >= Constants.Vi_TransformB)
					goto Err_Out;

				if (ci.mode_param[i].mapping >= ci.maps)
					goto Err_Out;

				if (ci.mode_param[i].mapping < 0)
					goto Err_Out;
			}

			if (opb.Read(1) != 1)	// Top level EOP check
				goto Err_Out;

			return 0;

			Err_Out:
			Vorbis_Info_Clear(vi);

			return (c_int)VorbisError.BadHeader;
		}



		/********************************************************************/
		/// <summary>
		/// Is this packet a vorbis ID header?
		/// </summary>
		/********************************************************************/
		public static c_int Vorbis_Synthesis_Idheader(Ogg_Packet op)
		{
			if (op != null)
			{
				OggPack.ReadInit(out OggPack opb, op.Packet, (c_int)op.Bytes);

				if (!op.Bos)
					return 0;	// Not the initial packet

				if (opb.Read(8) != 1)
					return 0;   // Not an ID header

				byte[] buffer = new byte[6];
				V_ReadString(opb, buffer, 6);

				if (CMemory.MemCmp(buffer, "vorbis", 6) != 0)
					return 0;	// Not vorbis

				return 1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// The Vorbis header is in three packets; the initial small packet
		/// in the first page that identifies basic parameters, a second
		/// packet with bitstream comments and a third packet that holds the
		/// codebook
		/// </summary>
		/********************************************************************/
		public static c_int Vorbis_Synthesis_Headerin(VorbisInfo vi, VorbisComment vc, Ogg_Packet op)
		{
			if (op != null)
			{
				OggPack.ReadInit(out OggPack opb, op.Packet, (c_int)op.Bytes);

				// Which of the three types of header is this?
				// Also verify header-ness, vorbis
				{
					byte[] buffer = new byte[6];

					c_int packtype = (c_int)opb.Read(8);
					V_ReadString(opb, buffer, 6);

					if (CMemory.MemCmp(buffer, "vorbis", 6) != 0)
					{
						// Not a vorbis header
						return (c_int)VorbisError.NotVorbis;
					}

					switch (packtype)
					{
						// Least significant *bit* is read first
						case 0x01:
						{
							if (!op.Bos)
							{
								// Not initial packet
								return (c_int)VorbisError.BadHeader;
							}

							if (vi.rate != 0)
							{
								// Previously initialized info header
								return (c_int)VorbisError.BadHeader;
							}

							return Vorbis_Unpack_Info(vi, opb);
						}

						// Least significant *bit* is read first
						case 0x03:
						{
							if (vi.rate == 0)
							{
								// Um... we didn't get the initial header
								return (c_int)VorbisError.BadHeader;
							}

							if (vc.vendor.IsNotNull)
							{
								// Previously initialized comment header
								return (c_int)VorbisError.BadHeader;
							}

							return Vorbis_Unpack_Comment(vc, opb);
						}

						// Least significant *bit* is read first
						case 0x05:
						{
							if ((vi.rate == 0) || vc.vendor.IsNull)
							{
								// Um... we didn't get the initial header or comments yet
								return (c_int)VorbisError.BadHeader;
							}

							if (vi.codec_setup == null)
							{
								// Improperly initialized vorbis_info
								return (c_int)VorbisError.Fault;
							}

							if (((CodecSetupInfo)vi.codec_setup).books > 0)
							{
								// Previously initialized setup header
								return (c_int)VorbisError.BadHeader;
							}

							return Vorbis_Unpack_Books(vi, opb);
						}

						default:
						{
							// Not a valid vorbis header type
							return (c_int)VorbisError.BadHeader;
						}
					}
				}
			}

			return (c_int)VorbisError.BadHeader;
		}
	}
}
