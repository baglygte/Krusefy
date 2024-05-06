using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krusefy.KMedoidPartitioning
{
    internal class KMedoidPartitioner
    {
        private List<DataPoint> dataPoints;
        private List<Cluster> clusters;

        internal KMedoidPartitioner(List<float[]> dataCoordinates)
        {
            this.dataPoints = new List<DataPoint>();
            foreach (float[] dataCoordinate in dataCoordinates)
            {
                this.dataPoints.Add(new DataPoint(dataCoordinate));
            }
        }

        internal List<float[]> GetClusterPoints(int numberOfPoints)
        {
            this.InitializeClusters(numberOfPoints);

            float minCost = CalculateCost();
            int iterations = 0;
            while(iterations< 1000)
            {
                int randomIndex = this.GetRandomPointIndex();
                DataPoint randomPoint = this.dataPoints[randomIndex];
                Cluster pointCluster = randomPoint.AssignedCluster;

                DataPoint newPoint;
                Cluster newCluster;
                (newPoint, newCluster) = this.SwapClusterAndPoint(pointCluster, randomPoint);

                float cost = this.CalculateCost();
                if (cost < minCost)
                {
                    minCost = cost;
                }
                else
                {
                    this.SwapClusterAndPoint(newCluster, newPoint);
                    break;
                }
            }
            return this.ConvertClustersToFloats(clusters);
        }

        private List<float[]> ConvertClustersToFloats(List<Cluster> clusters)
        {
            List<float[]> floats = new List<float[]>();
            foreach(Cluster cluster in clusters)
            {
                floats.Add(cluster.ConvertToFloatArray());
            }

            return floats;
        }
        
        private float CalculateCost()
        {
            float cost = 0;
            foreach (DataPoint point in this.dataPoints)
            {
                point.CalculateDissimilarities(this.clusters);
                cost += point.Cost;
            }
            return cost;
        }
        
        private void InitializeClusters(int numberOfClusters)
        {
            this.clusters = new List<Cluster>();
            for (int i = 0; i < numberOfClusters; i++)
            {
                int randomIndex = this.GetRandomPointIndex();
                float[] randomPoint = this.dataPoints[randomIndex].Coordinates;
                Cluster cluster = new Cluster(randomPoint);
                this.clusters.Add(cluster);
                this.dataPoints.RemoveAt(randomIndex);
            }
        }

        private (DataPoint clusterAsPoint, Cluster pointAsCluster) SwapClusterAndPoint(Cluster cluster, DataPoint dataPoint)
        {
            DataPoint clusterAsPoint = new DataPoint(cluster.Coordinates);
            Cluster pointAsCluster = new Cluster(dataPoint.Coordinates);

            this.clusters.Remove(cluster);
            this.clusters.Add(pointAsCluster);

            this.dataPoints.Remove(dataPoint);
            this.dataPoints.Add(clusterAsPoint);

            return (clusterAsPoint, pointAsCluster);
        }
        
        private int GetRandomPointIndex()
        {
            return (int)Math.Round(this.dataPoints.Count / 2d);
            //return new Random().Next(this.dataPoints.Count);
        }
    }
}
