using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using System.IO;

namespace RandomWalkAnalysis
{

    public class HitProperties<TVertex>
        
    {

        public TVertex VertexID { get; private set; }
        public decimal FirstHittingTime { get; private set; }
        public decimal LastHittingTime { get; private set; }
        public decimal Hits { get; private set; }
        public decimal Weight { get; private set; }
        public int Degree { get; private set; }


        public HitProperties(TVertex vertex, decimal currentStep)
            : this(vertex, currentStep, default(decimal), default(int))
        {            
        }

        public void Hit(decimal currentStep)
        {
            Hits++;
            LastHittingTime = currentStep;
        }

        public HitProperties(TVertex vertex, decimal currentStep, decimal weight, int degree)            
        {
            VertexID = vertex;
            FirstHittingTime = currentStep;
            LastHittingTime = currentStep;
            Hits = 1;
            Weight = weight;
            Degree = degree;
        }



        public override string ToString()
        {
            return VertexID + "," + Weight + "," + Degree + "," + FirstHittingTime + "," + LastHittingTime + "," + Hits;
        }
    }

    [Flags]
    public enum LoggerType
    {
        NONE = 0x0,
        STEP = 0x1,
        REVISITS = 0x2,
        HITS = 0x4,
        DEGREECOVER = 0x8,
        RANDOMVARIABLE = 0x10,
        HIDDENPARTITION = 0x20,
		CYCLICFORMULA = 0x40
    }



	public class RandomWalkCyclicFormulaStepsLogger<TVertex, TEdge> : RandomWalkProgressiveLogger<TVertex, TEdge> 
		where TEdge : IEdge<TVertex> {

		Func<TVertex, double>[] values;

		public RandomWalkCyclicFormulaStepsLogger(RandomWalkStepObserver<TVertex, TEdge> obs, string logPath, params Func<TVertex, double>[] functions)
            : base(obs, logPath)
        {
			this.values = functions;
        }

        protected override void obs_ObservationEvent(RandomWalkObserver<TVertex, TEdge> sampler, TVertex current, TEdge transition, object ObservationParameters)
        {
			double[] results = new double[values.Length];
			for (int i = 0;i < values.Length;i++) {
				results[i] = values[i](current);
			}
			
            object[] objs = new object[] 
			{ 
				sampler.Observed.TotalSteps, 
				current, sampler.Observed.GetStateWeight(current), 
				sampler.Observed.GetTransitionWeight(transition), 
				(ObservationParameters!=null?ObservationParameters.ToString():null) 
			};			
            logger.LogLine(objs.Concat(results.Select(d => (object)d)));
        }
	}


    public class RandomWalkHiddenPartitionLogger<TVertex, TEdge> : RandomWalkProgressiveLogger<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {

        Func<TVertex, bool> inPartition;

        public RandomWalkHiddenPartitionLogger(RandomWalkStepObserver<TVertex, TEdge> obs, string logPath, Func<TVertex,bool> inPartition)
            : base(obs, logPath)
        {
            this.inPartition = inPartition;
        }

        protected override void obs_ObservationEvent(RandomWalkObserver<TVertex, TEdge> sampler, TVertex current, TEdge transition, object ObservationParameters)
        {
            int dS = 0;
            if (typeof(TVertex) == typeof(int))
            {                
                for (int i = 0; i < sampler.Observed.GetAdjacentTransitionCount(current); i++)
                {
                    if (inPartition(sampler.Observed.GetAdjacentTransition(current, i).GetOtherVertex(current)))
                        dS++;
                }
            }

            object[] objs = new object[] { sampler.Observed.TotalSteps, current, sampler.Observed.GetStateWeight(current), sampler.Observed.GetTransitionWeight(transition), (ObservationParameters!=null?ObservationParameters.ToString():null), dS };
            logger.LogLine(objs);
        }
    }

         

    public class RandomWalkStepLogger<TVertex, TEdge> : RandomWalkProgressiveLogger<TVertex, TEdge>
       where TEdge : IEdge<TVertex>
        
    {
        public RandomWalkStepLogger(RandomWalkStepObserver<TVertex, TEdge> obs, string logPath)
            : base(obs, logPath)
        {   
        }

        protected override void obs_ObservationEvent(RandomWalkObserver<TVertex, TEdge> sampler, TVertex current, TEdge transition, object ObservationParameters)
        {
            object[] objs = new object[] { sampler.Observed.TotalSteps, current, sampler.Observed.GetStateWeight(current), sampler.Observed.GetTransitionWeight(transition), (ObservationParameters!=null?ObservationParameters.ToString():null) };
            logger.LogLine(objs);
        }
    }


