using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.Ape;

namespace Krusefy.DBSCAN
{
    internal class Cluster
    {
        internal List<Color> colors = new List<Color>();

        internal Color GetP90Color()
        {
            int[] reds = new int[this.colors.Count];
            int[] greens = new int[this.colors.Count];
            int[] blues = new int[this.colors.Count];

            for (int i = 0; i < this.colors.Count; i++)
            {
                reds[i] = this.colors[i].R;
                greens[i] = this.colors[i].G;
                blues[i] = this.colors[i].B;
            }
            int rPercentile = (int)Math.Round(this.Percentile(reds, 0.9f));
            int gPercentile = (int)Math.Round(this.Percentile(greens, 0.9f));
            int bPercentile = (int)Math.Round(this.Percentile(blues, 0.9f));

            return Color.FromArgb(rPercentile, gPercentile, bPercentile);
        }

        private double Percentile(int[] sequence, float percentile)
        {
            Array.Sort(sequence);
            double realIndex = percentile * (sequence.Length - 1);
            int index = (int)realIndex;
            double frac = realIndex - index;

            if (index + 1 < sequence.Length)
            {
                return sequence[index] * (1 - frac) + sequence[index + 1] * frac;
            }
            else
                return sequence[index];
        }
    }
}
