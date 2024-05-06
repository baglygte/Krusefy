using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Krusefy.DBSCAN
{
    internal class ClusterAnalyzer
    {
        private List<Cluster> clusters = new List<Cluster>();

        internal Color[] GetAccentColors(Bitmap image)
        {
            var sampleColors = this.SampleColors(image);
            var clusters = this.GetClusters(sampleColors);
            var primaryColor = clusters[clusters.Count-1].GetP90Color();

            Color secondaryColor = Color.White;
            if (clusters.Count > 1)
            { secondaryColor = clusters[clusters.Count - 2].GetP90Color(); }
            return new Color[] { primaryColor, secondaryColor };
        }

        private List<Color> SampleColors(Bitmap image)
        {
            int sampleDensity = 20;

            List<Color> sampleColors = new List<Color>();

            for (int ww = 0; ww < image.Width; ww += sampleDensity)
            {
                for (int hh = 0; hh < image.Height; hh += sampleDensity)
                {
                    Color color = image.GetPixel(ww, hh);
                    sampleColors.Add(color);
                }
            }

            return sampleColors;
        }

        private List<Cluster> GetClusters(List<Color> sampleColors)
        {
            Random random = new Random(50000);

            var clusters = new List<Cluster>();

            List<Color> notChecked; // Colors that will be checked this iteration
            List<Color> nextCheck = sampleColors; // Colots that will be checked next iteration
            int minClusterPoints = 50; // Minimum number of points in a cluster
            float minDist = 50f;

            while (nextCheck.Count > minClusterPoints)
            {
                if(clusters.Count > 10) { break; }

                notChecked = nextCheck;
                nextCheck = new List<Color>();

                int randomIndex = random.Next(0, notChecked.Count-1);

                Color sampleColor = notChecked[randomIndex];
                notChecked.RemoveAt(randomIndex);

                Cluster cluster = new Cluster();
                cluster.colors.Add(sampleColor);

                foreach (Color color in notChecked)
                {
                    double dRsqr = Math.Pow(sampleColor.R - color.R, 2);
                    double dGsqr = Math.Pow(sampleColor.B - color.B, 2);
                    double dBsqr = Math.Pow(sampleColor.G - color.G, 2);
                    double dist = Math.Sqrt(dRsqr + dGsqr + dBsqr);
                    if (dist < minDist)
                    {
                        cluster.colors.Add(color);
                    }
                    else
                    {
                        nextCheck.Add(color);
                    }
                }

                clusters.Add(cluster);
            }

            clusters = clusters.OrderBy(x=>x.colors.Count).ToList();

            return clusters;
        }
    }
}
