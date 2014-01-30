using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Linq;

public class MainScript : MonoBehaviour 
{
	public int count = 100;
	public Vector3 boxSize = new Vector3(0.0f, 0.0f, 0.0f);

	public BlinkingType blinkingType = BlinkingType.EQUAL;
	public InterpolationType interpolationType = InterpolationType.LINEAR;

	private bool initiated = false;
	private MolObject[] molObjects;
	private MolObject focusObject; 

	//static Color[] molColors = {Color.blue, Color.red, Color.yellow, Color.green, Color.cyan, Color.magenta}; 
	static Color[] molColors = {Color.blue, Color.green, Color.cyan}; 
	//static Color[] molColors = {Color.white}; 


	public void CreateMolObjects()
	{

		molObjects = new MolObject[count];		

		for(int i = 0; i< count; i++)
			molObjects[i] = MolObject.CreateNewMolObject(gameObject.transform, "molObject_" + i, boxSize, new MolColor(molColors[UnityEngine.Random.Range(0, molColors.Count ())]));	

		initiated = true;

		focusObject = null; 
	}

	private Rect windowRect = new Rect(20, 20, 170, 180);

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
		
		if (Input.GetKey ("escape"))
		{
			Application.Quit();
		}

		if (Input.GetKeyDown ("space"))
		{
			MakeObjectBlink(molObjects[UnityEngine.Random.Range(0, molObjects.Count()-1)]);
		}

		if (Input.GetKeyDown ("l")) 
		{
			ChangeObjectLuminance(GetRandomMolObject());
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