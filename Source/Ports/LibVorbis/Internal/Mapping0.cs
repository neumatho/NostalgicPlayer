/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Internal
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Mapping0
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Free_Info(IVorbisInfoMapping i)
		{
			VorbisInfoMapping0 info = i as VorbisInfoMapping0;

			if (info != null)
				info.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IVorbisInfoMapping Unpack(VorbisInfo vi, OggPack opb)
		{
			VorbisInfoMapping0 info = new VorbisInfoMapping0();
			CodecSetupInfo ci = (CodecSetupInfo)vi.codec_setup;

			if (vi.channels <= 0)
				goto ErrOut;

			c_int b = (c_int)opb.Read(1);
			if (b < 0)
				goto ErrOut;

			if (b != 0)
			{
				info.submaps = (c_int)opb.Read(4) + 1;
				if (info.submaps <= 0)
					goto ErrOut;
			}
			else
				info.submaps = 1;

			b = (c_int)opb.Read(1);
			if (b < 0)
				goto ErrOut;

			if (b != 0)
			{
				info.coupling_steps = (c_int)opb.Read(8) + 1;
				if (info.coupling_steps <= 0)
					goto ErrOut;

				for (c_int i = 0; i < info.coupling_steps; i++)
				{
					// vi->channels > 0 is enforced in the caller
					c_int testM = info.coupling_mag[i] = (c_int)opb.Read(Sharedbook.Ov_ILog((ogg_uint32_t)vi.channels - 1));
					c_int testA = info.coupling_ang[i] = (c_int)opb.Read(Sharedbook.Ov_ILog((ogg_uint32_t)vi.channels - 1));

					if ((testM < 0) || (testA < 0) || (testM == testA) || (testM >= vi.channels) || (testA >= vi.channels))
						goto ErrOut;
				}
			}

			if (opb.Read(2) != 0)	// 2,3: reserved
				goto ErrOut;

			if (info.submaps > 1)
			{
				for (c_int i = 0; i < vi.channels; i++)
				{
					info.chmuxlist[i] = (c_int)opb.Read(4);

					if ((info.chmuxlist[i] >= info.submaps) || (info.chmuxlist[i] < 0))
						goto ErrOut;
				}
			}

			for (c_int i = 0; i < info.submaps; i++)
			{
				opb.Read(8);    // time submap unused

				info.floorsubmap[i] = (c_int)opb.Read(8);

				if ((info.floorsubmap[i] >= ci.floors) || (info.floorsubmap[i] < 0))
					goto ErrOut;

				info.residuesubmap[i] = (c_int)opb.Read(8);

				if ((info.residuesubmap[i] >= ci.residues) || (info.residuesubmap[i] < 0))
					goto ErrOut;
			}

			return info;

			ErrOut:
			Free_Info(info);

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Inverse(VorbisBlock vb, IVorbisInfoMapping l)
		{
			VorbisDspState vd = vb.vd;
			VorbisInfo vi = vd.vi;
			CodecSetupInfo ci = (CodecSetupInfo)vi.codec_setup;
			PrivateState b = (PrivateState)vd.backend_state;
			VorbisInfoMapping0 info = (VorbisInfoMapping0)l;

			c_long n = ci.blocksizes[vb.W];
			vb.pcmend = (c_int)n;

			CPointer<c_float>[] pcmbundle = new CPointer<c_float>[vi.channels];
			bool[] zerobundle = new bool[vi.channels];

			bool[] nonzero = new bool[vi.channels];
			CPointer<byte>[] floormemo = new CPointer<byte>[vi.channels];

			// Recover the spectral envelope; store it in the PCM vector for now
			for (c_int i = 0; i < vi.channels; i++)
			{
				c_int submap = info.chmuxlist[i];
				floormemo[i] = Registry.floor_P[ci.floor_type[info.floorsubmap[submap]]].Inverse1(vb, b.flr[info.floorsubmap[submap]]);

				if (floormemo[i].IsNotNull)
					nonzero[i] = true;
				else
					nonzero[i] = false;

				vb.pcm[i].Clear((c_int)(n / 2));
			}

			// Channel coupling can 'dirty' the nonzero listing
			for (c_int i = 0; i < info.coupling_steps; i++)
			{
				if (nonzero[info.coupling_mag[i]] || nonzero[info.coupling_ang[i]])
				{
					nonzero[info.coupling_mag[i]] = true;
					nonzero[info.coupling_ang[i]] = true;
				}
			}

			// Recover the residue into our working vectors
			for (c_int i = 0; i < info.submaps; i++)
			{
				c_int ch_in_bundle = 0;

				for (c_int j = 0; j < vi.channels; j++)
				{
					if (info.chmuxlist[j] == i)
					{
						if (nonzero[j])
							zerobundle[ch_in_bundle] = true;
						else
							zerobundle[ch_in_bundle] = false;

						pcmbundle[ch_in_bundle++] = vb.pcm[j];
					}
				}

				Registry.residue_P[ci.residue_type[info.residuesubmap[i]]].Inverse(vb, b.residue[info.residuesubmap[i]], pcmbundle, zerobundle, ch_in_bundle);
			}

			// Channel coupling
			for (c_int i = info.coupling_steps - 1; i >= 0; i--)
			{
				CPointer<c_float> pcmM = vb.pcm[info.coupling_mag[i]];
				CPointer<c_float> pcmA = vb.pcm[info.coupling_ang[i]];

				for (c_int j = 0; j < (n / 2); j++)
				{
					c_float mag = pcmM[j];
					c_float ang = pcmA[j];

					if (mag > 0)
					{
						if (ang > 0)
						{
							pcmM[j] = mag;
							pcmA[j] = mag - ang;
						}
						else
						{
							pcmA[j] = mag;
							pcmM[j] = mag + ang;
						}
					}
					else
					{
						if (ang > 0)
						{
							pcmM[j] = mag;
							pcmA[j] = mag + ang;
						}
						else
						{
							pcmA[j] = mag;
							pcmM[j] = mag - ang;
						}
					}
				}
			}

			// Compute and apply spectral envelope
			for (c_int i = 0; i < vi.channels; i++)
			{
				CPointer<c_float> pcm = vb.pcm[i];
				c_int submap = info.chmuxlist[i];

				Registry.floor_P[ci.floor_type[info.floorsubmap[submap]]].Inverse2(vb, b.flr[info.floorsubmap[submap]], floormemo[i], pcm);
			}

			// Transform the PCM data; takes PCM vector, vb; modifies PCM vector
			// only MDCT right now
			for (c_int i = 0; i < vi.channels; i++)
			{
				CPointer<c_float> pcm = vb.pcm[i];
				Mdct.Mdct_Backward((MdctLookup)b.transform[vb.W][0], pcm, pcm);
			}

			// All done!
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly VorbisFuncMapping ExportBundle = new VorbisFuncMapping
		{
			Unpack = Unpack,
			Free_Info = Free_Info,
			Inverse = Inverse
		};
	}
}
