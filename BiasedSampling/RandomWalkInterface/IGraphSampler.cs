using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using System.IO;

namespace RandomWalks.RandomWalkInterface
{

    public delegate void TransitionEvent<TState, TTransition>(IWeightedRandomWalk<TState, TTransition> sampler, TState previous, TState current, TTransition transition, decimal weight)
                where TTransition : IEdge<TState>;
                


    public interface IWeightedRandomWalk<TVertex, TEdge> 
        where TEdge : IEdge<TVertex>
        
      
    {

        decimal TimeIncrement { get; }
        KeyValuePair<string, string> Name { get; }
        event TransitionEvent<TVertex, TEdge> Step;
        void Initialize();
        TVertex NextSample();
		
        void Terminate();
        event EventHandler Terminated;

        decimal TotalSteps { get; }
        ulong DiscreetSteps { get; }
        TVertex CurrentState { get; }

        int GetAdjacentTransitionCount(TVertex state);
        TEdge GetAdjacentTransition(TVertex state, int index);
        decimal GetStateWeight(TVertex state);
        decimal GetTransitionWeight(TEdge transition);
    }
  

}
