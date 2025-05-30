/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Base class that can be used for player agents
	/// </summary>
	public abstract class PlayerAgentBase : IPlayerAgent
	{
		internal bool doNotTrigEvents;

		private readonly Dictionary<int, ModuleInfoChanged> changedModuleInfo = new Dictionary<int, ModuleInfoChanged>();

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public abstract string[] FileExtensions
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the identity priority number. Players with the lowest
		/// numbers will be called first.
		///
		/// Normally, you should not change this, but make your Identify()
		/// method to be aware of similar formats
		/// </summary>
		/********************************************************************/
		public virtual int IdentifyPriority => int.MaxValue / 2;



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public abstract AgentResult Identify(PlayerFileInfo fileInfo);



		/********************************************************************/
		/// <summary>
		/// Return some extra information about the format. If it returns
		/// null or an empty string, nothing extra is shown
		/// </summary>
		/********************************************************************/
		public virtual string ExtraFormatInfo => null;



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public virtual string ModuleName => string.Empty;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public virtual string Author => string.Empty;



		/********************************************************************/
		/// <summary>
		/// Return the comment separated in lines
		/// </summary>
		/********************************************************************/
		public virtual string[] Comment => Array.Empty<string>();



		/********************************************************************/
		/// <summary>
		/// Return a specific font to be used for the comments
		/// </summary>
		/********************************************************************/
		public virtual Font CommentFont => null;



		/********************************************************************/
		/// <summary>
		/// Return the lyrics separated in lines
		/// </summary>
		/********************************************************************/
		public virtual string[] Lyrics => Array.Empty<string>();



		/********************************************************************/
		/// <summary>
		/// Return a specific font to be used for the lyrics
		/// </summary>
		/********************************************************************/
		public virtual Font LyricsFont => null;



		/********************************************************************/
		/// <summary>
		/// Return all pictures available
		/// </summary>
		/********************************************************************/
		public virtual PictureInfo[] Pictures => null;
		#endregion

		#region IModuleInformation implementation
		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public virtual bool GetInformationString(int line, out string description, out string value)
		{
			description = null;
			value = null;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Return all module information changed since last call
		/// </summary>
		/********************************************************************/
		public virtual ModuleInfoChanged[] GetChangedInformation()
		{
			ModuleInfoChanged[] changedInfo = changedModuleInfo.Values.ToArray();
			changedModuleInfo.Clear();

			return changedInfo;
		}
		#endregion

		#region IEndDetection implementation
		/********************************************************************/
		/// <summary>
		/// This flag is set to true, when end is reached
		/// </summary>
		/********************************************************************/
		public virtual bool HasEndReached
		{
			get; set;
		}
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Call this when your player has reached the end of the module
		/// </summary>
		/********************************************************************/
		protected void OnEndReached()
		{
			// Set flag
			HasEndReached = true;
		}



		/********************************************************************/
		/// <summary>
		/// Call this every time your player change some information shown
		/// in the module information window
		/// </summary>
		/********************************************************************/
		protected void OnModuleInfoChanged(int line, string newValue)
		{
			if (!doNotTrigEvents)
				changedModuleInfo[line] = new ModuleInfoChanged(line, newValue);
		}
		#endregion
	}
}
