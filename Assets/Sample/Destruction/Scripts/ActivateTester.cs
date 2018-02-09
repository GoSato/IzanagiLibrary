using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IzanagiLibrary;

public class ActivateTester : MonoBehaviour
{
    [SerializeField]
    private GameObject _targetObject;
    [SerializeField]
    private ShaderPropertyChanger _changer;
    [SerializeField]
    private float _startValue = 0.0f;
    [SerializeField]
    private float _goalValue = 2.0f;
    [SerializeField]
    private float _duration = 2.0f;
    [SerializeField]
    private bool _activeOnAwake = false;

    private bool _isActivated = false;

    private void Awake()
    {
        _targetObject.SetActive(_activeOnAwake);

        _isActivated = _activeOnAwake;
    }

    private void Update()
    {
        // デバッグ用
        if (Input.GetKeyDown(KeyCode.A))
        {
            ActivateTest();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            DeactivateTest();
        }
    }

    [ContextMenu("ActivateTest")]
    public void ActivateTest()
    {
        if (_isActivated)
        {
            return;
        }

        _targetObject.SetActive(true);

        _changer.OnFinish = () =>
        {
            _isActivated = true;
        };

        _changer.ChangeProperty(_startValue, _goalValue, _duration);
    }

    [ContextMenu("DeactivateTest")]
    public void DeactivateTest()
    {
        if (!_isActivated)
        {
            return;
        }

        _changer.OnFinish = () =>
        {
            _targetObject.SetActive(false);
            _isActivated = false;
        };

        _changer.ChangeProperty(_goalValue, _startValue, _duration);
    }
}
