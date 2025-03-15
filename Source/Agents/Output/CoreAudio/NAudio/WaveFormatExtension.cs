/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Reflection;
using NAudio.Wave;

namespace Polycode.NostalgicPlayer.Agent.Output.CoreAudio.NAudio
{
	internal static class WaveFormatExtension
	{
		/********************************************************************/
		/// <summary>
		/// Return the current channel mask
		/// </summary>
		/********************************************************************/
		public static int ChannelMask(this WaveFormat waveFormat)
		{
			if (waveFormat is WaveFormatExtensible extensible)
			{
				FieldInfo field = extensible.GetType().GetField("dwChannelMask", BindingFlags.NonPublic | BindingFlags.Instance);
				if (field != null)
					return (int)field.GetValue(extensible);
			}

			return 0;
		}
	}
}
