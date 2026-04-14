/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder
{
	/// <summary>
	/// 
	/// </summary>
	internal abstract class SmartPtrBase<T>
	{
		protected CPointer<T> bufBegin;
		protected CPointer<T> bufEnd;
		protected CPointer<T> pBufCurrent;
		protected c_ulong bufLen;
		protected bool status;
		protected bool doFree;
		protected T dummy;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected SmartPtrBase(CPointer<T> buffer, c_ulong bufferLen, bool bufOwner = false)
		{
			dummy = default;
			doFree = bufOwner;

			if (bufferLen >= 1)
			{
				pBufCurrent = (bufBegin = buffer);
				bufEnd = bufBegin + bufferLen;
				bufLen = bufferLen;
				status = true;
			}
			else
			{
				pBufCurrent.SetToNull();
				bufBegin.SetToNull();
				bufEnd.SetToNull();
				bufLen = 0;
				status = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual CPointer<T> TellBegin()
		{
			return bufBegin;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual c_ulong TellLength()
		{
			return bufLen;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual bool CheckIndex(c_ulong index)
		{
			return (pBufCurrent + index) < bufEnd;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ref T this[c_ulong index]
		{
			get
			{
				if (CheckIndex(index))
					return ref pBufCurrent[index];
				else
				{
					status = false;

					return ref dummy;
				}
			}
		}
	}



	/// <summary>
	/// 
	/// </summary>
	internal class SmartPtr<T> : SmartPtrBase<T>
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SmartPtr(CPointer<T> buffer, c_ulong bufferLen, bool bufOwner = false) : base(buffer, bufferLen, bufOwner)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SmartPtr() : base(null, 0)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetBuffer(CPointer<T> buffer, c_ulong bufferLen)
		{
			if (bufferLen >= 1)
			{
				pBufCurrent = (bufBegin = buffer);
				bufEnd = bufBegin + bufferLen;
				bufLen = bufferLen;
				status = true;
			}
			else
			{
				pBufCurrent.SetToNull();
				bufBegin.SetToNull();
				bufEnd.SetToNull();
				bufLen = 0;
				status = false;
			}
		}
	}
}
