/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class Decoder
	{
		/// <summary></summary>
		protected string author;

		/// <summary></summary>
		protected string title;

		/// <summary></summary>
		protected string game;

		/// <summary></summary>
		protected string comment;

		/// <summary></summary>
		protected bool songEnd;

		/// <summary></summary>
		protected bool realSongEnd;		// TNE: Added for song end detection when loopMode is enabled

		/// <summary></summary>
		protected bool loopMode;

		/// <summary></summary>
		protected udword rate;

		/// <summary></summary>
		protected udword tickFp;

		/// <summary></summary>
		protected udword tickFpAdd;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Decoder()
		{
			songEnd = true;
			realSongEnd = false;
			loopMode = false;
			rate = 50 << 8;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual bool Init(IPointer data, udword length, IPointer sample, udword sampleLength, out string errorMessage)
		{
			errorMessage = string.Empty;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void InitSong(c_int songNumber)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual bool Detect(IPointer data, udword len)
		{
			return false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void SetPaulaVoice(ubyte v, PaulaVoice p)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual c_int Run()
		{
			return 20;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual ubyte GetVoices()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual c_int GetSongs()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public string GetInfoString(string which)
		{
			if (which == "artist")
				return author;
			else if (which == "title")
				return title;
			else if (which == "game")
				return game;
			else if (which == "comment")
				return comment;

			return string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool GetSongEndFlag()
		{
			return realSongEnd;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetLoopMode(bool x)
		{
			loopMode = x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public udword GetRate()
		{
			return rate;
		}

		#region TNE: Added extra methods to retrieve different information
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual c_int GetPositions()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual c_int GetTracks()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual c_int GetPlayingPosition()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual ubyte[] GetPlayingTracks()
		{
			return Array.Empty<ubyte>();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual c_int GetSpeed()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Return a list of samples used
		/// </summary>
		/********************************************************************/
		public virtual IEnumerable<Sample> GetSamples()
		{
			return [];
		}
		#endregion

		#region THE: Added extra methods for snapshot support
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract ISnapshot CreateSnapshot();



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public abstract void SetSnapshot(ISnapshot snapshot);
		#endregion

		/********************************************************************/
		/// <summary>
		/// Since 8-bit fixed point arithmetics are used
		/// </summary>
		/********************************************************************/
		protected void SetRate(udword r)
		{
			rate = r;
			tickFp = (1000 << 16) / rate;	// in [ms]*256
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void SetBpm(uword bpm)
		{
			SetRate((udword)(((bpm << 8) * 2) / 5));
		}
	}
}
