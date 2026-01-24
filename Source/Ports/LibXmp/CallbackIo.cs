/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class CallbackIo
	{
		private class CbFile
		{
			public object Priv;
			public Xmp_Callbacks Callbacks;
			public bool Eof;
		}

		private CbFile f;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private CallbackIo(CbFile cbFile)
		{
			f = cbFile;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint8 CbRead8(out c_int err)
		{
			uint8[] x = [ 0xff ];

			size_t r = f.Callbacks.Read_Func(x, 1, 1, f.Priv);
			err = (r == 1) ? 0 : Constants.EOF;
			f.Eof = err != 0;

			return x[0];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int8 CbRead8S(out c_int err)
		{
			return (int8)CbRead8(out err);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint16 CbRead16L(out c_int err)
		{
			uint8[] buf = new uint8[2];
			uint16 x = 0xffff;

			size_t r = f.Callbacks.Read_Func(buf, 2, 1, f.Priv);
			err = (r == 1) ? 0 : Constants.EOF;
			f.Eof = err != 0;

			if (r != 0)
				x = DataIo.ReadMem16L(buf);

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint16 CbRead16B(out c_int err)
		{
			uint8[] buf = new uint8[2];
			uint16 x = 0xffff;

			size_t r = f.Callbacks.Read_Func(buf, 2, 1, f.Priv);
			err = (r == 1) ? 0 : Constants.EOF;
			f.Eof = err != 0;

			if (r != 0)
				x = DataIo.ReadMem16B(buf);

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32 CbRead24L(out c_int err)
		{
			uint8[] buf = new uint8[3];
			uint32 x = 0xffffffff;

			size_t r = f.Callbacks.Read_Func(buf, 3, 1, f.Priv);
			err = (r == 1) ? 0 : Constants.EOF;
			f.Eof = err != 0;

			if (r != 0)
				x = DataIo.ReadMem24L(buf);

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32 CbRead24B(out c_int err)
		{
			uint8[] buf = new uint8[3];
			uint32 x = 0xffffffff;

			size_t r = f.Callbacks.Read_Func(buf, 3, 1, f.Priv);
			err = (r == 1) ? 0 : Constants.EOF;
			f.Eof = err != 0;

			if (r != 0)
				x = DataIo.ReadMem24B(buf);

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32 CbRead32L(out c_int err)
		{
			uint8[] buf = new uint8[4];
			uint32 x = 0xffffffff;

			size_t r = f.Callbacks.Read_Func(buf, 4, 1, f.Priv);
			err = (r == 1) ? 0 : Constants.EOF;
			f.Eof = err != 0;

			if (r != 0)
				x = DataIo.ReadMem32L(buf);

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32 CbRead32B(out c_int err)
		{
			uint8[] buf = new uint8[4];
			uint32 x = 0xffffffff;

			size_t r = f.Callbacks.Read_Func(buf, 4, 1, f.Priv);
			err = (r == 1) ? 0 : Constants.EOF;
			f.Eof = err != 0;

			if (r != 0)
				x = DataIo.ReadMem32B(buf);

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t CbRead(CPointer<uint8> dest, size_t len, size_t nMemB)
		{
			size_t r = f.Callbacks.Read_Func(dest, (c_ulong)len, (c_ulong)nMemB, f.Priv);
			f.Eof = r < nMemB;

			return r;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public c_int CbSeek(c_long offset, SeekOrigin whence)
		{
			f.Eof = false;

			return f.Callbacks.Seek_Func(f.Priv, offset, whence);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public c_long CbTell()
		{
			return f.Callbacks.Tell_Func(f.Priv);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CbEof()
		{
			return f.Eof;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public c_long CbFileLength()
		{
			c_long pos = f.Callbacks.Tell_Func(f.Priv);

			if (pos < 0)
				return Constants.EOF;

			c_int r = f.Callbacks.Seek_Func(f.Priv, 0, SeekOrigin.End);
			if (r < 0)
				return Constants.EOF;

			c_long length = f.Callbacks.Tell_Func(f.Priv);
			r = f.Callbacks.Seek_Func(f.Priv, pos, SeekOrigin.Begin);

			return length;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CallbackIo CbOpen(object priv, Xmp_Callbacks callbacks)
		{
			if ((priv == null) || (callbacks == null) || (callbacks.Read_Func == null) || (callbacks.Seek_Func == null) || (callbacks.Tell_Func == null))
				goto Err;

			CbFile f = new CbFile();
			if (f == null)
				goto Err;

			f.Priv = priv;
			f.Callbacks = callbacks;
			f.Eof = false;

			return new CallbackIo(f);

			Err:
			if ((priv != null) && (callbacks?.Close_Func != null))
				callbacks.Close_Func(priv);

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CallbackIo CbOpen(CallbackIo from)
		{
			CbFile f = new CbFile();
			if (f == null)
				goto Err;

			f.Priv = from.f.Priv;
			f.Callbacks = new Xmp_Callbacks
			{
				Read_Func = from.f.Callbacks.Read_Func,
				Seek_Func = from.f.Callbacks.Seek_Func,
				Tell_Func = from.f.Callbacks.Tell_Func,
				Close_Func = null
			};
			f.Eof = from.f.Eof;

			return new CallbackIo(f);

			Err:
			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public c_int CbClose()
		{
			c_int r = 0;

			if (f.Callbacks.Close_Func != null)
				r = f.Callbacks.Close_Func(f.Priv);

			f = null;

			return r;
		}
	}
}
