using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using System.Threading;
using System.IO;
using RandomWalks.RandomWalkInterface;
using RandomWalks;


namespace RandomWalkAnalysis.Samplers
{

    public class MultiSamplers<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        
    {
        private List<ManualResetEvent> mres;

        private List<KeyValuePair<Type, IGraphQuerier<TVertex, TEdge>>> Types;
        private int LoopCount { set; get; }
        private Func<IWeightedRandomWalk<TVertex, TEdge>, int, ITerminationConditions<TVertex, TEdge>> CondGen;
        private IUndirectedGraph<TVertex, TEdge> GraphRef { set; get; }
        private List<SamplingThread<TVertex, TEdge>> Samplers { get; set; }                
        private string LogPathBase { get; set; }
        private List<int> EntryPoints { set; get; }
        private LoggerType Loggers { set; get; }
        private Random r = RNG.RNGProvider.r;

        public int WaitingThreads { get { return mres.Count; } }
        public int FinishedThreads { get { return LoopCount * Types.Count - WaitingThreads; } }

        public MultiSamplers(
            List<KeyValuePair<Type, IGraphQuerier<TVertex, TEdge>>> d,
            Func<IWeightedRandomWalk<TVertex, TEdge>, int, ITerminationConditions<TVertex, TEdge>> condGen,
            List<int> entryPoints, 
            string filePath,
            int loops,             
            LoggerType loggers,
            IUndirectedGraph<TVertex, TEdge> GraphRef
            )
        {
            Types = d;
            CondGen = condGen;            
            LoopCount = loops;                        
            this.LogPathBase = filePath;            
            this.EntryPoints = entryPoints;
            this.Loggers = loggers;
            this.GraphRef = GraphRef;
            Samplers = new List<SamplingThread<TVertex, TEdge>>();            
        }


        public void MultiSample()
        {

            int maxWorker, maxIOC;
            int minWorker, minIOC;
            // Get the current settings.
            ThreadPool.GetMaxThreads(out maxWorker, out maxIOC);
            ThreadPool.GetMinThreads(out minWorker, out minIOC);

            if (!ThreadPool.SetMaxThreads(minWorker, minIOC))
                Console.WriteLine("Did not succeed setting thread pool size, continuing with defaults: {0},{1}", maxWorker, maxIOC);
            else 
            {
                ThreadPool.GetMaxThreads(out maxWorker, out maxIOC);
                Console.WriteLine("Max threads: {0},{1}", maxWorker, maxIOC);
            }

            mres = new List<ManualResetEvent>();
            for (int j = 0; j < LoopCount; j++)
            {
                foreach (var t in Types)
                    mres.Add(Queue(t, j).ResetFlag);
            }

            Console.WriteLine("Threads {0} finished {1} pending", FinishedThreads, WaitingThreads);

            do
            {
                int ind = ManualResetEvent.WaitAny(mres.ToArray(), 6000);
                if (ind != ManualResetEvent.WaitTimeout)
                    mres.RemoveAt(ind);
                Console.WriteLine("Threads {0} finished {1} pending", FinishedThreads, WaitingThreads);
                

            } while (mres.Count > 0);

            
        }


        private SamplingThread<TVertex, TEdge> Queue(KeyValuePair<Type, IGraphQuerier<TVertex, TEdge>> samplerType, int loop)
        {
                            

            IWeightedRandomWalk<TVertex, TEdge> w = (IWeightedRandomWalk<TVertex, TEdge>)Activator.CreateInstance(samplerType.Key, EntryPoints[r.Next(EntryPoints.Count)], samplerType.Value);
            w.Initialize();

            var Sampler = new SamplingThread<TVertex, TEdge>(w, CondGen(w, loop));

            if (!Directory.Exists(LogPathBase + Path.DirectorySeparatorChar + "Loop" + loop))
                Directory.CreateDirectory(LogPathBase + Path.DirectorySeparatorChar + "Loop" + loop);
            Sampler.AttachLoggers(Loggers, GraphRef, LogPathBase + Path.DirectorySeparatorChar + "Loop" + loop);            
            Samplers.Add(Sampler);
            ThreadPool.QueueUserWorkItem(new WaitCallback(Sampler.Sample));
            return Sampler;
        }



    }
}
