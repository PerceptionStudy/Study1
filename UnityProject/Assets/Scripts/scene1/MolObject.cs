using UnityEngine;
using System;
using System.Diagnostics;

public class MolObject : MonoBehaviour
{
	public bool animate = true;
	public bool luminanceFlicker = false;

	private MolColor defaultColor;
	private MolColor currentColor; 

	private Stopwatch stimulusWatch = new Stopwatch ();
	private Stopwatch waveWatch = new Stopwatch ();

	public static MolObject CreateNewMolObject (Transform parent, string name, MolColor color)
	{
		var position = new Vector3 ((UnityEngine.Random.value - 0.5f) * MainScript.BoxSize.x, (UnityEngine.Random.value - 0.5f) * MainScript.BoxSize.y, (UnityEngine.Random.value - 0.5f) * MainScript.BoxSize.z);
		var molGameObject = Instantiate (Resources.Load ("MolPrefab"), position, Quaternion.identity) as GameObject;

		if (molGameObject != null) 
		{
			molGameObject.name = name;
			molGameObject.transform.parent = parent;

			var molObject = molGameObject.GetComponent<MolObject> ();

			molObject.defaultColor = color; 
			molObject.currentColor = color; 

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

		rigidbody.drag = Settings.Values.drag;

		Vector3 force = UnityEngine.Random.insideUnitSphere * Settings.Values.randomForce;
		force.z = 0;

		rigidbody.AddForce (force);
	}

	void Update ()
	{
		if(stimulus) StimulusUpdate();

		transform.localScale = new Vector3(Settings.Values.molScale, Settings.Values.molScale, Settings.Values.molScale);

		Vector3 temp = rigidbody.position;
		
		if(rigidbody.position.x > transform.parent.position.x + MainScript.BoxSize.x * 0.5f)
		{
			temp.x = transform.parent.position.x + MainScript.BoxSize.x * 0.5f - gameObject.GetComponent<SphereCollider>().radius * 2; 
		}
		else if(rigidbody.position.x < transform.parent.position.x - MainScript.BoxSize.x * 0.5f)
		{
			temp.x = transform.parent.position.x - MainScript.BoxSize.x * 0.5f + gameObject.GetComponent<SphereCollider>().radius * 2; 
		}
		
		if(rigidbody.position.y > transform.parent.position.y + MainScript.BoxSize.y * 0.5f)
		{
			temp.y = transform.parent.position.y + MainScript.BoxSize.y * 0.5f - gameObject.GetComponent<SphereCollider>().radius * 2; 
		}		
		else if(rigidbody.position.y < transform.parent.position.y - MainScript.BoxSize.y * 0.5f)
		{
			temp.y = transform.parent.position.y - MainScript.BoxSize.y * 0.5f + gameObject.GetComponent<SphereCollider>().radius * 2; 
		}

		temp.z = 0;
		
//		if(rigidbody.position.z > transform.parent.position.z + MainScript.BoxSize.z * 0.5f)
//		{
//			temp.z = transform.parent.position.z + MainScript.BoxSize.z * 0.5f - gameObject.GetComponent<SphereCollider>().radius * 2; 
//		}		
//		else if(rigidbody.position.z < transform.parent.position.z - MainScript.BoxSize.z * 0.5f)
//		{
//			temp.z = transform.parent.position.z - MainScript.BoxSize.z * 0.5f + gameObject.GetComponent<SphereCollider>().radius * 2; 
//		}
		
		rigidbody.position = temp; 
	}

	int halfWaveLength = 0;
	int amplitude = 0;
	int duration = 0;
		
	bool firstWave = false;
	bool up = true;
	public bool stimulus = false;

	public void StartStimulus(int waveLength, int amplitude, int duration)
	{
		stimulus = true;
		firstWave = true;

		this.halfWaveLength = waveLength / 2;
		this.amplitude = amplitude;
		this.duration = duration;

		stimulusWatch.Start();
		waveWatch.Start();
	}

	public void StopStimulus()
	{
		stimulus = false; 
		currentColor = defaultColor; 
		gameObject.GetComponent<MeshRenderer> ().material.color = currentColor.rgba; 

		stimulusWatch.Stop();
		stimulusWatch.Reset();

		waveWatch.Stop();
		waveWatch.Reset();
	}

	private void StimulusUpdate()
	{
		int currentStimulusTime = (int)stimulusWatch.ElapsedMilliseconds;
		int currentWaveTime = (int)waveWatch.ElapsedMilliseconds;

		if(firstWave)
		{
			currentWaveTime += halfWaveLength / 2;
		}

		float progress = (float) currentWaveTime / halfWaveLength;	
		float intensityShift = Mathf.Clamp((up) ? progress * 2.0f - 1.0f : (1.0f-progress) * 2.0f - 1.0f, -1.0f, 1.0f) * amplitude;
		float currentIntensity = Mathf.Clamp(defaultColor.L + intensityShift, 0, 100);		

		currentColor = new MolColor(currentIntensity, defaultColor.a, defaultColor.b);
		gameObject.GetComponent<MeshRenderer> ().material.color = currentColor.rgba;

		if(currentWaveTime > halfWaveLength)
		{
			up = !up;
			waveWatch.Reset();
			waveWatch.Start();
			firstWave = false;
		}

		if(currentStimulusTime > duration)
		{
			StopStimulus();
		}
	}

	public void StartLuminanceFlicker()
	{
//		luminanceFlicker = true; 
//
//		stopWatch.Start (); 
	}

	public void StopLuminanceFlicker()
	{
//		luminanceFlicker = false; 
//		currentColor = defaultColor; 
//		gameObject.GetComponent<MeshRenderer> ().material.color = currentColor.rgba; 
//
//		stopWatch.Reset (); 
	}

	
	void LuminanceFlickerUpdate()
	{		
//		if(!stopWatch_2.IsRunning) stopWatch_2.Start();
//
//		int currentTimeMillis = (int)stopWatch.ElapsedMilliseconds;
//		float progress_1 = Mathf.Clamp((float)currentTimeMillis / Settings.Values.interpolationDuration, 0.0f, 1.0f);
//		
//		float currentHalfWaveLength = Settings.Values.startHalfWaveLength + (Settings.Values.endHalfWaveLength - Settings.Values.startHalfWaveLength) * progress_1;
//		float currentAmplitude = Settings.Values.startAmplitude + (Settings.Values.endAmplitude - Settings.Values.startAmplitude) * progress_1;		
//		
//		int currentWaveTime = (int)stopWatch_2.ElapsedMilliseconds;
//		float progress_2 = (float) currentWaveTime / currentHalfWaveLength;	
//
//		float intensityShift = Mathf.Clamp((up) ? progress_2 * 2.0f - 1.0f : (1.0f-progress_2) * 2.0f - 1.0f, -1.0f, 1.0f) * currentAmplitude;
//		float currentIntensity = Mathf.Clamp(intensityShift + Settings.Values.amplitudeOffset + defaultColor.L, 0, 100);		
//
//		currentColor = new MolColor(currentIntensity, defaultColor.a, defaultColor.b);
//		gameObject.GetComponent<MeshRenderer> ().material.color = currentColor.rgba;
//
//		if(currentWaveTime > currentHalfWaveLength)
//		{
//			up = !up;
//			stopWatch_2.Reset();
//			stopWatch_2.Start();
//		}
//
//		if(currentTimeMillis > Settings.Values.totalDuration)
//		{
//			StopLuminanceFlicker();
//		}
	}
}