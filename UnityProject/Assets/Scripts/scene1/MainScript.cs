using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System; 
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

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
	private bool setupGUI = true; 
	private bool countdown = false; 
	private bool intermediate = false; 
	private bool settingsGUI = false; 
	private bool stimulus = false; 
	private bool stimulusEnd = false; 
	private MolObject[] molObjects;
	private MolObject focusObject; 
	private GameObject collideBox;
	private List<Stimulus> stimuli = new List<Stimulus>();

	// isoluminent RGB tripels from http://www.cs.ubc.ca/~tmm/courses/infovis/morereadings/FaceLumin.pdf (Figure 7)
	public static Color[] molColors = {new Color (0.847f,0.057f,0.057f), new Color(0.000f,0.592f,0.000f), new Color(0.316f,0.316f,0.991f), new Color(0.527f,0.527f,0.00f), new Color(0.000f,0.559f,0.559f), new Color(0.718f,0.000f,0.718f)}; 
	public static Vector3 BoxSize = new Vector3();

	private string userID = ""; 
	private string conditionID = ""; 

	public int stimulusTimeout = 5000; 

	private Stopwatch stopWatch = new Stopwatch ();

	LogLib.Logger<int> distLogger; 
	LogLib.Logger<int> targetLogger; 

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
		var shuffle = (from stimulus in stimuli orderby Guid.NewGuid() select stimulus);
		stimuli = shuffle.ToList();
	}

	void OnGUI()
	{
		if(setupGUI){
			GUI.Window (1, new Rect(0.0f, 0.0f, Screen.width, Screen.height), SetupGUI, "Setup"); 
		}

		if(intermediate){
			GUI.Window (2, new Rect(0.0f, 0.0f, Screen.width, Screen.height), IntermediateGUI, "Intermediate"); 
		}

		if(countdown){
			GUI.Window (3, new Rect(0.0f, 0.0f, Screen.width, Screen.height), CountdownGUI, "Countdown"); 
		}

		if(stimulusEnd){
			GUI.Window (4, new Rect(0.0f, 0.0f, Screen.width, Screen.height), StimulusEndGUI, "Stimulus Finished: Click where you spotted the target or press 'n' if you did not see any target"); 
		}
	}

	Dictionary<string, string> tempSettings = new Dictionary<string, string>();

	void StimulusEndGUI(int windowID)
	{
		GUI.DrawTexture (new Rect (0.0f, 0.0f, Screen.width, Screen.height), (UnityEngine.Texture)Resources.Load ("stimulusEnd")); 
	}

	void CountdownGUI(int windowID)
	{
		int time = (int)stopWatch.ElapsedMilliseconds;
		string texName = "1"; 
		if(time < 2000) texName = "2";
		if(time < 1000) texName = "3";
		GUI.DrawTexture (new Rect (0.0f, 0.0f, Screen.width, Screen.height), (UnityEngine.Texture)Resources.Load (texName)); 

		if(time >= 3000){
			countdown = false; 
			stimulus = true; 

			// TODO: load stimulus

			stopWatch.Stop (); 
			stopWatch.Reset (); 
			stopWatch.Start (); 
		}
	}

	void IntermediateGUI(int windowID)
	{
		GUI.DrawTexture (new Rect (0.0f, 0.0f, Screen.width, Screen.height), (UnityEngine.Texture)Resources.Load ("intermediate")); 
	}

	void SetupGUI(int windowID)
	{
		float xOffset = Screen.width / 3.0f + Screen.width / 12.0f; 
		float yOffset = Screen.height / 3.0f; 
		const float elemWidth = 150.0f; 
		const float elemHeight = 20.0f; 

		GUI.DrawTexture (new Rect (0.0f, 0.0f, Screen.width, Screen.height), (UnityEngine.Texture)Resources.Load ("guiBG")); 

		GUI.Label (new Rect (xOffset, yOffset, elemWidth, elemHeight), "User ID: "); 
		userID = GUI.TextField (new Rect (xOffset + elemWidth, yOffset, elemWidth, elemHeight), userID); 

		GUI.Label (new Rect (xOffset, yOffset + elemHeight, elemWidth, elemHeight), "Condition: "); 
		conditionID = GUI.TextField (new Rect (xOffset + elemWidth, yOffset + elemHeight, elemWidth, elemHeight), conditionID); 

		if(GUI.Button (new Rect(xOffset + elemWidth, yOffset + 2 * elemHeight, elemWidth, elemHeight), "Start!"))
		{
			if(userID.Length > 0)
			{
				distLogger = CreateLogger("distance"); 
				targetLogger = CreateLogger ("target"); 
				setupGUI = false; 
				intermediate = true; 
			}
		}
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

//			LogLib.Logger<int> distLogger = new LogLib.Logger<int>("distance", "TODO:username", ""); 
//			distLogger.AddFactor("rep"); 
//			distLogger.AddFactor("ecc"); 
//			// TODO: add
//			// distLogger.AddFactor("dur"); 
//			// distLogger.AddFactor("amp"); 
//
//			// TODO: remove (just a test) 
//			distLogger.NewEntry(); 
//			distLogger.Log ("rep", "1"); 
//			distLogger.Log ("ecc", "F"); 
//			distLogger.Log (1); 
//
//			distLogger.NewEntry(); 
//			distLogger.Log ("rep", "1"); 
//			distLogger.Log ("ecc", "P"); 
//			distLogger.Log (2); 
//
//			distLogger.NewEntry(); 
//			distLogger.Log ("rep", "2"); 
//			distLogger.Log ("ecc", "P"); 
//			distLogger.Log (4); 
//
//			distLogger.NewEntry(); 
//			distLogger.Log ("rep", "2"); 
//			distLogger.Log ("ecc", "F"); 
//			distLogger.Log (3); 
//
//			// file will be written to UnityProject folder
//			const string fileName = "distanceTest.csv"; 
//			StreamWriter fileWriter = new StreamWriter(fileName, true); 
//			bool writeHeader = (new FileInfo(fileName).Length == 0); 
//			distLogger.WriteSingleRowCSV(fileWriter, writeHeader);
//			// TODO: end test
		}
	}

	LogLib.Logger<int> CreateLogger(String name)
	{
		LogLib.Logger<int> logger = new LogLib.Logger<int> (name, userID, conditionID); 
		// TODO: hardcoded for pilot1
		logger.AddFactor ("rep"); 
		logger.AddFactor ("ecc"); 
		logger.AddFactor ("dur"); 
		logger.AddFactor ("amp"); 
		return logger; 
	}

	void FiniLogger(LogLib.Logger<int> logger, String name)
	{
		// TODO: this method needs to be called before shutting down
		string fileName = name + ".csv"; 
		StreamWriter fileWriter = new StreamWriter (fileName, true); 
		bool writeHeader = (new FileInfo(fileName).Length == 0); 
		logger.WriteSingleRowCSV(fileWriter, writeHeader);
	}

	void StopStimulus()
	{
		stopWatch.Stop(); 
		stopWatch.Reset (); 
		
		// TODO: freeze stimulus 
		
		stimulus = false; 
		stimulusEnd = true; 

		Screen.showCursor = true; 
	}

	void Update () 
	{
		Camera.main.orthographicSize = Screen.height * 0.5f;
		
		BoxSize.x = Screen.width;
		BoxSize.y = Screen.height;
		BoxSize.z = Settings.Values.molScale * 2;
		
		collideBox.transform.localScale = new Vector3(BoxSize.x, BoxSize.y, BoxSize.z);

		if(stimulus)
		{
			int time = (int)stopWatch.ElapsedMilliseconds; 
			if(time >= stimulusTimeout || Input.GetKeyDown("space"))
			{
				StopStimulus(); 
			}
		}

		if(stimulusEnd)
		{
			Vector3 mousePos = new Vector3(-1.0f, -1.0f, -1.0f); 
			bool noTarget = false; 
			if(Input.GetMouseButtonUp(0)){
				mousePos = Input.mousePosition; 
				stimulusEnd = false; 
			}
			if(Input.GetKeyDown ("n")){
				noTarget = true; 
				stimulusEnd = false; 
			}
			if(!stimulusEnd){
				// TODO: log mouse position, noTarget value, distance mouse position -- last known center of target
				// TODO: check if there are any more stimuli, otherwise close the program
				intermediate = true; 			}

		}

		if (Input.GetKeyDown ("escape"))
		{
			Application.Quit();
		}

		if(Input.GetKeyDown ("return"))
		{
			if(intermediate)
			{
				countdown = true; 
				stopWatch.Reset(); 
				stopWatch.Start (); 
				intermediate = false; 
				Screen.showCursor = false; 
			}
		}

		if (Input.GetKeyDown ("s") && Input.GetKey(KeyCode.LeftShift))
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