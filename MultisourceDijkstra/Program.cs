﻿using System;
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

            Console.WriteLine("Which node between 0 and " + (vCount - 1) + " is the source?");
            string sText = Console.ReadLine();
            int s = Int32.Parse(sText);

            for (int v = 0; v < vCount; v++)
            {
                if (v == s)
                {
                    distanceTo[v] = 0.0f;
                }
                else
                {
                    distanceTo[v] = int.MaxValue;
                }
            }

            nodes.Add(distanceTo[s], s);
            nodesRef.Add(s, distanceTo[s]);

            while (nodes.Count != 0)
            {
                int nextNode = nodes.Values.First();
                Console.WriteLine("Taking " + nextNode + "off the queue");
                float nodeRef = nodesRef[nextNode];
                nodes.Remove(nodeRef);
                nodesRef.Remove(nextNode);
                relax(ewd, nextNode);
            }
            foreach (KeyValuePair<float, int> item in nodes)
            {
                Console.WriteLine(item.Value + ": " + item.Key);
            }

            printSPT();

            Console.ReadKey();
        }

        private static void relax(EdgeWeightedDigraph graph, int vertex)
        {
            List<int> tails = graph.adj(vertex);

            foreach (int node in tails)
            {
                float weight = graph.getWeight(vertex, node);
                Console.WriteLine("Path from " + vertex + " to " + node);
                Console.WriteLine("Checking if new distance " + (distanceTo[vertex] + weight) + " is faster than old distance " + distanceTo[node]);
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
                Console.WriteLine("Edge from " + edgeTo[i] + ".");
                Console.WriteLine("Distance to: " + distanceTo[i]);
            }
        }
    }

    public class EdgeWeightedDigraph
    {
        public int vertexCount { get; protected set; }
        public int edgeCount { get; protected set; }
        private Dictionary<int, float>[] edges;

        public EdgeWeightedDigraph (string pathToFile)
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

        public void printVertexCount ()
        {
            Console.WriteLine(vertexCount);
        }

        public void printEdgesFrom (int source)
        {
            foreach (KeyValuePair<int, float> edge in edges[source])
            {
                Console.WriteLine("The edge from " + source + " to " + edge.Key + " has weight " + edge.Value);
            }
        }

        public void addEdge(int source, int target, float weight)
        {
            edges[source].Add(target, weight);
        }
    }
}
