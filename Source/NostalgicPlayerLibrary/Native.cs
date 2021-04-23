/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.PlayerLibrary
{
	/// <summary>
	/// Handle loading and calling native library
	/// </summary>
	internal static class Native
	{
		public const int FRACBITS = 11;

		public const int CLICK_SHIFT = 6;
		public const int CLICK_BUFFER = 1 << CLICK_SHIFT;

		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibrary(string dllToLoad);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

		[DllImport("kernel32.dll")]
		private static extern bool FreeLibrary(IntPtr hModule);

		private static IntPtr dllPtr = IntPtr.Zero;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		static Native()
		{
			AppDomain.CurrentDomain.ProcessExit += Native_Dtor;

			string oldPath = Environment.CurrentDirectory;

			try
			{
				Assembly ass = Assembly.GetEntryAssembly();
				string path = Path.GetDirectoryName(ass.Location);

				// Load the dll file
				Environment.CurrentDirectory = path;

				if (Environment.Is64BitProcess)
					dllPtr = LoadLibrary(@"NostalgicPlayerLibrary_Native-x64.dll");
				else
					dllPtr = LoadLibrary(@"NostalgicPlayerLibrary_Native-x86.dll");
			}
			finally
			{
				Environment.CurrentDirectory = oldPath;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Destructor
		/// </summary>
		/********************************************************************/
		private static void Native_Dtor(object sender, EventArgs e)
		{
			if (dllPtr != IntPtr.Zero)
			{
				FreeLibrary(dllPtr);
				dllPtr = IntPtr.Zero;
			}
		}

		#region Mixer calls

		#region 32 bit mixers (only used on 32-bit platforms)

		#region 8-bit samples
		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private delegate int del_Mix8MonoNormal32(IntPtr source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel);
		public static int Mix8MonoNormal32(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel)
		{
			if (_Mix8MonoNormal32 == null)
			{
				IntPtr f = GetProcAddress(dllPtr, "_Mix8MonoNormal32@28");
				_Mix8MonoNormal32 = (del_Mix8MonoNormal32)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix8MonoNormal32));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix8MonoNormal32(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix8MonoNormal32 _Mix8MonoNormal32 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private delegate int del_Mix8StereoNormal32(IntPtr source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel);
		public static int Mix8StereoNormal32(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			if (_Mix8StereoNormal32 == null)
			{
				IntPtr f = GetProcAddress(dllPtr, "_Mix8StereoNormal32@32");
				_Mix8StereoNormal32 = (del_Mix8StereoNormal32)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix8StereoNormal32));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix8StereoNormal32(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, rVolSel);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix8StereoNormal32 _Mix8StereoNormal32 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit surround sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		public static int Mix8SurroundNormal32(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a mono output buffer with interpolation
		/// </summary>
		/********************************************************************/
		private delegate int del_Mix8MonoInterp32(IntPtr source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int oldLVol, ref int rampVol);
		public static int Mix8MonoInterp32(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int oldLVol, ref int rampVol)
		{
			if (_Mix8MonoInterp32 == null)
			{
				IntPtr f = GetProcAddress(dllPtr, "_Mix8MonoInterp32@36");
				_Mix8MonoInterp32 = (del_Mix8MonoInterp32)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix8MonoInterp32));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix8MonoInterp32(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, oldLVol, ref rampVol);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix8MonoInterp32 _Mix8MonoInterp32 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private delegate int del_Mix8StereoInterp32(IntPtr source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol);
		public static int Mix8StereoInterp32(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			if (_Mix8StereoInterp32 == null)
			{
				IntPtr f = GetProcAddress(dllPtr, "_Mix8StereoInterp32@44");
				_Mix8StereoInterp32 = (del_Mix8StereoInterp32)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix8StereoInterp32));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix8StereoInterp32(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, rVolSel, oldLVol, oldRVol, ref rampVol);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix8StereoInterp32 _Mix8StereoInterp32 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit surround sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		public static int Mix8SurroundInterp32(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			return index;
		}
		#endregion

		#region 16-bit samples
		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private delegate int del_Mix16MonoNormal32(IntPtr source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel);
		public static int Mix16MonoNormal32(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel)
		{
			if (_Mix16MonoNormal32 == null)
			{
				IntPtr f = GetProcAddress(dllPtr, "_Mix16MonoNormal32@28");
				_Mix16MonoNormal32 = (del_Mix16MonoNormal32)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix16MonoNormal32));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix16MonoNormal32(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix16MonoNormal32 _Mix16MonoNormal32 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private delegate int del_Mix16StereoNormal32(IntPtr source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel);
		public static int Mix16StereoNormal32(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			if (_Mix16StereoNormal32 == null)
			{
				IntPtr f = GetProcAddress(dllPtr, "_Mix16StereoNormal32@32");
				_Mix16StereoNormal32 = (del_Mix16StereoNormal32)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix16StereoNormal32));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix16StereoNormal32(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, rVolSel);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix16StereoNormal32 _Mix16StereoNormal32 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit surround sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		public static int Mix16SurroundNormal32(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a mono output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private delegate int del_Mix16MonoInterp32(IntPtr source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int oldLVol, ref int rampVol);
		public static int Mix16MonoInterp32(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int oldLVol, ref int rampVol)
		{
			if (_Mix16MonoInterp32 == null)
			{
				IntPtr f = GetProcAddress(dllPtr, "_Mix16MonoInterp32@36");
				_Mix16MonoInterp32 = (del_Mix16MonoInterp32)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix16MonoInterp32));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix16MonoInterp32(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, oldLVol, ref rampVol);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix16MonoInterp32 _Mix16MonoInterp32 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private delegate int del_Mix16StereoInterp32(IntPtr source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol);
		public static int Mix16StereoInterp32(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			if (_Mix16StereoInterp32 == null)
			{
				IntPtr f = GetProcAddress(dllPtr, "_Mix16StereoInterp32@44");
				_Mix16StereoInterp32 = (del_Mix16StereoInterp32)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix16StereoInterp32));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix16StereoInterp32(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, rVolSel, oldLVol, oldRVol, ref rampVol);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix16StereoInterp32 _Mix16StereoInterp32 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit surround sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		public static int Mix16SurroundInterp32(sbyte[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			return index;
		}
		#endregion

		#endregion

		#region 64-bit mixers (used on both platforms)

		#region 8-bit samples
		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private delegate long del_Mix8MonoNormal64(IntPtr source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel);
		public static long Mix8MonoNormal64(sbyte[] source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel)
		{
			if (_Mix8MonoNormal64 == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "Mix8MonoNormal64") : GetProcAddress(dllPtr, "_Mix8MonoNormal64@36");
				_Mix8MonoNormal64 = (del_Mix8MonoNormal64)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix8MonoNormal64));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix8MonoNormal64(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix8MonoNormal64 _Mix8MonoNormal64 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private delegate long del_Mix8StereoNormal64(IntPtr source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int rVolSel);
		public static long Mix8StereoNormal64(sbyte[] source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int rVolSel)
		{
			if (_Mix8StereoNormal64 == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "Mix8StereoNormal64") : GetProcAddress(dllPtr, "_Mix8StereoNormal64@40");
				_Mix8StereoNormal64 = (del_Mix8StereoNormal64)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix8StereoNormal64));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix8StereoNormal64(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, rVolSel);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix8StereoNormal64 _Mix8StereoNormal64 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit surround sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		public static long Mix8SurroundNormal64(sbyte[] source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int rVolSel)
		{
			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a mono output buffer with interpolation
		/// </summary>
		/********************************************************************/
		private delegate long del_Mix8MonoInterp64(IntPtr source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int oldLVol, ref int rampVol);
		public static long Mix8MonoInterp64(sbyte[] source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int oldLVol, ref int rampVol)
		{
			if (_Mix8MonoInterp64 == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "Mix8MonoInterp64") : GetProcAddress(dllPtr, "_Mix8MonoInterp64@44");
				_Mix8MonoInterp64 = (del_Mix8MonoInterp64)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix8MonoInterp64));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix8MonoInterp64(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, oldLVol, ref rampVol);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix8MonoInterp64 _Mix8MonoInterp64 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private delegate long del_Mix8StereoInterp64(IntPtr source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol);
		public static long Mix8StereoInterp64(sbyte[] source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			if (_Mix8StereoInterp64 == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "Mix8StereoInterp64") : GetProcAddress(dllPtr, "_Mix8StereoInterp64@52");
				_Mix8StereoInterp64 = (del_Mix8StereoInterp64)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix8StereoInterp64));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix8StereoInterp64(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, rVolSel, oldLVol, oldRVol, ref rampVol);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix8StereoInterp64 _Mix8StereoInterp64 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 8 bit surround sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		public static long Mix8SurroundInterp64(sbyte[] source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			return index;
		}
		#endregion

		#region 16-bit samples
		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private delegate long del_Mix16MonoNormal64(IntPtr source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel);
		public static long Mix16MonoNormal64(sbyte[] source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel)
		{
			if (_Mix16MonoNormal64 == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "Mix16MonoNormal64") : GetProcAddress(dllPtr, "_Mix16MonoNormal64@36");
				_Mix16MonoNormal64 = (del_Mix16MonoNormal64)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix16MonoNormal64));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix16MonoNormal64(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix16MonoNormal64 _Mix16MonoNormal64 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private delegate long del_Mix16StereoNormal64(IntPtr source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int rVolSel);
		public static long Mix16StereoNormal64(sbyte[] source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int rVolSel)
		{
			if (_Mix16StereoNormal64 == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "Mix16StereoNormal64") : GetProcAddress(dllPtr, "_Mix16StereoNormal64@40");
				_Mix16StereoNormal64 = (del_Mix16StereoNormal64)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix16StereoNormal64));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix16StereoNormal64(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, rVolSel);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix16StereoNormal64 _Mix16StereoNormal64 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit surround sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		public static long Mix16SurroundNormal64(sbyte[] source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int rVolSel)
		{
			return index;
		}



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a mono output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private delegate long del_Mix16MonoInterp64(IntPtr source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int oldLVol, ref int rampVol);
		public static long Mix16MonoInterp64(sbyte[] source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int oldLVol, ref int rampVol)
		{
			if (_Mix16MonoInterp64 == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "Mix16MonoInterp64") : GetProcAddress(dllPtr, "_Mix16MonoInterp64@44");
				_Mix16MonoInterp64 = (del_Mix16MonoInterp64)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix16MonoInterp64));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix16MonoInterp64(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, oldLVol, ref rampVol);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix16MonoInterp64 _Mix16MonoInterp64 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		private delegate long del_Mix16StereoInterp64(IntPtr source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol);
		public static long Mix16StereoInterp64(sbyte[] source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			if (_Mix16StereoInterp64 == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "Mix16StereoInterp64") : GetProcAddress(dllPtr, "_Mix16StereoInterp64@52");
				_Mix16StereoInterp64 = (del_Mix16StereoInterp64)Marshal.GetDelegateForFunctionPointer(f, typeof(del_Mix16StereoInterp64));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _Mix16StereoInterp64(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, rVolSel, oldLVol, oldRVol, ref rampVol);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_Mix16StereoInterp64 _Mix16StereoInterp64 = null;



		/********************************************************************/
		/// <summary>
		/// Mixes a 16 bit surround sample into a stereo output buffer with
		/// interpolation
		/// </summary>
		/********************************************************************/
		public static long Mix16SurroundInterp64(sbyte[] source, IntPtr dest, int offset, long index, long increment, int todo, int lVolSel, int rVolSel, int oldLVol, int oldRVol, ref int rampVol)
		{
			return index;
		}
		#endregion

		#endregion

		#region DSP
		/********************************************************************/
		/// <summary>
		/// Adds the Amiga LED filter
		/// </summary>
		/********************************************************************/
		private delegate int del_AddAmigaFilter(bool stereo, IntPtr dest, int todo, ref int filterPrevLeft, ref int filterPrevRight);
		public static void AddAmigaFilter(bool stereo, int[] dest, int todo, ref int filterPrevLeft, ref int filterPrevRight)
		{
			if (_AddAmigaFilter == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "AddAmigaFilter") : GetProcAddress(dllPtr, "_AddAmigaFilter@20");
				_AddAmigaFilter = (del_AddAmigaFilter)Marshal.GetDelegateForFunctionPointer(f, typeof(del_AddAmigaFilter));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(dest, GCHandleType.Pinned);

			try
			{
				_AddAmigaFilter(stereo, pinnedBuf.AddrOfPinnedObject(), todo, ref filterPrevLeft, ref filterPrevRight);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_AddAmigaFilter _AddAmigaFilter = null;
		#endregion

		#region Buffer conversion calls
		/********************************************************************/
		/// <summary>
		/// Converts the mixed data to a 16 bit sample buffer
		/// </summary>
		/********************************************************************/
		private delegate void del_MixConvertTo16(IntPtr dest, int offset, IntPtr source, int count, bool swapSpeakers);
		public static void MixConvertTo16(byte[] dest, int offset, int[] source, int count, bool swapSpeakers)
		{
			if (_MixConvertTo16 == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "MixConvertTo16") : GetProcAddress(dllPtr, "_MixConvertTo16@20");
				_MixConvertTo16 = (del_MixConvertTo16)Marshal.GetDelegateForFunctionPointer(f, typeof(del_MixConvertTo16));
			}

			GCHandle destBuf = GCHandle.Alloc(dest, GCHandleType.Pinned);

			try
			{
				GCHandle sourceBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

				try
				{
					_MixConvertTo16(destBuf.AddrOfPinnedObject(), offset, sourceBuf.AddrOfPinnedObject(), count, swapSpeakers);
				}
				finally
				{
					sourceBuf.Free();
				}
			}
			finally
			{
				destBuf.Free();
			}
		}
		private static del_MixConvertTo16 _MixConvertTo16 = null;



		/********************************************************************/
		/// <summary>
		/// Converts the mixed data to a 32 bit sample buffer
		/// </summary>
		/********************************************************************/
		private delegate void del_MixConvertTo32(IntPtr dest, int offset, IntPtr source, int count, bool swapSpeakers);
		public static void MixConvertTo32(byte[] dest, int offset, int[] source, int count, bool swapSpeakers)
		{
			if (_MixConvertTo32 == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "MixConvertTo32") : GetProcAddress(dllPtr, "_MixConvertTo32@20");
				_MixConvertTo32 = (del_MixConvertTo32)Marshal.GetDelegateForFunctionPointer(f, typeof(del_MixConvertTo32));
			}

			GCHandle destBuf = GCHandle.Alloc(dest, GCHandleType.Pinned);

			try
			{
				GCHandle sourceBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

				try
				{
					_MixConvertTo32(destBuf.AddrOfPinnedObject(), offset, sourceBuf.AddrOfPinnedObject(), count, swapSpeakers);
				}
				finally
				{
					sourceBuf.Free();
				}
			}
			finally
			{
				destBuf.Free();
			}
		}
		private static del_MixConvertTo32 _MixConvertTo32 = null;
		#endregion

		#endregion

		#region Resampler calls
		/********************************************************************/
		/// <summary>
		/// Resample a mono sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private delegate int del_ResampleMonoToMonoNormal(IntPtr source, IntPtr dest, int offset, int index, int increment, int todo, int volSel);
		public static int ResampleMonoToMonoNormal(int[] source, IntPtr dest, int offset, int index, int increment, int todo, int volSel)
		{
			if (_ResampleMonoToMonoNormal == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "ResampleMonoToMonoNormal") : GetProcAddress(dllPtr, "_ResampleMonoToMonoNormal@28");
				_ResampleMonoToMonoNormal = (del_ResampleMonoToMonoNormal)Marshal.GetDelegateForFunctionPointer(f, typeof(del_ResampleMonoToMonoNormal));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _ResampleMonoToMonoNormal(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, volSel);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_ResampleMonoToMonoNormal _ResampleMonoToMonoNormal = null;



		/********************************************************************/
		/// <summary>
		/// Resample a mono sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private delegate int del_ResampleMonoToStereoNormal(IntPtr source, IntPtr dest, int offset, int index, int increment, int todo, int VolSel);
		public static int ResampleMonoToStereoNormal(int[] source, IntPtr dest, int offset, int index, int increment, int todo, int volSel)
		{
			if (_ResampleMonoToStereoNormal == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "ResampleMonoToStereoNormal") : GetProcAddress(dllPtr, "_ResampleMonoToStereoNormal@28");
				_ResampleMonoToStereoNormal = (del_ResampleMonoToStereoNormal)Marshal.GetDelegateForFunctionPointer(f, typeof(del_ResampleMonoToStereoNormal));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _ResampleMonoToStereoNormal(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, volSel);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_ResampleMonoToStereoNormal _ResampleMonoToStereoNormal = null;



		/********************************************************************/
		/// <summary>
		/// Resample a stereo sample into a mono output buffer
		/// </summary>
		/********************************************************************/
		private delegate int del_ResampleStereoToMonoNormal(IntPtr source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel);
		public static int ResampleStereoToMonoNormal(int[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			if (_ResampleStereoToMonoNormal == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "ResampleStereoToMonoNormal") : GetProcAddress(dllPtr, "_ResampleStereoToMonoNormal@32");
				_ResampleStereoToMonoNormal = (del_ResampleStereoToMonoNormal)Marshal.GetDelegateForFunctionPointer(f, typeof(del_ResampleStereoToMonoNormal));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _ResampleStereoToMonoNormal(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, rVolSel);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_ResampleStereoToMonoNormal _ResampleStereoToMonoNormal = null;



		/********************************************************************/
		/// <summary>
		/// Resample a stereo sample into a stereo output buffer
		/// </summary>
		/********************************************************************/
		private delegate int del_ResampleStereoToStereoNormal(IntPtr source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel);
		public static int ResampleStereoToStereoNormal(int[] source, IntPtr dest, int offset, int index, int increment, int todo, int lVolSel, int rVolSel)
		{
			if (_ResampleStereoToStereoNormal == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "ResampleStereoToStereoNormal") : GetProcAddress(dllPtr, "_ResampleStereoToStereoNormal@32");
				_ResampleStereoToStereoNormal = (del_ResampleStereoToStereoNormal)Marshal.GetDelegateForFunctionPointer(f, typeof(del_ResampleStereoToStereoNormal));
			}

			GCHandle pinnedBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

			try
			{
				return _ResampleStereoToStereoNormal(pinnedBuf.AddrOfPinnedObject(), dest, offset, index, increment, todo, lVolSel, rVolSel);
			}
			finally
			{
				pinnedBuf.Free();
			}
		}
		private static del_ResampleStereoToStereoNormal _ResampleStereoToStereoNormal = null;

		#region Buffer conversion calls
		/********************************************************************/
		/// <summary>
		/// Converts the resampled data to a 16 bit sample buffer
		/// </summary>
		/********************************************************************/
		private delegate void del_ResampleConvertTo16(IntPtr dest, int offset, IntPtr source, int count, bool swapSpeakers);
		public static void ResampleConvertTo16(byte[] dest, int offset, int[] source, int count, bool swapSpeakers)
		{
			if (_ResampleConvertTo16 == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "ResampleConvertTo16") : GetProcAddress(dllPtr, "_ResampleConvertTo16@20");
				_ResampleConvertTo16 = (del_ResampleConvertTo16)Marshal.GetDelegateForFunctionPointer(f, typeof(del_ResampleConvertTo16));
			}

			GCHandle destBuf = GCHandle.Alloc(dest, GCHandleType.Pinned);

			try
			{
				GCHandle sourceBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

				try
				{
					_ResampleConvertTo16(destBuf.AddrOfPinnedObject(), offset, sourceBuf.AddrOfPinnedObject(), count, swapSpeakers);
				}
				finally
				{
					sourceBuf.Free();
				}
			}
			finally
			{
				destBuf.Free();
			}
		}
		private static del_ResampleConvertTo16 _ResampleConvertTo16 = null;



		/********************************************************************/
		/// <summary>
		/// Converts the resampled data to a 32 bit sample buffer
		/// </summary>
		/********************************************************************/
		private delegate void del_ResampleConvertTo32(IntPtr dest, int offset, IntPtr source, int count, bool swapSpeakers);
		public static void ResampleConvertTo32(byte[] dest, int offset, int[] source, int count, bool swapSpeakers)
		{
			if (_ResampleConvertTo32 == null)
			{
				IntPtr f = Environment.Is64BitProcess ? GetProcAddress(dllPtr, "ResampleConvertTo32") : GetProcAddress(dllPtr, "_ResampleConvertTo32@20");
				_ResampleConvertTo32 = (del_ResampleConvertTo32)Marshal.GetDelegateForFunctionPointer(f, typeof(del_ResampleConvertTo32));
			}

			GCHandle destBuf = GCHandle.Alloc(dest, GCHandleType.Pinned);

			try
			{
				GCHandle sourceBuf = GCHandle.Alloc(source, GCHandleType.Pinned);

				try
				{
					_ResampleConvertTo32(destBuf.AddrOfPinnedObject(), offset, sourceBuf.AddrOfPinnedObject(), count, swapSpeakers);
				}
				finally
				{
					sourceBuf.Free();
				}
			}
			finally
			{
				destBuf.Free();
			}
		}
		private static del_ResampleConvertTo32 _ResampleConvertTo32 = null;
		#endregion

		#endregion
	}
}
