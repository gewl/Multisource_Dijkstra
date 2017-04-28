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
        // Pseudo-priority-queue that stores vertices sorted on distance from source
        // Key: distance (float)
        // Value: list of vertices found at that distance (List<int>)
        // Use: While loop during pathfinding process dequeues lowest vertex at shortest distance, adds to tree, and queues eligible edges from that vertex
        static SortedDictionary<float, List<int>> verticesPriorityQueue;
        // Stores distances to vertices in `vertices` sortedDict for easy reference
        // Key: vertex (int)
        // Value: distance (float)
        static Dictionary<int, float> verticesQueueRef;
        
        // stores distance to vertex n from source s at distanceTo[n] in shortest-paths tree (SPT)
        static float[] distanceTo;
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
            edgeTo = new int[vCount];

            verticesPriorityQueue = new SortedDictionary<float, List<int>>();
            verticesQueueRef = new Dictionary<int, float>();

            Console.WriteLine("Which nodes between 0 and " + (vCount - 1) + " is/are the source(s)? (Format: Integers separate by spaces, e.g. `2 4 5 7`)");
            string sText = Console.ReadLine();
            s = parseSources(sText);

            for (int v = 0; v < vCount; v++)
            {
                if (s.Contains<int>(v))
                {
                    distanceTo[v] = 0.0f;
                }
                else
                {
                    distanceTo[v] = int.MaxValue;
                }
            }

            // add each source vertex to sortedDictionary with distance of 0.0f
            foreach (int source in s)
            {
                Console.WriteLine(source);
                queueVertex(source);
            }

            // while sortedDictionary has vertices, remove closest vertex, add to tree, and add edges from that vertex to sortedDictionary
            while (verticesPriorityQueue.Count != 0)
            {
                int nextVertex = verticesPriorityQueue.Values.First().First<int>();
                removeVertex(nextVertex);
                relax(ewd, nextVertex);
            }

            printSPT();

            Console.ReadKey();
        }

        // places vertex into `verticesPriorityQueue` sortedDict and `verticesQueueRef` dict
        private static void queueVertex(int vertex)
        {
            // if nodes does not contain list for nodes at given distance, initializes new list under distance key
            if (!verticesPriorityQueue.ContainsKey(distanceTo[vertex]))
            {
                verticesPriorityQueue.Add(distanceTo[vertex], new List<int>());
            }
            // adds vertex to list under distance key in sortedDictionary
            verticesPriorityQueue[distanceTo[vertex]].Add(vertex);
            // adds distance to sortedDictionary under vertex key
            verticesQueueRef.Add(vertex, distanceTo[vertex]);
        }

        // removes vertex from priority queue & reference dict
        private static void removeVertex(int vertex)
        {
            float nodeRef = verticesQueueRef[vertex];
            verticesPriorityQueue[verticesQueueRef[vertex]].Remove(vertex);
            // if vertex being removed is last under 'distance' key in sortedDict, delete the key/value in sortedDict to help with garbage collection
            if (verticesPriorityQueue[verticesQueueRef[vertex]].Count == 0)
            {
                verticesPriorityQueue.Remove(verticesQueueRef[vertex]);
            }
            verticesQueueRef.Remove(vertex);
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

        // Relax edge to nearest-distance node from pseudo-priority-queue `nodes`
        // Add edges to `nodes` (and references to `nodesRefs`)
        private static void relax(EdgeWeightedDigraph graph, int vertex)
        {
            List<int> tails = graph.adj(vertex);

            foreach (int tail in tails)
            {
                float weight = graph.getWeight(vertex, tail);
                if (distanceTo[tail] > distanceTo[vertex] + weight)
                {
                    distanceTo[tail] = distanceTo[vertex] + weight;
                    edgeTo[tail] = vertex;
                    if (verticesQueueRef.ContainsKey(tail))
                    {
                        removeVertex(tail);
                    }
                    queueVertex(tail);
                }
            }
        }

        public static void printSPT()
        {
            for (int i = 0; i < distanceTo.Length; i++)
            {
                Console.WriteLine("Vertex " + i + ":");
                if (s.Contains<int>(i))
                {
                    Console.WriteLine("Source vertex.");
                }
                else
                {
                    Console.WriteLine("Edge from " + edgeTo[i] + ".");
                    Console.WriteLine("Distance from nearest source to " + i + ": " + distanceTo[i]);
                }
            }
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
