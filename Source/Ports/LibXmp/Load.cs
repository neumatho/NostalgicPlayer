/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Security.Cryptography;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.Loaders;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Load
	{
		private const c_int BufLen = 16384;

		private readonly LibXmp lib;
		private readonly Xmp_Context ctx;

		private Guid? formatToUse;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Load(LibXmp libXmp, Xmp_Context ctx)
		{
			lib = libXmp;
			this.ctx = ctx;
		}



		/********************************************************************/
		/// <summary>
		/// Test if a memory buffer is a valid module. Testing memory does
		/// not affect the current player context or any currently loaded
		/// module
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Test_Module_From_Memory(uint8[] mem, c_long size, out Xmp_Test_Info info)
		{
			info = null;

			if (size <= 0)
				return -(c_int)Xmp_Error.Invalid;

			Hio h = Hio.Hio_Open_Mem(mem, size, false);
			if (h == null)
				return -(c_int)Xmp_Error.System;

			c_int ret = Test_Module(out info, h);

			h.Hio_Close();

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Test if a module from a stream is a valid module. Testing streams
		/// does not affect the current player context or any currently
		/// loaded module
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Test_Module_From_File(Stream file, out Xmp_Test_Info info)
		{
			info = null;

			Hio h = Hio.Hio_Open_File(file);
			if (h == null)
				return -(c_int)Xmp_Error.System;

			c_int ret = Test_Module(out info, h);

			h.Hio_Close();

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Test if a module from a custom stream is a valid module. Testing
		/// custom streams does not affect the current player context or any
		/// currently loaded module
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Test_Module_From_Callbacks(object priv, Xmp_Callbacks callbacks, out Xmp_Test_Info info)
		{
			info = null;

			Hio h = Hio.Hio_Open_Callbacks(priv, callbacks);
			if (h == null)
				return -(c_int)Xmp_Error.System;

			c_int ret = Test_Module(out info, h);

			h.Hio_Close();

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Will set a specific format, so the loader only will check against
		/// this
		/// </summary>
		/********************************************************************/
		public void Xmp_Set_Load_Format(Guid formatId)
		{
			formatToUse = formatId;
		}



		/********************************************************************/
		/// <summary>
		/// Load a module from memory into the specified player context
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Load_Module_From_Memory(uint8[] mem, c_long size)
		{
			Module_Data m = ctx.M;

			if (size <= 0)
				return -(c_int)Xmp_Error.Invalid;

			Hio h = Hio.Hio_Open_Mem(mem, size, false);
			if (h == null)
				return -(c_int)Xmp_Error.System;

			if (ctx.State > Xmp_State.Unloaded)
				lib.Xmp_Release_Module();

			m.FileName = null;
			m.BaseName = null;
			m.DirName = null;
			m.Size = h.Hio_Size();

			c_int ret = Load_Module(h);

			h.Hio_Close();

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Load a module from a stream into the specified player context
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Load_Module_From_File(Stream file)
		{
			Module_Data m = ctx.M;

			Hio h = Hio.Hio_Open_File(file);
			if (h == null)
				return -(c_int)Xmp_Error.System;

			if (ctx.State > Xmp_State.Unloaded)
				lib.Xmp_Release_Module();

			m.FileName = null;
			m.BaseName = null;
			m.DirName = null;
			m.Size = h.Hio_Size();

			c_int ret = Load_Module(h);

			h.Hio_Close();

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Load a module from a custom stream into the specified player
		/// context
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Load_Module_From_Callbacks(object priv, Xmp_Callbacks callbacks)
		{
			Module_Data m = ctx.M;

			Hio h = Hio.Hio_Open_Callbacks(priv, callbacks);
			if (h == null)
				return -(c_int)Xmp_Error.System;

			if (ctx.State > Xmp_State.Unloaded)
				lib.Xmp_Release_Module();

			m.FileName = null;
			m.BaseName = null;
			m.DirName = null;
			m.Size = h.Hio_Size();

			c_int ret = Load_Module(h);

			h.Hio_Close();

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// Release memory allocated by a module from the specified player
		/// context
		/// </summary>
		/********************************************************************/
		public void Xmp_Release_Module()
		{
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			if (ctx.State > Xmp_State.Loaded)
				lib.Xmp_End_Player();

			ctx.State = Xmp_State.Unloaded;

			lib.extras.LibXmp_Release_Module_Extras();

			if (mod.Xxt != null)
				mod.Xxt = null;

			if (mod.Xxp != null)
				mod.Xxp = null;

			if (mod.Xxi != null)
				mod.Xxi = null;

			if (mod.Xxs != null)
			{
				for (c_int i = 0; i < mod.Smp; i++)
					Sample.LibXmp_Free_Sample(mod.Xxs[i]);

				mod.Xxs = null;
			}

			m.Xtra = null;
			m.Midi = null;

			lib.loadHelpers.LibXmp_Free_Scan();

			m.Comment = null;

			m.BaseName = null;
			m.DirName = null;
		}



		/********************************************************************/
		/// <summary>
		/// Scan the loaded module for sequences and timing. Scanning is
		/// automatically performed by xmp_load_module() and this function
		/// should be called only if xmp_set_player() is used to change
		/// player timing (with parameter XMP_PLAYER_VBLANK) in libxmp 4.0.2
		/// or older
		/// </summary>
		/********************************************************************/
		public void Xmp_Scan_Module()
		{
			if (ctx.State < Xmp_State.Loaded)
				return;

			lib.scan.LibXmp_Scan_Sequences();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Set_Md5Sum(Hio f, byte[] digest)
		{
			byte[] buf = new byte[BufLen];

			f.Hio_Seek(0, SeekOrigin.Begin);

			using (MD5 md5 = MD5.Create())
			{
				c_int bytes_Read;

				while ((bytes_Read = (c_int)f.Hio_Read(buf, 1, BufLen)) > 0)
					md5.TransformBlock(buf, 0, bytes_Read, null, 0);

				md5.TransformFinalBlock(buf, 0, 0);
				Array.Copy(md5.Hash, digest, 16);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Test_Module(out Xmp_Test_Info info, Hio h)
		{
			for (c_int i = 0; Format.format_Loaders[i] != null; i++)
			{
				h.Hio_Seek(0, SeekOrigin.Begin);

				IFormatLoader loader = Format.format_Loaders[i].Create(lib);

				if (loader.Test(h, out string buf, 0) == 0)
				{
					info = new Xmp_Test_Info
					{
						Name = buf,
						Type = Format.format_Loaders[i].Name,
						Id = Format.format_Loaders[i].Id
					};

					return 0;
				}
			}

			info = null;
			return -(c_int)Xmp_Error.Format;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Load_Module(Hio h)
		{
			Module_Data m = ctx.M;

			lib.loadHelpers.LibXmp_Load_Prologue();

			c_int test_Result = -1, load_Result = -1;

			for (c_int i = 0; Format.format_Loaders[i] != null; i++)
			{
				if (!formatToUse.HasValue || (Format.format_Loaders[i].Id == formatToUse.Value))
				{
					h.Hio_Seek(0, SeekOrigin.Begin);

					IFormatLoader loader = Format.format_Loaders[i].Create(lib);

					test_Result = loader.Test(h, out _, 0);
					if (test_Result == 0)
					{
						h.Hio_Seek(0, SeekOrigin.Begin);

						load_Result = loader.Loader(m, h, 0);
						break;
					}
				}
			}

			if (test_Result < 0)
			{
				lib.Xmp_Release_Module();
				return -(c_int)Xmp_Error.Format;
			}

			if (load_Result < 0)
				goto Err_Load;

			Xmp_Module mod = m.Mod;

			// Sanity check: number of channels, module length
			if ((mod.Chn > Constants.Xmp_Max_Channels) || (mod.Len > Constants.Xmp_Max_Mod_Length))
				goto Err_Load;

			// Sanity check: channel pan
			for (c_int i = 0; i < mod.Chn; i++)
			{
				if ((mod.Xxc[i].Vol < 0) || (mod.Xxc[i].Vol > 0xff))
					goto Err_Load;

				if ((mod.Xxc[i].Pan < 0) || (mod.Xxc[i].Pan > 0xff))
					goto Err_Load;
			}

			// Sanity check: patterns
			if (mod.Xxp == null)
				goto Err_Load;

			for (c_int i = 0; i < mod.Pat; i++)
			{
				if (mod.Xxp[i] == null)
					goto Err_Load;

				for (c_int j = 0; j < mod.Chn; j++)
				{
					c_int t = mod.Xxp[i].Index[j];
					if ((t < 0) || (t >= mod.Trk) || (mod.Xxt[t] == null))
						goto Err_Load;
				}
			}

			Load_Helpers.LibXmp_Adjust_String(ref mod.Name);

			for (c_int i = 0; i < mod.Ins; i++)
				Load_Helpers.LibXmp_Adjust_String(ref mod.Xxi[i].Name);

			for (c_int i = 0; i < mod.Smp; i++)
				Load_Helpers.LibXmp_Adjust_String(ref mod.Xxs[i].Name);

			if ((test_Result == 0) && (load_Result == 0))
				Set_Md5Sum(h, m.Md5);

			lib.loadHelpers.LibXmp_Load_Epilogue();

			c_int ret = lib.loadHelpers.LibXmp_Prepare_Scan();
			if (ret < 0)
			{
				lib.Xmp_Release_Module();
				return ret;
			}

			ret = lib.scan.LibXmp_Scan_Sequences();
			if (ret < 0)
			{
				lib.Xmp_Release_Module();
				return -(c_int)Xmp_Error.Load;
			}

			ctx.State = Xmp_State.Loaded;

			return 0;

			Err_Load:
			lib.Xmp_Release_Module();
			return -(c_int)Xmp_Error.Load;
		}
		#endregion
	}
}
