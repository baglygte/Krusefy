using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krusefy.KMedoidPartitioning
{
    internal class DataPoint
    {
        internal float[] Coordinates { get; set; }

        internal Cluster AssignedCluster { get; set; }
        internal float Cost { get; set; }
        Dictionary<Cluster, float> dissimilarities;

        internal DataPoint(float[] coordinates)
        {
            this.Coordinates = coordinates;
        }

        internal void CalculateDissimilarities(List<Cluster> clusters)
        {
            this.dissimilarities = new Dictionary<Cluster, float>();
            foreach (Cluster cluster in clusters)
            {
                this.dissimilarities.Add(cluster, cluster.CalculateDissimilarity(this));
            }
            this.SetMinCluster();
        }
        private void SetMinCluster()
        {
            float minValue = this.dissimilarities.Min(x => x.Value);
            this.Cost = minValue;
            this.AssignedCluster = this.dissimilarities.Where(kvp => kvp.Value == minValue).Select(kvp => kvp.Key).First();
        }

        internal float[] ConvertToFloatArray()
        {
            return this.Coordinates;
        }
    }
}
