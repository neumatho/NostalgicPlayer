/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.Exceptions;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidTune;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp
{
	/// <summary>
	/// Handle loading of a tune
	/// </summary>
	public class SidTune
	{
		/// <summary>
		/// Default sid tune file name extensions
		/// </summary>
		private static readonly string[] defaultFileNameExt =
		{
			// Preferred default file extension for single-file sid tunes
			// or sid tune description files in SIDPLAY INFOFILE format
			"sid",

			// File extensions used (and created) by various C64 emulators and
			// related utilities. These extensions are recommended to be used as
			// a replacement for ".dat" in conjunction with two-file sid tunes
			"c64", "prg", "p00",

			// Stereo Sidplayer (.mus ought not be included because
			// these must be loaded first; it sometimes contains the first
			// credit lines of a mus/str pair)
			"str", "mus"
		};

		private string[] fileNameExtensions;

		private SidTuneBase tune;

		private bool status;
		private string statusString;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SidTune(PlayerFileInfo fileInfo)
		{
			SetFileNameExtensions();
			Load(fileInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Select sub-song (0 = default starting song)
		/// </summary>
		/********************************************************************/
		public uint SelectSong(uint songNum)
		{
			return tune != null ? tune.SelectSong(songNum) : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve current active sub-song specific information
		/// </summary>
		/********************************************************************/
		public SidTuneInfo GetInfo()
		{
			return tune != null ? tune.GetInfo() : null;
		}



		/********************************************************************/
		/// <summary>
		/// Determine current state of object.
		/// Upon error condition use StatusString to get a descriptive text
		/// string
		/// </summary>
		/********************************************************************/
		public bool GetStatus()
		{
			return status;
		}



		/********************************************************************/
		/// <summary>
		/// Error/status message of last operation
		/// </summary>
		/********************************************************************/
		public string StatusString()
		{
			return statusString;
		}



		/********************************************************************/
		/// <summary>
		/// Copy sid tune into C64 memory (64 KB)
		/// </summary>
		/********************************************************************/
		public bool PlaceSidTuneInC64Mem(ISidMemory mem)
		{
			if (tune == null)
				return false;

			tune.PlaceSidTuneInC64Mem(mem);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the MD5 hash of the tune, new method, introduced in
		/// HVSC #68
		/// </summary>
		/********************************************************************/
		public byte[] CreateMD5New()
		{
			return tune != null ? tune.CreateMD5New(): null;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// The used file name extensions to the default ones
		/// </summary>
		/********************************************************************/
		private void SetFileNameExtensions()
		{
			fileNameExtensions = defaultFileNameExt;
		}



		/********************************************************************/
		/// <summary>
		/// Load a sid tune into an existing object
		/// </summary>
		/********************************************************************/
		private void Load(PlayerFileInfo fileInfo)
		{
			try
			{
				tune = SidTuneBase.Load(fileInfo, fileNameExtensions);
				status = true;
				statusString = Resources.IDS_SID_ERR_NO_ERRORS;
			}
			catch (LoadErrorException ex)
			{
				tune = null;
				status = false;
				statusString = ex.Message;
			}
		}
		#endregion
	}
}
