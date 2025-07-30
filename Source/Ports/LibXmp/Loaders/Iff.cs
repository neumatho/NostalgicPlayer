/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Iff;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// Handles loading of IFF formatted files
	/// </summary>
	internal class Iff
	{
		private class Iff_Data
		{
			public List<Iff_Info> Iff_List { get; } = new List<Iff_Info>();
			public uint Id_Size { get; set; }
			public Iff_Quirk_Flag Flags { get; set; }
		}

		private const c_long Iff_Max_Chunk_Size = 0x800000;

		private Iff_Data data;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Iff()
		{
			data = new Iff_Data();
			data.Id_Size = 4;
			data.Flags = Iff_Quirk_Flag.None;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Iff LibXmp_Iff_New()
		{
			return new Iff();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Iff_Load(Module_Data m, Hio f, object parm)
		{
			while (!f.Hio_Eof())
			{
				c_int ret = Iff_Chunk(m, f, parm);
				if (ret > 0)
					break;

				if (ret < 0)
					return -1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Iff_Register(CPointer<uint8> id, Iff_Info.Loader_Delegate loader)
		{
			Iff_Info f = new Iff_Info();
			if (f == null)
				return -1;

			c_int i = 0;
			for (; (i < 4) && id.IsNotNull && (id[i] != 0); i++)
				f.Id[i] = id[i];

			for (; i < 4; i++)
				f.Id[i] = 0;

			f.Loader = loader;

			data.Iff_List.Add(f);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Iff_Release()
		{
			data.Iff_List.Clear();
			data = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Iff_Id_Size(c_int n)
		{
			data.Id_Size = (uint)n;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Iff_Set_Quirk(Iff_Quirk_Flag i)
		{
			data.Flags |= i;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Iff_Process(Module_Data m, CPointer<uint8> id, c_long size, Hio f, object parm)
		{
			c_int pos = f.Hio_Tell();

			foreach (Iff_Info i in data.Iff_List)
			{
				if (id.IsNotNull && (CMemory.MemCmp(id, i.Id, (int)data.Id_Size) == 0))
				{
					if (size > Iff_Max_Chunk_Size)
						return -1;

					if (i.Loader(m, size, f, parm) < 0)
						return -1;

					break;
				}
			}

			if (f.Hio_Seek(pos + size, SeekOrigin.Begin) < 0)
				return -1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Iff_Chunk(Module_Data m, Hio f, object parm)
		{
			CPointer<uint8> id = new CPointer<uint8>(17);
			uint size;

			if (f.Hio_Read(id, 1, data.Id_Size) != data.Id_Size)
			{
				f.Hio_Error();		// Clear error flag
				return 1;
			}

			if ((data.Flags & Iff_Quirk_Flag.Skip_Embedded) != 0)
			{
				// Embedded RIFF hack
				if (CMemory.StrNCmp(id, "RIFF", 4) == 0)
				{
					f.Hio_Read32B();
					f.Hio_Read32B();

					// Read first chunk ID instead
					if (f.Hio_Read(id, 1, data.Id_Size) != data.Id_Size)
						return 1;
				}
			}

			if ((data.Flags & Iff_Quirk_Flag.Little_Endian) != 0)
				size = f.Hio_Read32L();
			else
				size = f.Hio_Read32B();

			if (f.Hio_Error() != 0)
				return -1;

			if ((data.Flags & Iff_Quirk_Flag.Chunk_Align2) != 0)
			{
				// Sanity check
				if (size > 0xfffffffe)
					return -1;

				size = (uint)((size + 1) & ~1);
			}

			if ((data.Flags & Iff_Quirk_Flag.Chunk_Align4) != 0)
			{
				// Sanity check
				if (size > 0xfffffffc)
					return -1;

				size = (uint)((size + 3) & ~3);
			}

			// PT 3.6 hack: this does not seem to ever apply to "PTDT".
			// This broke several modules (city lights.pt36, acid phase.pt36)
			if (((data.Flags & Iff_Quirk_Flag.Full_Chunk_Size) != 0) && (CMemory.MemCmp(id, "PTDT", 4) != 0))
			{
				if (size < (data.Id_Size + 4))
					return -1;

				size -= data.Id_Size + 4;
			}

			return Iff_Process(m, id, (c_long)size, f, parm);
		}
		#endregion
	}
}
