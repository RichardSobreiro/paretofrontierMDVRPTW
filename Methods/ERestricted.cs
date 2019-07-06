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

            parameters.qtdvnap = parameters.qViagens;

            SolutionReturn solutionReturn = new SolutionReturn();
            do
            {    
                solutionReturn = MaximizeProfit.Solve(parameters);

                if( solutionReturn.Status != Cplex.Status.Infeasible &&
                    solutionReturn.Function1ObjValue.HasValue && 
                    solutionReturn.Function2ObjValue.HasValue)
                {
                    solutionReturns.Add(solutionReturn);
                    parameters.qtdvnap = (int)solutionReturn.Function2ObjValue - 1;
                }

            } while (parameters.qtdvnap >= 0 && 
                solutionReturn?.Status != Cplex.Status.Infeasible);

            return solutionReturns;
        }
    }
}
