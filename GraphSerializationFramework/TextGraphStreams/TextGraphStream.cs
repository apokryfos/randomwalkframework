using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using QuickGraph;
using GraphSerializationFramework.GraphStreamFramework;
using ProgressingUtilities;

namespace GraphSerializationFramework
{
    class TextGraphWriter : GraphStreamBase, IGraphWriter
    {
        private StreamWriter stream;

        public TextGraphWriter(string file) 
            : this(file, (int)Math.Pow(2,11))
        {
        }
        public TextGraphWriter(string file, int bufferSize) 
        {
            stream = new StreamWriter(file, false, Encoding.ASCII, bufferSize);
            caps = GraphStreamCaps.EdgeStreamable;
        }

        ~TextGraphWriter()
        {
            Dispose();
        }

        public override string[] SupportedExtensions
        {
            get { return new string[] { ".txt" } ; }
        }

      
        public override void Dispose()
        {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
            base.Dispose();
        }

        #region IGraphWriter Members


        public void WriteNextPart(IVertexAndEdgeListGraph<int, Edge<int>> graph)
        {
            WriteNextEdges(graph);
        }

        public void WriteNextPart(IDictionary<int, ICollection<int>> graph)
        {
            foreach (var kv in graph)
            {
                foreach (var v in kv.Value)
                    stream.WriteLine(kv.Key.ToString() + " " + v.ToString());
            }
        }
    
        
        public void WriteGraph(IVertexAndEdgeListGraph<int, Edge<int>> graph)
        {
            WriteNextPart(graph);
        }


        public void WriteGraph(IBidirectionalGraph<int, Edge<int>> graph)
        {
            WriteNextPart((IVertexAndEdgeListGraph<int, Edge<int>>)graph);
        }

        public void WriteNextEdges(IEnumerable<Edge<int>> edges)
        {
            foreach (var e in edges)
                stream.WriteLine(e.Source.ToString() + " " + e.Target.ToString());
        }

        public void WriteNextPart(IBidirectionalGraph<int, Edge<int>> graph)
        {
            WriteNextPart((IVertexAndEdgeListGraph<int, Edge<int>>)graph);
        }

        public void WriteNextEdges(IEdgeSet<int, Edge<int>> edges)
        {
            WriteNextEdges(edges.Edges);
        }

        #endregion
    }

    class CSVGraphWriter : GraphStreamBase, IGraphWriter
    {
        private StreamWriter stream;

        public CSVGraphWriter(string file) 
            : this(file, (int)Math.Pow(2,11))
        {
            
        }
        public CSVGraphWriter(string file, int bufferSize) 
        {
            stream = new StreamWriter(file, false, Encoding.ASCII, bufferSize);
            caps = GraphStreamCaps.AdjecencyStreamable;
        }

        ~CSVGraphWriter()
        {
            Dispose();
        }


        public override string[] SupportedExtensions
        {
            get { return new string[] { ".csv" }; }
        }

        public override void Dispose()
        {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
            base.Dispose();
        }

        #region IGraphWriter Members

        public void WriteNextPart(IVertexAndEdgeListGraph<int, Edge<int>> graph)
        {
            foreach (var v in graph.Vertices)
            {
                stream.Write(v.ToString());
                foreach (var e in graph.OutEdges(v))
                    stream.Write("," + e.Target.ToString());
                stream.WriteLine();
            }
        }
            
        public void WriteNextPart(IDictionary<int, ICollection<int>> graph)
        {
            foreach (var kv in graph)
            {
                stream.Write(kv.Key.ToString());
                foreach (var v in kv.Value)
                    stream.Write("," + v.ToString());
                stream.WriteLine();
            }
        }

        public void WriteGraph(IVertexAndEdgeListGraph<int, Edge<int>> graph)
        {
            WriteNextPart(graph);
        }


        public void WriteGraph(IBidirectionalGraph<int, Edge<int>> graph)
        {
            WriteNextPart((IVertexAndEdgeListGraph<int, Edge<int>>)graph);
        }


        public void WriteNextPart(IBidirectionalGraph<int, Edge<int>> graph)
        {
            WriteNextPart((IVertexAndEdgeListGraph<int, Edge<int>>)graph);
        }
        
        public void WriteNextEdges(IEdgeSet<int, Edge<int>> edges)
        {
            throw new NotSupportedException();
        }

