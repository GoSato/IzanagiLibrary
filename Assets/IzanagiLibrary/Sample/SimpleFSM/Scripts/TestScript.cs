using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IzanagiLibrary.FSM;

public class TestScript : MonoBehaviour
{
    private void Start()
    {
        var mainMenuState = new MainMenuState();
        var stateMachine = new StateMachine(mainMenuState);
        StartCoroutine(stateMachine.Execute().GetEnumerator());
    } 
}
