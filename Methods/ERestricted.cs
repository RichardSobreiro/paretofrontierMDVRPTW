using ILOG.CPLEX;
using System.Collections.Generic;
using System.IO;

namespace ParetoFrontier_MDVRPTW.Methods
{
    public static class ERestricted
    {
        public static List<SolutionReturn> Solve(Parameters parameters)
        {
            List<SolutionReturn> solutionReturns = new List<SolutionReturn>();
            InstanceProblemGenerator.Generate(parameters, 
                true, parameters.dataFilename,
                true, parameters.dataOptimizationStudio);

            parameters.somatorioAtrasos = 10000;
            SolutionReturn solutionReturn = new SolutionReturn();
            do
            {    
                solutionReturn = MaximizeProfit.Solve(parameters);

                if(solutionReturn.Function1ObjValue.HasValue && 
                    solutionReturn.Function2ObjValue.HasValue)
                {
                    solutionReturns.Add(solutionReturn);

                    string filename = $"ERestricted.csv";

                    StreamWriter file = new StreamWriter(filename);

                    file.WriteLine($"{(int)solutionReturn.qtdvnap},{(int)solutionReturn.Function1ObjValue},{(int)solutionReturn.Function2ObjValue},{(int)solutionReturn.ElapsedTimeToFindSolution}");

                    file.Close();
                }

                parameters.somatorioAtrasos -= 500;
            } while (parameters.somatorioAtrasos >= 0 && 
                solutionReturn?.Status != Cplex.Status.Infeasible);

            return solutionReturns;
        }
    }
}
