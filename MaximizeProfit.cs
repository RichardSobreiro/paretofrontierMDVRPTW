using ILOG.Concert;
using ILOG.CPLEX;
using System;
using System.Diagnostics;

namespace ParetoFrontier_MDVRPTW
{
    public static class MaximizeProfit
    {
        public static SolutionReturn Solve(Parameters parameters)
        {
            SolutionReturn solutionReturn = new SolutionReturn();
            try
            {
                using (Cplex cplex = new Cplex())
                {
                    //Variables
                    INumVar[] atrc = cplex.NumVarArray(parameters.qViagens, 0.0, double.MaxValue);
                    INumVar[] avnc = cplex.NumVarArray(parameters.qViagens, 0.0, double.MaxValue);
                    INumVar[] atrp = cplex.NumVarArray(parameters.qViagens, 0.0, double.MaxValue);
                    INumVar[] avnp = cplex.NumVarArray(parameters.qViagens, 0.0, double.MaxValue);
                    INumVar[] tfp = cplex.NumVarArray(parameters.qViagens, 0.0, double.MaxValue);
                    INumVar[] tfb = cplex.NumVarArray(parameters.qViagens, 0.0, double.MaxValue);
                    INumVar[] hcc = cplex.NumVarArray(parameters.qViagens, 0.0, double.MaxValue);

                    IIntVar[][][] x = new IIntVar[parameters.qViagens][][];
                    IIntVar[][][] y = new IIntVar[parameters.qViagens][][];
                    IIntVar[][] vp = new IIntVar[parameters.qViagens][];
                    IIntVar[][] vb = new IIntVar[parameters.qViagens][];

                    IIntVar qtdvnap = cplex.IntVar(0, int.MaxValue, "qtdvnap");
                    INumVar somatorioAtrasos = 
                        cplex.NumVar(0.0,
                        parameters.somatorioAtrasos.HasValue ? 
                            parameters.somatorioAtrasos.Value : double.MaxValue, "somatorioAtrasos");
                    cplex.Add(somatorioAtrasos);

                    for (var i = 0; i < parameters.qViagens; i++)
                    {
                        x[i] = new IIntVar[parameters.qViagens][];
                        for (var j = 0; j < parameters.qViagens; j++)
                        {
                            x[i][j] = cplex.BoolVarArray(parameters.qPontosCarga);
                        }
                    }
                    for (var i = 0; i < parameters.qViagens; i++)
                    {
                        y[i] = new IIntVar[parameters.qViagens][];
                        for (var j = 0; j < parameters.qViagens; j++)
                        {
                            y[i][j] = cplex.BoolVarArray(parameters.qBetoneiras);
                        }
                    }
                    for (var i = 0; i < parameters.qViagens; i++)
                    {
                        vp[i] = cplex.BoolVarArray(parameters.qPontosCarga);
                    }
                    for (var i = 0; i < parameters.qViagens; i++)
                    {
                        vb[i] = cplex.BoolVarArray(parameters.qBetoneiras);
                    }

                    //ViagemNaoPodeSucederElaMesmaNaPesagem:
                    //forall(i in I, j in I, k in K : i == j){
                    //    x[i][j][k] <= 0;
                    //}
                    for (int i = 0; i < parameters.qViagens; i++)
                    {
                        for (int j = 0; j < parameters.qViagens; j++)
                        {
                            for (int k = 0; k < parameters.qPontosCarga; k++)
                            {
                                if (i == j)
                                {
                                    cplex.AddLe(x[i][j][k], 0);
                                }
                            }
                        }
                    }
                    //ViagemNaoPodeSucederElaMesmaNaBetoneira:
                    //forall(i in I, j in I, b in B : i == j){
                    //    y[i][j][b] <= 0;
                    //}
                    for (int i = 0; i < parameters.qViagens; i++)
                    {
                        for (int j = 0; j < parameters.qViagens; j++)
                        {
                            for (int b = 0; b < parameters.qBetoneiras; b++)
                            {
                                if (i == j)
                                {
                                    cplex.AddLe(y[i][j][b], 0);
                                }
                            }
                        }
                    }
                    //TodaViagemDeveAntecederSucederNoMaximoAlgumaViagemNaPesagemExcetoAPrimeira:
                    //forall(p in K, v in I){
                    //    sum(i in I)(x[i][v][p]) <= 1;
                    //    sum(i in I)(x[v][i][p]) <= 1;
                    //    sum(i in I)(x[v][i][p]) <= sum(i in I)(x[i][v][p]);
                    //    sum(i in I)(x[v][i][p]) >= sum(i in I)(x[i][v][p]);
                    //}
                    for (int v = 0; v < parameters.qViagens; v++)
                    {
                        for (int p = 0; p < parameters.qPontosCarga; p++)
                        {
                            ILinearNumExpr sum1 = cplex.LinearNumExpr();
                            ILinearNumExpr sum2 = cplex.LinearNumExpr();
                            for (int i = 0; i < parameters.qViagens; i++)
                            {
                                sum1.AddTerm(1, x[i][v][p]);
                                sum2.AddTerm(1, x[v][i][p]);
                            }
                            cplex.AddLe(sum1, 1);
                            cplex.AddLe(sum2, 1);

                            cplex.AddLe(sum2, sum1);
                            cplex.AddLe(sum1, sum2);
                        }
                    }
                    //TodaViagemDeveAntecederSucederNoMaximoApenasUmaViagemEmUmaBetoneiraExcetoAPrimeira:
                    //forall(b in B, v in I) {
                    //    sum(i in I)(y[i][v][b]) <= 1;
                    //    sum(i in I)(y[v][i][b]) <= 1;
                    //    sum(i in I)(y[v][i][b]) <= sum(i in I)(y[i][v][b]);
                    //    sum(i in I)(y[v][i][b]) >= sum(i in I)(y[i][v][b]);
                    //}
                    for (int v = 0; v < parameters.qViagens; v++)
                    {
                        for (int b = 0; b < parameters.qBetoneiras; b++)
                        {
                            ILinearNumExpr sum1 = cplex.LinearNumExpr();
                            ILinearNumExpr sum2 = cplex.LinearNumExpr();
                            for (int i = 0; i < parameters.qViagens; i++)
                            {
                                sum1.AddTerm(1, y[i][v][b]);
                                sum2.AddTerm(1, y[v][i][b]);
                            }
                            cplex.AddLe(sum1, 1);
                            cplex.AddLe(sum2, 1);

                            cplex.AddLe(sum2, sum1);
                            cplex.AddLe(sum1, sum2);
                        }
                    }
                    //ViagemFicticia1AntecedeNoMaximoAlgumaViagemEmCadaPontoDeCarga:
                    //forall(p in K){
                    //    sum(v in I : v > 1)(x[1][v][p]) <= 1;
                    //}
                    for (int p = 0; p < parameters.qPontosCarga; p++)
                    {
                        ILinearNumExpr sum = cplex.LinearNumExpr();
                        for (int v = 1; v < parameters.qViagens; v++)
                        {
                            sum.AddTerm(1, x[1][v][p]);
                        }
                        cplex.AddLe(sum, 1);
                    }
                    //ViagemFicticia1AntecedeNoMaximoAlgumaViagemEmCadaBetoneira:
                    //forall(b in B) {
                    //    sum(v in I : v > 1)(y[1][v][b]) <= 1;
                    //}
                    for (int b = 0; b < parameters.qBetoneiras; b++)
                    {
                        ILinearNumExpr sum = cplex.LinearNumExpr();
                        for (int v = 1; v < parameters.qViagens; v++)
                        {
                            sum.AddTerm(1, y[1][v][b]);
                        }
                        cplex.AddLe(sum, 1);
                    }
                    //SequenciamentoDePesagemDeBetoneirasNoMesmoPontoDeCarga:
                    //forall(p in K, i in I, j in I : j > 1){
                    //    (M * (1 - (x[i][j][p]))) + tfp[j] >=
                    //        tfp[i] +
                    //        sum(p in K)(vp[j][p] * dp[j][p]);
                    //}
                    for (int p = 0; p < parameters.qPontosCarga; p++)
                    {
                        for (int i = 0; i < parameters.qViagens; i++)
                        {
                            for (int j = 1; j < parameters.qViagens; j++)
                            {
                                cplex.AddGe(cplex.Sum(cplex.Prod(parameters.M, cplex.Diff(1, x[i][j][p])), tfp[j]),
                                    cplex.Sum(tfp[i], cplex.ScalProd(vp[j], parameters.dp[j])));
                            }
                        }
                    }
                    //GarantiaTempoDeVidaDoConcreto:
                    //forall(j in I){
                    //    hs[j] - tfp[j] <= tmaxvc[j];
                    //}
                    for (int j = 0; j < parameters.qViagens; j++)
                    {
                        cplex.AddLe(cplex.Diff(parameters.hs[j], tfp[j]), parameters.tmaxvc[j]);
                    }
                    //SequenciamentoDoAtendimentoDeViagensPelaMesmaBetoneira:
                    //forall(b in B, i in I, j in I: j > 1){
                    //    (M * (1 - y[i][j][b])) + tfb[j] >=
                    //        tfb[i] +
                    //        sum(p in K)(vp[j][p] * (dp[j][p] + (2 * dv[j][p]))) +
                    //        td[j];

                    //    hcc[j] >= tfb[j] - td[j] - sum(p in K)(vp[j][p] * (dv[j][p] + dp[j][p]));
                    //    hcc[j] <= tfb[j] - td[j] - sum(p in K)(vp[j][p] * (dv[j][p] + dp[j][p]));
                    //}
                    for (int b = 0; b < parameters.qBetoneiras; b++)
                    {
                        for (int i = 0; i < parameters.qViagens; i++)
                        {
                            for (int j = 1; j < parameters.qViagens; j++)
                            {
                                cplex.AddGe(cplex.Sum(cplex.Prod(parameters.M, cplex.Diff(1, y[i][j][b])), tfb[j]),
                                    cplex.Sum(tfb[i],
                                    cplex.Sum(
                                        cplex.Sum(
                                            cplex.ScalProd(vp[j], parameters.dp[j]),
                                            cplex.ScalProd(vp[j], parameters.dv[j]),
                                            cplex.ScalProd(vp[j], parameters.dv[j])),
                                    parameters.td[j])));

                                cplex.AddGe(hcc[j],
                                    cplex.Diff(tfb[j],
                                        cplex.Diff(parameters.td[j],
                                            cplex.Sum(
                                                cplex.ScalProd(vp[j], parameters.dv[j]),
                                                cplex.ScalProd(vp[j], parameters.dp[j])))));

                                cplex.AddLe(hcc[j],
                                    cplex.Diff(tfb[j],
                                        cplex.Diff(parameters.td[j],
                                            cplex.Sum(
                                                cplex.ScalProd(vp[j], parameters.dv[j]),
                                                cplex.ScalProd(vp[j], parameters.dp[j])))));
                            }
                        }
                    }
                    //SeAViagemSucedeAlgumaViagemEmUmPontoDeCargaElaDeveSerAtribuidaAoPontoDeCarga:
                    //forall(v in I, p in K){
                    //    vp[v][p] >= sum(i in I)(x[i][v][p]);
                    //    //vp[v][p] <= sum(i in I)(x[i][v][p]);	
                    //}
                    for (int v = 0; v < parameters.qViagens; v++)
                    {
                        for (int p = 0; p < parameters.qPontosCarga; p++)
                        {
                            ILinearNumExpr sum = cplex.LinearNumExpr();
                            for (int i = 1; i < parameters.qViagens; i++)
                            {
                                sum.AddTerm(1, x[i][v][p]);
                            }
                            cplex.AddGe(vp[v][p], sum);
                        }
                    }
                    //SeAViagemEhAtendidaPelaBetoneiraAposAlgumaViagemEssaViagemSoPodeSerAtendidaPorEssaBetoneira:
                    //forall(v in I, b in B){
                    //    vb[v][b] >= sum(i in I)(y[i][v][b]);
                    //    vb[v][b] <= sum(i in I)(y[i][v][b]);
                    //}
                    for (int v = 0; v < parameters.qViagens; v++)
                    {
                        for (int b = 0; b < parameters.qBetoneiras; b++)
                        {
                            ILinearNumExpr sum = cplex.LinearNumExpr();
                            for (int i = 0; i < parameters.qViagens; i++)
                            {
                                sum.AddTerm(1, y[i][v][b]);
                            }
                            cplex.AddGe(vb[v][b], sum);
                            cplex.AddLe(vb[v][b], sum);
                        }
                    }
                    //AtribuicaoViagemPontoCarga:
                    //forall(v in I){
                    //    sum(p in K)(vp[v][p]) <= 1;
                    //}
                    for (int v = 0; v < parameters.qPontosCarga; v++)
                    {
                        ILinearNumExpr sum = cplex.LinearNumExpr();
                        for (int p = 0; p < parameters.qPontosCarga; p++)
                        {
                            sum.AddTerm(1, vp[v][p]);
                        }
                        cplex.AddLe(sum, 1);
                    }
                    //forall(p in K, b in B, v in I){
                    //    if (pb[p][b] == 1)
                    //    {
                    //        vb[v][b] + vp[v][p] <= 2 * pb[p][b];
                    //    }
                    //    else
                    //    {
                    //        vb[v][b] + vp[v][p] <= 1;
                    //    }
                    //}
                    for (int p = 0; p < parameters.qPontosCarga; p++)
                    {
                        for (int b = 0; b < parameters.qBetoneiras; b++)
                        {
                            for (int v = 0; v < parameters.qViagens; v++)
                            {
                                if (parameters.pb[p][b] == 1)
                                {
                                    cplex.AddLe(cplex.Sum(vb[v][b], vp[v][p]), 2 * parameters.pb[p][b]);
                                }
                                else
                                {
                                    cplex.AddLe(cplex.Sum(vb[v][b], vp[v][p]), 1);
                                }
                            }
                        }
                    }
                    //AtrasoAvancoDasViagens:
                    //forall(i in I){
                    //    sum(b in B)(vb[i][b]) >= sum(p in K)(vp[i][p]);

                    //    atrc[i] >= hcc[i] - hs[i];
                    //    atrc[i] <= hcc[i] - hs[i];

                    //    avnc[i] >= hs[i] - hcc[i];
                    //    avnc[i] <= hs[i] - hcc[i];

                    //    atrp[i] >= hcc[i] - sum(p in K)((dv[i][p] + dp[i][p]) * vp[i][p]) - tfp[i];
                    //    atrp[i] <= hcc[i] - sum(p in K)((dv[i][p] + dp[i][p]) * vp[i][p]) - tfp[i];

                    //    avnp[i] >= sum(p in K)((dv[i][p] + dp[i][p]) * vp[i][p]) + tfp[i] - hcc[i];
                    //    avnp[i] <= sum(p in K)((dv[i][p] + dp[i][p]) * vp[i][p]) + tfp[i] - hcc[i];
                    //}
                    for (int i = 0; i < parameters.qViagens; i++)
                    {
                        ILinearNumExpr sumB = cplex.LinearNumExpr();
                        for (int b = 0; b < parameters.qBetoneiras; b++)
                        {
                            sumB.AddTerm(1, vb[i][b]);
                        }
                        ILinearNumExpr sumP = cplex.LinearNumExpr();
                        for (int p = 0; p < parameters.qPontosCarga; p++)
                        {
                            sumP.AddTerm(1, vp[i][p]);
                        }
                        cplex.AddGe(sumB, sumP);

                        cplex.AddGe(atrc[i], cplex.Diff(hcc[i], parameters.hs[i]));
                        //cplex.AddLe(atrc[i], cplex.Diff(hcc[i], parameters.hs[i]));

                        cplex.AddGe(avnc[i], cplex.Diff(parameters.hs[i], hcc[i]));
                        //cplex.AddLe(avnc[i], cplex.Diff(parameters.hs[i], hcc[i]));


                        cplex.AddGe(atrp[i],
                            cplex.Diff(
                                cplex.Diff(hcc[i],
                                    cplex.Sum(
                                        cplex.ScalProd(vp[i], parameters.dv[i]),
                                        cplex.ScalProd(vp[i], parameters.dp[i]))),
                            tfp[i]));
                        //cplex.AddLe(atrp[i],
                        //    cplex.Diff(
                        //        cplex.Diff(hcc[i],
                        //            cplex.Sum(
                        //                cplex.ScalProd(vp[i], parameters.dv[i]),
                        //                cplex.ScalProd(vp[i], parameters.dp[i]))),
                        //    tfp[i]));

                        cplex.AddGe(avnp[i],
                            cplex.Diff(
                                cplex.Sum(
                                    cplex.ScalProd(vp[i], parameters.dv[i]),
                                    cplex.ScalProd(vp[i], parameters.dp[i]),
                                    tfp[i]),
                                hcc[i]));
                        //cplex.AddLe(avnp[i],
                        //    cplex.Diff(
                        //        cplex.Sum(
                        //            cplex.ScalProd(vp[i], parameters.dv[i]),
                        //            cplex.ScalProd(vp[i], parameters.dp[i]),
                        //            tfp[i]),
                        //        hcc[i]));
                    }
                    //NaoNegatividadeDasVariaveisReais:
                    //forall(i in I){
                    //    atrc[i] >= 0;
                    //    avnc[i] >= 0;
                    //    atrp[i] >= 0;
                    //    avnp[i] >= 0;
                    //    tfp[i] >= 0;
                    //    tfb[i] >= 0;
                    //    hcc[i] >= 0;
                    //}
                    //Definicao das variaveis 

                    //QuantidadeViagensNaoAtendidades:
                    //qtdvnap >= 40;
                    //qtdvnap == (qViagens - sum(v in I, p in K)(vp[v][p]));
                    //cplex.AddLe(qtdvnap, parameters.qtdvnap.Value);
                    //qtdvnap == (qViagens - sum(v in I, p in K)(vp[v][p]));
                    ILinearNumExpr sumvp = cplex.LinearNumExpr();
                    for (int v = 0; v < parameters.qViagens; v++)
                    {
                        for (int p = 0; p < parameters.qPontosCarga; p++)
                        {
                            sumvp.AddTerm(1, vp[v][p]);
                        }
                    }
                    cplex.AddEq(qtdvnap, cplex.Diff(parameters.qViagens, sumvp));

                    ILinearNumExpr sumatr = cplex.LinearNumExpr();
                    for (int i = 0; i < parameters.qViagens; i++)
                    {
                        sumatr.AddTerm(1, atrc[i]);
                        sumatr.AddTerm(1, avnc[i]);
                        sumatr.AddTerm(1, atrp[i]);
                        sumatr.AddTerm(1, avnp[i]);
                    }
                    cplex.AddEq(somatorioAtrasos, sumatr);

                    //maximize
                    //    sum(v in I, p in K)(vp[v][p] * (f[v][p] - c[v][p]));
                    INumExpr profit = cplex.NumExpr();
                    for (int v = 0; v < parameters.qViagens; v++)
                    {
                        for (int p = 0; p < parameters.qPontosCarga; p++)
                        {
                            profit = cplex.Sum(profit, 
                                cplex.Prod(vp[v][p], (parameters.f[v][p] - parameters.c[v][p])));
                        }
                    }

                    cplex.AddMaximize(profit);

                    //cplex.SetOut(null);

                    cplex.SetParam(Cplex.Param.MIP.Tolerances.MIPGap, 0.1);
                    //cplex.SetParam(Cplex.DoubleParam.TimeLimit, 10.0);


                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    if (cplex.Solve())
                    {
                        stopWatch.Stop();
                        solutionReturn.Function1ObjValue = cplex.GetObjValue();
                        solutionReturn.Function2ObjValue = cplex.GetValue(somatorioAtrasos);
                        solutionReturn.qtdvnap = cplex.GetValue(qtdvnap);
                    }

                    TimeSpan ts = stopWatch.Elapsed;

                    solutionReturn.ElapsedTimeToFindSolution = ts.TotalSeconds;
                    solutionReturn.Status = cplex.GetStatus();

                    cplex.End();

                    return solutionReturn;
                }
            }
            catch (ILOG.Concert.Exception exc)
            {
                System.Console.WriteLine("Concert exception '" + exc + "' caught");
                return solutionReturn;
            }
            catch (System.IO.IOException exc)
            {
                System.Console.WriteLine("Error reading file " + parameters.dataFilename + ": " + exc);
                return solutionReturn;
            }
            catch (InputDataReader.InputDataReaderException exc)
            {
                System.Console.WriteLine(exc);
                return solutionReturn;
            }
        }
    }
}
