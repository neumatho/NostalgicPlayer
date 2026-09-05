/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt
{
	/// <summary>
	/// 
	/// </summary>
	public class Module
	{
		private Module_Impl impl;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		internal Module()
		{
			impl = null;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Module(Stream stream, Guid? formatId = null, map<string, string> ctls = null)
		{
			impl = new Module_Impl(stream, formatId, ctls ?? new map<string, string>());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal void Set_Impl(Module_Impl i)//XX 183
		{
			impl = i;
		}



		/********************************************************************/
		/// <summary>
		/// Select a sub-song from a multi-song module
		/// </summary>
		/********************************************************************/
		public void Select_SubSong(int32_t subSong)//XX 236
		{
			impl.Select_SubSong(subSong);
		}



		/********************************************************************/
		/// <summary>
		/// Get the restart order of the specified sub-song
		/// </summary>
		/********************************************************************/
		public int32_t Get_Restart_Order(int32_t subSong)//XX 243
		{
			return impl.Get_Restart_Order(subSong);
		}



		/********************************************************************/
		/// <summary>
		/// Get the restart row of the specified sub-song
		/// </summary>
		/********************************************************************/
		public int32_t Get_Restart_Row(int32_t subSong)//XX 246
		{
			return impl.Get_Restart_Row(subSong);
		}



		/********************************************************************/
		/// <summary>
		/// Get approximate song duration
		/// </summary>
		/********************************************************************/
		public c_double Get_Duration_Seconds()//XX 257
		{
			return impl.Get_Duration_Seconds();
		}



		/********************************************************************/
		/// <summary>
		/// Get approximate playback time in seconds at given position
		/// </summary>
		/********************************************************************/
		public c_double Get_Time_At_Position(int32_t order, int32_t row)//XX 261
		{
			return impl.Get_Time_At_Position(order, row);
		}



		/********************************************************************/
		/// <summary>
		/// Set approximate current song position
		/// </summary>
		/********************************************************************/
		public c_double Set_Position_Seconds(c_double seconds)//XX 265
		{
			return impl.Set_Position_Seconds(seconds);
		}



		/********************************************************************/
		/// <summary>
		/// Set render parameter
		/// </summary>
		/********************************************************************/
		public void Set_Render_Param(Render_Param param, int32_t value)//XX 279
		{
			impl.Set_Render_Param(param, value);
		}



		/********************************************************************/
		/// <summary>
		/// Render audio data
		/// </summary>
		/********************************************************************/
		public size_t Read(int32_t sampleRate, size_t count, CPointer<int16_t> left, CPointer<int16_t> right)//XX 286
		{
			return impl.Read(sampleRate, count, left, right);
		}



		/********************************************************************/
		/// <summary>
		/// Render audio data
		/// </summary>
		/********************************************************************/
		public size_t Read(int32_t sampleRate, size_t count, CPointer<int16_t> left, CPointer<int16_t> right, CPointer<int16_t> rear_Left, CPointer<int16_t> rear_Right)//XX 289
		{
			return impl.Read(sampleRate, count, left, right, rear_Left, rear_Right);
		}



		/********************************************************************/
		/// <summary>
		/// Get a metadata item value
		/// </summary>
		/********************************************************************/
		public string Get_Metadata(string key)//XX 317
		{
			return impl.Get_Metadata(key);
		}



		/********************************************************************/
		/// <summary>
		/// Get the current estimated beats per minute (BPM)
		/// </summary>
		/********************************************************************/
		public c_double Get_Current_Estimated_Bpm()//XX 321
		{
			return impl.Get_Current_Estimated_Bpm();
		}



		/********************************************************************/
		/// <summary>
		/// Get the current speed
		/// </summary>
		/********************************************************************/
		public int32_t Get_Current_Speed()//XX 324
		{
			return impl.Get_Current_Speed();
		}



		/********************************************************************/
		/// <summary>
		/// Get the current tempo
		/// </summary>
		/********************************************************************/
		public c_double Get_Current_Tempo2()//XX 330
		{
			return impl.Get_Current_Tempo2();
		}



		/********************************************************************/
		/// <summary>
		/// Get the current order
		/// </summary>
		/********************************************************************/
		public int32_t Get_Current_Order()//XX 333
		{
			return impl.Get_Current_Order();
		}



		/********************************************************************/
		/// <summary>
		/// Get the current pattern
		/// </summary>
		/********************************************************************/
		public int32_t Get_Current_Pattern()//XX 336
		{
			return impl.Get_Current_Pattern();
		}



		/********************************************************************/
		/// <summary>
		/// Get the number of sub-songs
		/// </summary>
		/********************************************************************/
		public int32_t Get_Num_SubSongs()//XX 362
		{
			return impl.Get_Num_SubSongs();
		}



		/********************************************************************/
		/// <summary>
		/// Get the number of pattern channels
		/// </summary>
		/********************************************************************/
		public int32_t Get_Num_Channels()//XX 365
		{
			return impl.Get_Num_Channels();
		}



		/********************************************************************/
		/// <summary>
		/// The number of orders in the current sequence of the module
		/// </summary>
		/********************************************************************/
		public int32_t Get_Num_Orders()//XX 368
		{
			return impl.Get_Num_Orders();
		}



		/********************************************************************/
		/// <summary>
		/// Get the number of patterns
		/// </summary>
		/********************************************************************/
		public int32_t Get_Num_Patterns()//XX 371
		{
			return impl.Get_Num_Patterns();
		}



		/********************************************************************/
		/// <summary>
		/// Get the number of instruments
		/// </summary>
		/********************************************************************/
		public int32_t Get_Num_Instruments()//XX 374
		{
			return impl.Get_Num_Instruments();
		}



		/********************************************************************/
		/// <summary>
		/// Get the number of instruments
		/// </summary>
		/********************************************************************/
		public int32_t Get_Num_Samples()//XX 377
		{
			return impl.Get_Num_Samples();
		}



		/********************************************************************/
		/// <summary>
		/// Get a list of sub-song names
		/// </summary>
		/********************************************************************/
		public vector<string> Get_SubSong_Names()//XX 381
		{
			return impl.Get_SubSong_Names();
		}



		/********************************************************************/
		/// <summary>
		/// Set ctl text value
		/// </summary>
		/********************************************************************/
		public void Ctl_Set_Text(string ctl, string value)//XX 479
		{
			impl.Ctl_Set_Text(ctl, value);
		}
	}
}
