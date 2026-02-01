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
	public enum AvTxType
	{
		// Standard complex to complex FFT with sample data type of AVComplexFloat,
		// AVComplexDouble or AVComplexInt32, for each respective variant.
		// 
		// Output is not 1/len normalized. Scaling currently unsupported.
		// The stride parameter must be set to the size of a single sample in bytes

		/// <summary></summary>
		Float_Fft = 0,
		/// <summary></summary>
		Double_Fft = 2,
		/// <summary></summary>
		Int32_Fft = 4,

		// Standard MDCT with a sample data type of float, double or int32_t,
		// respectively. For the float and int32 variants, the scale type is
		// 'float', while for the double variant, it's 'double'.
		// If scale is NULL, 1.0 will be used as a default.
		// 
		// Length is the frame size, not the window size (which is 2x frame).
		// For forward transforms, the stride specifies the spacing between each
		// sample in the output array in bytes. The input must be a flat array.
		// 
		// For inverse transforms, the stride specifies the spacing between each
		// sample in the input array in bytes. The output must be a flat array.
		// 
		// NOTE: the inverse transform is half-length, meaning the output will not
		// contain redundant data. This is what most codecs work with. To do a full
		// inverse transform, set the AV_TX_FULL_IMDCT flag on init

		/// <summary></summary>
		Float_Mdct = 1,
		/// <summary></summary>
		Double_Mdct = 3,
		/// <summary></summary>
		Int32_Mdct = 5,

		// Real to complex and complex to real DFTs.
		// For the float and int32 variants, the scale type is 'float', while for
		// the double variant, it's a 'double'. If scale is NULL, 1.0 will be used
		// as a default.
		// 
		// For forward transforms (R2C), stride must be the spacing between two
		// samples in bytes. For inverse transforms, the stride must be set
		// to the spacing between two complex values in bytes.
		// 
		// The forward transform performs a real-to-complex DFT of N samples to
		// N/2+1 complex values.
		// 
		// The inverse transform performs a complex-to-real DFT of N/2+1 complex
		// values to N real samples. The output is not normalized, but can be
		// made so by setting the scale value to 1.0/len.
		// NOTE: the inverse transform always overwrites the input

		/// <summary></summary>
		Float_Rdft = 6,
		/// <summary></summary>
		Double_Rdft = 7,
		/// <summary></summary>
		Int32_Rdft = 8,

		// Real to real (DCT) transforms.
		// 
		// The forward transform is a DCT-II.
		// The inverse transform is a DCT-III.
		// 
		// The input array is always overwritten. DCT-III requires that the
		// input be padded with 2 extra samples. Stride must be set to the
		// spacing between two samples in bytes

		/// <summary></summary>
		Float_Dct = 9,
		/// <summary></summary>
		Double_Dct = 10,
		/// <summary></summary>
		Int32_Dct = 11,

		// Discrete Cosine Transform I
		// 
		// The forward transform is a DCT-I.
		// The inverse transform is a DCT-I multiplied by 2/(N + 1).
		// 
		// The input array is always overwritten

		/// <summary></summary>
		Float_Dct_I = 12,
		/// <summary></summary>
		Double_Dct_I = 13,
		/// <summary></summary>
		Int32_Dct_I = 14,

		// Discrete Sine Transform I
		// 
		// The forward transform is a DST-I.
		// The inverse transform is a DST-I multiplied by 2/(N + 1).
		// 
		// The input array is always overwritten

		/// <summary></summary>
		Float_Dst_I = 15,
		/// <summary></summary>
		Double_Dst_I = 16,
		/// <summary></summary>
		Int32_Dst_I = 17,

		/// <summary>
		/// Not part of the API, do not use
		/// </summary>
		Nb,

		/// <summary>
		/// Special type to allow all types
		/// </summary>
		Any = int32_t.MaxValue
	}
}
