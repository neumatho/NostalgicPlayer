/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Format;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.Loaders;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Format
	{
		private const int Num_Formats = 27;
		private const int Num_Pw_Formats = 0;

		private static Xmp_Format_Info[] _fArray = new Xmp_Format_Info[Num_Formats + Num_Pw_Formats + 1];

		/// <summary>
		/// List of all supported formats
		/// </summary>
		public static readonly Format_Loader[] format_Loaders = new Format_Loader[Num_Formats + 1]
		{
			Xm_Load.LibXmp_Loader_Xm,
			Mod_Load.LibXmp_Loader_Fast,
			Mod_Load.LibXmp_Loader_Take,
			Mod_Load.LibXmp_Loader_Scream3,
			Mod_Load.LibXmp_Loader_OpenMpt,
			Mod_Load.LibXmp_Loader_ModsGrave,
			Mod_Load.LibXmp_Loader_Dt,
			Mod_Load.LibXmp_Loader_Octalyser,
			Mod_Load.LibXmp_Loader_FlexTrax,
			Xm_Load.LibXmp_Loader_OggMod,
			It_Load.LibXmp_Loader_It,
			S3M_Load.LibXmp_Loader_S3M,
			Stm_Load.LibXmp_Loader_Stm,
			Stx_Load.LibXmp_Loader_Stx,
			Mtm_Load.LibXmp_Loader_Mtm,
			Imf_Load.LibXmp_Loader_Imf,
			Ptm_Load.LibXmp_Loader_Ptm,
			Ult_Load.LibXmp_Loader_Ult,
			Amf_Load.LibXmp_Loader_Amf,
			Asylum_Load.LibXmp_Loader_Asylum,
			Gdm_Load.LibXmp_Loader_Gdm,
			Arch_Load.LibXmp_Loader_Arch,
			Far_Load.LibXmp_Loader_Far,
			_669_Load.LibXmp_Loader_Composer669,
			_669_Load.LibXmp_Loader_Unis669,
			Dsm_Load.LibXmp_Loader_Dsm,

			Mod_Load.LibXmp_Loader_TestOnly,
			null		// List terminator
		};

		/********************************************************************/
		/// <summary>
		/// Query the list of supported module formats
		/// </summary>
		/********************************************************************/
		public static string[] Format_List()
		{
			return Format_Info_List().Select(x => x?.Name).ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Query the list of supported module formats
		/// </summary>
		/********************************************************************/
		public static Xmp_Format_Info[] Format_Info_List()
		{
			if (_fArray[0] == null)
			{
				c_int count = 0;

				for (c_int i = 0; format_Loaders[i] != null; i++)
				{
					Format_Loader fl = format_Loaders[i];

					if (fl.OnlyAvailableInTest && !LibXmp.UnitTestMode)
						continue;

					_fArray[count++] = new Xmp_Format_Info
					{
						Id = fl.Id,
						Name = fl.Name,
						Description = fl.Description
					};
				}

				_fArray[count] = null;
				Array.Resize(ref _fArray, count + 1);
			}

			return _fArray;
		}
	}
}
