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
        static SortedDictionary<float, int> nodes;
        static SortedDictionary<int, float> nodesRef;
        
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

            nodes = new SortedDictionary<float, int>();
            nodesRef = new SortedDictionary<int, float>();

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

            foreach (int source in s)
            {
                Console.WriteLine(source);
                nodes.Add(distanceTo[source], source);
                nodesRef.Add(source, distanceTo[source]);
            }


            while (nodes.Count != 0)
            {
                int nextNode = nodes.Values.First();
                float nodeRef = nodesRef[nextNode];
                nodes.Remove(nodeRef);
                nodesRef.Remove(nextNode);
                relax(ewd, nextNode);
            }

            printSPT();

            Console.ReadKey();
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
                    if (nodesRef.ContainsValue(node))
                    {
                        float nodeRef = nodesRef[node];
                        nodes.Remove(nodeRef);
                        nodesRef.Remove(node);
                    }
                    nodes.Add(distanceTo[node], node);
                    nodesRef.Add(node, distanceTo[node]);
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
                    Console.WriteLine("Distance to: " + distanceTo[i]);
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