        public void WriteNextEdges(IEnumerable<Edge<int>> edges)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    class TextGraphReader : GraphStreamBase, IGraphReader
    {
        private StreamReader stream;
        private int bufferSize;
        private bool srcIsCSV = false;


        public TextGraphReader(string file)
            : this(file, (int)Math.Pow(2,11))                
        {
 
        }
        public TextGraphReader(string file, int bufferSize)
        {   
            stream = new StreamReader(file, Encoding.ASCII, false, bufferSize);
            this.bufferSize = bufferSize;
            srcIsCSV = (Path.GetExtension(file) == ".csv");
            caps = GraphStreamCaps.AdjecencyStreamable | (srcIsCSV ? GraphStreamCaps.VertexSet : GraphStreamCaps.EdgeStreamable);
 
        }

        ~TextGraphReader()
        {
            Dispose();
        }

        public override void Dispose()
        {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
            base.Dispose();
        }
        
        #region IGraphReader Members

        public override string[] SupportedExtensions
        {
            get { return new string[] { ".txt", ".csv" }; }
        }

        private void MergeNextPart(IMutableVertexAndEdgeListGraph<int, Edge<int>> g)
        {
            var adjLst = ReadAdjacencyList();
            if (adjLst != null)
            {
                foreach (var kv in adjLst)
                    g.AddVerticesAndEdgeRange(kv.Value.Select<int, Edge<int>>(v => new Edge<int>(kv.Key, v)));
            }
        }


        private void MergeNextPart(IMutableUndirectedGraph<int, Edge<int>> g)
        {
            var adjLst = ReadAdjacencyList();
            if (adjLst != null)
            {
                foreach (var kv in adjLst)
                    g.AddVerticesAndEdgeRange(kv.Value.Select<int, Edge<int>>(v => new Edge<int>(kv.Key, v)));
            }
        }

        public IDictionary<int, ICollection<int>> ReadAdjacencyList()
        {
            if (stream.EndOfStream)
                return null;

            var adjList = PrefferedDataTypes.GetAdjecencyListInstance();
            ICollection<int> current;
            int count = 0;
            do
            {
                string s = stream.ReadLine();

                var parts = s.Split(' ', ',','\t');
                int source, target;
                if (!int.TryParse(parts[0], out source))
                    continue;

                if (!adjList.TryGetValue(source, out current)) 
                {
                    current = PrefferedDataTypes.GetCollectionInstance();
                    adjList.Add(source, current);
                    count++;
                }

                for (int i = 1; i < parts.Length; i++)
                {
                    if (int.TryParse(parts[i], out target))
                    {
                        current.Add(target);
                        count++;
                    }
                }

            } while (!stream.EndOfStream && count < bufferSize);
            return adjList;
        }

        public IVertexAndEdgeListGraph<int, Edge<int>> ReadPartialGraph()
        {
            if (stream.EndOfStream)
                return null;

            AdjacencyGraph<int, Edge<int>> g = new AdjacencyGraph<int, Edge<int>>();
            MergeNextPart(g);
            return g;
        }

        public IBidirectionalGraph<int, Edge<int>> ReadPartialGraphAsBidirectional()
        {
            if (stream.EndOfStream)
                return null;

            BidirectionalGraph<int, Edge<int>> g = new BidirectionalGraph<int, Edge<int>>();
            MergeNextPart(g);
            return g;
        }

        

        public void ResetStream()
        {
            stream.BaseStream.Seek(0, SeekOrigin.Begin);
            stream.DiscardBufferedData();
        }

        public IVertexAndEdgeListGraph<int, Edge<int>> ReadEntireGraph()
        {
            ResetStream();
            AdjacencyGraph<int, Edge<int>> g = new AdjacencyGraph<int, Edge<int>>();
            do
            {
                MergeNextPart(g);
            } while (!stream.EndOfStream);
            return g;
        }

        public IBidirectionalGraph<int, Edge<int>> ReadEntireGraphAsBidirectional()
        {
            ResetStream();
            BidirectionalGraph<int, Edge<int>> g = new BidirectionalGraph<int, Edge<int>>();
            do
            {
                MergeNextPart(g);
            } while (!stream.EndOfStream);
            return g;
        }

        public long Position
        {
            get { return stream.BaseStream.Position; }
            set 
            {
                stream.BaseStream.Position = Math.Min(value,Length); 
                Calibrate(); 
            }
        }

        public long Length
        {
            get { return stream.BaseStream.Length; }
        }

      
        public void Calibrate()
        {
            if (Position < Length) 
            {
                Position -= Environment.NewLine.Length;
                stream.ReadLine();
            }
            
        }

        public IEnumerable<Edge<int>> StreamAllEdges()
        {
            ResetStream();
            var adj = ReadAdjacencyList();
            while (adj != null)
            {
                foreach (var kv in adj)
                {
                    foreach (var v in kv.Value)
                        yield return new Edge<int>(kv.Key, v);
                }
                adj = ReadAdjacencyList();
            }
            yield break;
        }


        public IUndirectedGraph<int, Edge<int>> ReadEntireGraphAsUndirected(bool ape)
        {
            ResetStream();
            UndirectedGraph<int, Edge<int>> g = new UndirectedGraph<int, Edge<int>>(ape);
            do
            {
                MergeNextPart(g);
            } while (!stream.EndOfStream);
            return g;
        }

        public IUndirectedGraph<int, Edge<int>> ReadEntireGraphAsUndirected()
        {
            return ReadEntireGraphAsUndirected(true);
        }

        public IUndirectedGraph<int, Edge<int>> ReadPartialGraphAsUndirected()
        {
            if (stream.EndOfStream)
                return null;

            UndirectedGraph<int, Edge<int>> g = new UndirectedGraph<int, Edge<int>>();
            MergeNextPart(g);
            return g;
        }

        #endregion
    }
}
