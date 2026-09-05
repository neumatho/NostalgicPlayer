/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Default pattern channel settings
	/// </summary>
	internal class ModChannelSettings : IDeepCloneable<ModChannelSettings>
	{
		/// <summary>
		/// Channel flags
		/// </summary>
		public ChannelFlags dwFlags;

		/// <summary>
		/// Initial pan (0...256)
		/// </summary>
		public uint16 nPan = 128;

		/// <summary>
		/// Initial channel volume (0...64)
		/// </summary>
		public uint8 nVolume = 64;

		/// <summary>
		/// Assigned plugin
		/// </summary>
		public PlugIndex nMixPlugin = 0;

		/// <summary>
		/// Channel name
		/// </summary>
		public CharBuf szName = new CharBuf(Snd_Def.Max_ChannelName);

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public ModChannelSettings MakeDeepClone()
		{
			ModChannelSettings clone = (ModChannelSettings)MemberwiseClone();

			clone.szName = szName.MakeDeepClone();

			return clone;
		}
	}
}
