/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Opus_MultiStream
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool Validate_Layout(ChannelLayout layout)
		{
			c_int max_channel = layout.nb_streams + layout.nb_coupled_streams;
			if (max_channel > 255)
				return false;

			for (c_int i = 0; i < layout.nb_channels; i++)
			{
				if ((layout.mapping[i] >= max_channel) && (layout.mapping[i] != 255))
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Get_Left_Channel(ChannelLayout layout, c_int stream_id, c_int prev)
		{
			c_int i = prev < 0 ? 0 : prev + 1;

			for (; i < layout.nb_channels; i++)
			{
				if (layout.mapping[i] == (stream_id * 2))
					return i;
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Get_Right_Channel(ChannelLayout layout, c_int stream_id, c_int prev)
		{
			c_int i = prev < 0 ? 0 : prev + 1;

			for (; i < layout.nb_channels; i++)
			{
				if (layout.mapping[i] == (stream_id * 2 + 1))
					return i;
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Get_Mono_Channel(ChannelLayout layout, c_int stream_id, c_int prev)
		{
			c_int i = prev < 0 ? 0 : prev + 1;

			for (; i < layout.nb_channels; i++)
			{
				if (layout.mapping[i] == (stream_id + layout.nb_coupled_streams))
					return i;
			}

			return -1;
		}
	}
}
