/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// All function delegates
	/// </summary>
	public static class UtilFunc
	{
		/// <summary></summary>
		public delegate CPointer<char> ItemName_Delegate(IClass ctx);
		/// <summary></summary>
		public delegate AvClassCategory GetCategory_Delegate(IClass ctx);
		/// <summary></summary>
		public delegate c_int QueryRanges_Delegate(out AvOptionRanges ranges, IOptionContext obj, CPointer<char> key, AvOptSearch flags);
		/// <summary></summary>
		public delegate IEnumerable<IOptionContext> ChildNext_Delegate(IOptionContext obj);
		/// <summary></summary>
		public delegate IEnumerable<AvClass> ChildClassIterater_Delegate();

		/// <summary></summary>
		internal delegate size_t Read_Delegate(FFFile f, CPointer<char> buf, size_t len);

		/// <summary></summary>
		public delegate void Success_Delegate<T>(CPointer<T> array, size_t count) where T : class, new();
		/// <summary></summary>
		public delegate void Failure_Delegate<T>(ref CPointer<T> array, ref size_t count);

		/// <summary></summary>
		public delegate c_double Func0_Delegate(c_double a);
		/// <summary></summary>
		public delegate c_double Func1_Delegate(IOpaque o, c_double a);
		/// <summary></summary>
		public delegate c_double Func2_Delegate(IOpaque o, c_double a, c_double b);

		/// <summary></summary>
		public delegate IOpaque Allocator_Delegate();

		/// <summary></summary>
		public delegate void Log_Delegate(IClass ctx, c_int level, CPointer<char> fmt, params object[] args);

		/// <summary></summary>
		public delegate void Buffer_Free_Delegate(IOpaque opaque, IDataContext data);

		/// <summary></summary>
		public delegate void Utf8_Write_Delegate(uint8_t @byte);
		/// <summary></summary>
		public delegate ushort Utf8_Get16_Delegate();

		/// <summary></summary>
		public delegate c_int Init_Cb_Delegate(AvRefStructOpaque opaque, IOpaque obj);
		/// <summary></summary>
		public delegate void Reset_Cb_Delegate(AvRefStructOpaque opaque, IOpaque obj);
		/// <summary></summary>
		public delegate void Free_Entry_Delegate(AvRefStructOpaque opaque, IOpaque obj);
		/// <summary></summary>
		public delegate void Free_Cb_Delegate(AvRefStructOpaque opaque);

		/// <summary></summary>
		public delegate void Free2_Cb_Delegate(AvRefStructOpaque opaque, IRefCount obj);
		/// <summary></summary>
		internal delegate void Free_Delegate(IRefCount @ref);

		/// <summary></summary>
		public delegate void HwFreeFrames_Delegate(AvHwFramesContext ctx);
		/// <summary></summary>
		public delegate void HwFreeDevice_Delegate(AvHwDeviceContext ctx);

		/// <summary></summary>
		public delegate c_int DeviceCreate_Delegate(AvHwDeviceContext ctx, CPointer<char> name, ref AvDictionary opts, c_int flags);
		/// <summary></summary>
		public delegate c_int DeviceDerive_Delegate(AvHwDeviceContext dst_Ctx, AvHwDeviceContext src_Ctx, ref AvDictionary opts, c_int flags);
		/// <summary></summary>
		public delegate c_int DeviceInit_Delegate(AvHwDeviceContext ctx);
		/// <summary></summary>
		public delegate void DeviceUninit_Delegate(AvHwDeviceContext ctx);
		/// <summary></summary>
		public delegate c_int FramesGetConstraints_Delegate(AvHwDeviceContext ctx, object hwConfig, AvHwFramesConstraints constraints);//XX
		/// <summary></summary>
		public delegate c_int FramesInit_Delegate(AvHwFramesContext ctx);
		/// <summary></summary>
		public delegate void FramesUninit_Delegate(AvHwFramesContext ctx);
		/// <summary></summary>
		public delegate c_int FramesGetBuffer_Delegate(AvHwFramesContext ctx, AvFrame frame);
		/// <summary></summary>
		public delegate c_int TransferGetFormats_Delegate(AvHwFramesContext ctx, AvHwFrameTransferDirection dir, out CPointer<AvPixelFormat> formats);
		/// <summary></summary>
		public delegate c_int TransferDataTo_Delegate(AvHwFramesContext ctx, AvFrame dst, AvFrame src);
		/// <summary></summary>
		public delegate c_int TransferDataFrom_Delegate(AvHwFramesContext ctx, AvFrame dst, AvFrame src);
		/// <summary></summary>
		public delegate c_int MapTo_Delegate(AvHwFramesContext ctx, AvFrame dst, AvFrame src, c_int flags);
		/// <summary></summary>
		public delegate c_int MapFrom_Delegate(AvHwFramesContext ctx, AvFrame dst, AvFrame src, c_int flags);
		/// <summary></summary>
		public delegate c_int FramesDeriveTo_Delegate(AvHwFramesContext dst_Ctx, AvHwFramesContext src_Ctx, c_int flags);
		/// <summary></summary>
		public delegate c_int FramesDeriveFrom_Delegate(AvHwFramesContext dst_Ctx, AvHwFramesContext src_Ctx, c_int flags);

		/// <summary></summary>
		internal delegate void Unmap_Delegate(AvHwFramesContext ctx, HwMapDescriptor hwMap);

		/// <summary></summary>
		public delegate AvBufferRef Alloc_Delegate(size_t size);
		/// <summary></summary>
		public delegate AvBufferRef Alloc2_Delegate(IOpaque opaque, size_t size);
		/// <summary></summary>
		public delegate void Pool_Free_Delegate(IOpaque opaque);

		/// <summary></summary>
		public delegate IDataContext Alloc_DataContext_Delegate();

		/// <summary></summary>
		public delegate void Vector_FMul_Delegate(CPointer<c_float> dst, CPointer<c_float> src0, CPointer<c_float> src1, c_int len);
		/// <summary></summary>
		public delegate void Vector_FMac_Scalar_Delegate(CPointer<c_float> dst, CPointer<c_float> src, c_float mul, c_int len);
		/// <summary></summary>
		public delegate void Vector_DMac_Scalar_Delegate(CPointer<c_double> dst, CPointer<c_double> src, c_double mul, c_int len);
		/// <summary></summary>
		public delegate void Vector_FMul_Scalar_Delegate(CPointer<c_float> dst, CPointer<c_float> src, c_float mul, c_int len);
		/// <summary></summary>
		public delegate void Vector_DMul_Scalar_Delegate(CPointer<c_double> dst, CPointer<c_double> src, c_double mul, c_int len);
		/// <summary></summary>
		public delegate void Vector_FMul_Window_Delegate(CPointer<c_float> dst, CPointer<c_float> src0, CPointer<c_float> src1, CPointer<c_float> win, c_int len);
		/// <summary></summary>
		public delegate void Vector_FMul_Add_Delegate(CPointer<c_float> dst, CPointer<c_float> src0, CPointer<c_float> src1, CPointer<c_float> src2, c_int len);
		/// <summary></summary>
		public delegate void Vector_FMul_Reverse_Delegate(CPointer<c_float> dst, CPointer<c_float> src0, CPointer<c_float> src1, c_int len);
		/// <summary></summary>
		public delegate void Butterflies_Float_Delegate(CPointer<c_float> v1, CPointer<c_float> v2, c_int len);
		/// <summary></summary>
		public delegate c_float ScalarProduct_Float_Delegate(CPointer<c_float> v1, CPointer<c_float> v2, c_int len);
		/// <summary></summary>
		public delegate void Vector_DMul_Delegate(CPointer<c_double> dst, CPointer<c_double> src0, CPointer<c_double> src1, c_int len);
		/// <summary></summary>
		public delegate c_double ScalarProduct_Double_Delegate(CPointer<c_double> v1, CPointer<c_double> v2, size_t len);

		/// <summary></summary>
		public delegate void Av_Tx_Fn(AvTxContext s, IPointer @out, IPointer @in, ptrdiff_t stride);
		/// <summary></summary>
		public delegate c_int Tx_Init_Delegate(AvTxContext s, FFTxCodelet cd, AvTxFlags flags, FFTxCodeletOptions opts, c_int len, c_int inv, object scale);
		/// <summary></summary>
		public delegate void Tx_Uninit_Delegate(AvTxContext s);

		/// <summary></summary>
		public delegate void Thread_Worker_Func_Delegate(IOpaque priv, c_int jobNr, c_int threadNr, c_int nb_Jobs, c_int nb_Threads);
		/// <summary></summary>
		public delegate void Thread_Main_Func_Delegate(IOpaque priv);
	}
}
