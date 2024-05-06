using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Krusefy.DBSCAN
{
    internal class ClusterAnalyzer
    {
        private List<Cluster> clusters = new List<Cluster>();

        SolidColorBrush[] GetAccentClusters(Color[] sampleColors)
        {
            return new SolidColorBrush[] { new SolidColorBrush(sampleColors[0]) };
        }
    }
}
