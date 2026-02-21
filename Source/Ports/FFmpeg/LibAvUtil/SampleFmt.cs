/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class SampleFmt
	{
		/// <summary>
		/// This table gives more information about formats
		/// </summary>
		private static readonly SampleFmtInfo[] sample_Fmt_Info = BuildInfo();

		#region Build info
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static SampleFmtInfo[] BuildInfo()
		{
			SampleFmtInfo[] arr = new SampleFmtInfo[(c_int)AvSampleFormat.Nb];

			arr[(c_int)AvSampleFormat.U8] = new SampleFmtInfo("u8", 8, 0, AvSampleFormat.U8P);
			arr[(c_int)AvSampleFormat.S16] = new SampleFmtInfo("s16", 16, 0, AvSampleFormat.S16P);
			arr[(c_int)AvSampleFormat.S32] = new SampleFmtInfo("s32", 32, 0, AvSampleFormat.S32P);
			arr[(c_int)AvSampleFormat.S64] = new SampleFmtInfo("s64", 64, 0, AvSampleFormat.S64P);
			arr[(c_int)AvSampleFormat.Flt] = new SampleFmtInfo("flt", 32, 0, AvSampleFormat.FltP);
			arr[(c_int)AvSampleFormat.Dbl] = new SampleFmtInfo("dbl", 64, 0, AvSampleFormat.DblP);
			arr[(c_int)AvSampleFormat.U8P] = new SampleFmtInfo("u8p", 8, 1, AvSampleFormat.U8);
			arr[(c_int)AvSampleFormat.S16P] = new SampleFmtInfo("s16p", 16, 1, AvSampleFormat.S16);
			arr[(c_int)AvSampleFormat.S32P] = new SampleFmtInfo("s32p", 32, 1, AvSampleFormat.S32);
			arr[(c_int)AvSampleFormat.S64P] = new SampleFmtInfo("s64p", 64, 1, AvSampleFormat.S64);
			arr[(c_int)AvSampleFormat.FltP] = new SampleFmtInfo("fltp", 32, 1, AvSampleFormat.Flt);
			arr[(c_int)AvSampleFormat.DblP] = new SampleFmtInfo("dblp", 64, 1, AvSampleFormat.Dbl);

			return arr;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Return the name of sample_fmt, or NULL if sample_fmt is not
		/// recognized
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Get_Sample_Fmt_Name(AvSampleFormat sample_Fmt)//XX 51
		{
			if ((sample_Fmt < 0) || (sample_Fmt >= AvSampleFormat.Nb))
				return null;

			return sample_Fmt_Info[(c_int)sample_Fmt].Name;
		}



		/********************************************************************/
		/// <summary>
		/// Return a sample format corresponding to name, or
		/// AV_SAMPLE_FMT_NONE on error
		/// </summary>
		/********************************************************************/
		public static AvSampleFormat Av_Get_Sample_Fmt(CPointer<char> name)//XX 58
		{
			for (c_int i = 0; i < (c_int)AvSampleFormat.Nb; i++)
			{
				if (CString.strcmp(sample_Fmt_Info[i].Name, name) == 0)
					return (AvSampleFormat)i;
			}

			return AvSampleFormat.None;
		}



		/********************************************************************/
		/// <summary>
		/// Get the packed alternative form of the given sample format.
		///
		/// If the passed sample_fmt is already in packed format, the format
		/// returned is the same as the input
		/// </summary>
		/********************************************************************/
		public static AvSampleFormat Av_Get_Packed_Sample_Fmt(AvSampleFormat sample_Fmt)//XX 77
		{
			if ((sample_Fmt < 0) || (sample_Fmt >= AvSampleFormat.Nb))
				return AvSampleFormat.None;

			if (sample_Fmt_Info[(int)sample_Fmt].Planar != 0)
				return sample_Fmt_Info[(int)sample_Fmt].AltForm;

			return sample_Fmt;
		}



		/********************************************************************/
		/// <summary>
		/// Get the planar alternative form of the given sample format.
		///
		/// If the passed sample_fmt is already in planar format, the format
		/// returned is the same as the input
		/// </summary>
		/********************************************************************/
		public static AvSampleFormat Av_Get_Planar_Sample_Fmt(AvSampleFormat sample_Fmt)//XX 86
		{
			if ((sample_Fmt < 0) || (sample_Fmt >= AvSampleFormat.Nb))
				return AvSampleFormat.None;

			if (sample_Fmt_Info[(int)sample_Fmt].Planar != 0)
				return sample_Fmt;

			return sample_Fmt_Info[(int)sample_Fmt].AltForm;
		}



		/********************************************************************/
		/// <summary>
		/// Return number of bytes per sample
		/// </summary>
		/********************************************************************/
		public static c_int Av_Get_Bytes_Per_Sample(AvSampleFormat sample_Fmt)//XX 108
		{
			return (sample_Fmt < 0) || (sample_Fmt >= AvSampleFormat.Nb) ? 0 : sample_Fmt_Info[(c_int)sample_Fmt].Bits >> 3;
		}



		/********************************************************************/
		/// <summary>
		/// Check if the sample format is planar
		/// </summary>
		/********************************************************************/
		public static c_int Av_Sample_Fmt_Is_Planar(AvSampleFormat sample_Fmt)//XX 114
		{
			if ((sample_Fmt < 0) || (sample_Fmt >= AvSampleFormat.Nb))
				return 0;

			return sample_Fmt_Info[(c_int)sample_Fmt].Planar != 0 ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get the required buffer size for the given audio parameters
		/// </summary>
		/********************************************************************/
		public static c_int Av_Samples_Get_Buffer_Size(CPointer<c_int> lineSize, c_int nb_Channels, c_int nb_Samples, AvSampleFormat sample_Fmt, c_int align)//XX 121
		{
			c_int sample_Size = Av_Get_Bytes_Per_Sample(sample_Fmt);
			c_int planar = Av_Sample_Fmt_Is_Planar(sample_Fmt);

			// Validate parameter ranges
			if ((sample_Size == 0) || (nb_Samples <= 0) || (nb_Channels <= 0))
				return Error.EINVAL;

			// Auto-select alignment if not specified
			if (align == 0)
			{
				if (nb_Samples > (c_int.MaxValue - 31))
					return Error.EINVAL;

				align = 1;
				nb_Samples = Macros.FFAlign(nb_Samples, 32);
			}

			// Check for integer overflow
			if ((nb_Channels > (c_int.MaxValue / align)) || (((int64_t)nb_Samples * nb_Samples) > ((c_int.MaxValue - (align * nb_Samples)) / sample_Size)))
				return Error.EINVAL;

			c_int line_Size = planar != 0 ? Macros.FFAlign(nb_Samples * sample_Size, align) : Macros.FFAlign(nb_Samples * sample_Size * nb_Channels, align);

			if (lineSize.IsNotNull)
				lineSize[0] = line_Size;

			return planar != 0 ? line_Size * nb_Channels : line_Size;
		}



		/********************************************************************/
		/// <summary>
		/// Copy samples from src to dst
		/// </summary>
		/********************************************************************/
		public static c_int Av_Samples_Copy(CPointer<CPointer<uint8_t>> dst, CPointer<CPointer<uint8_t>> src, c_int dst_Offset, c_int src_Offset, c_int nb_Samples, c_int nb_Channels, AvSampleFormat sample_Fmt)//XX 222
		{
			c_int planar = Av_Sample_Fmt_Is_Planar(sample_Fmt);
			c_int planes = planar != 0 ? nb_Channels : 1;
			c_int block_Align = Av_Get_Bytes_Per_Sample(sample_Fmt) * (planar != 0 ? 1 : nb_Channels);
			c_int data_Size = nb_Samples * block_Align;

			dst_Offset *= block_Align;
			src_Offset *= block_Align;

			if ((dst[0] < src[0] ? src[0] - dst[0] : dst[0] - src[0]) >= data_Size)
			{
				for (c_int i = 0; i < planes; i++)
					CMemory.memcpy(dst[i] + dst_Offset, src[i] + src_Offset, (size_t)data_Size);
			}
			else
			{
				for (c_int i = 0; i < planes; i++)
					CMemory.memmove(dst[i] + dst_Offset, src[i] + src_Offset, (size_t)data_Size);
			}

			return 0;
		}
	}
}
