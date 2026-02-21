/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// Logging functions
	/// </summary>
	public static class Log
	{
		/// <summary>
		/// Print no output
		/// </summary>
		public const c_int Av_Log_Quiet = -8;

		/// <summary>
		/// Something went really wrong and we will crash now
		/// </summary>
		public const c_int Av_Log_Panic = 0;

		/// <summary>
		/// Something went wrong and recovery is not possible.
		/// For example, no header was found for a format which depends
		/// on headers or an illegal combination of parameters is used
		/// </summary>
		public const c_int Av_Log_Fatal = 8;

		/// <summary>
		/// Something went wrong and cannot losslessly be recovered.
		/// However, not all future data is affected
		/// </summary>
		public const c_int Av_Log_Error = 16;

		/// <summary>
		/// Something somehow does not look correct. This may or may not
		/// lead to problems. An example would be the use of '-vstrict -2'
		/// </summary>
		public const c_int Av_Log_Warning = 24;

		/// <summary>
		/// Standard information
		/// </summary>
		public const c_int Av_Log_Info = 32;

		/// <summary>
		/// Detailed information
		/// </summary>
		public const c_int Av_Log_Verbose = 40;

		/// <summary>
		/// Stuff which is only useful for libav* developers
		/// </summary>
		public const c_int Av_Log_Debug = 48;

		/// <summary>
		/// Extremely verbose debugging, useful for libav* development
		/// </summary>
		public const c_int Av_Log_Trace = 56;

		private const c_int Nb_Levels = 8;
		private const c_int Line_Sz = 1024;

		// Current log state
		private static AvMutex mutex = new AvMutex();

		private static volatile c_int av_Log_Level = Av_Log_Info;
		private static AvLog flags = AvLog.None;
		private static UtilFunc.Log_Delegate av_Log_Callback = Av_Log_Default_Callback;

		private static c_int callback_Print_Prefix = 1;
		private static c_int callback_Count = 0;
		private static CPointer<char> callback_Prev = new CPointer<char>(Line_Sz);

		/********************************************************************/
		/// <summary>
		/// Return the context name
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Default_Item_Name(IClass ptr)//XX 241
		{
			return ((AvClass)ptr).Class_Name;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Av_Log_Default_Callback(IClass ptr, c_int level, CPointer<char> fmt, params object[] args)//XX 378
		{
			AVBPrint[] part = new AVBPrint[5];
			CPointer<char> line = new CPointer<char>(Line_Sz);
			c_int[] type = new c_int[2];
			c_uint tint = 0;

			if (level >= 0)
			{
				tint = (c_uint)level & 0xff00;
				level &= 0xff;
			}

			if (level > av_Log_Level)
				return;

			CThread.pthread_mutex_lock(mutex);

			Format_Line(ptr, level, fmt, args, part, ref callback_Print_Prefix, type);
			CString.snprintf(line, (size_t)line.Length, "%s%s%s%s", part[0].Str, part[1].Str, part[2].Str, part[3].Str);

			if ((callback_Print_Prefix != 0) && ((flags & AvLog.Skip_Repeated) != 0) && (CString.strcmp(line, callback_Prev) == 0) && (line[0] != '\0') && (line[CString.strlen(line) - 1] != '\r'))
			{
				callback_Count++;
				goto End;
			}

			if (callback_Count > 0)
			{
				CConsole.printf(Console.Error, "    Last message repeated %d times\n", callback_Count);
				callback_Count = 0;
			}

			CString.strcpy(callback_Prev, line);

			Sanitize(part[4].Str);
			Colored_FPuts(7, 0, part[4].Str);
			Sanitize(part[0].Str);
			Colored_FPuts(type[0], 0, part[0].Str);
			Sanitize(part[1].Str);
			Colored_FPuts(type[1], 0, part[1].Str);
			Sanitize(part[2].Str);
			Colored_FPuts(Common.Av_Clip(level >> 3, 0, Nb_Levels - 1), (c_int)(tint >> 8), part[2].Str);
			Sanitize(part[3].Str);
			Colored_FPuts(Common.Av_Clip(level >> 3, 0, Nb_Levels - 1), (c_int)(tint >> 8), part[3].Str);

			End:
			BPrint.Av_BPrint_Finalize(part[3], out _);

			CThread.pthread_mutex_unlock(mutex);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FF_DLog(IClass ctx, string fmt, params object[] args)
		{
			Av_Log(ctx, Av_Log_Debug, fmt, args);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FF_TLog(IClass ctx, string fmt, params object[] args)
		{
			Av_Log(ctx, Av_Log_Trace, fmt, args);
		}



		/********************************************************************/
		/// <summary>
		/// Send the specified message to the log if the level is less than
		/// or equal to the current av_log_level. By default, all logging
		/// messages are sent to stderr. This behavior can be altered by
		/// setting a different logging callback function.
		/// See av_log_set_callback
		/// </summary>
		/********************************************************************/
		public static void Av_Log(IClass avcl, c_int level, string fmt, params object[] args)
		{
			Av_Log(avcl, level, fmt.ToCharPointer(), args);
		}



		/********************************************************************/
		/// <summary>
		/// Send the specified message to the log if the level is less than
		/// or equal to the current av_log_level. By default, all logging
		/// messages are sent to stderr. This behavior can be altered by
		/// setting a different logging callback function.
		/// See av_log_set_callback
		/// </summary>
		/********************************************************************/
		public static void Av_Log(IClass avcl, c_int level, CPointer<char> fmt, params object[] args)//XX 442
		{
			AvClass avc = avcl != null ? (AvClass)avcl : null;
			UtilFunc.Log_Delegate log_Callback = av_Log_Callback;

			if ((avc != null) && (avc.Version >= ((50 << 16) | (15 << 8) | 2)) && (avc.Log_Level_Offset_Name != null) && (level >= Av_Log_Fatal))
			{
				FieldInfo field = avcl.GetType().GetField(avc.Log_Level_Offset_Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				level += (c_int)field.GetValue(avcl);
			}

			if (log_Callback != null)
				log_Callback(avcl, level, fmt, args);
		}



		/********************************************************************/
		/// <summary>
		/// Get the current log level
		/// </summary>
		/********************************************************************/
		public static c_int Av_Log_Get_Level()//XX 470
		{
			return av_Log_Level;
		}



		/********************************************************************/
		/// <summary>
		/// Set the log level
		/// </summary>
		/********************************************************************/
		public static void Av_Log_Set_Level(c_int level)//XX 475
		{
			av_Log_Level = level;
		}



		/********************************************************************/
		/// <summary>
		/// Set the logging callback
		///
		/// Note: The callback must be thread safe, even if the application
		/// does not use threads itself as some codecs are multithreaded.
		///
		/// See av_log_default_callback
		/// </summary>
		/********************************************************************/
		public static void Av_Log_Set_Callback(UtilFunc.Log_Delegate callback)//XX 490
		{
			av_Log_Callback = callback;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void AvPriv_Request_Sample(IClass avc, string msg, params object[] args)//XX 509
		{
			Missing_Feature_Sample(1, avc, msg, args);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void AvPriv_Report_Missing_Feature(IClass avc, string msg, params object[] args)//XX 518
		{
			Missing_Feature_Sample(0, avc, msg, args);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Ansi_Fputs(c_int level, c_int tint, CPointer<char> str, c_int local_Use_Color)//XX 189
		{
			CFile.fputs(str, Console.Error);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Colored_FPuts(c_int level, c_int tint, CPointer<char> str)//XX 213
		{
			c_int local_Use_Color = 0;

			if (str[0] == '\0')
				return;

			Ansi_Fputs(level, tint, str, local_Use_Color);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Sanitize(CPointer<char> line)//XX 251
		{
			while (line[0] != '\0')
			{
				if ((line[0] < 0x08) || ((line[0] > 0x0d) && (line[0] < 0x20)))
					line[0] = '?';

				line++;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Get_Category(IClass ptr)//XX 259
		{
			AvClass avc = ptr as AvClass;

			if ((avc == null) || ((avc.Version & 0xff) < 100) || (avc.Version < ((51 << 16) | (59 << 8))) || (avc.Category >= AvClassCategory.Nb))
				return (c_int)(AvClassCategory.Na + 16);

			if (avc.Get_Category != null)
				return (c_int)(avc.Get_Category(ptr) + 16);

			return (c_int)(avc.Category + 16);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> Get_Level_Str(c_int level)//XX 272
		{
			switch (level)
			{
				case Av_Log_Quiet:
					return "quiet".ToCharPointer();

				case Av_Log_Debug:
					return "debug".ToCharPointer();

				case Av_Log_Trace:
					return "trace".ToCharPointer();

				case Av_Log_Verbose:
					return "verbose".ToCharPointer();

				case Av_Log_Info:
					return "info".ToCharPointer();

				case Av_Log_Warning:
					return "warning".ToCharPointer();

				case Av_Log_Error:
					return "error".ToCharPointer();

				case Av_Log_Fatal:
					return "fatal".ToCharPointer();

				case Av_Log_Panic:
					return "panic".ToCharPointer();

				default:
					return CString.Empty;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<char> Item_Name(IClass obj, AvClass cls)//XX 298
		{
			return cls.Item_Name != null ? cls.Item_Name(obj) : Av_Default_Item_Name(obj);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Format_Date_Now(AVBPrint bp_Time, c_int include_Date)//XX 303
		{
			int64_t time_Us = Time.Av_GetTime();
			int64_t time_Ms = time_Us / 1000;
			time_t time_S = time_Ms / 1000;
			c_int milliSec = (c_int)(time_Ms - (time_S * 1000));
			tm ptm = CTime.localtime_r(time_S, out tm tmBuf);

			if (include_Date != 0)
				BPrint.Av_BPrint_Strftime(bp_Time, "%Y-%m-%d ", ptm);

			BPrint.Av_BPrint_Strftime(bp_Time, "%H:%M:%S", ptm);
			BPrint.Av_BPrintf(bp_Time, ".%03d ", milliSec);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Format_Line(IClass avcl, c_int level, CPointer<char> fmt, object[] args, AVBPrint[] part, ref c_int print_Prefix, c_int[] type)//XX 320
		{
			AvClass avc = avcl != null ? (AvClass)avcl : null;

			BPrint.Av_BPrint_Init(out part[0], 0, BPrint.Av_BPrint_Size_Automatic);
			BPrint.Av_BPrint_Init(out part[1], 0, BPrint.Av_BPrint_Size_Automatic);
			BPrint.Av_BPrint_Init(out part[2], 0, BPrint.Av_BPrint_Size_Automatic);
			BPrint.Av_BPrint_Init(out part[3], 0, 65536);
			BPrint.Av_BPrint_Init(out part[4], 0, BPrint.Av_BPrint_Size_Automatic);

			if (type != null)
				type[0] = type[1] = (c_int)AvClassCategory.Na + 16;

			if ((print_Prefix != 0) && (avc != null))
			{
				if (avc.Parent_Log_Context_Name != null)
				{
					FieldInfo fieldInfo = avcl.GetType().GetField(avc.Parent_Log_Context_Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					AvClass parent = (AvClass)fieldInfo.GetValue(avcl);

					if (parent != null)
					{
						BPrint.Av_BPrintf(part[0], "[%s @ %p] ", Item_Name(parent, parent), parent);

						if (type != null)
							type[0] = Get_Category(parent);
					}
				}

				BPrint.Av_BPrintf(part[1], "[%s @ %p] ", Item_Name(avcl, avc), avcl);

				if (type != null)
					type[1] = Get_Category(avcl);
			}

			if ((print_Prefix != 0) && (level > Av_Log_Quiet) && ((flags & (AvLog.Print_Time | AvLog.Print_DateTime)) != 0))
				Format_Date_Now(part[4], (c_int)(flags & AvLog.Print_DateTime));

			if ((print_Prefix != 0) && (level > Av_Log_Quiet) && ((flags & AvLog.Print_Level) != 0))
				BPrint.Av_BPrintf(part[2], "[%s] ", Get_Level_Str(level));

			BPrint.Av_BPrintf(part[3], fmt, args);

			if ((part[0].Str[0] != '\0') || (part[1].Str[0] != '\0') || (part[2].Str[0] != '\0') || (part[3].Str[0] != '\0'))
			{
				char lastc = (part[3].Len != 0) && (part[3].Len <= part[3].Size) ? part[3].Str[part[3].Len - 1] : '\0';
				print_Prefix = (lastc == '\n') || (lastc == '\r') ? 1 : 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Missing_Feature_Sample(c_int sample, IClass avc, string msg, params object[] args)//XX 495
		{
			Av_Log(avc, Av_Log_Warning, msg, args);
			Av_Log(avc, Av_Log_Warning, " is not implemented. Update your FFmpeg version to the newest one from Git. If the problem still occurs, it means that your file has a feature which has not been implemented.\n");

			if (sample != 0)
				Av_Log(avc, Av_Log_Warning, "If you want to help, upload a sample of this file to https://streams.videolan.org/upload/ and contact the ffmpeg-devel mailing list. (ffmpeg-devel@ffmpeg.org)\n");
		}
		#endregion
	}
}
