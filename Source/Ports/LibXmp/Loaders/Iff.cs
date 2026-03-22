/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Iff;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// Handles loading of IFF formatted files
	/// </summary>
	internal class Iff
	{
		/// <summary></summary>
		public delegate c_int Iff_Loader(Module_Data m, uint32 size, Hio f, object parm);

		private class Iff_Info
		{
			public readonly byte[] Id = new byte[4];
			public Iff_Loader Loader;
			public Iff_Info Next;
		}

		private class Iff_Data
		{
			public Iff_Info Head;
			public Iff_Info Tail;
			public uint Id_Size;
			public Iff_Quirk_Flag Flags;
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
			data.Head = data.Tail = null;
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

			// Reached end of file, or there was an IFF structural issue.
			// Either way, clear the error flag to allow loading to continue
			f.Hio_Error();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Iff_Register(CPointer<uint8> id, Iff_Loader loader)
		{
			Iff_Info f = CMemory.mallocObj<Iff_Info>();
			if (f == null)
				return -1;

			c_int i = 0;
			for (; (i < 4) && id.IsNotNull && (id[i] != 0); i++)
				f.Id[i] = id[i];

			for (; i < 4; i++)
				f.Id[i] = 0;

			f.Loader = loader;
			f.Next = null;

			Iff_Append(data, f);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Iff_Release()
		{
			for (Iff_Info i = data.Head; i != null;)
			{
				Iff_Info tmp = i.Next;

				CMemory.free(tmp);

				i = tmp;
			}

			CMemory.free(data);
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
		private void Iff_Append(Iff_Data data, Iff_Info i)
		{
			if ((data.Head != null) && (data.Tail != null))
			{
				data.Tail.Next = i;
				data.Tail = i;
			}
			else
			{
				data.Head = i;
				data.Tail = i;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Iff_Process(Module_Data m, CPointer<uint8> id, uint32 size, Hio f, object parm)
		{
			c_long pos = (c_int)f.Hio_Tell();

			for (Iff_Info i = data.Head; i != null; i = i.Next)
			{
				if (id.IsNotNull && (CMemory.memcmp(id, i.Id, data.Id_Size) == 0))
				{
					if (size > Iff_Max_Chunk_Size)
						return -1;

					if (i.Loader(m, size, f, parm) < 0)
						return -1;

					break;
				}
			}

			if (f.Hio_Seek(pos + size, SeekOrigin.Begin) < 0)
			{
				// IFF container issue--exit without error
				return 1;
			}

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
			uint32 size;

			if (f.Hio_Read(id, 1, data.Id_Size) != data.Id_Size)
			{
				// End of file or IFF container issue--exit without error
				return 1;
			}

			if ((data.Flags & Iff_Quirk_Flag.Skip_Embedded) != 0)
			{
				// Embedded RIFF hack
				if (CMemory.strncmp(id, "RIFF", 4) == 0)
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
			{
				// IFF container issue--exit without error
				return 1;
			}

			if ((data.Flags & Iff_Quirk_Flag.Chunk_Align2) != 0)
			{
				// Sanity check
				if (size > 0xfffffffe)
				{
					// IFF container issue--exit without error
					return 1;
				}

				size = (uint)((size + 1) & ~1);
			}

			if ((data.Flags & Iff_Quirk_Flag.Chunk_Align4) != 0)
			{
				// Sanity check
				if (size > 0xfffffffc)
				{
					// IFF container issue--exit without error
					return 1;
				}

				size = (uint)((size + 3) & ~3);
			}

			// PT 3.6 hack: this does not seem to ever apply to "PTDT".
			// This broke several modules (city lights.pt36, acid phase.pt36)
			if (((data.Flags & Iff_Quirk_Flag.Full_Chunk_Size) != 0) && (CMemory.memcmp(id, "PTDT", 4) != 0))
			{
				if (size < (data.Id_Size + 4))
					return -1;

				size -= data.Id_Size + 4;
			}

			return Iff_Process(m, id, size, f, parm);
		}
		#endregion
	}
}
