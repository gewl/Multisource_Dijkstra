// This is an implementation of Dijkstra's Algorithm in C# that solves the multisource shortest-paths problem
// on edge-weighted digraphs with positive edge-weights; given an edge-weighted digraph and a set of sources,
// it will return the shortest path to each vertex from any source.
// This implementation reads the EWD from a text file in the format used in Sedgewick's "Algorithms"—examples
// at http://algs4.cs.princeton.edu/44sp/tinyEWD.txt, http://algs4.cs.princeton.edu/44sp/mediumEWD.txt and
// http://algs4.cs.princeton.edu/44sp/largeEWD.txt. It then prompts the user to list sources, and then writes
// the shortest-paths forest to the console.
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace MultisourceDijkstra
{
    class MultisourceDijkstra
    {
        static EdgeWeightedDigraph ewd;
        // Acts as priority queue for eligible edges
        static SortedSet<Vertex> vertices;
        
        // stores distance to vertex n from source s at distanceTo[n] in shortest-paths tree (SPT)
        static float[] distanceTo;
        // stores weight of last edge for printing purposes
        static float[] lastWeight;
        // stores reference to vertex at head of edge for which vertex n is the tail at edgeTo[n] in SPT
        static int[] edgeTo;
        // sources for SPT
        static int[] s;

        static void Main()
        {
            Console.WriteLine("Filepath to txt file containing edge-weighted digraph:");
            string fp = Console.ReadLine();
            ewd = new EdgeWeightedDigraph(fp);
            int vCount = ewd.vertexCount;

            distanceTo = new float[vCount];
            lastWeight = new float[vCount];
            edgeTo = new int[vCount];

            vertices = new SortedSet<Vertex>(new VertexComparer());

            Console.WriteLine("Which nodes between 0 and " + (vCount - 1) + " is/are the source(s)? (Format: Integers separate by spaces, e.g. `2 4 5 7`)");
            string sText = Console.ReadLine();
            s = parseSources(sText);

            for (int v = 0; v < vCount; v++)
            {
                if (s.Contains<int>(v))
                {
                    distanceTo[v] = 0.0f;
                    queueVertex(v);
                }
                else
                {
                    distanceTo[v] = int.MaxValue;
                }
            }

            while (vertices.Count != 0)
            {
                Vertex nextVertex = vertices.First();
                vertices.Remove(nextVertex);
                relax(ewd, nextVertex);
            }

            printSPT();

            Console.ReadKey();
        }

        // Adds vertex to priority queue
        private static void queueVertex(int vertexId)
        {
            Vertex newVertex = new Vertex(vertexId, distanceTo[vertexId]);
            vertices.Add(newVertex);
        }

        private static int[] parseSources(string input)
        {
            string rePattern = @"(\d+)";
            List<int> sources = new List<int>();

            try
            {
                foreach (Match sourceString in Regex.Matches(input, rePattern))
                {
                    int source = Int32.Parse(sourceString.Value);
                    sources.Add(source);
                }

                return sources.ToArray<int>();
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't parse sources input");
                throw new Exception(e.Message);
            }
        }

        private static void relax(EdgeWeightedDigraph graph, Vertex vertex)
        {
            int vertexId = vertex.id;
            List<int> tails = graph.adj(vertexId);

            // Adds eligible edges to priority queue
            foreach (int tail in tails)
            {
                float weight = graph.getWeight(vertexId, tail);
                if (distanceTo[tail] > distanceTo[vertexId] + weight)
                {
                    distanceTo[tail] = distanceTo[vertexId] + weight;
                    lastWeight[tail] = weight;
                    edgeTo[tail] = vertexId;
                    // If vertex already on priority queue, its entry there has a greater weight and so is obsolete;
                    vertices.RemoveWhere(v => v.id == vertexId);
                    queueVertex(tail);
                }
            }
        }

        public static void printSPT()
        {
            //Console.WriteLine("\nVertex\tEdge\tEdge Weight\tTotal Weight\n");
            Console.WriteLine("\n{0, -8} {1, -20} {2, -20} {3, -20}\n", "Vertex", "Edge", "Edge Weight", "Total Weight");
            for (int i = 0; i < distanceTo.Length; i++)
            {
                if (s.Contains<int>(i))
                {
                    Console.WriteLine("{0, -8} {1, -20} {2, -20} {3, -20}", i, "Source", "0.0", "0.0");
                }
                else
                {
                    Console.WriteLine("{0, -8} {1, -20} {2, -20} {3, -20}", i, edgeTo[i] + "->" + i, lastWeight[i], distanceTo[i]);
                }
            }
        }
    }

    // default class to hold vertices in priority queue
    public class Vertex
    {
        public int id { get; protected set; }
        public float weight { get; protected set; }
        public Vertex(int vertexID, float vertexWeight)
        {
            id = vertexID;
            weight = vertexWeight;
        }
    }

    // used by SortedSet `vertices` to sort contents; primarily by weight/distance, secondarily by int vertexId
    internal class VertexComparer : IComparer<Vertex>
    {
        public int Compare (Vertex x, Vertex y)
        {
            int result = x.weight.CompareTo(y.weight);

            if (result == 0)
            {
                result = x.id.CompareTo(y.id);
            }

            return result;
        }
    }

    public class EdgeWeightedDigraph
    {
        public int vertexCount { get; protected set; }
        public int edgeCount { get; protected set; }
        private Dictionary<int, float>[] edges;

        public EdgeWeightedDigraph(string pathToFile)
        {
            try
            {
                StreamReader txtContents = new StreamReader(pathToFile);
                // First two lines in txt are # of vertices and # of edges
                vertexCount = Int32.Parse(txtContents.ReadLine());
                edgeCount = Int32.Parse(txtContents.ReadLine());

                edges = new Dictionary<int, float>[vertexCount];

                for (int i = 0; i < vertexCount; i++)
                {
                    edges[i] = new Dictionary<int, float>();
                }

                // Subsequent lines are in following format: [int source] [int target] [float weight]
                string edgeLine = txtContents.ReadLine();
                string rePattern = @"(\d+)\s+(\d+)\s+(\d+\.\d+)";

                while (edgeLine != null)
                {
                    Match match = Regex.Match(edgeLine, rePattern);

                    int src = Int32.Parse(match.Groups[1].Value);
                    int tar = Int32.Parse(match.Groups[2].Value);
                    float wt = float.Parse(match.Groups[3].Value);

                    // Build structure
                    addEdge(src, tar, wt);

                    edgeLine = txtContents.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("File could not be read.");
                throw new Exception(e.Message);
            }
        }

        public float getWeight(int source, int target)
        {
            if (source < 0 || source > edges.Length)
            {
                Console.WriteLine("getWeight: Source not found.");
            }
            if (!edges[source].ContainsKey(target))
            {
                Console.WriteLine("getWeight: Edge not found.");
            }

            return edges[source][target];
        }

        // Lists tails of all edges for which @source is the head
        public List<int> adj(int source)
        {
            List<int> adjVertices = new List<int>();

            foreach (KeyValuePair<int, float> edge in edges[source])
            {
                adjVertices.Add(edge.Key);
            }

            return adjVertices;
        }

        public void addEdge(int source, int target, float weight)
        {
            edges[source].Add(target, weight);
        }
    }
}
