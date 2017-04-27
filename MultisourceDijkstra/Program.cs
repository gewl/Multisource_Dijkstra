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
        static SortedDictionary<float, List<int>> nodes;
        static Dictionary<int, float> nodesRef;
        
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

            nodes = new SortedDictionary<float, List<int>>();
            nodesRef = new Dictionary<int, float>();

            Console.WriteLine("Which nodes between 0 and " + (vCount - 1) + " is the source? (Format: Integers separate by spaces, e.g. `2 4 5 7`)");
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

            // add each sourceNode to sortedDictionary with distance of 0.0f
            // sortedDictionary 'nodes':
            // Keys: distance from source to node (float)
            // Values: Lists of integers, containing nodes found at that distance (List<int>)
            // dictionary 'nodesRef': reverse lookup for nodes
            // Keys: node (int)
            // Values: distance to node from source (float)
            foreach (int source in s)
            {
                Console.WriteLine(source);
                queueNode(source);
            }

            // while sortedDictionary has nodes, remove closest node, add to tree, and add edges from that node to sortedDictionary
            while (nodes.Count != 0)
            {
                int nextNode = nodes.Values.First().First<int>();
                removeNode(nextNode);
                relax(ewd, nextNode);
            }

            printSPT();

            Console.ReadKey();
        }

        // places node into nodes sortedDict and nodeRefs dict
        private static void queueNode(int node)
        {
            // if nodes does not contain list for nodes at given distance, initializes new list under distance key
            if (!nodes.ContainsKey(distanceTo[node]))
            {
                nodes.Add(distanceTo[node], new List<int>());
            }
            // adds node to list under distance key in sortedDictionary
            nodes[distanceTo[node]].Add(node);
            // adds distance to sortedDictionary under node key
            nodesRef.Add(node, distanceTo[node]);
        }

        // removes node from nodes sortedDict and nodeRefs dict
        private static void removeNode(int node)
        {
            float nodeRef = nodesRef[node];
            nodes[nodesRef[node]].Remove(node);
            // if node being removed is last under 'distance' key in sortedDict, delete the key/value in sortedDict to help with garbage collection
            if (nodes[nodesRef[node]].Count == 0)
            {
                nodes.Remove(nodesRef[node]);
            }
            nodesRef.Remove(node);
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

        private static void relax(EdgeWeightedDigraph graph, int vertex)
        {
            List<int> tails = graph.adj(vertex);

            foreach (int node in tails)
            {
                float weight = graph.getWeight(vertex, node);
                if (distanceTo[node] > distanceTo[vertex] + weight)
                {
                    distanceTo[node] = distanceTo[vertex] + weight;
                    edgeTo[node] = vertex;
                    if (nodesRef.ContainsKey(node))
                    {
                        removeNode(node);
                    }
                    queueNode(node);
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
            StreamReader txtContents = new StreamReader(pathToFile);

            try
            {
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
                string rePattern = @"(\d+)\s(\d+)\s(\d+\.\d+)";

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
                Console.WriteLine(e.Message);
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
