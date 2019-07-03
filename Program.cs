﻿using ParetoFrontier_MDVRPTW;
using ParetoFrontier_MDVRPTW.Methods;
using ParetoFrontier_MDVRPTW.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class ParetoFrontier
{
    public static void Main(string[] args)
    {
        int qViagens = 40;
        int qPontosCarga = 2;
        int qBetoneiras = 2;
        int M = 10000;
        int? qtdvnap = qViagens;
        GenerateParetoFrontier(Method.E_RESTRICTED, qViagens, qPontosCarga, qBetoneiras, M, qtdvnap);
    }

    public static void  GenerateParetoFrontier(Method method, int qViagens, 
        int qPontosCarga, int qBetoneiras, int M, int? qtdvnap = null)
    {
        Parameters parameters = new Parameters(qViagens, qPontosCarga, qBetoneiras, M, qtdvnap);
        List<SolutionReturn> solutionReturns = new List<SolutionReturn>();

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        switch (method)
        {
            case Method.E_RESTRICTED:
                solutionReturns = ERestricted.Solve(parameters);
                break;
            default:
                solutionReturns = ERestricted.Solve(parameters);
                break;
        }
        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;
        var totalElapsedTime = ts.TotalSeconds;

        solutionReturns = solutionReturns.OrderBy(s => s.Function1ObjValue).ToList();

        PrintResults.PrintResultsToFile(solutionReturns, parameters);
    }
}
