using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MultisourceDijkstra
{
    class MultisourceDijkstra
    {
        static EdgeWeightedDigraph ewd;

        static void Main()
        {
            Console.WriteLine("Filepath to txt file containing edge-weighted digraph:");
            string fp = Console.ReadLine();
            
            ewd = new EdgeWeightedDigraph(fp);

            Console.ReadKey();

        }
    }

    public class EdgeWeightedDigraph
    {
        private int vertexCount;
        private int edgeCount;
        private Dictionary<int, float>[] edges;

        public EdgeWeightedDigraph (string pathToFile)
        {
            StreamReader txtContents = new StreamReader(pathToFile);

            try
            {
                vertexCount = Int32.Parse(txtContents.ReadLine());
                edgeCount = Int32.Parse(txtContents.ReadLine());

                edges = new Dictionary<int, float>[vertexCount];

                for (int i = 0; i < vertexCount; i++)
                {
                    edges[i] = new Dictionary<int, float>();
                }

                string edgeLine = txtContents.ReadLine();
                string rePattern = @"(\d+)\s(\d+)\s(\d+\.\d+)";

                while (edgeLine != null)
                {
                    Match match = Regex.Match(edgeLine, rePattern);

                    int src = Int32.Parse(match.Groups[1].Value);
                    int tar = Int32.Parse(match.Groups[2].Value);
                    float wt = float.Parse(match.Groups[3].Value);

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
