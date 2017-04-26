using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MultisourceDijkstra
{
    class MultisourceDijkstra
    {
        static EdgeWeightedDigraph ewd;
        static IndexMinPQ pq;
        
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

            pq = new IndexMinPQ(vCount);

            for (int v = 0; v <= vCount; v++)
            {
                distanceTo[v] = int.MaxValue;
            }

            Console.ReadKey();
        }
    }

    // Piecemeal IndexMinPQ implementation w/o methods that algo doesn't use.
    public class IndexMinPQ
    {
        public int maxLength { get; protected set; }
        public int currentLength { get; protected set; }
        private int[] pq;
        private int[] qp;
        private float[] dists;

        public IndexMinPQ (int mL)
        {
            maxLength = mL;
            currentLength = 0;
            pq = new int[maxLength + 1];
            dists = new float[maxLength + 1];
            qp = new int[maxLength + 1];
            for (int i = 0; i <= maxLength; i++)
            {
                qp[i] = -1;
            }
        }

        public bool isEmpty()
        {
            return currentLength == 0;
        }

        public bool contains(int i)
        {
            if (i < 0 || i >= maxLength)
            {
                throw new Exception("Queue index for contains not in range");
            }
            return qp[i] > -1;
        }

        private bool less (int i, int j)
        {
            return pq[i] < pq[j];
        }

        private void exch (int i, int j)
        {
            int temp = pq[i];
            pq[i] = pq[j];
            pq[j] = temp;
        }
        
        public void swim (int k)
        {
            while (k > 1 && less(k/2, k))
            {
                exch(k / 2, k);
                k = k / 2;
            }
        }

        public void sink (int k)
        {
            while (2*k <= maxLength)
            {
                int j = 2 * k;
                if (j < maxLength && less(j, j+1))
                {
                    j++;
                }
                if (!less(k, j))
                {
                    break;
                }
                exch(k, j);
                k = j;
            }
        }

        public void insert(int i, float dist)
        {
            if (i < 0 || i >= maxLength || contains(i))
            {
                throw new Exception("Could not insert");
            }

            currentLength++;
            qp[i] = currentLength;
            pq[currentLength] = i;
            dists[i] = dist;
            swim(currentLength);
        }

        public int delMinKey()
        {
            if (currentLength == 0)
            {
                throw new Exception("Queue empty at minDist call");
            }

            int min = pq[1];
            exch(1, currentLength--);
            sink(1);
            qp[min] = -1;
            pq[min + 1] = -1;
            return min;
        }

        public void changeDist (int i, float dist)
        {
            if (i < 0 || i >= maxLength || !contains(i))
            {
                throw new Exception("index not in queue or out of bounds");
            }
            dists[i] = dist;
            swim(qp[i]);
            sink(qp[i]);
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
