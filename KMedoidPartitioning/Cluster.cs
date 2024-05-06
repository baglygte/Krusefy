using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Krusefy.KMedoidPartitioning
{
    internal class Cluster : DataPoint
    {
        internal Cluster(float[] coordinates) : base(coordinates)
        {
            //this.Coordinates = coordinates;
        }
        internal float CalculateDissimilarity(DataPoint point)
        {
            float dissimilarity = 0;
            for (int i = 0; i < 3; i++)
            {
                dissimilarity += Math.Abs(this.Coordinates[i] - point.Coordinates[i]);
            }
            return dissimilarity;
        }
    }
}
