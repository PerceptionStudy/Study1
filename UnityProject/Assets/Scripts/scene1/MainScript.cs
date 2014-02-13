using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System; 
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class Stimulus
{
	int stimulusId;
	int focusRegion;
	int repeatId;
	int duration;
	int amplitude;

	public Stimulus(int stimulusId, int focusRegion, int repeatId, int duration, int amplitude)
	{
		this.stimulusId = stimulusId;
		this.focusRegion = focusRegion;
		this.repeatId = repeatId;
		this.duration = duration;
		this.amplitude = amplitude;
	}
}

public class MainScript : MonoBehaviour 
{
	private bool initiated = false;

	private MolObject focusObject; 	
	private GameObject collideBox;

	// isoluminent RGB tripels from http://www.cs.ubc.ca/~tmm/courses/infovis/morereadings/FaceLumin.pdf (Figure 7)
	public static Color[] molColors = {new Color (0.847f,0.057f,0.057f), new Color(0.000f,0.592f,0.000f), new Color(0.316f,0.316f,0.991f), new Color(0.527f,0.527f,0.00f), new Color(0.000f,0.559f,0.559f), new Color(0.718f,0.000f,0.718f)}; 
	public static Vector3 BoxSize = new Vector3();

	private MolObject[] molObjects;
	private List<Stimulus> stimuli = new List<Stimulus>();

	public void CreateMolObjects()
	{
		molObjects = new MolObject[(int)Settings.Values.molCount];		

		for(int i = 0; i< (int)Settings.Values.molCount; i++)
			molObjects[i] = MolObject.CreateNewMolObject(gameObject.transform, "molObject_" + i, new MolColor(molColors[UnityEngine.Random.Range(0, molColors.Count ())]));	

		initiated = true;
		focusObject = null; 
	}

	void LoadStimuli ()
	{
		int[] durationValues = {(int)Settings.Values.duration_1, (int)Settings.Values.duration_2, (int)Settings.Values.duration_3, (int)Settings.Values.duration_4, (int)Settings.Values.duration_5};
		int[] amplitudeValues = {(int)Settings.Values.amplitude_1, (int)Settings.Values.amplitude_2, (int)Settings.Values.amplitude_3, (int)Settings.Values.amplitude_4, (int)Settings.Values.amplitude_5};

		int count = 0;
		for(int i = 0; i < 2; i++)
		{
			for(int j = 0; j < Settings.Values.repeat; j++)
			{
				for(int k = 0; k < durationValues.Count(); k++)
				{
					for(int l = 0; l < amplitudeValues.Count(); l++)
					{
						Stimulus stimulus = new Stimulus(count, i, j, durationValues[k], amplitudeValues[l]);
						stimuli.Add(stimulus);

						count ++;
					}
				}
			}
		}

		var shuffle = (from stimulus in stimuli orderby  Guid.NewGuid() select stimulus);
		stimuli = shuffle.ToList();
	}

	void Start () 
	{
		if (!initiated) 
		{
			BoxSize.x = Screen.width;
			BoxSize.y = Screen.height;
			BoxSize.z = Settings.Values.molScale * 2;
			
			collideBox = GameObject.Find("Box Object");
			collideBox.transform.localScale = new Vector3(BoxSize.x, BoxSize.y, BoxSize.z);
			
			Settings.LoadSettings();

			LoadStimuli();

			CreateMolObjects();

			LogLib.Logger<int> distLogger = new LogLib.Logger<int>("distance", "TODO:username", ""); 
			distLogger.AddFactor("rep"); 
			distLogger.AddFactor("ecc"); 
			// TODO: add
			// distLogger.AddFactor("dur"); 
			// distLogger.AddFactor("amp"); 

			// TODO: remove (just a test) 
			distLogger.NewEntry(); 
			distLogger.Log ("rep", "1"); 
			distLogger.Log ("ecc", "F"); 
			distLogger.Log (1); 

			distLogger.NewEntry(); 
			distLogger.Log ("rep", "1"); 
			distLogger.Log ("ecc", "P"); 
			distLogger.Log (2); 

			distLogger.NewEntry(); 
			distLogger.Log ("rep", "2"); 
			distLogger.Log ("ecc", "P"); 
			distLogger.Log (4); 

			distLogger.NewEntry(); 
			distLogger.Log ("rep", "2"); 
			distLogger.Log ("ecc", "F"); 
			distLogger.Log (3); 

			// file will be written to UnityProject folder
			const string fileName = "distanceTest.csv"; 
			StreamWriter fileWriter = new StreamWriter(fileName, true); 
			bool writeHeader = (new FileInfo(fileName).Length == 0); 
			distLogger.WriteSingleRowCSV(fileWriter, writeHeader);
			// TODO: end test
		}
	}

	void Update () 
	{
		Camera.main.orthographicSize = Screen.height * 0.5f;
		
		BoxSize.x = Screen.width;
		BoxSize.y = Screen.height;
		BoxSize.z = Settings.Values.molScale * 2;
		
		collideBox.transform.localScale = new Vector3(BoxSize.x, BoxSize.y, BoxSize.z);

		if (Input.GetKeyDown ("escape"))
		{
			Application.Quit();
		}

		if (Input.GetKeyDown ("f")) 
		{
			InitLuminanceFlicker (GetRandomMolObject ());
		}
	}

	MolObject GetRandomMolObject()
	{
		return molObjects [UnityEngine.Random.Range (0, molObjects.Count () - 1)]; 
	}

	void InitLuminanceFlicker(MolObject molObject)
	{
		if (focusObject != null) 
		{
			focusObject.StopLuminanceFlicker (); 
		}

		molObject.StartLuminanceFlicker (); 
		focusObject = molObject; 
	}

	void FixedUpdate()
	{

	}
}