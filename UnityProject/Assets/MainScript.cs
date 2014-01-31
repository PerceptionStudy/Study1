using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Linq;

public class MainScript : MonoBehaviour 
{
	public int count = 100;
	public Vector3 boxSize = new Vector3(0.0f, 0.0f, 0.0f);

	public string modeText = ""; 
	public string parameterValueText = ""; 
	public int stepWidth = 10; 
	public int currParameterValue = 0; 

	public delegate void AdjustParamterDelegate(int step); 
	public AdjustParamterDelegate adjustParamFunc = null;  

	public BlinkingType blinkingType = BlinkingType.EQUAL;
	public InterpolationType interpolationType = InterpolationType.LINEAR;

	private bool initiated = false;
	private MolObject[] molObjects;
	private MolObject focusObject; 

	//static Color[] molColors = {Color.blue, Color.red, Color.yellow, Color.green, Color.cyan, Color.magenta}; 
	// isoluminent RGB tripels from http://www.cs.ubc.ca/~tmm/courses/infovis/morereadings/FaceLumin.pdf (Figure 7)
	static Color[] molColors = {new Color (0.847f,0.057f,0.057f), new Color(0.000f,0.592f,0.000f), new Color(0.316f,0.316f,0.991f), new Color(0.527f,0.527f,0.00f), new Color(0.000f,0.559f,0.559f), new Color(0.718f,0.000f,0.718f)}; 
	//static Color[] molColors = {Color.blue, Color.green, Color.cyan}; 
	//static Color[] molColors = {Color.magenta}; 

	// adjust parameters

	public void AdjustCycleLength(int step){
		foreach (MolObject molObject in molObjects) {
			molObject.cycleLength += step; 
			currParameterValue = molObject.cycleLength; 
		}
	}

	public void AdjustFlickerAmplitude(int step){
		foreach (MolObject molObject in molObjects) {
			molObject.LAmplitude1 += step; 
			currParameterValue = molObject.LAmplitude1; 
		}
	}

	public void AdjustFocusAmplitude(int step){
		foreach (MolObject molObject in molObjects) {
			molObject.LAmplitude2 += step; 
			currParameterValue = molObject.LAmplitude2; 
		}
	}

	public void AdjustFocusOffset(int step){
		foreach (MolObject molObject in molObjects) {
			molObject.LOffset += step; 
			currParameterValue = molObject.LOffset; 
		}
	}

	public void AdjustFlickerDuration(int step){
		foreach (MolObject molObject in molObjects) {
			molObject.flickerLength += step; 
			currParameterValue = molObject.flickerLength; 
		}
	}

	public void AdjustEaseOutDuration(int step){
		foreach (MolObject molObject in molObjects) {
			molObject.easeOutTime += step; 
			currParameterValue = molObject.easeOutTime; 
		}
	}


	public void CreateMolObjects()
	{

		molObjects = new MolObject[count];		

		for(int i = 0; i< count; i++)
			molObjects[i] = MolObject.CreateNewMolObject(gameObject.transform, "molObject_" + i, boxSize, new MolColor(molColors[UnityEngine.Random.Range(0, molColors.Count ())]));	

		initiated = true;

		focusObject = null; 
	}

	private Rect windowRect = new Rect(20, 20, 200, 240);

	private string startFrequency = "30";
	private string stopFrequency = "5";
	private string duration = "2";

	void OnGUI() 
	{
		windowRect = GUI.Window(0, windowRect, DoMyWindow, "My Window");
	}


	void DoMyWindow(int windowID) 
	{
		GUI.Label(new Rect(10, 25, 150, 20), "Start frequency (Hz)");
		startFrequency = GUI.TextField(new Rect(10, 50, 150, 20), startFrequency);

		GUI.Label(new Rect(10, 75, 150, 20), "Stop frequency (Hz)");
		stopFrequency = GUI.TextField(new Rect(10, 100, 150, 20), stopFrequency);

		GUI.Label(new Rect(10, 125, 150, 20), "Duration (s)");
		duration = GUI.TextField(new Rect(10, 150, 150, 20), duration);

		GUI.Label (new Rect (10, 175, 200, 20), modeText); 
		GUI.Label (new Rect (10, 200, 150, 20), parameterValueText); 

		GUI.DragWindow(new Rect(0, 0, 10000, 10000));
	}


	void Start () 
	{
		if (!initiated) 
		{
			CreateMolObjects();
		}
	}

	void Update () 
	{
		if (Input.GetMouseButtonDown (0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit))
			{
				MakeObjectBlink(hit.rigidbody.gameObject.GetComponent<MolObject>());
			}
		}
		
		if (Input.GetKeyDown ("escape"))
		{
			if(modeText.Length == 0){
				Application.Quit();
			}
			else{
				modeText = ""; 
				parameterValueText = ""; 
				adjustParamFunc = null; 
			}
		}

		if (Input.GetKeyDown ("space"))
		{
			MakeObjectBlink(molObjects[UnityEngine.Random.Range(0, molObjects.Count()-1)]);
		}

		if (Input.GetKeyDown ("l")) 
		{
			ChangeObjectLuminance(GetRandomMolObject());
		}

		if (Input.GetKeyDown ("f")) {
			InitLuminanceFlicker (GetRandomMolObject ());
		}

		// set parameters

		if (Input.GetKeyDown ("a")) {
			setCurrentParameter("L* flicker amp", 5, new AdjustParamterDelegate(AdjustFlickerAmplitude)); 
		}

		if (Input.GetKeyDown ("s")) {
			setCurrentParameter("L* focus amp", 1, new AdjustParamterDelegate(AdjustFocusAmplitude)); 
		}

		if (Input.GetKeyDown ("o")) {
			setCurrentParameter("L* focus offset", 1, new AdjustParamterDelegate(AdjustFocusOffset)); 
		}

		if (Input.GetKeyDown ("d")) {
			setCurrentParameter("flicker duration", 50, new AdjustParamterDelegate(AdjustFlickerDuration)); 
		}

		if (Input.GetKeyDown ("e")) {
			setCurrentParameter("ease-out time", 10, new AdjustParamterDelegate(AdjustEaseOutDuration)); 
		}

		if (Input.GetKeyDown ("c")) {
			setCurrentParameter("cycle length", 5, new AdjustParamterDelegate(AdjustCycleLength));  
		}

		// adjust parameters 

		if (Input.GetKeyDown ("up")) {
			updateParameter(stepWidth); 
		}

		if (Input.GetKeyDown ("down")) {
			updateParameter(-stepWidth); 
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

	void MakeObjectBlink(MolObject molObject)
	{
		molObject.blinkingType = blinkingType;
		molObject.interpolationType = interpolationType;
		molObject.StartFlicker();
		
		molObject.StartFrequency = float.Parse(startFrequency);
		molObject.StopFrequency = float.Parse(stopFrequency);
		molObject.Duration = float.Parse(duration);
	}

	void InitLuminanceFlicker(MolObject molObject)
	{
		if (focusObject != null) {
			focusObject.StopLuminanceFlicker (); 
		}
		molObject.StartLuminanceFlicker (); 
		focusObject = molObject; 
	}

	void ChangeObjectLuminance(MolObject molObject)
	{
		if(focusObject != null){
			focusObject.currentColor = new MolColor(molColors[UnityEngine.Random.Range(0, molColors.Count () - 1)]); 
		}
		molObject.currentColor = new MolColor (Color.black); 
		focusObject = molObject; 
	}

	void FixedUpdate()
	{

	}
}