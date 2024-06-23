/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers.Chunks;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers.Plugins;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers
{
	/// <summary>
	/// Holds the whole module in MO3 format
	/// </summary>
	internal class Mo3Module
	{
		public byte Version;
		public uint MusicSize;
		public ModuleType ModuleType;
		public FileHeader Header;
		public PatternInfo PatternInfo;
		public Track[] Tracks;
		public Instrument[] Instruments;
		public Sample[] Samples;
		public IPlugin[] Plugins;
		public IChunk[] Chunks;

		/********************************************************************/
		/// <summary>
		/// Try to find the chunk given
		/// </summary>
		/********************************************************************/
		public T FindChunk<T>() where T : IChunk
		{
			return (T)Chunks?.FirstOrDefault(x => x.GetType() == typeof(T));
		}



		/********************************************************************/
		/// <summary>
		/// Try to find the plugin given
		/// </summary>
		/********************************************************************/
		public IEnumerable<T> FindPlugin<T>() where T : IPlugin
		{
			return Plugins?.Where(x => x.GetType() == typeof(T)).Cast<T>();
		}
	}
}
