using System;
using System.Collections.Generic;

namespace PointCloudTraversal
{
    internal class Utilities
    {
        internal static HashSet<int> GenerateRandom(int count, int min, int max)
        {
            HashSet<int> candidates = new HashSet<int>();
            Random random = new Random();

            if (max <= min || count < 0 || (count > max - min && max - min > 0))
            {
                throw new ArgumentOutOfRangeException("Range " + min + " to " + max +
                     " (" + ((Int64)max - (Int64)min) + " values), or count " + count + " is illegal");
            }

            for (int top = max - count; top < max; top++)
            {
                if (!candidates.Add(random.Next(min, top + 1)))
                {
                    candidates.Add(top);
                }
            }

            return candidates;
        }
    }
}
