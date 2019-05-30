using ILOG.CPLEX;
using System;

namespace ParetoFrontier_MDVRPTW
{
    public class SolutionReturn
    {
        public Method method;
        public Cplex.Status Status { get; set; }
        public double? Function1ObjValue { get; set; }
        public double? Function2ObjValue { get; set; }
    }
}
