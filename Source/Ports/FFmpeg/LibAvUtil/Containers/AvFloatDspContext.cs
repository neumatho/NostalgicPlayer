/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvFloatDspContext
	{
		/// <summary>
		/// Calculate the entry wise product of two vectors of floats and store the result in
		/// a vector of floats
		/// </summary>
		public UtilFunc.Vector_FMul_Delegate Vector_FMul;

		/// <summary>
		/// Multiply a vector of floats by a scalar float and add to
		/// destination vector. Source and destination vectors must
		/// overlap exactly or not at all
		/// </summary>
		public UtilFunc.Vector_FMac_Scalar_Delegate Vector_FMac_Scalar;

		/// <summary>
		/// Multiply a vector of doubles by a scalar double and add to
		/// destination vector. Source and destination vectors must
		/// overlap exactly or not at all
		/// </summary>
		public UtilFunc.Vector_DMac_Scalar_Delegate Vector_DMac_Scalar;

		/// <summary>
		/// Multiply a vector of floats by a scalar float. Source and
		/// destination vectors must overlap exactly or not at all
		/// </summary>
		public UtilFunc.Vector_FMul_Scalar_Delegate Vector_FMul_Scalar;

		/// <summary>
		/// Multiply a vector of double by a scalar double. Source and
		/// destination vectors must overlap exactly or not at all
		/// </summary>
		public UtilFunc.Vector_DMul_Scalar_Delegate Vector_DMul_Scalar;

		/// <summary>
		/// Overlap/add with window function.
		/// Used primarily by MDCT-based audio codecs.
		/// Source and destination vectors must overlap exactly or not at all
		/// </summary>
		public UtilFunc.Vector_FMul_Window_Delegate Vector_FMul_Window;

		/// <summary>
		/// Calculate the entry wise product of two vectors of floats, add a third vector of
		/// floats and store the result in a vector of floats
		/// </summary>
		public UtilFunc.Vector_FMul_Add_Delegate Vector_FMul_Add;

		/// <summary>
		/// Calculate the entry wise product of two vectors of floats, and store the result
		/// in a vector of floats. The second vector of floats is iterated over
		/// in reverse order
		/// </summary>
		public UtilFunc.Vector_FMul_Reverse_Delegate Vector_FMul_Reverse;

		/// <summary>
		/// Calculate the sum and difference of two vectors of floats
		/// </summary>
		public UtilFunc.Butterflies_Float_Delegate Butterflies_Float;

		/// <summary>
		/// Calculate the scalar product of two vectors of floats
		/// </summary>
		public UtilFunc.ScalarProduct_Float_Delegate ScalarProduct_Float;

		/// <summary>
		/// Calculate the entry wise product of two vectors of doubles and store the result in
		/// a vector of doubles
		/// </summary>
		public UtilFunc.Vector_DMul_Delegate Vector_DMul;

		/// <summary>
		/// Calculate the scalar product of two vectors of doubles
		/// </summary>
		public UtilFunc.ScalarProduct_Double_Delegate ScalarProduct_Double;
	}
}
