using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class CodeCracker : MonoBehaviour
{
    // Initializing variables and objects.

    public KMBombInfo BombInfo;
    public KMBombModule Module;
    public KMAudio Audio;
    public KMSelectable[] ButtonSelectables;
    public MeshRenderer LeftSquare;
    public MeshRenderer RightSquare;
    public MeshRenderer SolvedModuleLight;
    public Material[] SolutionMaterials;
    public MeshRenderer[] ButtonObjects;
    public TextMesh text; // 90 on TP

    private bool _hasStarted;
    private bool light1, light2;
    private static int moduleIdCounter = 1;
    private int _moduleId;
    private bool[] _leftInputs = new bool[4];
    private bool[] _rightInputs = new bool[4];
    private bool[] _leftSolutions = new bool[4];
    private bool[] _rightSolutions = new bool[4];
    private bool _moduleSolved;
    private Coroutine _timer;

    private void Start()
    {
        _moduleId = moduleIdCounter++;
        text.text = "--";
        DecideSolutions();
        for (int i = 0; i < ButtonSelectables.Length; i++)
        {
            int j = i;
            ButtonSelectables[i].OnInteract += delegate ()
            {
                if (!_moduleSolved)
                    ButtonPress(j, 30);
                return false;
            };
        }
    }

    private void ButtonPress(int button, int initialTime)
    {
        if (!_hasStarted)
        {
            _timer = StartCoroutine(Timer(initialTime));
            _hasStarted = true;
        }
        if (button < 4)
        {
            _leftInputs[button] = !_leftInputs[button];
            ButtonObjects[button].material = _leftInputs[button] ? SolutionMaterials[1] : SolutionMaterials[0];
        }
        else
        {
            _rightInputs[button - 4] = !_rightInputs[button - 4];
            ButtonObjects[button].material = _rightInputs[button - 4] ? SolutionMaterials[1] : SolutionMaterials[0];
        }
        bool leftCorrect = true;
        bool rightCorrect = true;
        for (int i = 0; i < _leftSolutions.Length; i++)
            if (_leftSolutions[i] != _leftInputs[i])
                leftCorrect = false;
        for (int i = 0; i < _rightSolutions.Length; i++)
            if (_rightSolutions[i] != _rightInputs[i])
                rightCorrect = false;
        LeftSquare.material = leftCorrect ? SolutionMaterials[1] : SolutionMaterials[0];
        RightSquare.material = rightCorrect ? SolutionMaterials[1] : SolutionMaterials[0];
        if (leftCorrect && rightCorrect)
        {
            _moduleSolved = true;
            Module.HandlePass();
            SolvedModuleLight.material = SolutionMaterials[1];
            if (_timer != null)
                StopCoroutine(_timer);
            text.text = "!!";
            Debug.LogFormat("[Code Cracker #{0}] All the buttons were set to the right colors. Module solved!", _moduleId);
        }
    }

    private void DecideSolutions()
    {
    tryAgain:
        for (int i = 0; i < _leftSolutions.Length; i++)
        {
            int rand = Rnd.Range(0, 2);
            _leftSolutions[i] = rand == 0;
        }
        for (int i = 0; i < _rightSolutions.Length; i++)
        {
            int rand = Rnd.Range(0, 2);
            _rightSolutions[i] = rand == 0;
        }
        if (!_leftSolutions.Contains(true) || !_rightSolutions.Contains(true))
            goto tryAgain;
        for (int i = 0; i < _leftSolutions.Length; i++)
            Debug.LogFormat("[Code Cracker #{0}] Button {1} should be {2}", _moduleId, i + 1, _leftSolutions[i] ? "green" : "red");
        for (int i = 0; i < _rightSolutions.Length; i++)
            Debug.LogFormat("[Code Cracker #{0}] Button {1} should be {2}", _moduleId, i + 5, _rightSolutions[i] ? "green" : "red");
    }

    private IEnumerator Timer(int startingTime)
    {
        yield return null;
        for (int i = startingTime; i > 0; i--)
        {
            text.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        if (!_moduleSolved)
        {
            text.text = "--";
            _hasStarted = false;
            Debug.LogFormat("[Code Cracker #{0}] You ran out of time! Strike.", _moduleId);
            Module.HandleStrike();
            for (int i = 0; i < _leftInputs.Length; i++)
            {
                _leftInputs[i] = false;
                ButtonObjects[i].material = SolutionMaterials[0];
            }
            for (int i = 0; i < _rightInputs.Length; i++)
            {
                _rightInputs[i] = false;
                ButtonObjects[i + 4].material = SolutionMaterials[0];
            }
        }
    }

    private static string[] _twitchCommands = { "toggle", "press", "push" };

#pragma warning disable 0414
    private readonly string TwitchHelpMessage = "!{0} toggle 1 2 3 | Toggle buttons 1-8. Buttons 1-4 are in reading order on the left, buttons 5-8 are in reading order on the right. | You can only toggle 4 buttons at a time!";
#pragma warning restore 0414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        var pieces = command.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        if (pieces.Length == 0)
            yield break;
        var skip = _twitchCommands.Contains(pieces[0]) ? 1 : 0;
        if (pieces.Skip(skip).Any(p => { int val; return !int.TryParse(p.Trim(), out val) || val < 1 || val > 8; }))
            yield break;
        if (pieces.Length > 4)
        {
            yield return "sendtochaterror You can't toggle more than 4 buttons at a time!";
            yield break;
        }
        yield return null;
        foreach (var p in pieces.Skip(skip))
        {
            ButtonPress(int.Parse(p.Trim()) - 1, 90);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        for (int i = 0; i < _leftSolutions.Length; i++)
        {
            if (_leftInputs[i] != _leftSolutions[i])
            {
                ButtonPress(i, 90);
                yield return new WaitForSeconds(0.1f);
            }
            if (_rightInputs[i] != _rightSolutions[i])
            {
                ButtonPress(i + 4, 90);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
