using System;
using System.Collections.Generic;
using System.IO;

namespace ParetoFrontier_MDVRPTW.Results
{
    public static class PrintResults
    {
        public static void PrintResultsToFile(List<SolutionReturn> solutionReturns,
            Parameters parameters, int? sequenceNumber = null)
        {
            if (solutionReturns?.Count == 0)
                return;

            var d = DateTime.Now;
            string filename = $"ERestricted-{(sequenceNumber.HasValue ? sequenceNumber.Value : 0)}";

            StreamWriter file = new StreamWriter(filename);

            foreach(SolutionReturn solution in solutionReturns)
            {
                file.WriteLine($"{solution.Function1ObjValue},{solution.Function2ObjValue},{solution.ElapsedTimeToFindSolution}");
            }

            file.Close();
        }
    }
}
