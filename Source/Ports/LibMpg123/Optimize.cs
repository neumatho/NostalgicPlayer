/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// Get a grip on the different optimizations
	/// </summary>
	internal class Optimize
	{
		public delegate void Dct36_Delegate(Memory<Real> inBuf, Memory<Real> out1, Memory<Real> o2, Real[] winTab, Memory<Real> tsBuf);

		private const string Dn_AutoDec = "auto";
		private const string Dn_Generic = "generic";
		private const string Dn_Generic_Dither = "generic_dither";
		private const string Dn_IDrei = "i386";
		private const string Dn_IVier = "i486";
		private const string Dn_IFuenf = "i586";
		private const string Dn_IFuenf_Dither = "i586_dither";
		private const string Dn_Mmx = "MMX";
		private const string Dn_DreiDNow = "3DNow";
		private const string Dn_DreiDNowExt = "3DNowExt";
		private const string Dn_AltiVec = "AltiVec";
		private const string Dn_Sse = "SSE";
		private const string Dn_X86_64 = "x86-64";
		private const string Dn_Arm = "ARM";
		private const string Dn_Neon = "NEON";
		private const string Dn_Neon64 = "NEON64";
		private const string Dn_Avx = "AVX";
		private const string Dn_DreiDNow_Vintage = "3DNow_vintage";
		private const string Dn_DreiDNowExt_Vintage = "3DNowExt_vintage";
		private const string Dn_Sse_Vintage = "SSE_vintage";
		private const string Dn_NoDec = "nodec";

		private static readonly string[] decName =
		{
			Dn_AutoDec,
			Dn_Generic,
			Dn_Generic_Dither,
			Dn_IDrei,
			Dn_IVier,
			Dn_IFuenf,
			Dn_IFuenf_Dither,
			Dn_Mmx,
			Dn_DreiDNow,
			Dn_DreiDNowExt,
			Dn_AltiVec,
			Dn_Sse,
			Dn_X86_64,
			Dn_Arm,
			Dn_Neon,
			Dn_Neon64,
			Dn_Avx,
			Dn_DreiDNow_Vintage,
			Dn_DreiDNowExt_Vintage,
			Dn_Sse_Vintage,
			Dn_NoDec
		};

		private const OptDec DefOpt = OptDec.Generic;

		private readonly LibMpg123 lib;

		private readonly Synth_S synth_Base;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Optimize(LibMpg123 libMpg123)
		{
			lib = libMpg123;

			synth_Base = new Synth_S
			{
				Plain = new Synth_S.Func_Synth[,] { { lib.synth_s32.Synth_1To1_S32 }, { lib.synth_s32.Synth_2To1_S32 }, { lib.synth_s32.Synth_4To1_S32 }, { lib.synth_s32.Synth_NToM_S32 } },
				Stereo = new Synth_S.Func_Synth_Stereo[,] { { Synth_Stereo_Wrap }, { Synth_Stereo_Wrap }, { Synth_Stereo_Wrap }, { Synth_Stereo_Wrap } },
				Mono2Stereo = new Synth_S.Func_Synth_Mono[,] { { lib.synth_s32.Synth_1To1_S32_M2S }, { lib.synth_s32.Synth_2To1_S32_M2S }, { lib.synth_s32.Synth_4To1_S32_M2S }, { lib.synth_s32.Synth_NToM_S32_M2S } },
				Mono = new Synth_S.Func_Synth_Mono[,] { { lib.synth_s32.Synth_1To1_S32_Mono }, { lib.synth_s32.Synth_2To1_S32_Mono }, { lib.synth_s32.Synth_4To1_S32_Mono }, { lib.synth_s32.Synth_NToM_S32_Mono } },
			};
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public OptDec DefDec()
		{
			return DefOpt;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public OptCla DecClass(OptDec type)
		{
			return
				(type == OptDec.Mmx) ||
				(type == OptDec.Sse) ||
				(type == OptDec.Sse_Vintage) ||
				(type == OptDec.DreiDNowExt) ||
				(type == OptDec.DreiDNowExt_Vintage) ||
				(type == OptDec.X86_64) ||
				(type == OptDec.Neon) ||
				(type == OptDec.Avx) ? OptCla.MmxSse : OptCla.Normal;
		}



		/********************************************************************/
		/// <summary>
		/// Set synth functions for current frame
		/// </summary>
		/********************************************************************/
		public c_int Set_Synth_Functions(Mpg123_Handle fr)
		{
			Synth_Resample resample = Synth_Resample.None;
			Synth_Format basic_Format = Synth_Format.None;	// Default is always 16bit, or whatever

			if (false)
			{
			}
			// 24 bit integer means decoding to 32 bit first
			else if (((fr.Af.Dec_Enc & Mpg123_Enc_Enum.Enc_32) != 0) || ((fr.Af.Dec_Enc & Mpg123_Enc_Enum.Enc_24) != 0))
				basic_Format = Synth_Format.ThirtyTwo;

			// Make sure the chosen format is compiled into this lib
			if (basic_Format == Synth_Format.None)
				return -1;

			// Be explicit about downsampling variant
			switch (fr.Down_Sample)
			{
				case 0:
				{
					resample = Synth_Resample.OneToOne;
					break;
				}

				case 1:
				{
					resample = Synth_Resample.TwoToOne;
					break;
				}

				case 2:
				{
					resample = Synth_Resample.FourToOne;
					break;
				}

				case 3:
				{
					resample = Synth_Resample.NToM;
					break;
				}
			}

			if (resample == Synth_Resample.None)
				return -1;

			// Finally selecting the synth functions for stereo / mono
			fr.Synth = fr.Synths.Plain[(int)resample, (int)basic_Format];
			fr.Synth_Stereo = fr.Synths.Stereo[(int)resample, (int)basic_Format];
			fr.Synth_Mono = fr.Af.Channels == 2 ?
				fr.Synths.Mono2Stereo[(int)resample, (int)basic_Format] :	// Mono MPEG file decoded to stereo
				fr.Synths.Mono[(int)resample, (int)basic_Format];			// Mono MPEG file decoded to mono

			if (Find_DecType(fr) != Mpg123_Errors.Ok)	// Actually determine the currently active decoder breed
			{
				fr.Err = Mpg123_Errors.Bad_Decoder_Setup;
				return (c_int)Mpg123_Errors.Err;
			}

			if (lib.frame.Frame_Buffers(fr) != 0)
			{
				fr.Err = Mpg123_Errors.No_Buffers;
				return (c_int)Mpg123_Errors.Err;
			}

			{
				lib.layer3.Init_Layer3_Stuff(fr, lib.layer3.Init_Layer3_GainPow2);
				lib.layer2.Init_Layer12_Stuff(fr, lib.layer2.Init_Layer12_Table);

				fr.Make_Decode_Tables = lib.tabInit.Make_Decode_Tables;
			}

			// We allocated the table buffers just now, so (re)create the tables
			fr.Make_Decode_Tables(fr);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool Frame_Cpu_Opt(Mpg123_Handle fr, string cpu)
		{
			string chosen = string.Empty;		// The chosen decoder opt as string
			OptDec want_Dec = OptDec.NoDec;
			bool done = false;
			bool auto_Choose = false;

			want_Dec = DecType(cpu);
			auto_Choose = want_Dec == OptDec.AutoDec;

			// Fill whole array of synth functions with generic code first
			synth_Base.Copy(fr.Synths);

			{
				if (!auto_Choose && (want_Dec != DefOpt))
				{
					;
				}

				auto_Choose = true;
			}

			fr.Cpu_Opts.Type = OptDec.NoDec;

			if (!done && (auto_Choose || (want_Dec == OptDec.Generic)))
			{
				chosen = Dn_Generic;
				fr.Cpu_Opts.Type = OptDec.Generic;
				done = true;
			}

			fr.Cpu_Opts.Class = DecClass(fr.Cpu_Opts.Type);

			if (done)
				return true;
			else
				return false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Dct36_Delegate Opt_Dct36()
		{
			return lib.layer3.Dct36;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// The call of left and right plain synth, wrapped.
		/// This may be replaced by a direct stereo optimized synth
		/// </summary>
		/********************************************************************/
		private c_int Synth_Stereo_Wrap(Memory<Real> bandPtr_L, Memory<Real> bandPtr_R, Mpg123_Handle fr)
		{
			c_int clip = fr.Synth(bandPtr_L, 0, fr, false);
			clip += fr.Synth(bandPtr_R, 1, fr, true);

			return clip;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Find_Synth(Synth_S.Func_Synth synth, Synth_S.Func_Synth[,] synths)
		{
			for (c_int ri = 0; ri < (c_int)Synth_Resample.Limit; ++ri)
			{
				for (c_int fi = 0; fi < (c_int)Synth_Format.Limit; ++fi)
				{
					if (synth == synths[ri, fi])
						return true;
				}
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Determine what kind of decoder is actually active.
		/// </summary>
		/********************************************************************/
		private Mpg123_Errors Find_DecType(Mpg123_Handle fr)
		{
			OptDec type = OptDec.NoDec;

			// Direct and indirect usage, 1to1 stereo decoding.
			// Concentrating on the plain stereo synth should be fine, mono stuff is derived
			Synth_S.Func_Synth basic_Synth = fr.Synth;

			if (false)
			{
			}
			else if (Find_Synth(basic_Synth, synth_Base.Plain))
				type = OptDec.Generic;

			if (type != OptDec.NoDec)
			{
				fr.Cpu_Opts.Type = type;
				fr.Cpu_Opts.Class = DecClass(type);

				return Mpg123_Errors.Ok;
			}
			else
			{
				fr.Err = Mpg123_Errors.Bad_Decoder_Setup;
				return Mpg123_Errors.Err;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private OptDec DecType(string decoder)
		{
			if (string.IsNullOrEmpty(decoder))
				return OptDec.AutoDec;

			for (c_int i = 0; i < decName.Length; i++)
			{
				if (decName[i].Equals(decoder, StringComparison.InvariantCultureIgnoreCase))
					return (OptDec)i;
			}

			return OptDec.NoDec;	// If we found nothing
		}
		#endregion
	}
}
