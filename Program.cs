using ParetoFrontier_MDVRPTW;
using ParetoFrontier_MDVRPTW.Methods;
using ParetoFrontier_MDVRPTW.Results;
using System.Collections.Generic;

public class ParetoFrontier
{
    public static void Main(string[] args)
    {
        int qViagens = 50;
        int qPontosCarga = 1;
        int qBetoneiras = 1;
        int M = 10000;
        int? qtdvnap = 50;
        GenerateParetoFrontier(Method.E_RESTRICTED, qViagens, qPontosCarga, qBetoneiras, M, qtdvnap);
    }

    public static void  GenerateParetoFrontier(Method method, int qViagens, 
        int qPontosCarga, int qBetoneiras, int M, int? qtdvnap = null)
    {
        Parameters parameters = new Parameters(qViagens, qPontosCarga, qBetoneiras, M, qtdvnap);
        List<SolutionReturn> solutionReturns = new List<SolutionReturn>();

        switch (method)
        {
            case Method.E_RESTRICTED:
                solutionReturns = ERestricted.Solve(parameters);
                break;
            default:
                solutionReturns = ERestricted.Solve(parameters);
                break;
        }

        PrintResults.PrintResultsToFile(solutionReturns, parameters);
    }
}
