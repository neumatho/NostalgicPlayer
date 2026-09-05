/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Target seek mode for GetLength()
	/// </summary>
	internal struct GetLengthTarget
	{
		public enum Mode_
		{
			/// <summary>
			/// Don't seek, i.e. return complete length of the first subsong
			/// </summary>
			NoTarget,

			/// <summary>
			/// Same as NoTarget (i.e. get complete length), but returns
			/// the length of all sub songs
			/// </summary>
			GetAllSubsongs,

			/// <summary>
			/// Seek to given pattern position
			/// </summary>
			SeekPosition,

			/// <summary>
			/// Seek to given time
			/// </summary>
			SeekSeconds
		}

		public struct Pos_Type
		{
			public RowIndex Row;
			public OrderIndex Order;
		}

		public RowIndex StartRow;
		public OrderIndex StartOrder;
		public SequenceIndex Sequence;

		public c_double Time;
		public Pos_Type Pos;

		public Mode_ Mode;

		/********************************************************************/
		/// <summary>
		/// Don't seek, i.e. return complete module length
		/// </summary>
		/********************************************************************/
		public GetLengthTarget() : this(false)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Don't seek, i.e. return complete module length
		/// </summary>
		/********************************************************************/
		public GetLengthTarget(bool allSongs)
		{
			Mode = allSongs ? Mode_.GetAllSubsongs : Mode_.NoTarget;
			Sequence = Snd_Def.SequenceIndex_Invalid;
			StartOrder = 0;
			StartRow = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Seek to given pattern position if position is valid
		/// </summary>
		/********************************************************************/
		public GetLengthTarget(OrderIndex order, RowIndex row)
		{
			Mode = Mode_.NoTarget;
			Sequence = Snd_Def.SequenceIndex_Invalid;
			StartOrder = 0;
			StartRow = 0;

			if ((order != Snd_Def.OrderIndex_Invalid) && (row != Snd_Def.RowIndex_Invalid))
			{
				Mode = Mode_.SeekPosition;
				Pos.Row = row;
				Pos.Order = order;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Seek to given time if t is valid (i.e. not negative)
		/// </summary>
		/********************************************************************/
		public GetLengthTarget(c_double t)
		{
			Mode = Mode_.NoTarget;
			Sequence = Snd_Def.SequenceIndex_Invalid;
			StartOrder = 0;
			StartRow = 0;

			if (t >= 0.0)
			{
				Mode = Mode_.SeekSeconds;
				Time = t;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set start position from which seeking should begin
		/// </summary>
		/********************************************************************/
		public GetLengthTarget StartPos(SequenceIndex seq, OrderIndex order, RowIndex row)
		{
			Sequence = seq;
			StartOrder = order;
			StartRow = row;

			return this;
		}
	}
}
