using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IzanagiLibrary.FSM;
using System;

public class PlayState : IState
{
    private GameObject targetContainer;
    private List<GameObject> targets;

    public void BeginEnter()
    {
        targetContainer = new GameObject("TargetContainer");
        var targetPrefab = Resources.Load<GameObject>("Target");
        targets = new List<GameObject>(3);
        for(int i = 0; i < targets.Capacity; i++)
        {
            var target = UnityEngine.Object.Instantiate(targetPrefab);
            target.transform.parent = targetContainer.transform;
            target.transform.position += new Vector3(i * 2, 0, 0);
            targets.Add(target);
        }

        foreach (var target in targets)
        {
            SetTargetColor(target, Color.green);
        }
    }

    public void EndEnter()
    {
      
    }

    public IEnumerable Execute()
    {
        while(true)
        {
            if(Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
            yield return null;
        }
    }

    public event EventHandler<StateBeginExitEventArgs> OnBeginExit;

    public void EndExit()
    {
        UnityEngine.Object.Destroy(targetContainer);
    }

    private void HandleClick()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if(Physics.Raycast(ray, out hitInfo) == false)
        {
            return;
        }

        foreach(var target in targets)
        {
            if (target != null && hitInfo.transform == target.transform)
            {
                HitTarget(target);
                break;
            }
        }
    }

    private void HitTarget(GameObject target)
    {
        SetTargetColor(target, Color.red);

        targets.Remove(target);
        if(targets.Count == 0)
        {
            var nextState = new MainMenuState();
            var transition = new ScreenFadeTransition(2);
            var eventArgs = new StateBeginExitEventArgs(nextState, transition);
            OnBeginExit(this, eventArgs);
        }
    }

    private void SetTargetColor(GameObject target, Color color)
    {
        var renderer = target.GetComponent<Renderer>();
        renderer.material.color = color;
    }
}
