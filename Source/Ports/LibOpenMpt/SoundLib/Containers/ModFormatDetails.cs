/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Text;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class ModFormatDetails
	{
		/// <summary>
		/// "FastTracker 2"
		/// </summary>
		public string FormatName;

		/// <summary>
		/// "xm"
		/// </summary>
		public string Type;

		/// <summary>
		/// "OpenMPT 1.28.01.00"
		/// </summary>
		public string MadeWithTracker;

		/// <summary>
		/// "FastTracker 2" in the case of converted formats like MO3 or GDM
		/// </summary>
		public string OriginalFormatName;

		/// <summary>
		/// "xm" in the case of converted formats like MO3 or GDM
		/// </summary>
		public string OriginalType;

		/// <summary>
		/// 
		/// </summary>
		public Encoding CharSet = Encoding.UTF8;

		/// <summary>
		/// 
		/// </summary>
		public LogicalTimezone Timezone = LogicalTimezone.Unspecified;
	}
}
