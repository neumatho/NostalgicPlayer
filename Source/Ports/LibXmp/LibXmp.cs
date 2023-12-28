/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	public class LibXmp
	{
		private readonly Control control;
		private readonly Load load;
		internal readonly Load_Helpers loadHelpers;
		internal readonly Player player;
		internal readonly Extras extras;
		internal readonly Virtual virt;
		internal readonly Loaders.Common common;
		internal readonly Scan scan;
		internal readonly Period period;
		internal readonly Lfo lfo;
		internal readonly Mixer mixer;
		internal readonly SMix sMix;

		/// <summary>
		/// Used by unit tests, so don't remove it
		/// </summary>
		private readonly Xmp_Context ctx;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private LibXmp(Xmp_Context ctx)
		{
			this.ctx = ctx;

			control = new Control(this, ctx);
			load = new Load(this, ctx);
			loadHelpers = new Load_Helpers(this, ctx);
			player = new Player(this, ctx);
			extras = new Extras(ctx);
			virt = new Virtual(this, ctx);
			common = new Loaders.Common(this);
			scan = new Scan(ctx);
			period = new Period(ctx);
			lfo = new Lfo(ctx);
			mixer = new Mixer(this, ctx);
			sMix = new SMix(ctx);
		}



		/********************************************************************/
		/// <summary>
		/// Holds a value telling if LibXmp is running in unit test mode.
		/// Should not be set in normal use
		/// </summary>
		/********************************************************************/
		public static bool UnitTestMode
		{
			get; set;
		} = false;



		/********************************************************************/
		/// <summary>
		/// Create a new player context and return an opaque handle to be
		/// used in subsequent accesses to this context
		/// </summary>
		/********************************************************************/
		public static LibXmp Xmp_Create_Context()
		{
			return new LibXmp(Control.Xmp_Create_Context());
		}



		/********************************************************************/
		/// <summary>
		/// Destroy a player context previously created using
		/// Xmp_Create_Context()
		/// </summary>
		/********************************************************************/
		public void Xmp_Free_Context()
		{
			control.Xmp_Free_Context();
		}



		/********************************************************************/
		/// <summary>
		/// Will set a specific format, so the loader only will check against
		/// this
		/// </summary>
		/********************************************************************/
		public void Xmp_Set_Load_Format(Guid formatId)
		{
			load.Xmp_Set_Load_Format(formatId);
		}



		/********************************************************************/
		/// <summary>
		/// Load a module from memory into the specified player context
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Load_Module_From_Memory(uint8[] mem, c_long size)
		{
			return load.Xmp_Load_Module_From_Memory(mem, size);
		}



		/********************************************************************/
		/// <summary>
		/// Load a module from a stream into the specified player context
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Load_Module_From_File(Stream file)
		{
			return load.Xmp_Load_Module_From_File(file);
		}



		/********************************************************************/
		/// <summary>
		/// Load a module from a custom stream into the specified player
		/// context
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Load_Module_From_Callbacks(object priv, Xmp_Callbacks callbacks)
		{
			return load.Xmp_Load_Module_From_Callbacks(priv, callbacks);
		}



		/********************************************************************/
		/// <summary>
		/// Test if a memory buffer is a valid module. Testing memory does
		/// not affect the current player context or any currently loaded
		/// module
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Test_Module_From_Memory(uint8[] mem, c_long size, out Xmp_Test_Info info)
		{
			return load.Xmp_Test_Module_From_Memory(mem, size, out info);
		}



		/********************************************************************/
		/// <summary>
		/// Test if a module from a stream is a valid module. Testing streams
		/// does not affect the current player context or any currently
		/// loaded module
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Test_Module_From_File(Stream file, out Xmp_Test_Info info)
		{
			return load.Xmp_Test_Module_From_File(file, out info);
		}



		/********************************************************************/
		/// <summary>
		/// Test if a module from a custom stream is a valid module. Testing
		/// custom streams does not affect the current player context or any
		/// currently loaded module
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Test_Module_From_Callbacks(object priv, Xmp_Callbacks callbacks, out Xmp_Test_Info info)
		{
			return load.Xmp_Test_Module_From_Callbacks(priv, callbacks, out info);
		}



		/********************************************************************/
		/// <summary>
		/// Scan the loaded module for sequences and timing. Scanning is
		/// automatically performed by xmp_load_module() and this function
		/// should be called only if xmp_set_player() is used to change
		/// player timing (with parameter XMP_PLAYER_VBLANK) in libxmp 4.0.2
		/// or older
		/// </summary>
		/********************************************************************/
		public void Xmp_Scan_Module()
		{
			load.Xmp_Scan_Module();
		}



		/********************************************************************/
		/// <summary>
		/// Release memory allocated by a module from the specified player
		/// context
		/// </summary>
		/********************************************************************/
		public void Xmp_Release_Module()
		{
			load.Xmp_Release_Module();
		}



		/********************************************************************/
		/// <summary>
		/// Start playing the currently loaded module
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Start_Player(c_int rate, Xmp_Format format)
		{
			return player.Xmp_Start_Player(rate, format);
		}



		/********************************************************************/
		/// <summary>
		/// Play one frame of the module. Modules usually play at 50 frames
		/// per second
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Play_Frame()
		{
			return player.Xmp_Play_Frame();
		}



		/********************************************************************/
		/// <summary>
		/// Fill the buffer with PCM data up to the specified size. This is a
		/// convenience function that calls xmp_play_frame() internally to
		/// fill the user-supplied buffer. Don't call both xmp_play_frame()
		/// and xmp_play_buffer() in the same replay loop. If you don't need
		/// equally sized data chunks, xmp_play_frame() may result in better
		/// performance. Also note that silence is added at the end of a
		/// buffer if the module ends and no loop is to be performed
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Play_Buffer(Array out_Buffer, c_int size, c_int loop)
		{
			return player.Xmp_Play_Buffer(out_Buffer, size, loop);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve the current frame data
		/// </summary>
		/********************************************************************/
		public void Xmp_Get_Frame_Info(out Xmp_Frame_Info info)
		{
			player.Xmp_Get_Frame_Info(out info);
		}



		/********************************************************************/
		/// <summary>
		/// End module replay and release player memory
		/// </summary>
		/********************************************************************/
		public void Xmp_End_Player()
		{
			player.Xmp_End_Player();
		}



		/********************************************************************/
		/// <summary>
		/// Dynamically insert a new event into a playing module
		/// </summary>
		/********************************************************************/
		public void Xmp_Inject_Event(c_int channel, Xmp_Event e)
		{
			control.Xmp_Inject_Event(channel, e);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve current module data
		/// </summary>
		/********************************************************************/
		public void Xmp_Get_Module_Info(out Xmp_Module_Info info)
		{
			player.Xmp_Get_Module_Info(out info);
		}



		/********************************************************************/
		/// <summary>
		/// Query the list of supported module formats
		/// </summary>
		/********************************************************************/
		public static string[] Xmp_Get_Format_List()
		{
			return Control.Xmp_Get_Format_List();
		}



		/********************************************************************/
		/// <summary>
		/// Query the list of supported module formats
		/// </summary>
		/********************************************************************/
		public static Xmp_Format_Info[] Xmp_Get_Format_Info_List()
		{
			return Format.Format_Info_List();
		}



		/********************************************************************/
		/// <summary>
		/// Skip replay to the start of the next position
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Next_Position()
		{
			return control.Xmp_Next_Position();
		}



		/********************************************************************/
		/// <summary>
		/// Skip replay to the start of the previous position
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Prev_Position()
		{
			return control.Xmp_Prev_Position();
		}



		/********************************************************************/
		/// <summary>
		/// Skip replay to the start of the given position
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Set_Position(c_int pos)
		{
			return control.Xmp_Set_Position(pos);
		}



		/********************************************************************/
		/// <summary>
		/// Skip replay to the given row
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Set_Row(c_int row)
		{
			return control.Xmp_Set_Row(row);
		}



		/********************************************************************/
		/// <summary>
		/// Stop the currently playing module
		/// </summary>
		/********************************************************************/
		public void Xmp_Stop_Module()
		{
			control.Xmp_Stop_Module();
		}



		/********************************************************************/
		/// <summary>
		/// Restart the currently playing module
		/// </summary>
		/********************************************************************/
		public void Xmp_Restart_Module()
		{
			control.Xmp_Restart_Module();
		}



		/********************************************************************/
		/// <summary>
		/// Skip replay to the specified time
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Seek_Time(c_int time)
		{
			return control.Xmp_Seek_Time(time);
		}



		/********************************************************************/
		/// <summary>
		/// Mute or unmute the specified channel
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Channel_Mute(c_int chn, c_int status)
		{
			return control.Xmp_Channel_Mute(chn, status);
		}



		/********************************************************************/
		/// <summary>
		/// Set or retrieve the volume of the specified channel
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Channel_Vol(c_int chn, c_int vol)
		{
			return control.Xmp_Channel_Vol(chn, vol);
		}



		/********************************************************************/
		/// <summary>
		/// Set player parameter with the specified value
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Set_Player(Xmp_Player parm, c_int val)
		{
			return control.Xmp_Set_Player(parm, val);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve current value of the specified player parameter
		/// </summary>
		/********************************************************************/
		public c_int Xmp_Get_Player(Xmp_Player parm)
		{
			return control.Xmp_Get_Player(parm);
		}



		/********************************************************************/
		/// <summary>
		/// Will return the visualizer channels
		/// </summary>
		/********************************************************************/
		public ChannelChanged[] Xmp_Get_Visualizer_Channels()
		{
			return mixer.LibXmp_Mixer_GetVisualizerChannels();
		}



		/********************************************************************/
		/// <summary>
		/// Will create a snapshot of the current player state
		/// </summary>
		/********************************************************************/
		public ISnapshot Xmp_Create_Snapshot()
		{
			return new Snapshot(ctx.P, ctx.M.Extra);
		}



		/********************************************************************/
		/// <summary>
		/// Will set the player state to the given snapshot
		/// </summary>
		/********************************************************************/
		public void Xmp_Set_Snapshot(ISnapshot snapshot)
		{
			// Start to make a clone of the snapshot
			Snapshot currentSnapshot = (Snapshot)snapshot;
			Snapshot clonedSnapshot = new Snapshot(currentSnapshot.PlayerData, currentSnapshot.ModuleExtra);

			ctx.P = clonedSnapshot.PlayerData;
			ctx.M.Extra = clonedSnapshot.ModuleExtra;

			virt.LibXmp_Virt_Reset();
		}
	}
}
