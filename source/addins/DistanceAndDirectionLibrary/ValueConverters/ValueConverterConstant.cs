using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistanceAndDirectionLibrary
{
    public class ValueConverterConstant
    {
        public const double GradianToDegree = (360D / 400D);
        public const double DegreeToGradian = (400D / 360D);
        public const double MilsToGradian = (400D / 6400D);
        public const double GradianToMils = (6400D / 400D);
        public const double MilsToDegree = (360D / 6400D);
        public const double DegreeToMils = (6400D / 360D);
    }
}
