/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// May be set as default_val for AV_OPT_TYPE_FLAG_ARRAY options
	/// </summary>
	public class AvOptionArrayDef
	{
		/// <summary>
		/// Native access only.
		///
		/// Default value of the option, as would be serialized by av_opt_get() (i.e.
		/// using the value of sep as the separator)
		/// </summary>
		public CPointer<char> Def;

		/// <summary>
		/// Minimum number of elements in the array. When this field is non-zero, def
		/// must be non-NULL and contain at least this number of elements
		/// </summary>
		public c_uint Size_Min;

		/// <summary>
		/// Maximum number of elements in the array, 0 when unlimited
		/// </summary>
		public c_uint Size_Max;

		/// <summary>
		/// Separator between array elements in string representations of this
		/// option, used by av_opt_set() and av_opt_get(). It must be a printable
		/// ASCII character, excluding alphanumeric and the backslash. A comma is
		/// used when sep=0.
		///
		/// The separator and the backslash must be backslash-escaped in order to
		/// appear in string representations of the option value
		/// </summary>
		public char Sep;
	}
}
