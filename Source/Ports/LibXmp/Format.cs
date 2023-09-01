/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Format;
using Polycode.NostalgicPlayer.Ports.LibXmp.Loaders;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Format
	{
		private const int Num_Formats = 4;
		private const int Num_Pw_Formats = 0;

		private static readonly string[] _fArray = new string[Num_Formats + Num_Pw_Formats + 1];

		/// <summary>
		/// List of all supported formats
		/// </summary>
		public static readonly Format_Loader[] format_Loaders = new Format_Loader[Num_Formats + 2]
		{
			Xm_Load.LibXmp_Loader_Xm,
			It_Load.LibXmp_Loader_It,
			S3M_Load.LibXmp_Loader_S3M,
			Gdm_Load.LibXmp_Loader_Gdm,
			null,
			null
		};

		/********************************************************************/
		/// <summary>
		/// Query the list of supported module formats
		/// </summary>
		/********************************************************************/
		public static string[] Format_List()
		{
			if (_fArray[0] == null)
			{
				c_int count = 0;

				for (c_int i = 0; format_Loaders[i] != null; i++)
					_fArray[count++] = format_Loaders[i].Name;

				_fArray[count] = null;
			}

			return _fArray;
		}
	}
}
