/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Describe the class of an AVClass context structure. That is an
	/// arbitrary struct of which the first field is a pointer to an
	/// AVClass struct (e.g. AVCodecContext, AVFormatContext etc.)
	/// </summary>
	public class AvClass : IClass, IClearable, ICopyTo<IClass>
	{
		/// <summary>
		/// The name of the class; usually it is the same name as the
		/// context structure type to which the AVClass is associated
		/// </summary>
		public CPointer<char> Class_Name;

		/// <summary>
		/// A pointer to a function which returns the name of a context
		/// instance ctx associated with the class
		/// </summary>
		public UtilFunc.ItemName_Delegate Item_Name;

		/// <summary>
		/// An array of options for the structure or NULL.
		/// When non-NULL, the array must be terminated by an option with a NULL
		/// name
		/// </summary>
		public CPointer<AvOption> Option;

		/// <summary>
		/// LIBAVUTIL_VERSION with which this structure was created.
		/// This is used to allow fields to be added to AVClass without requiring
		/// major version bumps everywhere
		/// </summary>
		public c_int Version;

		/// <summary>
		/// Offset in the structure where the log level offset is stored. The log
		/// level offset is an int added to the log level for logging with this
		/// object as the context.
		///
		/// 0 means there is no such variable
		///
		/// TNE: Added Log_Level_Offset_Name instead
		/// </summary>
//		public c_int Log_Level_Offset_Offset;
		public string Log_Level_Offset_Name;

		/// <summary>
		/// Offset in the structure where a pointer to the parent context for
		/// logging is stored. For example a decoder could pass its AVCodecContext
		/// to eval as such a parent context, which an ::av_log() implementation
		/// could then leverage to display the parent context.
		///
		/// When the pointer is NULL, or this offset is zero, the object is assumed
		/// to have no parent
		///
		/// TNE: Added Parent_Log_Context_Name instead
		/// </summary>
//		public c_int Parent_Log_Context_Offset;
		public string Parent_Log_Context_Name;

		/// <summary>
		/// Category used for visualization (like color).
		///
		/// Only used when ::get_category() is NULL. Use this field when all
		/// instances of this class have the same category, use ::get_category()
		/// otherwise
		/// </summary>
		public AvClassCategory Category;

		/// <summary>
		/// Callback to return the instance category. Use this callback when
		/// different instances of this class may have different categories,
		/// ::category otherwise
		/// </summary>
		public UtilFunc.GetCategory_Delegate Get_Category;

		/// <summary>
		/// Callback to return the supported/allowed ranges
		/// </summary>
		public UtilFunc.QueryRanges_Delegate Query_Ranges;

		/// <summary>
		/// Return next AVOptions-enabled child or NULL
		/// </summary>
		public UtilFunc.ChildNext_Delegate Child_Next;

		/// <summary>
		/// Iterate over the AVClasses corresponding to potential AVOptions-enabled
		/// children.
		///
		/// Note: The difference between ::child_next() and ::child_class_iterate()
		///       is that ::child_next() iterates over _actual_ children of an
		///       _existing_ object instance, while ::child_class_iterate() iterates
		///       over the classes of all _potential_ children of any possible
		///       instance of this class
		/// </summary>
		public UtilFunc.ChildClassIterater_Delegate Child_Class_Iterate;

		/// <summary>
		/// When non-zero, offset in the object to an unsigned int holding object
		/// state flags, a combination of AVClassStateFlags values. The flags are
		/// updated by the object to signal its state to the generic code.
		///
		/// Added in version 59.41.100
		///
		/// TNE: Added State_Flags_Name instead
		/// </summary>
//		public c_int State_Flags_Offset;
		public string State_Flags_Name;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void Clear()
		{
			Class_Name.SetToNull();
			Item_Name = null;
			Option.SetToNull();
			Version = 0;
			Log_Level_Offset_Name = null;
			Parent_Log_Context_Name = null;
			Category = 0;
			Get_Category = null;
			Query_Ranges = null;
			Child_Next = null;
			Child_Class_Iterate = null;
			State_Flags_Name = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyTo(IClass destination)
		{
			AvClass dest = (AvClass)destination;

			dest.Class_Name = Class_Name;
			dest.Item_Name = Item_Name;
			dest.Option = Option;
			dest.Version = Version;
			dest.Log_Level_Offset_Name = Log_Level_Offset_Name;
			dest.Parent_Log_Context_Name = Parent_Log_Context_Name;
			dest.Category = Category;
			dest.Get_Category = Get_Category;
			dest.Query_Ranges = Query_Ranges;
			dest.Child_Next = Child_Next;
			dest.Child_Class_Iterate = Child_Class_Iterate;
			dest.State_Flags_Name = State_Flags_Name;
		}
	}
}
