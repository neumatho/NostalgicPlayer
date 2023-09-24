/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Hio
	{
		private enum Hio_Type
		{
			File,
			Memory,
			CbFile
		}

		private class Hio_Handle
		{
			public Hio_Type Type;
			public c_long Size;
			public (
				Stream File,
				MemIo Mem,
				CallbackIo CbFile
			) Handle;
			public c_int Error;
			public bool NoClose;
		}

		private Hio_Handle h;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Hio(Hio_Handle handle)
		{
			h = handle;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int8 Hio_Read8S()
		{
			c_int err;
			int8 ret;

			switch (h.Type)
			{
				case Hio_Type.File:
				{
					ret = DataIo.Read8S(h.Handle.File, out err);
					break;
				}

				case Hio_Type.Memory:
				{
					ret = h.Handle.Mem.MRead8S(out err);
					break;
				}

				case Hio_Type.CbFile:
				{
					ret = h.Handle.CbFile.CbRead8S(out err);
					break;
				}

				default:
					return 0;
			}

			if (err != 0)
				h.Error = err;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8 Hio_Read8()
		{
			c_int err;
			uint8 ret;

			switch (h.Type)
			{
				case Hio_Type.File:
				{
					ret = DataIo.Read8(h.Handle.File, out err);
					break;
				}

				case Hio_Type.Memory:
				{
					ret = h.Handle.Mem.MRead8(out err);
					break;
				}

				case Hio_Type.CbFile:
				{
					ret = h.Handle.CbFile.CbRead8(out err);
					break;
				}

				default:
					return 0;
			}

			if (err != 0)
				h.Error = err;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint16 Hio_Read16L()
		{
			c_int err;
			uint16 ret;

			switch (h.Type)
			{
				case Hio_Type.File:
				{
					ret = DataIo.Read16L(h.Handle.File, out err);
					break;
				}

				case Hio_Type.Memory:
				{
					ret = h.Handle.Mem.MRead16L(out err);
					break;
				}

				case Hio_Type.CbFile:
				{
					ret = h.Handle.CbFile.CbRead16L(out err);
					break;
				}

				default:
					return 0;
			}

			if (err != 0)
				h.Error = err;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint16 Hio_Read16B()
		{
			c_int err;
			uint16 ret;

			switch (h.Type)
			{
				case Hio_Type.File:
				{
					ret = DataIo.Read16B(h.Handle.File, out err);
					break;
				}

				case Hio_Type.Memory:
				{
					ret = h.Handle.Mem.MRead16B(out err);
					break;
				}

				case Hio_Type.CbFile:
				{
					ret = h.Handle.CbFile.CbRead16B(out err);
					break;
				}

				default:
					return 0;
			}

			if (err != 0)
				h.Error = err;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint32 Hio_Read24L()
		{
			c_int err;
			uint32 ret;

			switch (h.Type)
			{
				case Hio_Type.File:
				{
					ret = DataIo.Read24L(h.Handle.File, out err);
					break;
				}

				case Hio_Type.Memory:
				{
					ret = h.Handle.Mem.MRead24L(out err);
					break;
				}

				case Hio_Type.CbFile:
				{
					ret = h.Handle.CbFile.CbRead24L(out err);
					break;
				}

				default:
					return 0;
			}

			if (err != 0)
				h.Error = err;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint32 Hio_Read24B()
		{
			c_int err;
			uint32 ret;

			switch (h.Type)
			{
				case Hio_Type.File:
				{
					ret = DataIo.Read24L(h.Handle.File, out err);
					break;
				}

				case Hio_Type.Memory:
				{
					ret = h.Handle.Mem.MRead24B(out err);
					break;
				}

				case Hio_Type.CbFile:
				{
					ret = h.Handle.CbFile.CbRead24B(out err);
					break;
				}

				default:
					return 0;
			}

			if (err != 0)
				h.Error = err;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint32 Hio_Read32L()
		{
			c_int err;
			uint32 ret;

			switch (h.Type)
			{
				case Hio_Type.File:
				{
					ret = DataIo.Read32L(h.Handle.File, out err);
					break;
				}

				case Hio_Type.Memory:
				{
					ret = h.Handle.Mem.MRead32L(out err);
					break;
				}

				case Hio_Type.CbFile:
				{
					ret = h.Handle.CbFile.CbRead32L(out err);
					break;
				}

				default:
					return 0;
			}

			if (err != 0)
				h.Error = err;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint32 Hio_Read32B()
		{
			c_int err;
			uint32 ret;

			switch (h.Type)
			{
				case Hio_Type.File:
				{
					ret = DataIo.Read32B(h.Handle.File, out err);
					break;
				}

				case Hio_Type.Memory:
				{
					ret = h.Handle.Mem.MRead32B(out err);
					break;
				}

				case Hio_Type.CbFile:
				{
					ret = h.Handle.CbFile.CbRead32B(out err);
					break;
				}

				default:
					return 0;
			}

			if (err != 0)
				h.Error = err;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public size_t Hio_Read(Span<int8> buf, size_t size, size_t num)
		{
			return Hio_Read(MemoryMarshal.Cast<int8, uint8>(buf), size, num);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public size_t Hio_Read(Span<uint8> buf, size_t size, size_t num)
		{
			size_t ret = 0;

			switch (h.Type)
			{
				case Hio_Type.File:
				{
					try
					{
						ret = (size_t)h.Handle.File.Read(buf.Slice(0, (int)(num * size))) / size;
						if (ret != num)
							h.Error = Constants.EOF;
					}
					catch(IOException ex)
					{
						h.Error = ex.HResult;
					}
					break;
				}

				case Hio_Type.Memory:
				{
					ret = h.Handle.Mem.MRead(buf, size, num);
					if (ret != num)
						h.Error = Constants.EOF;

					break;
				}

				case Hio_Type.CbFile:
				{
					ret = h.Handle.CbFile.CbRead(buf, size, num);
					if (ret != num)
						h.Error = Constants.EOF;

					break;
				}
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Hio_Seek(c_long offset, SeekOrigin whence)
		{
			c_int ret = -1;

			switch (h.Type)
			{
				case Hio_Type.File:
				{
					try
					{
						h.Handle.File.Seek(offset, whence);
						ret = 0;

						if (h.Error == Constants.EOF)
							h.Error = 0;
					}
					catch(IOException ex)
					{
						h.Error = ex.HResult;
					}
					break;
				}

				case Hio_Type.Memory:
				{
					ret = h.Handle.Mem.MSeek(offset, whence);
					if (ret < 0)
						h.Error = ErrNo.EINVAL;
					else if (h.Error == Constants.EOF)
						h.Error = 0;

					break;
				}

				case Hio_Type.CbFile:
				{
					ret = h.Handle.CbFile.CbSeek(offset, whence);
					if (ret < 0)
						h.Error = ErrNo.EINVAL;
					else if (h.Error == Constants.EOF)
						h.Error = 0;

					break;
				}
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long Hio_Tell()
		{
			c_long ret = -1;

			switch (h.Type)
			{
				case Hio_Type.File:
				{
					try
					{
						ret = (c_long)h.Handle.File.Position;
					}
					catch(IOException ex )
					{
						h.Error = ex.HResult;
					}
					break;
				}

				case Hio_Type.Memory:
				{
					ret = h.Handle.Mem.MTell();
					if (ret < 0)
					{
						// Should _not_ happend!
						h.Error = ErrNo.EINVAL;
					}
					break;
				}

				case Hio_Type.CbFile:
				{
					ret = h.Handle.CbFile.CbTell();
					if (ret < 0)
						h.Error = ErrNo.EINVAL;

					break;
				}
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool Hio_Eof()
		{
			switch (h.Type)
			{
				case Hio_Type.File:
					return h.Handle.File.Position >= h.Handle.File.Length;

				case Hio_Type.Memory:
					return h.Handle.Mem.MEof();

				case Hio_Type.CbFile:
					return h.Handle.CbFile.CbEof();
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Hio_Error()
		{
			c_int error = h.Error;
			h.Error = 0;

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Hio Hio_Open_Mem(uint8[] ptr, c_long size, bool free_After_User)
		{
			if (size <= 0)
				return null;

			Hio_Handle h = new Hio_Handle();
			if (h == null)
				return null;

			h.Type = Hio_Type.Memory;
			h.Handle.Mem = MemIo.MOpen(ptr, size, free_After_User);
			h.Size = size;

			if (h.Handle.Mem == null)
				return null;

			return new Hio(h);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Hio Hio_Open_File(Stream f)
		{
			Hio_Handle h = new Hio_Handle();
			if (h == null)
				return null;

			h.NoClose = true;
			h.Type = Hio_Type.File;
			h.Handle.File = f;
			h.Size = Get_Size(f);

			if (h.Size < 0)
				return null;

			return new Hio(h);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Hio Hio_Open_Callbacks(object priv, Xmp_Callbacks callbacks)
		{
			CallbackIo f = CallbackIo.CbOpen(priv, callbacks);
			if (f == null)
				return null;

			Hio_Handle h = new Hio_Handle();
			if (h == null)
			{
				f.CbClose();
				return null;
			}

			h.Type = Hio_Type.CbFile;
			h.Handle.CbFile = f;
			h.Size = f.CbFileLength();

			if (h.Size < 0)
			{
				f.CbClose();
				return null;
			}

			return new Hio(h);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Hio_Close()
		{
			c_int ret = Hio_Close_Internal();

			h = null;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long Hio_Size()
		{
			return h.Size;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Hio GetSampleHio(int sampleNumber, int length)
		{
			if (h.Type == Hio_Type.File)
			{
				if (h.Handle.File is ModuleStream moduleStream)
					return Hio_Open_File(moduleStream.GetSampleDataStream(sampleNumber, length));

				return Hio_Open_File(h.Handle.File);
			}

			Hio_Handle newHandle = new Hio_Handle
			{
				Type = h.Type,
				Size = h.Size,
				Error = h.Error,
				NoClose = true
			};

			if (h.Type == Hio_Type.Memory)
				newHandle.Handle.Mem = MemIo.MOpen(h.Handle.Mem);
			else if (h.Type == Hio_Type.CbFile)
				newHandle.Handle.CbFile = CallbackIo.CbOpen(h.Handle.CbFile);
			else
				return null;

			return new Hio(newHandle);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_long Get_Size(Stream f)
		{
			if (f == null)
				return -1;

			if (!f.CanSeek)
				return -1;

			return (c_long)f.Length;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Hio_Close_Internal()
		{
			c_int ret = -1;

			switch (h.Type)
			{
				case Hio_Type.File:
				{
					if (!h.NoClose)
						h.Handle.File.Dispose();

					ret = 0;
					break;
				}

				case Hio_Type.Memory:
				{
					ret = h.Handle.Mem.MClose();
					break;
				}

				case Hio_Type.CbFile:
				{
					ret = h.Handle.CbFile.CbClose();
					break;
				}
			}

			return ret;
		}
		#endregion
	}
}
