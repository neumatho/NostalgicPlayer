/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Path
	{
		private LibXmp_Path p;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Path()
		{
			p = new LibXmp_Path();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Path LibXmp_Path_Init()
		{
			return new Path();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<char> CurrentPath => p.Path;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Path_Free()
		{
			CMemory.free(p.Path);
			p.Path.SetToNull();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Path_Move(Path src)
		{
			LibXmp_Path_Free();

			p = src.p.MakeDeepClone();
			src.p.Path.SetToNull();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Path_Set(CPointer<char> new_Path)
		{
			if (new_Path.IsNull)
				return -1;

			size_t sz = CString.strlen(new_Path) + 1;

			if (Fix_Size(sz) < 0)
				return -1;

			CMemory.memcpy(p.Path, new_Path, sz);
			p.Length = sz - 1;

			Clean_Slashes(0);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Path_Truncate(size_t new_Sz)
		{
			if (new_Sz > p.Length)
				return 0;

			if (Fix_Size(new_Sz + 1) < 0)
				return -1;

			p.Path[new_Sz] = '\0';
			p.Length = new_Sz;

			if (new_Sz != 0)
				Clean_Slashes(new_Sz - 1);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Path_Suffix_At(size_t ext_Pos, CPointer<char> ext)
		{
			if (ext.IsNull || (ext_Pos > p.Length))
				return -1;

			size_t sz = CString.strlen(ext) + 1;

			if (Fix_Size(ext_Pos + sz) < 0)
				return -1;

			CMemory.memcpy(p.Path + ext_Pos, ext, sz);
			p.Length = ext_Pos + sz - 1;

			Clean_Slashes(ext_Pos != 0 ? ext_Pos - 1 : 0);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Path_Append(CPointer<char> append_Path)
		{
			if (append_Path.IsNull)
				return -1;

			size_t append_Sz = CString.strlen(append_Path) + 1;
			size_t new_Sz = p.Length + 1 + append_Sz;
			size_t old_Sz = p.Length;

			if ((new_Sz < p.Length) || (Fix_Size(new_Sz) < 0))
				return -1;

			p.Path[p.Length++] = '/';
			CMemory.memcpy(p.Path + p.Length, append_Path, append_Sz);
			p.Length = new_Sz - 1;

			Clean_Slashes(old_Sz != 0 ? old_Sz - 1 : 0);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Path_Join(CPointer<char> prefix_Path, CPointer<char> suffix_Path)
		{
			if (prefix_Path.IsNull || suffix_Path.IsNull)
				return -1;

			c_int ret = CString.snprintf(null, 0, "%s/%s", prefix_Path, suffix_Path);
			if ((ret < 0) || (Fix_Size((size_t)ret + 1) < 0))
				return -1;

			p.Length = (size_t)CString.snprintf(p.Path, p.Alloc, "%s/%s", prefix_Path, suffix_Path);

			Clean_Slashes(0);

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Fix_Size(size_t sz)
		{
			if (p.Path.IsNull || (p.Alloc < sz))
			{
				CPointer<char> tmp = CMemory.realloc(p.Path, sz);
				if (tmp.IsNull)
					return -1;

				p.Path = tmp;
				p.Alloc = sz;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Is_Slash(char c)
		{
			return (c == '\\') || (c == '/');
		}



		/********************************************************************/
		/// <summary>
		/// Some console SDKs (3DS, possibly others) handle duplicate and
		/// trailing slashes very poorly. This function should be used to
		/// guarantee any path created by these functions will not have these
		/// issues
		/// </summary>
		/********************************************************************/
		private void Clean_Slashes(size_t pos)
		{
			size_t i, j;

			if (pos >= p.Length)
				return;

			// Normalize to /, detect duplicates
			for (i = pos; i < p.Length; i++)
			{
				if (Is_Slash(p.Path[i]))
				{
					p.Path[i] = '/';

					if (((i + 1) < p.Length) && Is_Slash(p.Path[i + 1]))
						break;
				}
			}

			// Clean duplicates if they exists
			for (j = i; i < p.Length;)
			{
				if (Is_Slash(p.Path[i]))
				{
					p.Path[j++] = '/';
					i++;

					while ((i < p.Length) && Is_Slash(p.Path[i]))
						i++;
				}
				else
					p.Path[j++] = p.Path[i++];
			}

			// Trim trailing slash, except at index 0
			if ((j > 1) && Is_Slash(p.Path[j - 1]))
				j--;

			p.Path[j] = '\0';
			p.Length = j;
		}
		#endregion
	}
}
