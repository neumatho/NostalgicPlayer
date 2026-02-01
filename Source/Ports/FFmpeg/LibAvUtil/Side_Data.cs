/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Side_Data
	{
		private static readonly AvSideDataDescriptor[] sd_Props = BuildProps();

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvSideDataDescriptor[] BuildProps()
		{
			AvSideDataDescriptor[] arr = new AvSideDataDescriptor[Enum.GetValues<AvFrameSideDataType>().Length];

			arr[(c_int)AvFrameSideDataType.PanScan] = new AvSideDataDescriptor("AVPanScan", AvSideDataProps.Size_Dependent);
			arr[(c_int)AvFrameSideDataType.A53_CC] = new AvSideDataDescriptor("ATSC A53 Part 4 Closed Captions", AvSideDataProps.None);
			arr[(c_int)AvFrameSideDataType.MatrixEncoding] = new AvSideDataDescriptor("AVMatrixEncoding", AvSideDataProps.Channel_Dependent);
			arr[(c_int)AvFrameSideDataType.DownMix_Info] = new AvSideDataDescriptor("Metadata relevant to a downmix procedure", AvSideDataProps.Channel_Dependent);
			arr[(c_int)AvFrameSideDataType.Afd] = new AvSideDataDescriptor("Active format description", AvSideDataProps.None);
			arr[(c_int)AvFrameSideDataType.Motion_Vectors] = new AvSideDataDescriptor("Motion vectors", AvSideDataProps.Size_Dependent);
			arr[(c_int)AvFrameSideDataType.Skip_Samples] = new AvSideDataDescriptor("Skip samples", AvSideDataProps.None);
			arr[(c_int)AvFrameSideDataType.Gop_TimeCode] = new AvSideDataDescriptor("GOP timecode", AvSideDataProps.None);
			arr[(c_int)AvFrameSideDataType.S12M_TimeCode] = new AvSideDataDescriptor("SMPTE 12-1 timecode", AvSideDataProps.None);
			arr[(c_int)AvFrameSideDataType.Dynamic_Hdr_Plus] = new AvSideDataDescriptor("HDR Dynamic Metadata SMPTE2094-40 (HDR10+)", AvSideDataProps.Color_Dependent);
			arr[(c_int)AvFrameSideDataType.Dynamic_Hdr_Vivid] = new AvSideDataDescriptor("HDR Dynamic Metadata CUVA 005.1 2021 (Vivid)", AvSideDataProps.Color_Dependent);
			arr[(c_int)AvFrameSideDataType.Regions_Of_Interest] = new AvSideDataDescriptor("Regions Of Interest", AvSideDataProps.Size_Dependent);
			arr[(c_int)AvFrameSideDataType.Video_Enc_Params] = new AvSideDataDescriptor("Video encoding parameters", AvSideDataProps.None);
			arr[(c_int)AvFrameSideDataType.Film_Grain_Params] = new AvSideDataDescriptor("Film grain parameters", AvSideDataProps.None);
			arr[(c_int)AvFrameSideDataType.Detection_BBoxes] = new AvSideDataDescriptor("Bounding boxes for object detection and classification", AvSideDataProps.Size_Dependent);
			arr[(c_int)AvFrameSideDataType.Dovi_Rpu_Buffer] = new AvSideDataDescriptor("Dolby Vision RPU Data", AvSideDataProps.Color_Dependent);
			arr[(c_int)AvFrameSideDataType.Dovi_Metadata] = new AvSideDataDescriptor("Dolby Vision Metadata", AvSideDataProps.Color_Dependent);
			arr[(c_int)AvFrameSideDataType.Lcevc] = new AvSideDataDescriptor("LCEVC NAL data", AvSideDataProps.Size_Dependent);
			arr[(c_int)AvFrameSideDataType.View_Id] = new AvSideDataDescriptor("View ID", AvSideDataProps.None);
			arr[(c_int)AvFrameSideDataType.Stereo3D] = new AvSideDataDescriptor("Stereo 3D", AvSideDataProps.Global);
			arr[(c_int)AvFrameSideDataType.ReplayGain] = new AvSideDataDescriptor("AVReplayGain", AvSideDataProps.Global);
			arr[(c_int)AvFrameSideDataType.DisplayMatrix] = new AvSideDataDescriptor("3x3 displaymatrix", AvSideDataProps.Global);
			arr[(c_int)AvFrameSideDataType.Audio_Service_Type] = new AvSideDataDescriptor("Audio service type", AvSideDataProps.Global);
			arr[(c_int)AvFrameSideDataType.Mastering_Display_Metadata] = new AvSideDataDescriptor("Mastering display metadata", AvSideDataProps.Global | AvSideDataProps.Color_Dependent);
			arr[(c_int)AvFrameSideDataType.Content_Light_Level] = new AvSideDataDescriptor("Content light level metadata", AvSideDataProps.Global | AvSideDataProps.Color_Dependent);
			arr[(c_int)AvFrameSideDataType.Ambient_Viewing_Environment] = new AvSideDataDescriptor("Ambient viewing environment", AvSideDataProps.Global);
			arr[(c_int)AvFrameSideDataType.Spherical] = new AvSideDataDescriptor("Spherical Mapping", AvSideDataProps.Global | AvSideDataProps.Size_Dependent);
			arr[(c_int)AvFrameSideDataType.Icc_Profile] = new AvSideDataDescriptor("ICC profile", AvSideDataProps.Global | AvSideDataProps.Color_Dependent);
			arr[(c_int)AvFrameSideDataType.Exif] = new AvSideDataDescriptor("EXIF metadata", AvSideDataProps.Global);
			arr[(c_int)AvFrameSideDataType.Sei_Unregistered] = new AvSideDataDescriptor("H.26[45] User Data Unregistered SEI message", AvSideDataProps.Multi);
			arr[(c_int)AvFrameSideDataType.Video_Hint] = new AvSideDataDescriptor("Encoding video hint", AvSideDataProps.Size_Dependent);
			arr[(c_int)AvFrameSideDataType._3D_Reference_Displays] = new AvSideDataDescriptor("3D Reference Displays Information", AvSideDataProps.Global);

			return arr;
		}



		/********************************************************************/
		/// <summary>
		/// Get a side data entry of a specific type from an array
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static AvFrameSideData Av_Frame_Side_Data_Get(CPointer<AvFrameSideData> sd, c_int nb_Sd, AvFrameSideDataType type)
		{
			return Av_Frame_Side_Data_Get_C(sd, nb_Sd, type);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static AvSideDataDescriptor Av_Frame_Side_Data_Desc(AvFrameSideDataType type)//XX 62
		{
			c_uint t = (c_uint)type;

			if ((t < Macros.FF_Array_Elems(sd_Props)) && sd_Props[t].Name.IsNotNull)
				return sd_Props[t];

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Frame_Side_Data_Name(AvFrameSideDataType type)//XX 70
		{
			AvSideDataDescriptor desc = Av_Frame_Side_Data_Desc(type);

			return desc != null ? desc.Name : null;
		}



		/********************************************************************/
		/// <summary>
		/// Remove and free all side data instances of the given type from
		/// an array
		/// </summary>
		/********************************************************************/
		public static void Av_Frame_Side_Data_Remove(ref CPointer<AvFrameSideData> sd, ref c_int nb_Sd, AvFrameSideDataType type)//XX 102
		{
			for (c_int i = nb_Sd - 1; i >= 0; i--)
			{
				AvFrameSideData entry = sd[i];

				if (entry.Type != type)
					continue;

				Free_Side_Data_Entry(ref entry);

				sd[i] = sd[nb_Sd - 1];
				nb_Sd--;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Free all side data entries and their contents, then zeroes out
		/// the values which the pointers are pointing to
		/// </summary>
		/********************************************************************/
		public static void Av_Frame_Side_Data_Free(ref CPointer<AvFrameSideData> sd, ref c_int nb_Sd)//XX 133
		{
			for (c_int i = 0; i < nb_Sd; i++)
				Free_Side_Data_Entry(ref sd[i]);

			nb_Sd = 0;

			Mem.Av_FreeP(ref sd);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal static AvFrameSideData FF_Frame_Side_Data_Add_From_Buf(ref CPointer<AvFrameSideData> sd, ref c_int nb_Sd, AvFrameSideDataType type, AvBufferRef buf)//XX 173
		{
			if (buf == null)
				return null;

			return Add_Side_Data_From_Buf_Ext(ref sd, ref nb_Sd, type, buf, buf.Data);
		}



		/********************************************************************/
		/// <summary>
		/// Add new side data entry to an array
		/// </summary>
		/********************************************************************/
		public static AvFrameSideData Av_Frame_Side_Data_New(ref CPointer<AvFrameSideData> sd, ref c_int nb_Sd, AvFrameSideDataType type, size_t size, AvFrameSideDataFlag flags)//XX 198
		{
			AvSideDataDescriptor desc = Av_Frame_Side_Data_Desc(type);
			AvBufferRef buf = Buffer.Av_Buffer_Alloc(size);
			AvFrameSideData ret = null;

			if ((flags & AvFrameSideDataFlag.Unique) != 0)
				Av_Frame_Side_Data_Remove(ref sd, ref nb_Sd, type);

			if (((desc == null) || ((desc.Props & AvSideDataProps.Multi) == 0)) && ((ret = Av_Frame_Side_Data_Get(sd, nb_Sd, type)) != null))
			{
				ret = Replace_Side_Data_From_Buf(ret, buf, flags);

				if (ret == null)
					Buffer.Av_Buffer_Unref(ref buf);

				return ret;
			}

			ret = FF_Frame_Side_Data_Add_From_Buf(ref sd, ref nb_Sd, type, buf);

			if (ret == null)
				Buffer.Av_Buffer_Unref(ref buf);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Add a new side data entry to an array from an existing
		/// AVBufferRef
		/// </summary>
		/********************************************************************/
		public static AvFrameSideData Av_Frame_Side_Data_Add(ref CPointer<AvFrameSideData> sd, ref c_int nb_Sd, AvFrameSideDataType type, ref AvBufferRef pBuf, AvFrameSideDataFlag flags)//XX 223
		{
			AvSideDataDescriptor desc = Av_Frame_Side_Data_Desc(type);
			AvFrameSideData sd_Dst = null;
			AvBufferRef buf = pBuf;

			if (((flags & AvFrameSideDataFlag.New_Ref) != 0) && ((buf = Buffer.Av_Buffer_Ref(pBuf)) == null))
				return null;

			if ((flags & AvFrameSideDataFlag.Unique) != 0)
				Av_Frame_Side_Data_Remove(ref sd, ref nb_Sd, type);

			if (((desc == null) || ((desc.Props & AvSideDataProps.Multi) == 0)) && ((sd_Dst = Av_Frame_Side_Data_Get(sd, nb_Sd, type)) != null))
				sd_Dst = Replace_Side_Data_From_Buf(sd_Dst, buf, flags);
			else
				sd_Dst = FF_Frame_Side_Data_Add_From_Buf(ref sd, ref nb_Sd, type, buf);

			if ((sd_Dst != null) && ((flags & AvFrameSideDataFlag.New_Ref) == 0))
				pBuf = null;
			else if ((sd_Dst == null) && ((flags & AvFrameSideDataFlag.New_Ref) != 0))
				Buffer.Av_Buffer_Unref(ref buf);

			return sd_Dst;
		}



		/********************************************************************/
		/// <summary>
		/// Add a new side data entry to an array based on existing side
		/// data, taking a reference towards the contained AVBufferRef
		/// </summary>
		/********************************************************************/
		public static c_int Av_Frame_Side_Data_Clone(ref CPointer<AvFrameSideData> sd, ref c_int nb_Sd, AvFrameSideData src, AvFrameSideDataFlag flags)//XX 248
		{
			AvBufferRef buf = null;
			AvFrameSideData sd_Dst = null;
			c_int ret = Error.Bug;

			if ((src == null) || ((nb_Sd != 0) && sd.IsNull))
				return Error.EINVAL;

			AvSideDataDescriptor desc = Av_Frame_Side_Data_Desc(src.Type);

			if ((flags & AvFrameSideDataFlag.Unique) != 0)
				Av_Frame_Side_Data_Remove(ref sd, ref nb_Sd, src.Type);

			if (((desc == null) || ((desc.Props & AvSideDataProps.Multi) == 0)) && ((sd_Dst = Av_Frame_Side_Data_Get(sd, nb_Sd, src.Type)) != null))
			{
				AvDictionary dict = null;

				if ((flags & AvFrameSideDataFlag.Replace) == 0)
					return Error.EEXIST;

				ret = Dict.Av_Dict_Copy(ref dict, src.Metadata, AvDict.None);

				if (ret < 0)
					return ret;

				ret = Buffer.Av_Buffer_Replace(ref sd_Dst.Buf, src.Buf);

				if (ret < 0)
				{
					Dict.Av_Dict_Free(ref dict);

					return ret;
				}

				Dict.Av_Dict_Free(ref sd_Dst.Metadata);

				sd_Dst.Metadata = dict;
				sd_Dst.Data = src.Data;

				return 0;
			}

			buf = Buffer.Av_Buffer_Ref(src.Buf);

			if (buf == null)
				return Error.ENOMEM;

			sd_Dst = Add_Side_Data_From_Buf_Ext(ref sd, ref nb_Sd, src.Type, buf, src.Data);

			if (sd_Dst == null)
			{
				Buffer.Av_Buffer_Unref(ref buf);

				return Error.ENOMEM;
			}

			ret = Dict.Av_Dict_Copy(ref sd_Dst.Metadata, src.Metadata, AvDict.None);

			if (ret < 0)
			{
				Remove_Side_Data_By_Entry(ref sd, ref nb_Sd, sd_Dst);

				return ret;
			}

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Free_Side_Data_Entry(ref AvFrameSideData ptr_Sd)//XX 76
		{
			AvFrameSideData sd = ptr_Sd;

			Buffer.Av_Buffer_Unref(ref sd.Buf);
			Dict.Av_Dict_Free(ref sd.Metadata);
			Mem.Av_FreeP(ref ptr_Sd);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Remove_Side_Data_By_Entry(ref CPointer<AvFrameSideData> sd, ref c_int nb_Sd, AvFrameSideData target)//XX 85
		{
			for (c_int i = nb_Sd - 1; i >= 0; i--)
			{
				AvFrameSideData entry = sd[i];

				if (entry != target)
					continue;

				Free_Side_Data_Entry(ref entry);

				sd[i] = sd[nb_Sd - 1];
				nb_Sd--;

				return;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvFrameSideData Add_Side_Data_From_Buf_Ext(ref CPointer<AvFrameSideData> sd, ref c_int nb_Sd, AvFrameSideDataType type, AvBufferRef buf, IDataContext data)//XX 142
		{
			// *nb_sd + 1 needs to fit into an int and a size_t
			if ((c_uint)nb_Sd >= Macros.FFMin((uint64_t)c_int.MaxValue, size_t.MaxValue))
				return null;

			CPointer<AvFrameSideData> tmp = Mem.Av_Realloc_ArrayObj(sd, (size_t)nb_Sd + 1);

			if (tmp.IsNull)
				return null;

			sd = tmp;

			AvFrameSideData ret = Mem.Av_MAlloczObj<AvFrameSideData>();

			if (ret == null)
				return null;

			ret.Buf = buf;
			ret.Data = data;
			ret.Type = type;

			sd[nb_Sd++] = ret;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static AvFrameSideData Replace_Side_Data_From_Buf(AvFrameSideData dst, AvBufferRef buf, AvFrameSideDataFlag flags)//XX 184
		{
			if ((flags & AvFrameSideDataFlag.Replace) == 0)
				return null;

			Dict.Av_Dict_Free(ref dst.Metadata);
			Buffer.Av_Buffer_Unref(ref dst.Buf);

			dst.Buf = buf;
			dst.Data = buf.Data;

			return dst;
		}



		/********************************************************************/
		/// <summary>
		/// Get a side data entry of a specific type from an array
		/// </summary>
		/********************************************************************/
		private static AvFrameSideData Av_Frame_Side_Data_Get_C(CPointer<AvFrameSideData> sd, c_int nb_Sd, AvFrameSideDataType type)//XX 306
		{
			for (c_int i = 0; i < nb_Sd; i++)
			{
				if (sd[i].Type == type)
					return sd[i];
			}

			return null;
		}
		#endregion
	}
}
