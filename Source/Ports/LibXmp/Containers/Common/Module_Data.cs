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
		public Xmp_Module Mod = new Xmp_Module();

		/// <summary>
		/// File dirname
		/// </summary>
		public string DirName;

		/// <summary>
		/// File basename
		/// </summary>
		public string BaseName;

		/// <summary>
		/// Module file name
		/// </summary>
		public string FileName;

		/// <summary>
		/// Comments, if any
		/// </summary>
		public string Comment;

		/// <summary>
		/// MD5 message digest
		/// </summary>
		public readonly uint8[] Md5 = new uint8[16];

		/// <summary>
		/// File size
		/// </summary>
		public c_int Size;

		/// <summary>
		/// Replay rate
		/// </summary>
		public c_double RRate;

		/// <summary>
		/// Time conversion constant
		/// </summary>
		public c_double Time_Factor;

		/// <summary>
		/// C4 replay rate
		/// </summary>
		public c_int C4Rate;

		/// <summary>
		/// Volume base
		/// </summary>
		public c_int VolBase;

		/// <summary>
		/// Global volume base
		/// </summary>
		public c_int GVolBase;

		/// <summary>
		/// Global volume
		/// </summary>
		public c_int GVol;

		/// <summary>
		/// Mix volume base (S3M/IT)
		/// </summary>
		public c_int MVolBase;

		/// <summary>
		/// Mix volume (S3M/IT)
		/// </summary>
		public c_int MVol;

		/// <summary>
		/// Volume translation table
		/// </summary>
		public c_int[] Vol_Table;

		/// <summary>
		/// Player quirks
		/// </summary>
		public Quirk_Flag Quirk;

		/// <summary>
		/// Flow quirks, esp. Pattern Loop
		/// </summary>
		public FlowMode_Flag Flow_Mode;

		/// <summary>
		/// 
		/// </summary>
		public Read_Event Read_Event_Type;

		/// <summary>
		/// 
		/// </summary>
		public Period Period_Type;

		/// <summary>
		/// Sample control flags
		/// </summary>
		public Xmp_SmpCtl_Flag SmpCtl;

		/// <summary>
		/// Default pan setting
		/// </summary>
		public c_int DefPan;

		/// <summary>
		/// 
		/// </summary>
		public Ord_Data[] Xxo_Info = ArrayHelper.InitializeArray<Ord_Data>(Constants.Xmp_Max_Mod_Length);

		/// <summary>
		/// 
		/// </summary>
		public c_int Num_Sequences;

		/// <summary>
		/// 
		/// </summary>
		public Xmp_Sequence[] Seq_Data = ArrayHelper.InitializeArray<Xmp_Sequence>(Constants.Max_Sequences);

		/// <summary>
		/// 
		/// </summary>
		public string Instrument_Path;

		/// <summary>
		/// Format-specific extra fields
		/// </summary>
		public IModuleExtra Extra;

		/// <summary>
		/// Scan counters
		/// </summary>
		public uint8[][] Scan_Cnt;

		/// <summary>
		/// 
		/// </summary>
		public Extra_Sample_Data[] Xtra;

		/// <summary>
		/// 
		/// </summary>
		public Midi_Macro_Data Midi;

		/// <summary>
		/// 
		/// </summary>
		public bool Compare_VBlank;

		/// <summary>
		/// 
		/// </summary>
		public Xmp_Module_Flags Module_Flags;

		/// <summary>
		/// Names of found DSP effects/VST plugins
		/// </summary>
		public string[] DspEffects;
	}
}
