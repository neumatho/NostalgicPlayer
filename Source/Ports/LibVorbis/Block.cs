/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Internal;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis
{
	/* PCM accumulator examples (not exhaustive):

	 <-------------- lW ---------------->
	                   <--------------- W ---------------->
	:            .....|.....       _______________         |
	:        .'''     |     '''_---      |       |\        |
	:.....'''         |_____--- '''......|       | \_______|
	:.................|__________________|_______|__|______|
	                  |<------ Sl ------>|      > Sr <     |endW
	                  |beginSl           |endSl  |  |endSr
	                  |beginW            |endlW  |beginSr


	                      |< lW >|
	                   <--------------- W ---------------->
	                  |   |  ..  ______________            |
	                  |   | '  `/        |     ---_        |
	                  |___.'___/`.       |         ---_____|
	                  |_______|__|_______|_________________|
	                  |      >|Sl|<      |<------ Sr ----->|endW
	                  |       |  |endSl  |beginSr          |endSr
	                  |beginW |  |endlW
	                  mult[0] |beginSl                     mult[n]

	 <-------------- lW ----------------->
	                          |<--W-->|
	:            ..............  ___  |   |
	:        .'''             |`/   \ |   |
	:.....'''                 |/`....\|...|
	:.........................|___|___|___|
	                          |Sl |Sr |endW
	                          |   |   |endSr
	                          |   |beginSr
	                          |   |endSl
	                          |beginSl
	                          |beginW
	*/
	/// <summary></summary>
	public static class Block
	{
		private const int Word_Align = 8;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Vorbis_Block_Init(VorbisDspState v, VorbisBlock vb)
		{
			vb.Clear();

			vb.vd = v;
			vb.localalloc = 0;
			vb.localstore.SetToNull();

			if (v.analysisp)
			{
				VorbisBlockInternal vbi = new VorbisBlockInternal();
				vb.@internal = vbi;
				vbi.ampmax = -9999;

				for (c_int i = 0; i < Constants.PacketBlobs; i++)
				{
					OggPack.WriteInit(out vbi.packetblob[i]);

					if (i == (Constants.PacketBlobs / 2))
						vb.opb = vbi.packetblob[i];
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// TNE: The original code of this method and Vorbis_Block_Ripcord
		/// are still kept as reference below. Because C# does not have any
		/// pointers, it is impossible to do an allocation of pointers to
		/// an array as needed at _01inverse() in Residue.cs.
		///
		/// I have then decided to allocate memory everytime and do not
		/// reused allocated memory as the original code does.
		/// </summary>
		/********************************************************************/
		internal static CPointer<T> Vorbis_Block_Alloc<T>(VorbisBlock vb, c_long items)
		{
			return new CPointer<T>((c_int)items);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static CPointer<byte> Vorbis_Block_Alloc(VorbisBlock vb, c_long bytes)
		{
			// TNE: This method as well as Vorbis_Block_Ripcord has been modified,
			//      since some part of the original code is not possible in C#.
			//      Therefore it is not as memory efficient as the original code
			bytes = (bytes + (Word_Align - 1)) & ~(Word_Align - 1);

			if ((bytes + vb.localtop) > vb.localalloc)
			{
				// Can't just _ogg_realloc... there are outstanding pointers
				if (vb.localstore.IsNotNull)
				{
					AllocChain link = new AllocChain();

					vb.totaluse += vb.localtop;

					link.next = vb.reap;
					link.ptr = vb.localstore;
					vb.reap = link;
				}

				// Highly conservative
				vb.localalloc = bytes;
				vb.localstore = Memory.Ogg_MAlloc<byte>((size_t)vb.localalloc);
				vb.localtop = 0;
			}

			{
				CPointer<byte> ret = vb.localstore + vb.localtop;
				vb.localtop += bytes;

				return ret;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static void Vorbis_Block_Ripcord(VorbisBlock vb)
		{
			// TNE Do nothing. Original code is kept below as a reference
/*			// Reap the chain
			AllocChain reap = vb.reap;

			while (reap != null)
			{
				AllocChain next = reap.next;
	
				Memory.Ogg_Free(reap.ptr);
				reap.Clear();

				reap = next;
			}

			// Consolidate storage
			if (vb.totaluse != 0)
			{
				vb.localstore = Memory.Ogg_Realloc(vb.localstore, (size_t)(vb.totaluse + vb.localalloc));
				vb.localalloc += vb.totaluse;
				vb.totaluse = 0;
			}

			// Pull the ripcord
			vb.localtop = 0;
			vb.reap = null;*/
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Vorbis_Block_Clear(VorbisBlock vb)
		{
			VorbisBlockInternal vbi = vb.@internal as VorbisBlockInternal;

			Vorbis_Block_Ripcord(vb);

			if (vb.localstore.IsNotNull)
				Memory.Ogg_Free(vb.localstore);

			if (vbi != null)
			{
				for (c_int i = 0; i < Constants.PacketBlobs; i++)
				{
					vbi.packetblob[i].WriteClear();

					if (i != (Constants.PacketBlobs / 2))
						vbi.packetblob[i] = null;
				}
			}

			vb.Clear();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Analysis side code, but directly related to blocking. Thus it's
		/// here and not in analysis.c (which is for analysis transforms
		/// only). The init is here because some of it is shared
		/// </summary>
		/********************************************************************/
		private static c_int Vds_Shared_Init(VorbisDspState v, VorbisInfo vi, bool encp)
		{
			CodecSetupInfo ci = vi.codec_setup as CodecSetupInfo;

			if ((ci == null) || (ci.modes <= 0) || (ci.blocksizes[0] < 64) || (ci.blocksizes[1] < ci.blocksizes[0]))
				return 1;

			c_int hs = ci.halfrate_flag;

			v.Clear();

			PrivateState b = new PrivateState();
			v.backend_state = b;

			v.vi = vi;
			b.modebits = Sharedbook.Ov_ILog((ogg_uint32_t)ci.modes - 1);

			b.transform[0] = new IVorbisLookTransform[Constants.Vi_TransformB];
			b.transform[1] = new IVorbisLookTransform[Constants.Vi_TransformB];

			// MDCT is transform 0

			b.transform[0][0] = new MdctLookup();
			b.transform[1][0] = new MdctLookup();

			Mdct.Mdct_Init((MdctLookup)b.transform[0][0], (c_int)ci.blocksizes[0] >> hs);
			Mdct.Mdct_Init((MdctLookup)b.transform[1][0], (c_int)ci.blocksizes[1] >> hs);

			// Vorbis I uses only window type 0.
			// Note that the correct computation below is technically:
			//      b->window[0]=ov_ilog(ci->blocksizes[0]-1)-6;
			//      b->window[1]=ov_ilog(ci->blocksizes[1]-1)-6;
			// but since blocksizes are always powers of two,
			// the below is equivalent
			b.window[0] = Sharedbook.Ov_ILog((ogg_uint32_t)ci.blocksizes[0]) - 7;
			b.window[1] = Sharedbook.Ov_ILog((ogg_uint32_t)ci.blocksizes[1]) - 7;

			if (encp)	// Encode/decode differ here
				throw new NotImplementedException("Vorbis encoding not implemented");
			else
			{
				// Finish the codebooks
				if (ci.fullbooks == null)
				{
					ci.fullbooks = ArrayHelper.InitializeArray<Codebook>(ci.books);

					for (c_int i = 0; i < ci.books; i++)
					{
						if (ci.book_param[i] == null)
							goto AbortBooks;

						if (Sharedbook.Vorbis_Book_Init_Decode(ci.fullbooks[i], ci.book_param[i]) != 0)
							goto AbortBooks;

						// Decode codebooks are now standalone after init
						Sharedbook.Vorbis_Staticbook_Destroy(ci.book_param[i]);
						ci.book_param[i] = null;
					}
				}
			}

			// Initialize the storage vectors. blocksize[1] is small for encode,
			// but the correct size for decode
			v.pcm_storage = (c_int)ci.blocksizes[1];
			v.pcm = new CPointer<c_float>[vi.channels];
			v.pcmret = new CPointer<c_float>[vi.channels];

			{
				for (c_int i = 0; i < vi.channels; i++)
					v.pcm[i] = Memory.Ogg_CAlloc<c_float>((size_t)v.pcm_storage);
			}

			// All 1 (large block) or 0 (small block)
			// Explicitly set for the sake of clarity
			v.lW = 0;	// Previous window size
			v.W = 0;	// Current window size

			// All vector indexes
			v.centerW = ci.blocksizes[1] / 2;

			v.pcm_current = (c_int)v.centerW;

			// Initialize all the backend lookups
			b.flr = new IVorbisLookFloor[ci.floors];
			b.residue = new IVorbisLookResidue[ci.residues];

			for (c_int i = 0; i < ci.floors; i++)
				b.flr[i] = Registry.floor_P[ci.floor_type[i]].Look(v, ci.floor_param[i]);

			for (c_int i = 0; i < ci.residues; i++)
				b.residue[i] = Registry.residue_P[ci.residue_type[i]].Look(v, ci.residue_param[i]);

			return 0;

			AbortBooks:
			for (c_int i = 0; i < ci.books; i++)
			{
				if (ci.book_param[i] != null)
				{
					Sharedbook.Vorbis_Staticbook_Destroy(ci.book_param[i]);
					ci.book_param[i] = null;
				}
			}

			Vorbis_Dsp_Clear(v);

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Vorbis_Dsp_Clear(VorbisDspState v)
		{
			if (v != null)
			{
				VorbisInfo vi = v.vi;
				CodecSetupInfo ci = vi != null ? (CodecSetupInfo)vi.codec_setup : null;
				PrivateState b = v.backend_state as PrivateState;

				if (b != null)
				{
					if (b.transform[0] != null)
					{
						Mdct.Mdct_Clear((MdctLookup)b.transform[0][0]);
						b.transform[0][0] = null;
						b.transform[0] = null;
					}

					if (b.transform[1] != null)
					{
						Mdct.Mdct_Clear((MdctLookup)b.transform[1][0]);
						b.transform[1][0] = null;
						b.transform[1] = null;
					}

					if (b.flr != null)
					{
						if (ci != null)
						{
							for (c_int i = 0; i < ci.floors; i++)
								Registry.floor_P[ci.floor_type[i]].Free_Look(b.flr[i]);
						}

						b.flr = null;
					}

					if (b.residue != null)
					{
						if (ci != null)
						{
							for (c_int i = 0; i < ci.residues; i++)
								Registry.residue_P[ci.residue_type[i]].Free_Look(b.residue[i]);
						}

						b.residue = null;
					}
				}

				if (v.pcm != null)
				{
					if (vi != null)
					{
						for (c_int i = 0; i < vi.channels; i++)
						{
							if (v.pcm[i].IsNotNull)
								Memory.Ogg_Free(v.pcm[i]);
						}
					}

					v.pcm = null;

					if (v.pcmret != null)
						v.pcmret = null;
				}

				v.Clear();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Vorbis_Synthesis_Restart(VorbisDspState v)
		{
			VorbisInfo vi = v.vi;

			if (v.backend_state == null)
				return -1;

			if (vi == null)
				return -1;

			CodecSetupInfo ci = vi.codec_setup as CodecSetupInfo;
			if (ci == null)
				return -1;

			c_int hs = ci.halfrate_flag;

			v.centerW = ci.blocksizes[1] >> (hs + 1);
			v.pcm_current = (c_int)v.centerW >> hs;

			v.pcm_returned = -1;
			v.granulepos = -1;
			v.sequence = -1;
			v.eofflag = false;
			((PrivateState)v.backend_state).sample_count = -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Vorbis_Synthesis_Init(VorbisDspState v, VorbisInfo vi)
		{
			if (Vds_Shared_Init(v, vi, false) != 0)
			{
				Vorbis_Dsp_Clear(v);

				return 1;
			}

			Vorbis_Synthesis_Restart(v);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Unlike the analysis, the window is only partially applied for
		/// each block. The time domain envelope is not yet handled at the
		/// point of calling (as it relies on the previous block)
		/// </summary>
		/********************************************************************/
		public static c_int Vorbis_Synthesis_Blockin(VorbisDspState v, VorbisBlock vb)
		{
			VorbisInfo vi = v.vi;
			CodecSetupInfo ci = (CodecSetupInfo)vi.codec_setup;
			PrivateState b = (PrivateState)v.backend_state;
			c_int hs = ci.halfrate_flag;

			if (vb == null)
				return (c_int)VorbisError.Inval;

			if ((v.pcm_current > v.pcm_returned) && (v.pcm_returned != -1))
				return (c_int)VorbisError.Inval;

			v.lW = v.W;
			v.W = vb.W;
			v.nW = -1;

			if ((v.sequence == -1) || ((v.sequence + 1) != vb.sequence))
			{
				v.granulepos = -1;	// Out of sequence; lose count
				b.sample_count = -1;
			}

			v.sequence = vb.sequence;

			if (vb.pcm != null)	// No pcm to process if vorbis_synthesis_trackonly was called on block
			{
				c_int n = (c_int)ci.blocksizes[v.W] >> (hs + 1);
				c_int n0 = (c_int)ci.blocksizes[0] >> (hs + 1);
				c_int n1 = (c_int)ci.blocksizes[1] >> (hs + 1);

				c_int thisCenter;
				c_int prevCenter;

				v.glue_bits += vb.glue_bits;
				v.time_bits += vb.time_bits;
				v.floor_bits += vb.floor_bits;
				v.res_bits += vb.res_bits;

				if (v.centerW != 0)
				{
					thisCenter = n1;
					prevCenter = 0;
				}
				else
				{
					thisCenter = 0;
					prevCenter = n1;
				}

				// v->pcm is now used like a two-stage double buffer. We don't want
				// to have to constantly shift *or* adjust memory usage. Don't
				// accept a new block until the old is shifted out
				for (c_int j = 0; j < vi.channels; j++)
				{
					// The overlap/add section
					if (v.lW != 0)
					{
						if (v.W != 0)
						{
							// Large/large
							c_float[] w = Window.Vorbis_Window_Get(b.window[1] - hs);
							CPointer<c_float> pcm = v.pcm[j] + prevCenter;
							CPointer<c_float> p = vb.pcm[j];

							for (c_int i = 0; i < n1; i++)
								pcm[i] = pcm[i] * w[n1 - i - 1] + p[i] * w[i];
						}
						else
						{
							// Large/small
							c_float[] w = Window.Vorbis_Window_Get(b.window[0] - hs);
							CPointer<c_float> pcm = v.pcm[j] + prevCenter + n1 / 2 - n0 / 2;
							CPointer<c_float> p = vb.pcm[j];

							for (c_int i = 0; i < n0; i++)
								pcm[i] = pcm[i] * w[n0 - i - 1] + p[i] * w[i];
						}
					}
					else
					{
						if (v.W != 0)
						{
							// Small/large
							c_float[] w = Window.Vorbis_Window_Get(b.window[0] - hs);
							CPointer<c_float> pcm = v.pcm[j] + prevCenter;
							CPointer<c_float> p = vb.pcm[j] + n1 / 2 - n0 / 2;

							c_int i;
							for (i = 0; i < n0; i++)
								pcm[i] = pcm[i] * w[n0 - i - 1] + p[i] * w[i];

							for (; i < (n1 / 2 + n0 / 2); i++)
								pcm[i] = p[i];
						}
						else
						{
							// Small/small
							c_float[] w = Window.Vorbis_Window_Get(b.window[0] - hs);
							CPointer<c_float> pcm = v.pcm[j] + prevCenter;
							CPointer<c_float> p = vb.pcm[j];

							for (c_int i = 0; i < n0; i++)
								pcm[i] = pcm[i] * w[n0 - i - 1] + p[i] * w[i];
						}
					}

					// The copy section
					{
						CPointer<c_float> pcm = v.pcm[j] + thisCenter;
						CPointer<c_float> p = vb.pcm[j] + n;

						for (c_int i = 0; i < n; i++)
							pcm[i] = p[i];
					}
				}

				if (v.centerW != 0)
					v.centerW = 0;
				else
					v.centerW = n1;

				// Deal with initial packet state; we do this using the explicit
				// pcm_returned==-1 flag otherwise we're sensitive to first block
				// being short or long
				if (v.pcm_returned == -1)
				{
					v.pcm_returned = thisCenter;
					v.pcm_current = thisCenter;
				}
				else
				{
					v.pcm_returned = prevCenter;
					v.pcm_current = (c_int)(prevCenter + ((ci.blocksizes[v.lW] / 4 + ci.blocksizes[v.W] / 4) >> hs));
				}
			}

			// Track the frame number... This is for convenience, but also
			// making sure our last packet doesn't end with added padding. If
			// the last packet is partial, the number of samples we'll have to
			// return will be past the vb->granulepos.
			//
			// This is not foolproof! It will be confused if we begin
			// decoding at the last page after a seek or hole. In that case,
			// we don't have a starting point to judge where the last frame
			// is. For this reason, vorbisfile will always try to make sure
			// it reads the last two marked pages in proper sequence
			if (b.sample_count == -1)
				b.sample_count = 0;
			else
				b.sample_count += ci.blocksizes[v.lW] / 4 + ci.blocksizes[v.W] / 4;

			if (v.granulepos == -1)
			{
				if (vb.granulepos != -1)	// Only set if we have a position to set to
				{
					v.granulepos = vb.granulepos;

					// Is this a short page?
					if (b.sample_count > v.granulepos)
					{
						// Corner case; if this is both the first and last audio page,
						// then spec says the end is cut, not beginning
						c_long extra = (c_long)(b.sample_count - vb.granulepos);

						// We use ogg_int64_t for granule positions because a
						// uint64 isn't universally available. Unfortunately,
						// that means granposes can be 'negative' and result in
						// extra being negative
						if (extra < 0)
							extra = 0;

						if (vb.eofflag)
						{
							// Trim the end
							// No preceding granulepos; assume we started at zero (we'd
							// have to in a short single-page stream)
							//
							// Granulepos could be -1 due to a seek, but that would result
							// in a long count, not short count
							//
							// Guard against corrupt/malicious frames that set EOP and
							// a backdated granpos; don't rewind more samples than we
							// actually have
							if (extra > ((v.pcm_current - v.pcm_returned) << hs))
								extra = (v.pcm_current - v.pcm_returned) << hs;

							v.pcm_current = (c_int)(v.pcm_current - (extra >> hs));
						}
						else
						{
							// Trim the beginning
							v.pcm_returned = (c_int)(v.pcm_returned + (extra >> hs));

							if (v.pcm_returned > v.pcm_current)
								v.pcm_returned = v.pcm_current;
						}
					}
				}
			}
			else
			{
				v.granulepos += (ci.blocksizes[v.lW] / 4) + (ci.blocksizes[v.W] / 4);

				if ((v.granulepos != -1) && (v.granulepos != vb.granulepos))
				{
					if (v.granulepos > vb.granulepos)
					{
						c_long extra = (c_long)(v.granulepos - vb.granulepos);

						if (extra != 0)
						{
							if (vb.eofflag)
							{
								// Partial last frame. Strip the extra samples off
								//
								// Guard against corrupt/malicious frames that set EOP and
								// a backdated granpos; don't rewind more samples than we
								// actually have
								if (extra > ((v.pcm_current - v.pcm_returned) << hs))
									extra = (v.pcm_current - v.pcm_returned) << hs;

								// We use ogg_int64_t for granule positions because a
								// uint64 isn't universally available. Unfortunately,
								// that means granposes can be 'negative' and result in
								// extra being negative
								if (extra < 0)
									extra = 0;

								v.pcm_current = (c_int)(v.pcm_current - (extra >> hs));
							}
							// else {Shouldn't happen *unless* the bitstream is out of
							// spec. Either way, believe the bitstream }
						}
					}
					// else {Shouldn't happen *unless* the bitstream is out of
					// spec. Either way, believe the bitstream }

					v.granulepos = vb.granulepos;
				}
			}

			// Update, cleanup
			if (vb.eofflag)
				v.eofflag = true;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Vorbis_Synthesis_Pcmout(VorbisDspState v)
		{
			return Vorbis_Synthesis_Pcmout(v, out _, false);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Vorbis_Synthesis_Pcmout(VorbisDspState v, out CPointer<c_float>[] pcm)
		{
			return Vorbis_Synthesis_Pcmout(v, out pcm, true);
		}



		/********************************************************************/
		/// <summary>
		/// pcm==null indicates we just want the pending samples, no more
		/// </summary>
		/********************************************************************/
		private static c_int Vorbis_Synthesis_Pcmout(VorbisDspState v, out CPointer<c_float>[] pcm, bool hasPcm)
		{
			pcm = null;

			VorbisInfo vi = v.vi;

			if ((v.pcm_returned > -1) && (v.pcm_returned < v.pcm_current))
			{
				if (hasPcm)
				{
					for (c_int i = 0; i < vi.channels; i++)
						v.pcmret[i] = v.pcm[i] + v.pcm_returned;

					pcm = v.pcmret;
				}

				return v.pcm_current - v.pcm_returned;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Vorbis_Synthesis_Read(VorbisDspState v, c_int n)
		{
			if ((n != 0) && ((v.pcm_returned + n) > v.pcm_current))
				return (c_int)VorbisError.Inval;

			v.pcm_returned += n;

			return 0;
		}
	}
}
