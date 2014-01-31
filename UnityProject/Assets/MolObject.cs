using UnityEngine;
using System.Diagnostics;

using System;

public enum BlinkingType
{
	EQUAL,
	ADD,
	SUB}
;

public enum InterpolationType
{
	LINEAR,
	EASE_IN,
	EASE_OUT,
	EASE_INOUT}
;

public class MolObject : MonoBehaviour
{
	public bool animate = true;
	public bool flicker = false;
	public bool luminanceFlicker = false;
	public BlinkingType blinkingType = BlinkingType.EQUAL;
	public InterpolationType interpolationType = InterpolationType.LINEAR;

	// Luminance-modulation properties
	public int cycleLength = 30;
	public int LAmplitude1 = 5;
	public int LAmplitude2 = 1;
	public int LOffset = 0;
	public int flickerLength = 1000;
	public int easeOutTime = 200; 

	//private bool toggle = false;	
	//private int frameCount = 0;

	private Stopwatch stopWatch = new Stopwatch ();
	private Stopwatch stopWatch_1 = new Stopwatch ();

	static float drag = 5;
	static float randomForce = 100;

	public MolColor defaultColor = new MolColor (Color.white);
	public MolColor currentColor; 
	public MolColor flickerHiColor; 
	public MolColor flickerLoColor; 

	private int cycleCounter = 0; 
	private int currentAmplitude = 0; 

//	private Color normalColor = Color.white;
//	private Color highlightedColor = Color.black;

	public float Frequency { get; set; }

	public static MolObject CreateNewMolObject (Transform parent, string name, Vector3 boxSize, MolColor color)
	{
		var position = new Vector3 ((UnityEngine.Random.value - 0.5f) * boxSize.x, (UnityEngine.Random.value - 0.5f) * boxSize.y, (UnityEngine.Random.value - 0.5f) * boxSize.z);

		var molGameObject = Instantiate (Resources.Load ("MolPrefab"), position, Quaternion.identity) as GameObject;

		if (molGameObject != null) {
			molGameObject.name = name;
			molGameObject.transform.parent = parent;

			var molObject = molGameObject.GetComponent<MolObject> ();

			molObject.rigidbody.angularDrag = drag;
			molObject.defaultColor = color; 
			molObject.currentColor = color; 
			molObject.flickerHiColor = null; 
			molObject.flickerLoColor = null; 

			molGameObject.GetComponent<MeshRenderer> ().material.color = color.rgba;

			return molObject;
		}
		return null;
	}

	void Start ()
	{

	}

	void FixedUpdate ()
	{
		if (!animate)
			return;

		rigidbody.drag = drag;
		rigidbody.AddForce (UnityEngine.Random.insideUnitSphere * randomForce);
	}

	public void StartFlicker ()
	{
		flicker = true;
		//animate = false;

		stopWatch_1.Reset ();
		stopWatch_1.Start ();
	}

	public void StopFlicker ()
	{
		flicker = false;
		//animate = true;

		gameObject.GetComponent<MeshRenderer> ().material.color = Color.white;
	}

	public void StartLuminanceFlicker()
	{
		luminanceFlicker = true; 
		cycleCounter = 0; 
		if (LOffset != 0) {
			float L = currentColor.L + (float)(LOffset); 
			if(L > 100.0f){
				L = currentColor.L - (float)(LOffset); 
			}
			MolColor focusColor = new MolColor(L, currentColor.a, currentColor.b); 
			currentColor = focusColor; 
		}
		currentAmplitude = LAmplitude1; 
		SetFlickerColors (); 

		stopWatch_1.Reset (); 
		stopWatch_1.Start (); 
	}

	public void StopLuminanceFlicker()
	{
		luminanceFlicker = false; 
		currentColor = defaultColor; 
		flickerHiColor = null; 
		flickerLoColor = null; 
	}

	private void SetFlickerColors()
	{
		// flicker around set luminance with given amplitude
		float hiL = currentColor.L + (float)(currentAmplitude) * 0.5f; 
		float loL = currentColor.L - (float)(currentAmplitude) * 0.5f; 

		// clamp luminances to [0...100]
		float clamp = 100.0f - hiL; 
		if (clamp < 0.0f) {
			hiL += clamp; 
			loL += clamp; 
		} else {
			if(loL < 0.0f){
				hiL -= loL; 
				loL = 0.0f; 
			}
		}

		flickerHiColor = new MolColor (hiL, currentColor.a, currentColor.b); 
		flickerLoColor = new MolColor (loL, currentColor.a, currentColor.b); 
	}

	public float StartFrequency = 5.0f;
	public float StopFrequency = 30.0f;
	public float Duration = 2.0f;