    public class RandomWalkRandomVariableIncrementalLogger<TVertex, TEdge> : RandomWalkProgressiveLogger<TVertex, TEdge>
      where TEdge : IEdge<TVertex>
    {
        Func<RandomWalkObserver<TVertex, TEdge>, TVertex, TEdge, double> Function;


        public RandomWalkRandomVariableIncrementalLogger(RandomWalkStepObserver<TVertex, TEdge> obs, string logPath, Func<RandomWalkObserver<TVertex, TEdge>, TVertex, TEdge, double> function)
            : base(obs, logPath)
        {
            Function = function;            
        }

        protected override void obs_ObservationEvent(RandomWalkObserver<TVertex, TEdge> sampler, TVertex current, TEdge transition, object ObservationParameters)
        {

            object[] objs = new object[] 
            { 
                sampler.Observed.TotalSteps, 
                current, 
                sampler.Observed.GetStateWeight(current),
                sampler.Observed.GetTransitionWeight(transition), 
                Function(sampler, current, transition) 
            };
            logger.LogLine(objs);

            //log.WriteLine(sampler.Observed.TotalSteps + "," + current + "," + sampler.Observed.GetStateWeight(current) + "," + sampler.Observed.GetTransitionWeight(transition) + "," + Function(sampler,current,transition));
        }

    }

    public class RandomWalkRevisitsLogger<TVertex, TEdge> : RandomWalkCumulativeLogger<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        
    {


        public RandomWalkRevisitsLogger(RandomWalkRevisitObserver<TVertex, TEdge> obs, string logPath)
            : base(obs, logPath)
        {
        }

        protected override void LogCumilativeData()
        {
            RandomWalkRevisitObserver<TVertex, TEdge> observer = (RandomWalkRevisitObserver<TVertex, TEdge>)obs;
            foreach (var kv in observer.VisitSteps)
            {
                if (kv.Value.Count == 0)
                    continue;


                List<object> objs = new List<object>();
                objs.Add(kv.Key);
                objs.Add(observer.StateWeights[kv.Key]);
                foreach (var v in kv.Value)
                    objs.Add(v);

                logger.LogLine(objs);

                //log.Write(kv.Key+","+observer.StateWeights[kv.Key]);
                //foreach (var v in kv.Value)
                //    log.Write("," + v);
                //log.WriteLine();



            }
        }

        protected override void obs_ObservationEvent(RandomWalkObserver<TVertex, TEdge> sampler, TVertex current, TEdge transition, object ObservationParameters)
        {
            
        }
    }


    public class RandomWalkHitsLogger<TVertex, TEdge> : RandomWalkCumulativeLogger<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        
    {

        Dictionary<TVertex, HitProperties<TVertex>> hits;


        public RandomWalkHitsLogger(RandomWalkStepObserver<TVertex, TEdge> obs, string logPath)
            : base(obs, logPath)
        {
            hits = new Dictionary<TVertex, HitProperties<TVertex>>();
        }

        protected override void obs_ObservationEvent(RandomWalkObserver<TVertex, TEdge> sampler, TVertex current, TEdge transition, object ObservationParameters)
        {
            HitProperties<TVertex> hp;
            if (!hits.TryGetValue(current, out hp))
            {
                hp = new HitProperties<TVertex>(current, sampler.Observed.TotalSteps, sampler.Observed.GetStateWeight(current), sampler.Observed.GetAdjacentTransitionCount(current));
                hits.Add(current, hp);
            }
            else
                hp.Hit(sampler.Observed.TotalSteps);
        }


        protected override void LogCumilativeData()
        {

           
            foreach (var kv in hits)
            {
                object[] objs = new object[] 
                {
                    kv.Value.VertexID,
                    kv.Value.Weight,
                    kv.Value.Degree,
                    kv.Value.FirstHittingTime,
                    kv.Value.LastHittingTime,
                    kv.Value.Hits
                };
                logger.LogLine(objs);
            }
            

            //foreach (var kv in hits)
            //    log.WriteLine(kv.Value.ToString());
        }

    }


    public class RandomWalkUndirectedDegreeCoverageLogger<TVertex, TEdge> : RandomWalkProgressiveLogger<TVertex, TEdge>
       where TEdge : IEdge<TVertex>
        
    {

        public RandomWalkUndirectedDegreeCoverageLogger(RandomWalkDegreeCoverageObserver<TVertex, TEdge> obs, string logPath)
            : base(obs, logPath)
        {  
        }

        protected override void obs_ObservationEvent(RandomWalkObserver<TVertex, TEdge> sampler, TVertex current, TEdge transition, object ObservationParameters)
        {

            
                object[] objs = new object[] 
                {
                    sampler.Observed.TotalSteps,
                    current,ObservationParameters
                };
                logger.LogLine(objs);
            
           // log.WriteLine(sampler.Observed.TotalSteps + ","+current+","+ObservationParameters);           
        }

    }

}
