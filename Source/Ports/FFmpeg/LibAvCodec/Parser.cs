/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// Audio and Video frame extraction
	/// </summary>
	public static class Parser
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static AvCodecParserContext Av_Parser_Init(AvCodecId codec_Id)//XX 33
		{
			if (codec_Id == AvCodecId.None)
				return null;

			AvCodecParser parser;

			foreach (AvCodecParser parser_ in Parsers.Av_Parser_Iterate())
			{
				if ((parser_.Codec_Ids[0] == codec_Id) ||
					(parser_.Codec_Ids[1] == codec_Id) ||
					(parser_.Codec_Ids[2] == codec_Id) ||
					(parser_.Codec_Ids[3] == codec_Id) ||
					(parser_.Codec_Ids[4] == codec_Id) ||
					(parser_.Codec_Ids[5] == codec_Id) ||
					(parser_.Codec_Ids[6] == codec_Id))
				{
					parser = parser_;
					goto Found;
				}
			}

			return null;

			Found:
			AvCodecParserContext s = Mem.Av_MAlloczObj<AvCodecParserContext>();

			if (s == null)
				goto Err_Out;

			s.Parser = parser;

			if (parser.Priv_Data_Alloc != null)
			{
				s.Priv_Data = parser.Priv_Data_Alloc();

				if (s.Priv_Data == null)
					goto Err_Out;
			}

			s.Fetch_Timestamp = 1;
			s.Pict_Type = AvPictureType.I;

			if (parser.Parser_Init != null)
			{
				c_int ret = parser.Parser_Init(s);
				if (ret != 0)
					goto Err_Out;
			}

			s.Key_Frame = -1;
			s.Dts_Sync_Point = c_int.MinValue;
			s.Dts_Ref_Dts_Delta = c_int.MinValue;
			s.Pts_Dts_Delta = c_int.MinValue;
			s.Format.Sample = AvSampleFormat.None;

			return s;

			Err_Out:
			if (s != null)
				Mem.Av_FreeP(ref s.Priv_Data);

			Mem.Av_Free(s);

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Fetch timestamps for a specific byte within the current access
		/// unit
		/// </summary>
		/********************************************************************/
		public static void FF_Fetch_Timestamp(AvCodecParserContext s, c_int off, c_int remove, c_int fuzzy)//XX 85
		{
			if (fuzzy == 0)
			{
				s.Dts = s.Pts = UtilConstants.Av_NoPts_Value;
				s.Pos = -1;
				s.Offset = 0;
			}

			for (c_int i = 0; i < AvCodecParserContext.Av_Parser_Pts_Nb; i++)
			{
				if (((s.Cur_Offset + off) >= s.Cur_Frame_Offset[i]) && ((s.Frame_Offset < s.Cur_Frame_Offset[i]) || ((s.Frame_Offset == 0) && (s.Next_Frame_Offset == 0))) && (s.Cur_Frame_End[i] != 0))
				{
					if ((fuzzy == 0) || (s.Cur_Frame_Dts[i] != UtilConstants.Av_NoPts_Value))
					{
						s.Dts = s.Cur_Frame_Dts[i];
						s.Pts = s.Cur_Frame_Pts[i];
						s.Pos = s.Cur_Frame_Pos[i];
						s.Offset = s.Next_Frame_Offset - s.Cur_Frame_Offset[i];
					}

					if (remove != 0)
						s.Cur_Frame_Offset[i] = int64_t.MaxValue;

					if ((s.Cur_Offset + off) < s.Cur_Frame_End[i])
						break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Parse a packet
		/// </summary>
		/********************************************************************/
		public static c_int Av_Parser_Parse2(AvCodecParserContext s, AvCodecContext avCtx, out CPointer<uint8_t> pOutBuf, out c_int pOutBufSize, CPointer<uint8_t> buf, c_int buf_Size, int64_t pts, int64_t dts, int64_t pos)//XX 116
		{
			CPointer<uint8_t> dummy_Buf = new CPointer<uint8_t>(Defs.Av_Input_Buffer_Padding_Size);

			if ((s.Flags & ParserFlag.Fetched_Offset) == 0)
			{
				s.Next_Frame_Offset = s.Cur_Offset = pos;
				s.Flags |= ParserFlag.Fetched_Offset;
			}

			if (buf_Size == 0)
			{
				// Padding is always necessary even if EOF, so we add it here
				CMemory.memset<uint8_t>(dummy_Buf, 0, (size_t)dummy_Buf.Length);
				buf = dummy_Buf;
			}
			else if ((s.Cur_Offset + buf_Size) != s.Cur_Frame_End[s.Cur_Frame_Start_Index])	// Skip remainder packets
			{
				// Add a new packet descriptor
				c_int i = (s.Cur_Frame_Start_Index + 1) & (AvCodecParserContext.Av_Parser_Pts_Nb - 1);

				s.Cur_Frame_Start_Index = i;
				s.Cur_Frame_Offset[i] = s.Cur_Offset;
				s.Cur_Frame_End[i] = s.Cur_Offset + buf_Size;
				s.Cur_Frame_Pts[i] = pts;
				s.Cur_Frame_Dts[i] = dts;
				s.Cur_Frame_Pos[i] = pos;
			}

			if (s.Fetch_Timestamp != 0)
			{
				s.Fetch_Timestamp = 0;
				s.Last_Pts = s.Pts;
				s.Last_Dts = s.Dts;
				s.Last_Pos = s.Pos;

				FF_Fetch_Timestamp(s, 0, 0, 0);
			}

			// WARNING: the returned index can be negative
			c_int index = s.Parser.Parser_Parse(s, avCtx, out pOutBuf, out pOutBufSize, buf, buf_Size);

			void Fill(c_int name, ref c_int ctxName)
			{
				if ((name > 0) && (ctxName <= 0))
					ctxName = name;
			}

			if (avCtx.Codec_Type == AvMediaType.Video)
			{
				if ((s.Field_Order > 0) && (avCtx.Field_Order <= 0))
					avCtx.Field_Order = s.Field_Order;

				Fill(s.Coded_Width, ref avCtx.Coded_Width);
				Fill(s.Coded_Height, ref avCtx.Coded_Height);
				Fill(s.Width, ref avCtx.PictureSize.Width);
				Fill(s.Height, ref avCtx.PictureSize.Height);
			}

			// Update the file pointer
			if (pOutBufSize != 0)
			{
				// Fill the data for the current frame
				s.Frame_Offset = s.Next_Frame_Offset;

				// Offset of the next frame
				s.Next_Frame_Offset = s.Cur_Offset + index;
				s.Fetch_Timestamp = 1;
			}
			else
			{
				// Don't return a pointer to dummy_buf
				pOutBuf = null;
			}

			if (index < 0)
				index = 0;

			s.Cur_Offset += index;

			return index;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Av_Parser_Close(AvCodecParserContext s)//XX 194
		{
			if (s != null)
			{
				if (s.Parser.Parser_Close != null)
					s.Parser.Parser_Close(s);

				Mem.Av_FreeP(ref s.Priv_Data);
				Mem.Av_Free(s);
			}
		}
	}
}
