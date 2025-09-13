/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class ChannelLayout : IDeepCloneable<ChannelLayout>
	{
		public c_int nb_channels;
		public c_int nb_streams;
		public c_int nb_coupled_streams;
		public readonly byte[] mapping = new byte[256];

		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public ChannelLayout MakeDeepClone()
		{
			ChannelLayout clone = new ChannelLayout
			{
				nb_channels = nb_channels,
				nb_streams = nb_streams,
				nb_coupled_streams = nb_coupled_streams
			};

			Array.Copy(mapping, clone.mapping, mapping.Length);

			return clone;
		}
	}
}
