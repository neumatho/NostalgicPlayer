/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvOption
	{
		#region DefaultValueUnion struct
		/// <summary>
		/// 
		/// </summary>
		public struct DefaultValueUnion
		{
			/// <summary>
			/// 
			/// </summary>
			public int64_t I64;

			/// <summary>
			/// 
			/// </summary>
			public c_double Dbl;

			/// <summary>
			/// 
			/// </summary>
			public CPointer<char> Str;

			/// <summary>
			/// 
			/// </summary>
			public AvRational Q;

			/// <summary>
			/// 
			/// </summary>
			public CPointer<AvOptionArrayDef> Arr;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AvOption()
		{
			Name.SetToNull();
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AvOption(string name, AvOptionType type, DefaultValueUnion default_Value, string unit) : this(name, null, null, type, default_Value, 0, 0, AvOptFlag.None, unit)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AvOption(string name, string help, string optionName, AvOptionType type, DefaultValueUnion default_Value, AvOptFlag flags, string unit) : this(name, help, optionName, type, default_Value, 0, 0, flags, unit)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AvOption(string name, string help, string optionName, AvOptionType type, DefaultValueUnion default_Value, c_double min, c_double max, AvOptFlag flags) : this(name, help, optionName, type, default_Value, min, max, flags, null)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AvOption(string name, string help, string optionName, AvOptionType type, DefaultValueUnion default_Value, c_double min, c_double max, AvOptFlag flags, string unit)
		{
			Name = name.ToCharPointer();
			Help = help != null ? help.ToCharPointer() : null;
			OptionName = optionName;
			Type = type;
			Default_Value = default_Value;
			Min = min;
			Max = max;
			Flags = flags;
			Unit = unit != null ? unit.ToCharPointer() : null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<char> Name { get; }



		/********************************************************************/
		/// <summary>
		/// Short English help text
		/// </summary>
		/********************************************************************/
		public CPointer<char> Help { get; }



		/********************************************************************/
		/// <summary>
		/// Native access only.
		///
		/// The offset relative to the context structure where the option
		/// value is stored. It should be 0 for named constants.
		///
		/// TNE: Added OptionName instead
		/// </summary>
		/********************************************************************/
//		public c_int Offset;
		public string OptionName { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public AvOptionType Type { get; }



		/********************************************************************/
		/// <summary>
		/// Native access only, except when documented otherwise.
		/// The default value for scalar options
		/// </summary>
		/********************************************************************/
		public DefaultValueUnion Default_Value { get; }



		/********************************************************************/
		/// <summary>
		/// Minimum valid value for the option
		/// </summary>
		/********************************************************************/
		public c_double Min { get; }



		/********************************************************************/
		/// <summary>
		/// Maximum valid value for the option
		/// </summary>
		/********************************************************************/
		public c_double Max { get; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public AvOptFlag Flags { get; }



		/********************************************************************/
		/// <summary>
		/// The logical unit to which the option belongs. Non-constant
		/// options and corresponding named constants share the same
		/// unit. May be NULL
		/// </summary>
		/********************************************************************/
		public CPointer<char> Unit { get; }
	}
}
