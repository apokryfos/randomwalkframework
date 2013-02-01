using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace RandomWalkAnalysis
{

    public enum LoggingMode { BINARY, TEXT} ;

    public abstract class RandomWalkProgressiveLogger<TVertex, TEdge> : RandomWalkCumulativeLogger<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        protected override void LogCumilativeData() { }

        public RandomWalkProgressiveLogger(RandomWalkObserver<TVertex, TEdge> obs, string logPath)
            : base(obs, logPath)
        {
            logger.OpenLogger();
        }

        public RandomWalkProgressiveLogger(RandomWalkObserver<TVertex, TEdge> obs, string logPath, LoggingMode mode)
            : base(obs, logPath, mode)
        {
            logger.OpenLogger();
        }

        public override void Dispose()
        {
            logger.Dispose();
        }
    }


    public abstract class RandomWalkCumulativeLogger<TVertex, TEdge> : IDisposable
        where TEdge : IEdge<TVertex>
        
    {

        protected IRandomWalkLogger<TVertex, TEdge> logger;
        protected RandomWalkObserver<TVertex, TEdge> obs;

        public RandomWalkCumulativeLogger(RandomWalkObserver<TVertex, TEdge> obs, string logPath)
            : this(obs, logPath, LoggingMode.TEXT)
        {
        }
       

        public RandomWalkCumulativeLogger(RandomWalkObserver<TVertex, TEdge> obs, string logPath, LoggingMode mode)            
        {
            if (mode == LoggingMode.BINARY)
                logger = new RandomWalkBinaryLogger<TVertex, TEdge>(logPath);
            else
                logger = new RandomWalkLogger<TVertex, TEdge>(logPath);

            this.obs = obs;
            obs.ObservationEvent += new ObserverEvent<TVertex, TEdge>(obs_ObservationEvent);
        }

        protected abstract void obs_ObservationEvent(RandomWalkObserver<TVertex, TEdge> sampler, TVertex current, TEdge transition, object ObservationParameters);
        
        protected abstract void LogCumilativeData();

        public virtual void Dispose()
        {
            try
            {   
                LogCumilativeData();             
            }
            catch
            {

            }
            finally
            {
                logger.Dispose();
            }
        }
    }
}
