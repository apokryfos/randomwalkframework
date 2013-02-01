using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using QuickGraph;
using GraphSerializationFramework.GraphStreamFramework;
using ProgressingUtilities;
using GraphSerializationFramework.BinaryGraphStreams;

namespace GraphSerializationFramework
{


    public class BinaryEdgeListReader : GraphStreamBase, IGraphReader
    {
        
        protected BinaryReader stream;
        private string file;
        private int bufferSize;

        public BinaryEdgeListReader(string file)
            : this(file, (int)Math.Pow(2, 16))
        {
        }
        public BinaryEdgeListReader(string file, int bufferSize)
        {   
            stream = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize));            
            this.file = file;
            caps = GraphStreamCaps.AdjecencyStreamable | GraphStreamCaps.EdgeStreamable;
            this.bufferSize = bufferSize / sizeof(int);
        }

        ~BinaryEdgeListReader()
        {
            Dispose();
        }

        public override string[] SupportedExtensions
        {
            get { return new string[] { ".bel", ".sbel" }; }
        }

        protected void ReadAndMergeGraph(IMutableVertexAndEdgeListGraph<int, Edge<int>> g)
        {

            var el = ReadEdgeList();            
            g.AddVerticesAndEdgeRange(el);

        }

        protected void ReadAndMergeGraph(IMutableUndirectedGraph<int, Edge<int>> g)
        {

            var el = ReadEdgeList();
            g.AddVerticesAndEdgeRange(el);

        }


        public IEnumerable<Edge<int>> StreamAllEdges()
        {
            ResetStream();
            do
            {
                var ints = BinaryDataFunctions.ReadInt32Buffer(stream, this.bufferSize);
                List<Edge<int>> edges = new List<Edge<int>>();
                for (int i = 0; i < ints.Length; i += 2)
                    yield return new Edge<int>(ints[i], ints[i + 1]);
            } while (stream.BaseStream.Position < stream.BaseStream.Length);
            yield break;
        }

        private ICollection<Edge<int>> ReadEdgeList()
        {
            var ints = BinaryDataFunctions.ReadInt32Buffer(stream, this.bufferSize);
            List<Edge<int>> edges = new List<Edge<int>>();
            for (int i = 0; i < ints.Length; i += 2)
                edges.Add(new Edge<int>(ints[i], ints[i + 1]));
            return edges;
        }



        #region IGraphReader Members

        public virtual IDictionary<int, ICollection<int>> ReadAdjacencyList()
        {

            if (stream.BaseStream.Position == sizeof(int))
                OnProgressStarted();
            if (stream.BaseStream.Position == stream.BaseStream.Length)
            {
                OnProgressDone();
                return null;
            }

            IDictionary<int, ICollection<int>> adjlist = PrefferedDataTypes.GetAdjecencyListInstance();
            var el = ReadEdgeList();
            foreach (var e in el)
            {
                ICollection<int> c;
                if (!adjlist.TryGetValue(e.Source, out c))
                {
                    c = PrefferedDataTypes.GetCollectionInstance();
                    adjlist.Add(e.Source, c);
                }
                c.Add(e.Target);
            }


            OnProgressTick("Reading graph...", stream.BaseStream.Position, stream.BaseStream.Length);
            return adjlist;
        }


        public virtual IVertexAndEdgeListGraph<int, Edge<int>> ReadPartialGraph()
        {
            if (stream.BaseStream.Position == sizeof(int))
                OnProgressStarted();
            if (stream.BaseStream.Position == stream.BaseStream.Length)
            {
                OnProgressDone();
                return null;
            }


            IMutableVertexAndEdgeListGraph<int, Edge<int>> g;
            g = new AdjacencyGraph<int, Edge<int>>();
            ReadAndMergeGraph(g);

            return g;

        }

        public virtual IBidirectionalGraph<int, Edge<int>> ReadPartialGraphAsBidirectional()
        {
            if (stream.BaseStream.Position == sizeof(int))
                OnProgressStarted();
            if (stream.BaseStream.Position == stream.BaseStream.Length)
            {
                OnProgressDone();
                return null;
            }

            IMutableBidirectionalGraph<int, Edge<int>> g;
            g = new BidirectionalGraph<int, Edge<int>>();
            ReadAndMergeGraph(g);
            return g;
        }

        public virtual void ResetStream()
        {
            stream.BaseStream.Seek(0, SeekOrigin.Begin);
        }




        public virtual IVertexAndEdgeListGraph<int, Edge<int>> ReadEntireGraph()
        {
            ResetStream();
            IMutableVertexAndEdgeListGraph<int, Edge<int>> g = new AdjacencyGraph<int, Edge<int>>();
            OnProgressStarted();
            while (stream.BaseStream.Position < stream.BaseStream.Length)
            {
                ReadAndMergeGraph(g);
                OnProgressTick("Reading graph...", stream.BaseStream.Position, stream.BaseStream.Length);
            }
            OnProgressDone();
            return g;
        }

        public virtual IBidirectionalGraph<int, Edge<int>> ReadEntireGraphAsBidirectional()
        {
            ResetStream();
            IMutableBidirectionalGraph<int, Edge<int>> g = new BidirectionalGraph<int, Edge<int>>();
            OnProgressStarted();
            while (stream.BaseStream.Position < stream.BaseStream.Length)
            {
                ReadAndMergeGraph(g);
                OnProgressTick("Reading graph...", stream.BaseStream.Position, stream.BaseStream.Length);
            }
            OnProgressDone();
            return g;
        }


        public IEdgeSet<int, Edge<int>> ReadNextEdges()
        {
            return ReadPartialGraph();
        }


        public long Position
        {
            get
            {
                return stream.BaseStream.Position;
            }
            set
            {
                stream.BaseStream.Position = Math.Min(value, Length);
                Calibrate();
            }
        }

        public long Length
        {
            get { return stream.BaseStream.Length; }
        }

        public void Calibrate()
        {
            if (Position < Length && Position > 0)
                stream.BaseStream.Seek(-(stream.BaseStream.Position % (2 * sizeof(int))), SeekOrigin.Current);
        }

        public IUndirectedGraph<int, Edge<int>> ReadEntireGraphAsUndirected(bool ape)
        {
            ResetStream();
            IMutableUndirectedGraph<int, Edge<int>> g = new UndirectedGraph<int, Edge<int>>(ape);
            OnProgressStarted();
            while (stream.BaseStream.Position < stream.BaseStream.Length)
            {
                ReadAndMergeGraph(g);
                OnProgressTick("Reading graph...", stream.BaseStream.Position, stream.BaseStream.Length);
            }
            OnProgressDone();
            return g;
        }


        public IUndirectedGraph<int, Edge<int>> ReadEntireGraphAsUndirected()
        {
            return ReadEntireGraphAsUndirected(true);
        }

        public IUndirectedGraph<int, Edge<int>> ReadPartialGraphAsUndirected()
        {
            if (stream.BaseStream.Position == sizeof(int))
                OnProgressStarted();
            if (stream.BaseStream.Position == stream.BaseStream.Length)
            {
                OnProgressDone();
                return null;
            }

            IMutableUndirectedGraph<int, Edge<int>> g = new UndirectedGraph<int, Edge<int>>();
            ReadAndMergeGraph(g);
            return g;
        }

        #endregion

        #region IDisposable Members

        public override void Dispose()
        {
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            base.Dispose();
        }

        #endregion
    }

    public class BinaryEdgeListWriter : GraphStreamBase, IGraphWriter
    {
        private FileStream baseStream;
        private BinaryWriter stream;
        
        public BinaryEdgeListWriter(string file)
            : this(file, 1024)
        {
        }
        public BinaryEdgeListWriter(string file, int bufferSize)
        {
            baseStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize);
            stream = new BinaryWriter(baseStream);
            caps = GraphStreamCaps.AdjecencyStreamable;
        }

        ~BinaryEdgeListWriter()
        {

            Dispose();
        }

        #region IGraphWriter Members

        
        public override string[] SupportedExtensions
        {
            get { return new string[] { ".bel", ".sbel" }; }
        }

        public void WriteNextPart(IVertexAndEdgeListGraph<int, Edge<int>> graph)
        {
            WriteNextEdges(graph.Edges);
        }


        public void WriteGraph(IVertexAndEdgeListGraph<int, Edge<int>> graph)
        {
            WriteNextPart(graph);  
        }

        


        public void WriteNextPart(IDictionary<int, ICollection<int>> graph)
        {
            foreach (var kv in graph)
            {
                WriteNextEdges(kv.Value.Select<int, Edge<int>>(v => new Edge<int>(kv.Key, v)));
            }
        }

        public void WriteNextPart(IBidirectionalGraph<int, Edge<int>> graph)
        {
            WriteNextEdges(graph.Edges);
        }




        public void WriteGraph(IBidirectionalGraph<int, Edge<int>> graph)
        {
            WriteNextPart(graph);            
        }

        public void WriteNextEdges(IEdgeSet<int, Edge<int>> edges)
        {
            WriteNextEdges(edges.Edges);
        }
        
        public void WriteNextEdges(IEnumerable<Edge<int>> edges)
        {
            foreach (var e in edges)
            {
                stream.Write(e.Source);
                stream.Write(e.Target);
            }
        }

        
        #endregion

        #region IDisposable Memebers

        public override void Dispose()
        {

            if (stream != null)
            {   
                stream.Close();
                stream = null;
            }
            base.Dispose();
        }


        #endregion



    }
}
