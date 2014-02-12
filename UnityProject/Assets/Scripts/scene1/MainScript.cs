using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System.IO; 

public class MainScript : MonoBehaviour 
{
	public int count = 0;

	public string modeText = ""; 
	public string parameterValueText = ""; 
	public int stepWidth = 10; 
	public int currParameterValue = 0; 

	public delegate void AdjustParamterDelegate(int step); 
	public AdjustParamterDelegate adjustParamFunc = null;  

	private bool initiated = false;
	private MolObject[] molObjects;
	private MolObject focusObject; 

	//static Color[] molColors = {Color.blue, Color.red, Color.yellow, Color.green, Color.cyan, Color.magenta}; 
	// isoluminent RGB tripels from http://www.cs.ubc.ca/~tmm/courses/infovis/morereadings/FaceLumin.pdf (Figure 7)
	static Color[] molColors = {new Color (0.847f,0.057f,0.057f), new Color(0.000f,0.592f,0.000f), new Color(0.316f,0.316f,0.991f), new Color(0.527f,0.527f,0.00f), new Color(0.000f,0.559f,0.559f), new Color(0.718f,0.000f,0.718f)}; 
	//static Color[] molColors = {Color.blue, Color.green, Color.cyan}; 
	//static Color[] molColors = {Color.magenta}; 

	public void CreateMolObjects()
	{

		molObjects = new MolObject[count];		

		for(int i = 0; i< count; i++)
			molObjects[i] = MolObject.CreateNewMolObject(gameObject.transform, "molObject_" + i, new MolColor(molColors[UnityEngine.Random.Range(0, molColors.Count ())]));	

		initiated = true;

		focusObject = null; 
	}

	private Rect windowRect = new Rect(20, 20, 225, 0);
	private const int guiTopOffset = 20;
	private const int guiDownOffset = 10;
	private const int guiIncrement = 15;
	private GUIStyle style = new GUIStyle();

	void OnGUI() 
	{
		windowRect = GUI.Window(0, windowRect, DoMyWindow, "My Window");
	}

	Dictionary<string, string> tempSettings = new Dictionary<string, string>();

	void DoMyWindow(int windowID) 
	{
		style.fontSize = 12;
		style.normal.textColor = Color.white;

		if(tempSettings.Count() == 0)
		{
			tempSettings = Settings.GetDictionarySettings();
		}

		int count = 0;
		char[] alphabet = Settings.Instance.alphabet.ToCharArray();
		Dictionary<string, string> temp = new Dictionary<string, string>(tempSettings);

		foreach( KeyValuePair<string, string> kvp in temp )
		{
			GUI.Label(new Rect(10, guiTopOffset + count * guiIncrement, 150, 30), "("+ alphabet[count].ToString() + ")  " + kvp.Key + ": ", style);
			string stringValue = GUI.TextField(new Rect(175, guiTopOffset + count * guiIncrement, 50, 20), kvp.Value.ToString(), style);
			stringValue = Regex.Replace(stringValue, @"[^0-9.]", "");

			if(stringValue != kvp.Value)
			{
				tempSettings[kvp.Key] = stringValue;
			}

			if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)		
			{				
				float tryParse = 0.0f;

				if(float.TryParse(stringValue, out tryParse))
				{
					tempSettings[kvp.Key] = tryParse.ToString();
				}
				else
				{
					Debug.Log("Input field parsing failed");
					tempSettings[kvp.Key] = "0";
				}
			}

			count ++;
		}

		if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)		
		{				
			Debug.Log("Applying and Saving Settings");
			
			string json = JsonConvert.SerializeObject(tempSettings);
			SettingsValues v = JsonConvert.DeserializeObject<SettingsValues>(json);
			Settings.Values = (SettingsValues)v.Clone();
			Settings.SaveSettings();
		}
		
		windowRect.height = guiTopOffset + guiDownOffset + count * guiIncrement;
		
		GUI.DragWindow(new Rect(0, 0, 10000, 10000));
	}

	GameObject box;

	void Start () 
	{
		if (!initiated) 
		{
			box = GameObject.Find("Box Object");

			Settings.LoadSettings();
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
		Camera.main.orthographicSize = Settings.Values.cameraSize;

		box.transform.localScale = new Vector3(Settings.Values.boxSizeX, Settings.Values.boxSizeY, Settings.Values.boxSizeZ);

		if (Input.GetKeyDown ("escape"))
		{
			if(modeText.Length == 0)
			{
				Application.Quit();
			}
			else
			{
				modeText = ""; 
				parameterValueText = ""; 
				adjustParamFunc = null; 
			}
		}

		if (Input.GetKeyDown ("s") && Input.GetKey(KeyCode.LeftShift))
		{
			Settings.SaveSettings();
		}

		if (Input.GetKeyDown ("l") && Input.GetKey(KeyCode.LeftShift))
		{
			Settings.LoadSettings();
		}

		if (Input.GetKeyDown ("f")) {
			InitLuminanceFlicker (GetRandomMolObject ());
		}
	}

	void setCurrentParameter(string name, int step, AdjustParamterDelegate func)
	{
		modeText = name; 
		stepWidth = step; 
		adjustParamFunc = func; 
		updateParameter (0); 
	}

	void updateParameter (int step)
	{
		if(adjustParamFunc != null){
			adjustParamFunc(step);
			parameterValueText = "" + currParameterValue; 
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