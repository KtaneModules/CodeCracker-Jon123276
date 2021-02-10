using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class CodeCracker : MonoBehaviour {
	// Initializing variables and objects.

	public KMBombInfo bomb;
	public KMBombModule module;
	public KMAudio audio;
	public KMSelectable[] AllButtons;
	public MeshRenderer correct1;
	public MeshRenderer correct2;
	public MeshRenderer SolvedModuleLight;
	public Material[] RedLightGreenLight;
	public MeshRenderer[] toggleButtons;
	public TextMesh text; // 90 on TP
	bool started = false;
	bool light1, light2 = false;
	bool[] checkTheToggles = { false, false, false, false, false, false, false, false };
	static int moduleId = 0;
	bool[] arr = {};
	bool solved = false;
	// Use this for initialization
	void Awake(){
		moduleId++;
		OnInteractArray(AllButtons, checkToggles);
	}
	KMSelectable.OnInteractHandler checkToggles(int i){
		return ()=>{
			if (!checkTheToggles [i]) {
				toggleButtons [i].material = RedLightGreenLight [0];
				checkTheToggles [i] = true;
				Debug.Log (i + " is being pressed");
			} 
			else {
				toggleButtons [i].material = RedLightGreenLight [1];
				checkTheToggles [i] = false;
				Debug.Log (i + " is being pressed");
			}
			for (int j = 0; j < checkTheToggles.Length; j++){
				Debug.Log(checkTheToggles[j]);
			}
			if (!started){
				StartCoroutine(timer());
				started = true;
			}
			checkForCorrectButtons();
			return false;
		};
	}
	void checkForCorrectButtons(){
		if (checkTheToggles[0] == arr[0] && checkTheToggles[1] == arr[1] && checkTheToggles[2] == arr[2] && checkTheToggles[3] == arr[3]){
			correct1.material = RedLightGreenLight[0];
			Debug.LogFormat("[Code Cracker #{0}]: Light 1 is on!", moduleId);
			light1 = true;
		}
		else{
			correct1.material = RedLightGreenLight[1];
		}
		if (checkTheToggles[4] == arr[4] && checkTheToggles[5] == arr[5] && checkTheToggles[6] == arr[6] && checkTheToggles[7] == arr[7]){
			correct2.material = RedLightGreenLight[0];
			Debug.LogFormat("[Code Cracker #{0}]: Light 2 is on!", moduleId);
			light2 = true;
		}
		else{
			correct2.material = RedLightGreenLight[1];
		}
		if (light1 && light2){
			SolvedModuleLight.material = RedLightGreenLight[0];
			solved = true;
			module.HandlePass();
		}
	}
	IEnumerator timer(){
		yield return null;
		while (int.Parse(text.text) != 0){
			text.text = (int.Parse(text.text)-1).ToString();
			yield return new WaitForSeconds(1f);
		}
		if (!solved){
			module.HandleStrike();
			started = false;
			text.text = 30.ToString();
			for (int i = 0; i < checkTheToggles.Length; i++){
				checkTheToggles[i] = false;
				toggleButtons [i].material = RedLightGreenLight [1];
			}
			Start();
		}
		else{
			text.text = "!!";
		}
	}
	void Start() {
		arr = RandomBools(8);
		for (int i = 0; i < arr.Length; i++){
			Debug.LogFormat("[Code Cracker #{0}] Button {1} should be: {2}", moduleId, i+1, arr[i]);
		}
	}
    private static void OnInteractArray(KMSelectable[] selectables, Func<int, KMSelectable.OnInteractHandler> method)
    {
        for (int i = 0; i < selectables.Length; i++)
            selectables[i].OnInteract += method(i);
    }
    private static bool[] RandomBools(int length, float weighting = 0.4f)
    {
        bool[] array = new bool[length];
        for (int i = 0; i < array.Length; i++)
            array[i] = Rnd.Range(0, 1f) > weighting;
        return array;
    }
}
