using UnityEngine;
using System.Collections;

public class MolColor {

	public Color rgba; 
	public float L; 
	public float a; 
	public float b; 
	public float X; 
	public float Y; 
	public float Z; 

	// D65 white point 
	static float Xw = 95.047f; 
	static float Yw = 100.000f; 
	static float Zw = 108.883f; 

	static double T1 = 0.008856; 
	static double T2 = 903.3; 
	
	public MolColor (Color c){
		// sRGB
		rgba = c;
		rgba.a = 1.0f; 

		// sRGB --> XYZ
		X = rgba.r * 0.4124f + rgba.g * 0.3576f + rgba.b * 0.1805f; 
		Y = rgba.r * 0.2126f + rgba.g * 0.7152f + rgba.b * 0.0722f; 
		Z = rgba.r * 0.0193f + rgba.g * 0.1192f + rgba.b * 0.9505f; 

		// XYZ --> L*a*b*
		float x_ = correctXYZ (X / Xw); 
		float y_ = correctXYZ (Y / Yw); 
		float z_ = correctXYZ (Z / Zw); 


		L = 116.0f * y_ - 16.0f;
		a = 500.0f * (x_ - y_); 
		b = 200.0f * (y_ - z_); 
	}

	public MolColor (float L, float a, float b){
		// L*a*b*
		this.L = L; 
		this.a = a; 
		this.b = b; 

		// L*a*b* --> XYZ
		float y_ = (L + 16.0f) / 116.0f; 
		float x_ = a / 500.0f + y_; 
		float z_ = y_ - b / 200.0f; 

		X = Xw * correctxz (x_); 
		Y = Yw * correctL (L); 
		Z = Zw * correctxz (z_); 

		rgba.r = X * 3.2406f + Y * -1.5372f + Z * -0.4986f; 
		rgba.g = X * -0.9689f + Y * 1.8758f + Z * 0.0415f; 
		rgba.b = X * 0.0557f + Y * -0.2040f + Z * 1.0570f; 
		rgba.a = 1.0f; 
	}

	private float correctXYZ(float s){
		if (s > T1) {
						return Mathf.Pow (s, 1 / 3); 
				}

		return (7.787f * s + 16.0f / 116.0f); 
	}


	private float correctxz(float s){
		float s3 = Mathf.Pow (s, 3); 
		if (s3 > T1) {
			s = s3; 
		} else {
			s = (s - 16.0f / 116.0f); 
		}
		return s / 7.787f; 
	}

	private float correctL(float s){
		if (s > T2 * T1) {
						return Mathf.Pow (((s + 16.0f) / 116.0f), 3.0f); 
				}
		return (float)(s / T2); 

	}
}
