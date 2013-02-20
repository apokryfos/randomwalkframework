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
        }

        ~GraphWriter()
        {
            Dispose();
        }

        public void Dispose()
        {
			baseWriter.Dispose();
        }



		#region IGraphWriter<int,Edge<int>> Members

		public void WriteGraph(IVertexAndEdgeListGraph<int, Edge<int>> graph) {
			baseWriter.WriteGraph(graph);
		}

		public void WriteNextPart(QuickGraph.Collections.IVertexEdgeDictionary<int, Edge<int>> graph) {
			baseWriter.WriteNextPart(graph);
		}

		#endregion
	}


	public class GraphReader : IGraphReader<int, Edge<int>>
    {
		private IGraphReader<int, Edge<int>> baseReader;

        public GraphReader(string file, int bufferSize)
        {
            baseReader = GraphReaderFactory.GetGraphReaderForExtension(file, bufferSize);            
        }

        public GraphReader(string file)
            : this(file, 1024)
        {
           
        }

		~GraphReader() { Dispose(); }
       
        public void Dispose()
        {
			baseReader.Dispose();
        }

        #region IGraphReader Members


        public IVertexAndEdgeListGraph<int, Edge<int>> ReadEntireGraph()
        {
            return baseReader.ReadEntireGraph();
        }

     
        


        public void ResetStream()
        {
            baseReader.ResetStream();
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


        public IUndirectedGraph<int, Edge<int>> ReadEntireGraphAsUndirected()
        {
            return baseReader.ReadEntireGraphAsUndirected();
        }
		     
        public IUndirectedGraph<int, Edge<int>> ReadPartialGraphAsUndirected()
        {
            return baseReader.ReadEntireGraphAsUndirected();
        }

		public QuickGraph.Collections.IVertexEdgeDictionary<int, Edge<int>> ReadAdjecencyList() {
			return baseReader.ReadAdjecencyList();
		}

		#endregion
	}

}
