/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.FormatExtras;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class Module_Data
	{
		public Xmp_Module Mod { get; set; } = new Xmp_Module();

		/// <summary>
		/// File dirname
		/// </summary>
		public string DirName { get; set; }

		/// <summary>
		/// File basename
		/// </summary>
		public string BaseName { get; set; }

		/// <summary>
		/// Module file name
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Comments, if any
		/// </summary>
		public string Comment { get; set; }

		/// <summary>
		/// MD5 message digest
		/// </summary>
		public uint8[] Md5 { get; } = new uint8[16];

		/// <summary>
		/// File size
		/// </summary>
		public c_int Size { get; set; }

		/// <summary>
		/// Replay rate
		/// </summary>
		public c_double RRate { get; set; }

		/// <summary>
		/// Time conversion constant
		/// </summary>
		public c_double Time_Factor { get; set; }

		/// <summary>
		/// C4 replay rate
		/// </summary>
		public c_int C4Rate { get; set; }

		/// <summary>
		/// Volume base
		/// </summary>
		public c_int VolBase { get; set; }

		/// <summary>
		/// Global volume base
		/// </summary>
		public c_int GVolBase { get; set; }

		/// <summary>
		/// Global volume
		/// </summary>
		public c_int GVol { get; set; }

		/// <summary>
		/// Mix volume base (S3M/IT)
		/// </summary>
		public c_int MVolBase { get; set; }

		/// <summary>
		/// Mix volume (S3M/IT)
		/// </summary>
		public ref c_int MVol => ref _MVol;
		private c_int _MVol;

		/// <summary>
		/// Volume translation table
		/// </summary>
		public c_int[] Vol_Table { get; set; }

		/// <summary>
		/// Player quirks
		/// </summary>
		public Quirk_Flag Quirk { get; set; }

		/// <summary>
		/// Flow quirks, esp. Pattern Loop
		/// </summary>
		public FlowMode_Flag Flow_Mode { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Read_Event Read_Event_Type { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Period Period_Type { get; set; }

		/// <summary>
		/// Sample control flags
		/// </summary>
		public Xmp_SmpCtl_Flag SmpCtl { get; set; }

		/// <summary>
		/// Default pan setting
		/// </summary>
		public c_int DefPan { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Ord_Data[] Xxo_Info { get; set; } = ArrayHelper.InitializeArray<Ord_Data>(Constants.Xmp_Max_Mod_Length);

		/// <summary>
		/// 
		/// </summary>
		public c_int Num_Sequences { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Xmp_Sequence[] Seq_Data { get; } = ArrayHelper.InitializeArray<Xmp_Sequence>(Constants.Max_Sequences);

		/// <summary>
		/// 
		/// </summary>
		public string Instrument_Path { get; set; }

		/// <summary>
		/// Format-specific extra fields
		/// </summary>
		public IModuleExtra Extra { get; set; }

		/// <summary>
		/// Scan counters
		/// </summary>
		public uint8[][] Scan_Cnt { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ref Extra_Sample_Data[] Xtra => ref _Xtra;
		private Extra_Sample_Data[] _Xtra;

		/// <summary>
		/// 
		/// </summary>
		public Midi_Macro_Data Midi { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool Compare_VBlank { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Xmp_Module_Flags Module_Flags { get; set; }

		/// <summary>
		/// Names of found DSP effects/VST plugins
		/// </summary>
		public string[] DspEffects { get; set; }
	}
}
