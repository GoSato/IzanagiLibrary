using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IzanagiLibrary.FSM
{
    public interface IStateMachine
    {
        IEnumerable Execute();
    }
}