	public MolObject ()
	{
		Frequency = 0.0f;
	}
	
	void Update ()
	{
		//gameObject.GetComponent<MeshRenderer> ().material.color = currentColor.rgba; 

		if (flicker) {
			flickerUpdate(); 
		}

		if (luminanceFlicker) {
			luminanceFlickerUpdate (); 
		}
	}

	void luminanceFlickerUpdate()
	{
		int t = (int)stopWatch_1.ElapsedMilliseconds; 

		int amp = LAmplitude1; 
		if (t > flickerLength) {
			// TODO: ease-out 
			amp = LAmplitude2; 
		}

		// flicker if: still in initial flicker phase or if target continuous to flicker
		if (amp > 0) {

			// adjust luminance to new amplitude
			if(amp != currentAmplitude){
				currentAmplitude = amp; 
				SetFlickerColors(); 
			}

			int prevCycleCounter = cycleCounter; 
			cycleCounter = t % cycleLength; 

			// a new cycle has started
			if (cycleCounter < prevCycleCounter) {
				bool hi = ((t / cycleLength) % 2) == 0; 

				// new amp is high or low
				if(hi){
					gameObject.GetComponent<MeshRenderer> ().material.color = flickerHiColor.rgba; 
				}
				else{
					gameObject.GetComponent<MeshRenderer> ().material.color = flickerLoColor.rgba; 
				}
			}
		}
	}

	void flickerUpdate ()
	{
		var t = (float)stopWatch_1.Elapsed.TotalSeconds;
		var p = t / Duration;
		var f = 0.0;
		
		switch (interpolationType) {
		case InterpolationType.LINEAR:
			f = linearTween (t, StartFrequency, StopFrequency, Duration);
			break;
		case InterpolationType.EASE_IN:
			f = easeInQuad (t, StartFrequency, StopFrequency, Duration);
			break;
		case InterpolationType.EASE_OUT:
			f = easeOutQuad (t, StartFrequency, StopFrequency, Duration);
			break;
		case InterpolationType.EASE_INOUT:
			f = easeInOutQuad (t, StartFrequency, StopFrequency, Duration);
			break;
		}
		
		if (!stopWatch.IsRunning)
			stopWatch.Start ();
		
		switch (blinkingType) {
		case BlinkingType.ADD:
		{					
			if (stopWatch.Elapsed.TotalSeconds >= ((double)1.0f / f)) {
				gameObject.GetComponent<MeshRenderer> ().material.color = Color.white;
				
				stopWatch.Reset ();
				stopWatch.Start ();
			} else {
				gameObject.GetComponent<MeshRenderer> ().material.color = new Color (p, p, p, 1);
			}
		}
			break;
			
		case BlinkingType.SUB:
		{
			if (stopWatch.Elapsed.TotalSeconds >= ((double)1.0f / f)) {						
				gameObject.GetComponent<MeshRenderer> ().material.color = new Color (p, p, p, 1);
				
				stopWatch.Reset ();
				stopWatch.Start ();
			} else {
				gameObject.GetComponent<MeshRenderer> ().material.color = Color.white;
			}
		}
			break;
			
		case BlinkingType.EQUAL:
		{
			if (stopWatch.Elapsed.TotalSeconds >= ((double)1.0f / f)) {
				if (gameObject.GetComponent<MeshRenderer> ().material.color == Color.white)
					gameObject.GetComponent<MeshRenderer> ().material.color = new Color (p, p, p, 1);
				else
					gameObject.GetComponent<MeshRenderer> ().material.color = Color.white;
				
				stopWatch.Reset ();
				stopWatch.Start ();
			}
		}
			break;
		}
		
		if (stopWatch_1.Elapsed.TotalSeconds >= Duration) {
			StopFlicker ();
		}
	}

	float linearTween (float t, float b, float c, float d)
	{
		float delta = c - b; 
		
		return delta * t / d + b;
	}

	float easeInQuad (float t, float b, float c, float d)
	{
		float delta = c - b; 
		
		return delta * (float)Math.Pow (2.0f, 10.0f * (t / d - 1.0f)) + b;
	}

	float easeInQuad_n (float t, float b, float c, float d)
	{
		float delta = c - b; 
		
		t /= d;
		return delta * t * t + b;
	}

	float easeOutQuad (float t, float b, float c, float d)
	{
		float delta = c - b; 
		
		t /= d;
		return -delta * t * (t - 2) + b;
	}

	float easeInOutQuad (float t, float b, float c, float d)
	{
		float delta = c - b; 
		
		t /= d / 2;
		if (t < 1)
			return delta / 2 * t * t + b;
		t--;
		return -delta / 2 * (t * (t - 2) - 1) + b;
	}
}