using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using RandomWalks.RandomWalkInterface;

namespace RandomWalkAnalysis
{
    public delegate void ObserverEvent<TState, TTransition>(RandomWalkObserver<TState, TTransition> sampler, TState current, TTransition transition, object ObservationParameters)
              where TTransition : IEdge<TState>;

    public abstract class RandomWalkObserver<TVertex, TEdge> : IDisposable
        where TEdge : IEdge<TVertex>
        
    {
        public IRandomWalk<TVertex, TEdge> Observed { get; set; }

        public RandomWalkObserver(IRandomWalk<TVertex, TEdge> observedSampler)
        {
            Observed = observedSampler;
            Observed.Step += new TransitionEvent<TVertex, TEdge>(Observed_Transition);
            

        }
        ~RandomWalkObserver()
        {
            Dispose();
        }

        protected abstract void Observed_Transition(IRandomWalk<TVertex, TEdge> sampler, TVertex previous, TVertex current, TEdge transition, decimal weight);

        public event ObserverEvent<TVertex, TEdge> ObservationEvent;

        protected void OnObservation(TVertex current, TEdge transition, object parameters)
        {
            if (ObservationEvent != null)
                ObservationEvent(this, current, transition, parameters);
        }

        #region IDisposable Members

        public abstract void Dispose();

        #endregion


    }
}
