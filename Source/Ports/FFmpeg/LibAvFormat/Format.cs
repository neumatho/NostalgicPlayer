/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat
{
	/// <summary>
	/// Format register and lookup
	/// </summary>
	public static class Format
	{
		private enum NoDat
		{
			No_Id3,
			Id3_Almost_Greater_Probe,
			Id3_Greater_Probe,
			Id3_Grater_Max_Probe
		}

		/********************************************************************/
		/// <summary>
		/// Return a positive value if the given filename has one of the
		/// given extensions, 0 otherwise
		/// </summary>
		/********************************************************************/
		public static c_int Av_Match_Ext(CPointer<char> fileName, CPointer<char> extensions)
		{
			if (fileName.IsNull)
				return 0;

			CPointer<char> ext = CString.strrchr(fileName, '.');

			if (ext.IsNotNull)
				return AvString.Av_Match_Name(ext + 1, extensions);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Guess the file format
		/// </summary>
		/********************************************************************/
		public static AvInputFormat Av_Probe_Input_Format3(AvProbeData pd, bool is_Opened, out c_int score_Ret)//XX 156
		{
			AvProbeData lPd = pd;
			AvInputFormat fmt = null;
			c_int score_Max = 0;
			uint8_t[] zeroBuffer = new uint8_t[AvProbe.Padding_Size];
			NoDat noDat = NoDat.No_Id3;

			if (lPd.Buf.IsNull)
				lPd.Buf = zeroBuffer;

			if ((lPd.Buf_Size > 10) && (Id3v2.FF_Id3v2_Match(lPd.Buf, Id3v2.Default_Magic) != 0))
			{
				c_int id3Len = Id3v2.FF_Id3v2_Tag_Len(lPd.Buf);

				if (lPd.Buf_Size > (id3Len + 16))
				{
					if (lPd.Buf_Size < ((2L * id3Len) + 16))
						noDat = NoDat.Id3_Almost_Greater_Probe;

					lPd.Buf += id3Len;
					lPd.Buf_Size -= id3Len;
				}
				else if (id3Len >= Internal.Probe_Buf_Max)
					noDat = NoDat.Id3_Grater_Max_Probe;
				else
					noDat = NoDat.Id3_Greater_Probe;
			}

			foreach (AvInputFormat fmt1 in AllFormats.Av_Demuxer_Iterate())
			{
				if ((fmt1.Flags & AvFmt.Experimental) != 0)
					continue;

				if ((!is_Opened == ((fmt1.Flags & AvFmt.NoFile) == 0)) && (CString.strcmp(fmt1.Name, "image2".ToCharPointer()) != 0))
					continue;

				c_int score = 0;

				if (Demux.FFIFmt(fmt1).Read_Header != null)
				{
					score = Demux.FFIFmt(fmt1).Read_Probe(lPd);

					if (score != 0)
						Log.Av_Log(null, Log.Av_Log_Trace, "Probing %s score:%d size:%d\n", fmt1.Name, score, lPd.Buf_Size);

					if (fmt1.Extensions.IsNotNull && (Av_Match_Ext(lPd.FileName, fmt1.Extensions) != 0))
					{
						switch (noDat)
						{
							case NoDat.No_Id3:
							{
								score = Macros.FFMax(score, 1);
								break;
							}

							case NoDat.Id3_Greater_Probe:
							case NoDat.Id3_Almost_Greater_Probe:
							{
								score = Macros.FFMax(score, (AvProbe.Score_Extension / 2) - 1);
								break;
							}

							case NoDat.Id3_Grater_Max_Probe:
							{
								score = Macros.FFMax(score, AvProbe.Score_Extension);
								break;
							}
						}
					}
				}
				else if (fmt1.Extensions.IsNotNull)
				{
					if (Av_Match_Ext(lPd.FileName, fmt1.Extensions) != 0)
						score = AvProbe.Score_Extension;
				}

				if (AvString.Av_Match_Name(lPd.Mime_Type, fmt1.Mime_Type) != 0)
				{
					c_int old_Score = score;
					score += AvProbe.Score_Mime_Bonus;

					if (score > AvProbe.Score_Max)
						score = AvProbe.Score_Max;

					Log.Av_Log(null, Log.Av_Log_Debug, "Probing %s score:%d increased to %d due to MIME type\n", fmt1.Name, old_Score, score);
				}

				if (score > score_Max)
				{
					score_Max = score;
					fmt = fmt1;
				}
				else if (score == score_Max)
					fmt = null;
			}

			if (noDat == NoDat.Id3_Greater_Probe)
				score_Max = Macros.FFMin((AvProbe.Score_Extension / 2) - 1, score_Max);

			score_Ret = score_Max;

			return fmt;
		}



		/********************************************************************/
		/// <summary>
		/// Guess the file format
		/// </summary>
		/********************************************************************/
		public static AvInputFormat Av_Probe_Input_Format2(AvProbeData pd, bool is_Opened, ref c_int score_Max)//XX 235
		{
			AvInputFormat fmt = Av_Probe_Input_Format3(pd, is_Opened, out c_int score_Ret);

			if (score_Ret > score_Max)
			{
				score_Max = score_Ret;

				return fmt;
			}
			else
				return null;
		}



		/********************************************************************/
		/// <summary>
		/// Probe a bytestream to determine the input format. Each time a
		/// probe returns with a score that is too low, the probe buffer size
		/// is increased and another attempt is made. When the maximum probe
		/// size is reached, the input format with the highest score is
		/// returned
		/// </summary>
		/********************************************************************/
		public static c_int Av_Probe_Input_Buffer2(AvIoContext pb, out AvInputFormat fmt, CPointer<char> fileName, IClass logCtx, c_uint offset, c_uint max_Probe_Size)//XX 253
		{
			fmt = null;

			AvProbeData pd = new AvProbeData { FileName = fileName };
			CPointer<uint8_t> buf = null;
			c_int ret = 0, buf_Offset = 0;
			c_int score = 0;
			c_int eof = 0;

			if (max_Probe_Size == 0)
				max_Probe_Size = Internal.Probe_Buf_Max;
			else if (max_Probe_Size < Internal.Probe_Buf_Min)
			{
				Log.Av_Log(logCtx, Log.Av_Log_Error, "Specified probe size value %u cannot be < %u\n", max_Probe_Size, Internal.Probe_Buf_Min);

				return Error.EINVAL;
			}

			if (offset >= max_Probe_Size)
				return Error.EINVAL;

			if (pb.Av_Class != null)
			{
				Opt.Av_Opt_Get(pb, "mime_type".ToCharPointer(), AvOptSearch.Search_Children, out CPointer<char> mime_Type_Opt);
				pd.Mime_Type = mime_Type_Opt;

				CPointer<char> semi = pd.Mime_Type.IsNotNull ? CString.strchr(pd.Mime_Type, ';') : null;
				if (semi.IsNotNull)
					semi[0] = '\0';
			}

			for (c_int probe_Size = Internal.Probe_Buf_Min; (probe_Size <= max_Probe_Size) && (fmt == null) && (eof == 0); probe_Size = (c_int)Macros.FFMin(probe_Size << 1, Macros.FFMax(max_Probe_Size, probe_Size + 1U)))
			{
				score = probe_Size < max_Probe_Size ? AvProbe.Score_Retry : 0;

				// Read probe data
				ret = Mem.Av_ReallocP(ref buf, (size_t)probe_Size + AvProbe.Padding_Size);

				if (ret < 0)
					goto Fail;

				ret = AvIoBuf.AvIo_Read(pb, buf + buf_Offset, probe_Size - buf_Offset);

				if (ret < 0)
				{
					// Fail if error was not end of file, otherwise, lower score
					if (ret != Error.EOF)
						goto Fail;

					score = 0;
					ret = 0;		// Error was end of file, nothing read
					eof = 1;
				}

				buf_Offset += ret;

				if (buf_Offset < offset)
					continue;

				pd.Buf_Size = (c_int)(buf_Offset - offset);
				pd.Buf = buf + offset;

				CMemory.memset<c_uchar>(pd.Buf + pd.Buf_Size, 0, AvProbe.Padding_Size);

				// Guess file format
				fmt = Av_Probe_Input_Format2(pd, true, ref score);

				if (fmt != null)
				{
					// This can only be true in the last iteration
					if (score <= AvProbe.Score_Retry)
						Log.Av_Log(logCtx, Log.Av_Log_Warning, "Format %s detected only with low score of %d, misdetection possible!\n", fmt.Name, score);
					else
						Log.Av_Log(logCtx, Log.Av_Log_Debug, "Format %s probed with size=%d and score=%d\n", fmt.Name, probe_Size, score);
				}
			}

			if (fmt == null)
				ret = Error.InvalidData;

			Fail:
			// Rewind. Reuse probe buffer to avoid seeking
			c_int ret2 = AvIoBuf.FFIo_Rewind_With_Probe_Data(pb, ref buf, buf_Offset);

			if (ret >= 0)
				ret = ret2;

			Mem.Av_FreeP(ref pd.Mime_Type);

			return ret < 0 ? ret : score;
		}
	}
}
