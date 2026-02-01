/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class Bsf
	{
		private static readonly AvClass bsf_Class = new AvClass
		{
			Class_Name = "AVBSFContext".ToCharPointer(),
			Item_Name = Bsf_To_Name,
			Version = Version.Version_Int,
			Child_Next = Bsf_Child_Next,
			Child_Class_Iterate = BitStream_Filters.FF_Bsf_Child_Class_Iterate,
			Category = AvClassCategory.Bitstream_Filter
		};

		private static readonly AvClass bsf_List_Class = new AvClass
		{
			Class_Name = "bsf_list".ToCharPointer(),
			Item_Name = Bsf_List_Item_Name,
			Version = Version.Version_Int
		};

		private static readonly FFBitStreamFilter list_Bsf = new FFBitStreamFilter
		{
			Name = "bsf_list".ToCharPointer(),
			Priv_Class = bsf_List_Class,
			Priv_Data_Alloc = Alloc_Priv_Data,
			Init = Bsf_List_Init,
			Filter = Bsf_List_Filter,
			Flush = Bsf_List_Flush,
			Close = Bsf_List_Close
		};

		/********************************************************************/
		/// <summary>
		/// Free a bitstream filter context and everything associated with
		/// it; write NULL into the supplied pointer
		/// </summary>
		/********************************************************************/
		public static void Av_Bsf_Free(ref AvBsfContext pCtx)//XX 52
		{
			if (pCtx == null)
				return;

			AvBsfContext ctx = pCtx;
			FFBsfContext bsfi = FFBsfContext(ctx);

			if (ctx.Priv_Data != null)
			{
				if (FF_Bsf(ctx.Filter).Close != null)
					FF_Bsf(ctx.Filter).Close(ctx);

				if (ctx.Filter.Priv_Class != null)
					Opt.Av_Opt_Free(ctx.Priv_Data);

				Mem.Av_FreeP(ref ctx.Priv_Data);
			}

			Packet.Av_Packet_Free(ref bsfi.Buffer_Pkt);

			Codec_Par.AvCodec_Parameters_Free(ref ctx.Par_In);
			Codec_Par.AvCodec_Parameters_Free(ref ctx.Par_Out);

			Mem.Av_FreeP(ref pCtx);
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a context for a given bitstream filter. The caller must
		/// fill in the context parameters as described in the documentation
		/// and then call av_bsf_init() before sending any data to the filter
		/// </summary>
		/********************************************************************/
		public static c_int Av_Bsf_Alloc(AvBitStreamFilter filter, out AvBsfContext pCtx)//XX 104
		{
			pCtx = null;

			FFBsfContext bsfi = Mem.Av_MAlloczObj<FFBsfContext>();
			c_int ret;

			if (bsfi == null)
				return Error.ENOMEM;

			AvBsfContext ctx = bsfi.Pub;

			bsf_Class.CopyTo(ctx.Av_Class);
			ctx.Filter = filter;

			ctx.Par_In = Codec_Par.AvCodec_Parameters_Alloc();
			ctx.Par_Out = Codec_Par.AvCodec_Parameters_Alloc();

			if ((ctx.Par_In == null) || (ctx.Par_Out == null))
			{
				ret = Error.ENOMEM;

				goto Fail;
			}

			// Allocate priv data and init private options
			if (FF_Bsf(filter).Priv_Data_Alloc != null)
			{
				ctx.Priv_Data = FF_Bsf(filter).Priv_Data_Alloc();

				if (ctx.Priv_Data == null)
				{
					ret = Error.ENOMEM;

					goto Fail;
				}

				if (filter.Priv_Class != null)
				{
					filter.Priv_Class.CopyTo(ctx.Priv_Data);

					Opt.Av_Opt_Set_Defaults(ctx.Priv_Data);
				}
			}

			bsfi.Buffer_Pkt = Packet.Av_Packet_Alloc();

			if (bsfi.Buffer_Pkt == null)
			{
				ret = Error.ENOMEM;

				goto Fail;
			}

			pCtx = ctx;

			return 0;

			Fail:
			Av_Bsf_Free(ref ctx);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Prepare the filter for use, after all the parameters and options
		/// have been set
		/// </summary>
		/********************************************************************/
		public static c_int Av_Bsf_Init(AvBsfContext ctx)//XX 149
		{
			// Check that the codec is supported
			if (ctx.Filter.Codec_Ids.IsNotNull)
			{
				c_int i;

				for (i = 0; ctx.Filter.Codec_Ids[i] != AvCodecId.None; i++)
				{
					if (ctx.Par_In.Codec_Id == ctx.Filter.Codec_Ids[i])
						break;
				}

				if (ctx.Filter.Codec_Ids[i] == AvCodecId.None)
				{
					AvCodecDescriptor desc = Codec_Desc.AvCodec_Descriptor_Get(ctx.Par_In.Codec_Id);

					Log.Av_Log(ctx, Log.Av_Log_Error, "Codec '%s' (%d) is not supported by the bitstream filter '%s', Supported codecs are: ", desc != null ? desc.Name : "unknown", ctx.Par_In.Codec_Id, ctx.Filter.Name);

					for (i = 0; ctx.Filter.Codec_Ids[i] != AvCodecId.None; i++)
					{
						AvCodecId codec_Id = ctx.Filter.Codec_Ids[i];
						Log.Av_Log(ctx, Log.Av_Log_Error, "%s (%d) ", Utils_Codec.AvCodec_Get_Name(codec_Id), codec_Id);
					}

					Log.Av_Log(ctx, Log.Av_Log_Error, "\n");

					return Error.EINVAL;
				}
			}

			// Initialize output parameters to be the same as input
			// init below might overwrite that
			c_int ret = Codec_Par.AvCodec_Parameters_Copy(ctx.Par_Out, ctx.Par_In);

			if (ret < 0)
				return ret;

			ctx.Time_Base_Out = ctx.Time_Base_In;

			if (FF_Bsf(ctx.Filter).Init != null)
			{
				ret = FF_Bsf(ctx.Filter).Init(ctx);

				if (ret < 0)
					return ret;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Reset the internal bitstream filter state. Should be called e.g.
		/// when seeking
		/// </summary>
		/********************************************************************/
		public static void Av_Bsf_Flush(AvBsfContext ctx)//XX 190
		{
			FFBsfContext bsfi = FFBsfContext(ctx);

			bsfi.Eof = 0;

			Packet.Av_Packet_Unref(bsfi.Buffer_Pkt);

			if (FF_Bsf(ctx.Filter).Flush != null)
				FF_Bsf(ctx.Filter).Flush(ctx);
		}



		/********************************************************************/
		/// <summary>
		/// Submit a packet for filtering.
		///
		/// After sending each packet, the filter must be completely drained
		/// by calling av_bsf_receive_packet() repeatedly until it returns
		/// AVERROR(EAGAIN) or AVERROR_EOF
		/// </summary>
		/********************************************************************/
		public static c_int Av_Bsf_Send_Packet(AvBsfContext ctx, AvPacket pkt)//XX 202
		{
			FFBsfContext bsfi = FFBsfContext(ctx);

			if ((pkt == null) || Packet.AvPacket_Is_Empty(pkt))
			{
				if (pkt != null)
					Packet.Av_Packet_Unref(pkt);

				bsfi.Eof = 1;

				return 0;
			}

			if (bsfi.Eof != 0)
			{
				Log.Av_Log(ctx, Log.Av_Log_Error, "A non-null packet sent after an EOF.\n");

				return Error.EINVAL;
			}

			if (!Packet.AvPacket_Is_Empty(bsfi.Buffer_Pkt))
				return Error.EAGAIN;

			c_int ret = Packet.Av_Packet_Make_RefCounted(pkt);

			if (ret < 0)
				return ret;

			Packet.Av_Packet_Move_Ref(bsfi.Buffer_Pkt, pkt);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve a filtered packet
		/// </summary>
		/********************************************************************/
		public static c_int Av_Bsf_Receive_Packet(AvBsfContext ctx, AvPacket pkt)//XX 230
		{
			return FF_Bsf(ctx.Filter).Filter(ctx, pkt);
		}



		/********************************************************************/
		/// <summary>
		/// Called by bitstream filters to get packet for filtering.
		/// The reference to packet is moved to provided packet structure
		/// </summary>
		/********************************************************************/
		internal static c_int FF_Bsf_Get_Packet_Ref(AvBsfContext ctx, AvPacket pkt)//XX 256
		{
			FFBsfContext bsfi = FFBsfContext(ctx);

			if (bsfi.Eof != 0)
				return Error.EOF;

			if (Packet.AvPacket_Is_Empty(bsfi.Buffer_Pkt))
				return Error.EAGAIN;

			Packet.Av_Packet_Move_Ref(pkt, bsfi.Buffer_Pkt);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Allocate empty list of bitstream filters.
		/// The list must be later freed by av_bsf_list_free()
		/// or finalized by av_bsf_list_finalize()
		/// </summary>
		/********************************************************************/
		public static AvBsfList Av_Bsf_List_Alloc()//XX 420
		{
			return Mem.Av_MAlloczObj<AvBsfList>();
		}



		/********************************************************************/
		/// <summary>
		/// Free list of bitstream filters
		/// </summary>
		/********************************************************************/
		public static void Av_Bsf_List_Free(ref AvBsfList lst)//XX 425
		{
			if (lst == null)
				return;

			for (c_int i = 0; i < lst.Nb_Bsfs; ++i)
				Av_Bsf_Free(ref lst.Bsfs[i]);

			Mem.Av_Free(lst.Bsfs);
			Mem.Av_FreeP(ref lst);
		}



		/********************************************************************/
		/// <summary>
		/// Append bitstream filter to the list of bitstream filters
		/// </summary>
		/********************************************************************/
		public static c_int Av_Bsf_List_Append(AvBsfList lst, AvBsfContext bsf)//XX 438
		{
			return Mem.Av_DynArray_Add_NoFreeObj(ref lst.Bsfs, ref lst.Nb_Bsfs, bsf);
		}



		/********************************************************************/
		/// <summary>
		/// Finalize list of bitstream filters.
		///
		/// This function will transform AVBSFList to single AVBSFContext,
		/// so the whole chain of bitstream filters can be treated as single
		/// filter freshly allocated by av_bsf_alloc().
		/// If the call is successful, AVBSFList structure is freed and lst
		/// will be set to NULL. In case of failure, caller is responsible
		/// for freeing the structure by av_bsf_list_free()
		/// </summary>
		/********************************************************************/
		public static c_int Av_Bsf_List_Finalize(ref AvBsfList lst, out AvBsfContext bsf)//XX 489
		{
			c_int ret = 0;

			if (lst.Nb_Bsfs == 1)
			{
				bsf = lst.Bsfs[0];

				Mem.Av_FreeP(ref lst.Bsfs);
				lst.Nb_Bsfs = 0;

				goto End;
			}

			ret = Av_Bsf_Alloc(list_Bsf.P, out bsf);

			if (ret < 0)
				return ret;

			BsfListContext ctx = (BsfListContext)bsf.Priv_Data;

			ctx.Bsfs = lst.Bsfs;
			ctx.Nb_Bsfs = lst.Nb_Bsfs;

			End:
			Mem.Av_FreeP(ref lst);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Parse string describing list of bitstream filters and create
		/// single AVBSFContext describing the whole chain of bitstream
		/// filters. Resulting AVBSFContext can be treated as any other
		/// AVBSFContext freshly allocated by av_bsf_alloc()
		/// </summary>
		/********************************************************************/
		public static c_int Av_Bsf_List_Parse_Str(CPointer<char> str, out AvBsfContext bsf_Lst)//XX 526
		{
			bsf_Lst = null;

			c_int ret;

			if (str.IsNull)
				return Av_Bsf_Get_Null_Filter(out bsf_Lst);

			AvBsfList lst = Av_Bsf_List_Alloc();

			if (lst == null)
				return Error.ENOMEM;

			do
			{
				CPointer<char> bsf_Str = AvString.Av_Get_Token(ref str, ",".ToCharPointer());

				ret = Bsf_Parse_Single(bsf_Str, lst);

				Mem.Av_Free(bsf_Str);

				if (ret < 0)
					goto End;
			}
			while ((str[0] != '\0') && (str[1, 1] != '\0'));

			ret = Av_Bsf_List_Finalize(ref lst, out bsf_Lst);

			End:
			if (ret < 0)
				Av_Bsf_List_Free(ref lst);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Get null/pass-through bitstream filter
		/// </summary>
		/********************************************************************/
		public static c_int Av_Bsf_Get_Null_Filter(out AvBsfContext bsf)//XX 553
		{
			return Av_Bsf_Alloc(list_Bsf.P, out bsf);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IPrivateData Alloc_Priv_Data()
		{
			return new BsfListContext();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static FFBitStreamFilter FF_Bsf(AvBitStreamFilter bsf)//XX 36
		{
			return (FFBitStreamFilter)bsf;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static FFBsfContext FFBsfContext(AvBsfContext ctx)//XX 47
		{
			return (FFBsfContext)ctx;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IEnumerable<IOptionContext> Bsf_Child_Next(IOptionContext obj)//XX 77
		{
			AvBsfContext ctx = (AvBsfContext)obj;

			if (ctx.Filter.Priv_Class != null)
				yield return ctx.Priv_Data;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> Bsf_To_Name(IClass bsf)//XX 85
		{
			return ((AvBsfContext)bsf).Filter.Name;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Bsf_List_Init(AvBsfContext bsf)//XX 283
		{
			BsfListContext lst = (BsfListContext)bsf.Priv_Data;
			AvCodecParameters cod_Par = bsf.Par_In;
			AvRational tb = bsf.Time_Base_In;
			c_int ret;

			for (c_int i = 0; i < lst.Nb_Bsfs; ++i)
			{
				ret = Codec_Par.AvCodec_Parameters_Copy(lst.Bsfs[i].Par_In, cod_Par);

				if (ret < 0)
					goto Fail;

				lst.Bsfs[i].Time_Base_In = tb;

				ret = Av_Bsf_Init(lst.Bsfs[i]);

				if (ret < 0)
					goto Fail;

				cod_Par = lst.Bsfs[i].Par_Out;
				tb = lst.Bsfs[i].Time_Base_Out;
			}

			bsf.Time_Base_Out = tb;

			ret = Codec_Par.AvCodec_Parameters_Copy(bsf.Par_Out, cod_Par);

			Fail:
			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Bsf_List_Filter(AvBsfContext bsf, AvPacket @out)//XX 312
		{
			BsfListContext lst = (BsfListContext)bsf.Priv_Data;
			c_int ret, eof = 0;

			if (lst.Nb_Bsfs == 0)
				return FF_Bsf_Get_Packet_Ref(bsf, @out);

			while (true)
			{
				// Get a packet from the previous filter up the chain
				if (lst.Idx != 0)
					ret = Av_Bsf_Receive_Packet(lst.Bsfs[lst.Idx - 1], @out);
				else
					ret = FF_Bsf_Get_Packet_Ref(bsf, @out);

				if (ret == Error.EAGAIN)
				{
					if (lst.Idx == 0)
						return ret;

					lst.Idx--;

					continue;
				}
				else if (ret == Error.EOF)
					eof = 1;
				else if (ret < 0)
					return ret;

				// Send it to the next filter down the chain
				if (lst.Idx < lst.Nb_Bsfs)
				{
					ret = Av_Bsf_Send_Packet(lst.Bsfs[lst.Idx], eof != 0 ? null : @out);

					if (ret < 0)
					{
						Packet.Av_Packet_Unref(@out);

						return ret;
					}

					lst.Idx++;
					eof = 0;
				}
				else if (eof != 0)
					return ret;
				else
					return 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Bsf_List_Flush(AvBsfContext bsf)//XX 354
		{
			BsfListContext lst = (BsfListContext)bsf.Priv_Data;

			for (c_int i = 0; i < lst.Nb_Bsfs; i++)
				Av_Bsf_Flush(lst.Bsfs[i]);

			lst.Idx = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Bsf_List_Close(AvBsfContext bsf)//XX 363
		{
			BsfListContext lst = (BsfListContext)bsf.Priv_Data;

			for (c_int i = 0; i < lst.Nb_Bsfs; ++i)
				Av_Bsf_Free(ref lst.Bsfs[i]);

			Mem.Av_FreeP(ref lst.Bsfs);
			Mem.Av_FreeP(ref lst.Item_Name);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> Bsf_List_Item_Name(IClass ctx)//XX 374
		{
			CPointer<char> null_Filter_Name = "null".ToCharPointer();
			AvBsfContext bsf_Ctx = (AvBsfContext)ctx;
			BsfListContext lst = (BsfListContext)bsf_Ctx.Priv_Data;

			if (lst.Nb_Bsfs == 0)
				return null_Filter_Name;

			if (lst.Item_Name == null)
			{
				BPrint.Av_BPrint_Init(out AVBPrint bp, 16, 128);

				BPrint.Av_BPrintf(bp, "bsf_list(");

				for (c_int i = 0; i < lst.Nb_Bsfs; i++)
					BPrint.Av_BPrintf(bp, i != 0 ? ",%s" : "%s", lst.Bsfs[i].Filter.Name);

				BPrint.Av_BPrintf(bp, ")");

				BPrint.Av_BPrint_Finalize(bp, out lst.Item_Name);
			}

			return lst.Item_Name;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Bsf_List_Append_Internal(AvBsfList lst, CPointer<char> bsf_Name, CPointer<char> options, ref AvDictionary options_Dict)//XX 443
		{
			AvBitStreamFilter filter = BitStream_Filters.Av_Bsf_Get_By_Name(bsf_Name);

			if (filter == null)
				return Error.Bsf_Not_Found;

			c_int ret = Av_Bsf_Alloc(filter, out AvBsfContext bsf);

			if (ret < 0)
				return ret;

			if (options.IsNotNull && (filter.Priv_Class != null))
			{
				AvOption opt = Opt.Av_Opt_Next(bsf.Priv_Data).FirstOrDefault();
				CPointer<CPointer<char>> shorthand = new CPointer<CPointer<char>>(2);

				if (opt != null)
					shorthand[0] = opt.Name;

				ret = Opt.Av_Opt_Set_From_String(bsf.Priv_Data, options, shorthand, "=".ToCharPointer(), ":".ToCharPointer());

				if (ret < 0)
					goto End;
			}

			if (options_Dict != null)
			{
				ret = Opt.Av_Opt_Set_Dict2(bsf, ref options_Dict, AvOptSearch.Search_Children);

				if (ret < 0)
					goto End;
			}

			ret = Av_Bsf_List_Append(lst, bsf);

			End:
			if (ret < 0)
				Av_Bsf_Free(ref bsf);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Bsf_Parse_Single(CPointer<char> str, AvBsfList bsf_Lst)//XX 515
		{
			CPointer<char> bsf_Options_Str = null;

			CPointer<char> bsf_Name = AvString.Av_Strtok(str, "=".ToCharPointer(), ref bsf_Options_Str);

			if (bsf_Name.IsNull)
				return Error.EINVAL;

			AvDictionary tmp = null;
			return Bsf_List_Append_Internal(bsf_Lst, bsf_Name, bsf_Options_Str, ref tmp);
		}
		#endregion
	}
}
