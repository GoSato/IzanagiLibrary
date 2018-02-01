using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IzanagiLibrary.FSM
{
    public class StateMachine : IStateMachine
    {
        private IState state;
        private IState nextState;
        private IStateTransition transition;

        public StateMachine(IState initialState)
        {
            State = initialState;
            state.EndEnter();
        }

        public IEnumerable Execute()
        {
            while (true)
            {
                for(var e = state.Execute().GetEnumerator(); transition == null && e.MoveNext();)
                {
                    yield return e.Current;
                }

                while(transition == null)
                {
                    yield return null;
                }

                state.OnBeginExit -= HandleStateBeginExit;

                if(nextState == null)
                {
                    break;
                }

                foreach(var e in transition.Exit())
                {
                    yield return e;
                }
                state.EndExit();

                State = nextState;
                nextState = null;

                foreach(var e in transition.Enter())
                {
                    yield return e;
                }

                transition = null;
                state.EndEnter();
            }
        }

        private IState State
        {
            set
            {
                state = value;
                state.OnBeginExit += HandleStateBeginExit;
                state.BeginEnter();
            }
        }

        private void HandleStateBeginExit(object sender, StateBeginExitEventArgs e)
        {
            nextState = e.NextState;
            transition = e.Transition;
        }
    }
}