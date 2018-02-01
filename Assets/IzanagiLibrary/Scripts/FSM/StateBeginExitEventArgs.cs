using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace IzanagiLibrary.FSM
{
    public class StateBeginExitEventArgs : EventArgs
    {
        public IState NextState { get; private set; }
        public IStateTransition Transition { get; private set; }

        public StateBeginExitEventArgs(IState nextState, IStateTransition transition)
        {
            NextState = nextState;
            Transition = transition;
        }
    }
}