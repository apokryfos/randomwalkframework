using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using QuickGraph;
using GraphSerializationFramework.GraphStreamFramework;
using ProgressingUtilities;

namespace GraphSerializationFramework
{



	public class GraphWriter : IGraphWriter<int, Edge<int>>
    {
		private IGraphWriter<int, Edge<int>> baseWriter;

        public GraphWriter(string file) : this(file, 1024)
        {

        }
        public GraphWriter(string file, int bufferSize)
        {
            baseWriter = GraphWriterFactory.GetGraphWriterForExtension(file, bufferSize);
            baseWriter.ProgressStart += new EventHandler((o, e) => { OnProgressStarted(); });
            baseWriter.ProgressTick += new ProgressEventDelegate(OnProgressTick);
            baseWriter.ProgressFinish += new EventHandler((o, e) => { OnProgressDone(); });
            baseWriter.StatusText += new EventHandler<ParametrizedEventArgs>((o, peh) => { OnStatusText(peh.Parameter); });
        }

        ~GraphWriter()
        {
            Dispose();
        }

        public override string[] SupportedExtensions
        {
            get { return baseWriter.SupportedExtensions; }
        }

        public override void Dispose()
        {
            if (baseWriter != null)
            {
                baseWriter.Dispose();
                baseWriter = null;
            }
        }



        #region IGraphWriter Members


        public void WriteGraph(IVertexAndEdgeListGraph<int, Edge<int>> graph)
        {
            baseWriter.WriteGraph(graph);
        }

        public void WriteGraph(IBidirectionalGraph<int, Edge<int>> graph)
        {
            baseWriter.WriteGraph(graph);
        }

        public void WriteNextPart(IVertexAndEdgeListGraph<int, Edge<int>> graph)
        {
            baseWriter.WriteNextPart(graph);
        }


        public void WriteNextPart(IDictionary<int, ICollection<int>> graph)
        {
            baseWriter.WriteNextPart(graph);
        }
      


        public void WriteNextPart(IBidirectionalGraph<int, Edge<int>> graph)
        {
            baseWriter.WriteNextPart(graph);
        }

        public void WriteNextEdges(IEdgeSet<int, Edge<int>> edges)
        {
            baseWriter.WriteNextEdges(edges);
        }
        public void WriteNextEdges(IEnumerable<Edge<int>> edges)
        {
            baseWriter.WriteNextEdges(edges);
        }

        #endregion
    }


	public class GraphReader : IGraphReader<int, Edge<int>>
    {
		private IGraphReader<int, Edge<int>> baseReader;

        public GraphReader(string file, int bufferSize)
        {
            baseReader = GraphReaderFactory.GetGraphReaderForExtension(file, bufferSize);
            baseReader.ProgressStart += new EventHandler((o, e) => { OnProgressStarted(); });
            baseReader.ProgressTick += new ProgressEventDelegate(OnProgressTick);
            baseReader.ProgressFinish += new EventHandler((o, e) => { OnProgressDone(); });
            baseReader.StatusText += new EventHandler<ParametrizedEventArgs>((o, peh) => { OnStatusText(peh.Parameter); });
        }

        public GraphReader(string file)
            : this(file, 1024)
        {
           
        }

        public override string[] SupportedExtensions
        {
            get { return baseReader.SupportedExtensions; }
        }

        public override void Dispose()
        {
            if (baseReader != null)
            {
                baseReader.Dispose();
                baseReader = null;
            }
        }

        #region IGraphReader Members


        public IVertexAndEdgeListGraph<int, Edge<int>> ReadEntireGraph()
        {
            return baseReader.ReadEntireGraph();
        }

     
        public IBidirectionalGraph<int, Edge<int>> ReadEntireGraphAsBidirectional()
        {
            return baseReader.ReadEntireGraphAsBidirectional();
        }    


        public void ResetStream()
        {
            baseReader.ResetStream();
        }

        public IVertexAndEdgeListGraph<int, Edge<int>> ReadPartialGraph()
        {
            return baseReader.ReadPartialGraph();
        }

        public IBidirectionalGraph<int, Edge<int>> ReadPartialGraphAsBidirectional()
        {
            return baseReader.ReadPartialGraphAsBidirectional();
        }


      
        public IDictionary<int, ICollection<int>> ReadAdjacencyList()
        {
            return baseReader.ReadAdjacencyList();
        }

      
        public long Position
        {
            get
            {
                return baseReader.Position;
            }
            set { baseReader.Position = value; }
        }

        public long Length
        {
            get { return baseReader.Length; }
        }

        public void Calibrate()
        {
            baseReader.Calibrate();
        }

      

        public IEnumerable<Edge<int>> StreamAllEdges()
        {
            return baseReader.StreamAllEdges();
        }

        public IUndirectedGraph<int, Edge<int>> ReadEntireGraphAsUndirected()
        {
            return baseReader.ReadEntireGraphAsUndirected();
        }

        public IUndirectedGraph<int, Edge<int>> ReadEntireGraphAsUndirected(bool ape)
        {
            return baseReader.ReadEntireGraphAsUndirected(ape);
        }


        public IUndirectedGraph<int, Edge<int>> ReadPartialGraphAsUndirected()
        {
            return baseReader.ReadEntireGraphAsUndirected();
        }

        #endregion
    }

}
