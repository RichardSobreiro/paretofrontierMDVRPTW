using ILOG.CPLEX;
using System.Collections.Generic;

namespace ParetoFrontier_MDVRPTW.Methods
{
    public static class ERestricted
    {
        public static List<SolutionReturn> Solve(Parameters parameters)
        {
            List<SolutionReturn> solutionReturns = new List<SolutionReturn>();
            InstanceProblemGenerator.Generate(parameters);

            bool continueSolving = true;
            do
            {    
                SolutionReturn solutionReturn = MaximizeProfit.Solve(parameters);

                if(solutionReturn?.Status == Cplex.Status.Optimal && 
                    solutionReturn.Function1ObjValue.HasValue && 
                    solutionReturn.Function2ObjValue.HasValue)
                {
                    solutionReturns.Add(solutionReturn);
                }
                else
                {
                    continueSolving = false;
                }

                parameters.qtdvnap--;
            } while (continueSolving);

            return solutionReturns;
        }
    }
}
