using UnityEngine;
using System;
using System.Diagnostics;

public class MolObject : MonoBehaviour
{
	public bool animate = true;
	public bool luminanceFlicker = false;

	private MolColor defaultColor;
	private MolColor currentColor; 

	private Stopwatch stopWatch = new Stopwatch ();
	private Stopwatch stopWatch_2 = new Stopwatch ();

	public static MolObject CreateNewMolObject (Transform parent, string name, MolColor color)
	{
		var position = new Vector3 ((UnityEngine.Random.value - 0.5f) * Settings.Values.boxSizeX, (UnityEngine.Random.value - 0.5f) * Settings.Values.boxSizeY, (UnityEngine.Random.value - 0.5f) * Settings.Values.boxSizeZ);
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
		rigidbody.AddForce (UnityEngine.Random.insideUnitSphere * Settings.Values.randomForce);
	}

	void Update ()
	{
		if (luminanceFlicker)
		{
			LuminanceFlickerUpdate (); 
		}

		transform.localScale = new Vector3(Settings.Values.molScale, Settings.Values.molScale, Settings.Values.molScale);

		Vector3 temp = rigidbody.position;
		
		if(rigidbody.position.x > transform.parent.position.x + Settings.Values.boxSizeX * 0.5f)
		{
			temp.x = transform.parent.position.x + Settings.Values.boxSizeX * 0.5f - gameObject.GetComponent<SphereCollider>().radius * 2; 
		}
		else if(rigidbody.position.x < transform.parent.position.x - Settings.Values.boxSizeX * 0.5f)
		{
			temp.x = transform.parent.position.x - Settings.Values.boxSizeX * 0.5f + gameObject.GetComponent<SphereCollider>().radius * 2; 
		}
		
		if(rigidbody.position.y > transform.parent.position.y + Settings.Values.boxSizeY * 0.5f)
		{
			temp.y = transform.parent.position.y + Settings.Values.boxSizeY * 0.5f - gameObject.GetComponent<SphereCollider>().radius * 2; 
		}		
		else if(rigidbody.position.y < transform.parent.position.y - Settings.Values.boxSizeY * 0.5f)
		{
			temp.y = transform.parent.position.y - Settings.Values.boxSizeY * 0.5f + gameObject.GetComponent<SphereCollider>().radius * 2; 
		}
		
		if(rigidbody.position.z > transform.parent.position.z + Settings.Values.boxSizeZ * 0.5f)
		{
			temp.z = transform.parent.position.z + Settings.Values.boxSizeZ * 0.5f - gameObject.GetComponent<SphereCollider>().radius * 2; 
		}		
		else if(rigidbody.position.z < transform.parent.position.z - Settings.Values.boxSizeZ * 0.5f)
		{
			temp.z = transform.parent.position.z - Settings.Values.boxSizeZ * 0.5f + gameObject.GetComponent<SphereCollider>().radius * 2; 
		}
		
		rigidbody.position = temp; 
	}

	Color ShiftColorIntensity (MolColor defaultColor, float intensity)
	{
		throw new NotImplementedException ();
	}

	private bool up = true;

	void LuminanceFlickerUpdate()
	{		
		if(!stopWatch_2.IsRunning) stopWatch_2.Start();

		int currentTimeMillis = (int)stopWatch.ElapsedMilliseconds;
		float progress_1 = Mathf.Clamp((float)currentTimeMillis / Settings.Values.interpolationDuration, 0.0f, 1.0f);
		
		float currentHalfWaveLength = Settings.Values.startHalfWaveLength + (Settings.Values.endHalfWaveLength - Settings.Values.startHalfWaveLength) * progress_1;
		float currentAmplitude = Settings.Values.startAmplitude + (Settings.Values.endAmplitude - Settings.Values.startAmplitude) * progress_1;		
		
		int currentWaveTime = (int)stopWatch_2.ElapsedMilliseconds;
		float progress_2 = (float) currentWaveTime / currentHalfWaveLength;	

		float intensityShift = Mathf.Clamp((up) ? progress_2 * 2.0f - 1.0f : (1.0f-progress_2) * 2.0f - 1.0f, -1.0f, 1.0f) * currentAmplitude;
		float currentIntensity = Mathf.Clamp(intensityShift + Settings.Values.amplitudeOffset + defaultColor.L, 0, 100);		

		currentColor = new MolColor(currentIntensity, defaultColor.a, defaultColor.b);
		gameObject.GetComponent<MeshRenderer> ().material.color = currentColor.rgba;

		if(currentWaveTime > currentHalfWaveLength)
		{
			up = !up;
			stopWatch_2.Reset();
			stopWatch_2.Start();
		}

		if(currentTimeMillis > Settings.Values.totalDuration)
		{
			StopLuminanceFlicker();
		}
	}

	public void StartLuminanceFlicker()
	{
		luminanceFlicker = true; 

		stopWatch.Start (); 
	}

	public void StopLuminanceFlicker()
	{
		luminanceFlicker = false; 
		currentColor = defaultColor; 
		gameObject.GetComponent<MeshRenderer> ().material.color = currentColor.rgba; 

		stopWatch.Reset (); 
	}
}