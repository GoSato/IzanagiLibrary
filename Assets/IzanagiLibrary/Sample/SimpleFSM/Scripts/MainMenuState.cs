using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IzanagiLibrary.FSM;
using System;
using UnityEngine.UI;

public class MainMenuState : IState
{
    private Canvas canvas;
    private Text frameCount;
    private int initialFrame;

    public void BeginEnter()
    {
        var canvasPrefab = Resources.Load<Canvas>("MainMenu");
        canvas = UnityEngine.Object.Instantiate(canvasPrefab);
        var playButtonGO = canvas.transform.Find("PlayButton");
        var playButton = playButtonGO.GetComponent<Button>();
        playButton.onClick.AddListener(HandlePlayButton);
        var frameCountGO = canvas.transform.Find("FrameCount");
        frameCount = frameCountGO.GetComponent<Text>();
        initialFrame = Time.frameCount;
    }

    public void EndEnter()
    {

    }

    public IEnumerable Execute()
    {
        while (true)
        {
            var numFrames = Time.frameCount - initialFrame;
            frameCount.text = "Frame spent on menu : " + numFrames;
            yield return null;
        }
    }

    public event EventHandler<StateBeginExitEventArgs> OnBeginExit;

    public void EndExit()
    {
        UnityEngine.Object.Destroy(canvas.gameObject);
    }

    private void HandlePlayButton()
    {
        var nextState = new PlayState();
        var transition = new ScreenFadeTransition(2);
        var eventArgs = new StateBeginExitEventArgs(nextState, transition);
        OnBeginExit(this, eventArgs);
    }
}
