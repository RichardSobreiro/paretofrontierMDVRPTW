using ILOG.Concert;
using ILOG.CPLEX;


public class ParetoFrontier
{
    internal static int qViagens; // Quantidade de viagens
    internal static int qPontosCarga; // Quantidade de pontos de carga que podem produzir concreto para atendimento das viagens
    internal static int qBetoneiras; // Quantidade de betoneiras
    internal static int M;

    // Conjuntos

    //range I = 1..qViagens;
    //range K = 1..qPontosCarga;
    //range B = 1..qBetoneiras;

    // Parâmetros

    internal static double[][] dp; // Tempo gasto para pesagem da viagem v no ponto de carga p
    internal static double[][] dv; // Tempo gasto no trajeto entre o ponto de carga p e o cliente da viagem i
    internal static double[] td; // Tempo de descarga no cliente da viagem v
    internal static double[] tmaxvc; // Tempo maximo de vida do concreto da viagem v
    internal static double[] hs; // Horario solicitado pelo cliente para chegada da viagem v

    internal static double[][] f; // Faturamento pelo atendimento da viagem v pelo ponto de carga p (Custo dos insumos + Custo rodoviário + Custo de pessoal)
    internal static double[][] c; // Custo de atendimento da viagem v pelo ponto de carga p (Custo dos insumos + Custo rodoviário + Custo de pessoal)

    internal static double[][] pb; // Se a betoneira b pertence ao ponto de carga p

    internal static void ReadDataFromFile(string fileName)
    {
        InputDataReader reader = new InputDataReader(fileName);

        qViagens = reader.ReadInt();
        qPontosCarga = reader.ReadInt();
        qBetoneiras = reader.ReadInt();
        M = reader.ReadInt();

        dp = reader.ReadDoubleArrayArray();
        dv = reader.ReadDoubleArrayArray();
        td = reader.ReadDoubleArray();
        tmaxvc = reader.ReadDoubleArray();
        hs = reader.ReadDoubleArray();

        f = reader.ReadDoubleArrayArray();
        c = reader.ReadDoubleArrayArray();

        pb = reader.ReadDoubleArrayArray();
    }

    public static void Main(string[] args)
    {
        try
        {
            string filename = "./Data.dat";
            ReadDataFromFile(filename);
            M = 10000;

            using (Cplex cplex = new Cplex())
            {

                //Variables
                INumVar[] atrc = cplex.NumVarArray(qViagens, 0.0, double.MaxValue);
                INumVar[] avnc = cplex.NumVarArray(qViagens, 0.0, double.MaxValue);
                INumVar[] atrp = cplex.NumVarArray(qViagens, 0.0, double.MaxValue);
                INumVar[] avnp = cplex.NumVarArray(qViagens, 0.0, double.MaxValue);
                INumVar[] tfp = cplex.NumVarArray(qViagens, 0.0, double.MaxValue);
                INumVar[] tfb = cplex.NumVarArray(qViagens, 0.0, double.MaxValue);
                INumVar[] hcc = cplex.NumVarArray(qViagens, 0.0, double.MaxValue);

                IIntVar[][][] x = new IIntVar[qViagens][][];
                IIntVar[][][] y = new IIntVar[qViagens][][];
                IIntVar[][] vp = new IIntVar[qViagens][];
                IIntVar[][] vb = new IIntVar[qViagens][];

                for (var i = 0; i < qViagens; i++)
                {
                    x[i] = new IIntVar[qViagens][];
                    for (var j = 0; j < qViagens; j++)
                    {
                        x[i][j] = cplex.BoolVarArray(qPontosCarga);
                    }
                }
                for (var i = 0; i < qViagens; i++)
                {
                    y[i] = new IIntVar[qViagens][];
                    for (var j = 0; j < qViagens; j++)
                    {
                        y[i][j] = cplex.BoolVarArray(qBetoneiras);
                    }
                }
                for (var i = 0; i < qViagens; i++)
                {
                    vp[i] = cplex.BoolVarArray(qPontosCarga);
                }
                for (var i = 0; i < qViagens; i++)
                {
                    vb[i] = cplex.BoolVarArray(qBetoneiras);
                }

                //ViagemNaoPodeSucederElaMesmaNaPesagem:
                //forall(i in I, j in I, k in K : i == j){
                //    x[i][j][k] <= 0;
                //}
                for (int i = 0; i < qViagens; i++)
                {
                    for (int j = 0; j < qViagens; j++)
                    {
                        for (int k = 0; k < qPontosCarga; k++)
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
                for (int i = 0; i < qViagens; i++)
                {
                    for (int j = 0; j < qViagens; j++)
                    {
                        for (int b = 0; b < qBetoneiras; b++)
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
                for (int v = 0; v < qViagens; v++)
                {
                    for (int p = 0; p < qPontosCarga; p++)
                    {
                        ILinearNumExpr sum1 = cplex.LinearNumExpr();
                        ILinearNumExpr sum2 = cplex.LinearNumExpr();
                        for (int i = 0; i < qViagens; i++)
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
                for (int v = 0; v < qViagens; v++)
                {
                    for (int b = 0; b < qBetoneiras; b++)
                    {
                        ILinearNumExpr sum1 = cplex.LinearNumExpr();
                        ILinearNumExpr sum2 = cplex.LinearNumExpr();
                        for (int i = 0; i < qViagens; i++)
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
                for (int p = 0; p < qPontosCarga; p++)
                {
                    ILinearNumExpr sum = cplex.LinearNumExpr();
                    for (int v = 1; v < qViagens; v++)
                    {
                        sum.AddTerm(1, x[1][v][p]);
                    }
                    cplex.AddLe(sum, 1);
                }
                //ViagemFicticia1AntecedeNoMaximoAlgumaViagemEmCadaBetoneira:
                //forall(b in B) {
                //    sum(v in I : v > 1)(y[1][v][b]) <= 1;
                //}
                for (int b = 0; b < qBetoneiras; b++)
                {
                    ILinearNumExpr sum = cplex.LinearNumExpr();
                    for (int v = 1; v < qViagens; v++)
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
                for (int p = 0; p < qPontosCarga; p++)
                {
                    for (int i = 0; i < qViagens; i++)
                    {
                        for (int j = 1; j < qViagens; j++)
                        {
                            cplex.AddGe(cplex.Sum(cplex.Prod(M, cplex.Diff(1, x[i][j][p])), tfp[j]),
                                cplex.Sum(tfp[i], cplex.ScalProd(vp[j], dp[j])));
                        }
                    }
                }
                //GarantiaTempoDeVidaDoConcreto:
                //forall(j in I){
                //    hs[j] - tfp[j] <= tmaxvc[j];
                //}
                for (int j = 0; j < qViagens; j++)
                {
                    cplex.AddLe(cplex.Diff(hs[j], tfp[j]), tmaxvc[j]);
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
                for (int b = 0; b < qBetoneiras; b++)
                {
                    for (int i = 0; i < qViagens; i++)
                    {
                        for (int j = 1; j < qViagens; j++)
                        {
                            cplex.AddGe(cplex.Sum(cplex.Prod(M, cplex.Diff(1, y[i][j][b])), tfb[j]),
                                cplex.Sum(tfb[i],
                                cplex.Sum(
                                    cplex.Sum(
                                        cplex.ScalProd(vp[j], dp[j]),
                                        cplex.ScalProd(vp[j], dv[j]),
                                        cplex.ScalProd(vp[j], dv[j])),
                                td[j])));

                            cplex.AddGe(hcc[j],
                                cplex.Diff(tfb[j],
                                    cplex.Diff(td[j],
                                        cplex.Sum(
                                            cplex.ScalProd(vp[j], dv[j]),
                                            cplex.ScalProd(vp[j], dp[j])))));

                            cplex.AddLe(hcc[j],
                                cplex.Diff(tfb[j],
                                    cplex.Diff(td[j],
                                        cplex.Sum(
                                            cplex.ScalProd(vp[j], dv[j]),
                                            cplex.ScalProd(vp[j], dp[j])))));
                        }
                    }
                }
                //SeAViagemSucedeAlgumaViagemEmUmPontoDeCargaElaDeveSerAtribuidaAoPontoDeCarga:
                //forall(v in I, p in K){
                //    vp[v][p] >= sum(i in I)(x[i][v][p]);
                //    //vp[v][p] <= sum(i in I)(x[i][v][p]);	
                //}
                for (int v = 0; v < qViagens; v++)
                {
                    for (int p = 0; p < qPontosCarga; p++)
                    {
                        ILinearNumExpr sum = cplex.LinearNumExpr();
                        for (int i = 1; i < qViagens; i++)
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
                for (int v = 0; v < qViagens; v++)
                {
                    for (int b = 0; b < qBetoneiras; b++)
                    {
                        ILinearNumExpr sum = cplex.LinearNumExpr();
                        for (int i = 0; i < qViagens; i++)
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
                for (int v = 0; v < qPontosCarga; v++)
                {
                    ILinearNumExpr sum = cplex.LinearNumExpr();
                    for (int p = 0; p < qPontosCarga; p++)
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
                for (int p = 0; p < qPontosCarga; p++)
                {
                    for (int b = 0; b < qBetoneiras; b++)
                    {
                        for (int v = 0; v < qViagens; v++)
                        {
                            if (pb[p][b] == 1)
                            {
                                cplex.AddLe(cplex.Sum(vb[v][b], vp[v][p]), 2 * pb[p][b]);
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
                for (int i = 0; i < qViagens; i++)
                {
                    ILinearNumExpr sumB = cplex.LinearNumExpr();
                    for (int b = 0; b < qBetoneiras; b++)
                    {
                        sumB.AddTerm(1, vb[i][b]);
                    }
                    ILinearNumExpr sumP = cplex.LinearNumExpr();
                    for (int p = 0; p < qPontosCarga; p++)
                    {
                        sumP.AddTerm(1, vp[i][p]);
                    }
                    cplex.AddGe(sumB, sumP);

                    cplex.AddGe(atrc[i], cplex.Diff(hcc[i], hs[i]));
                    cplex.AddLe(atrc[i], cplex.Diff(hcc[i], hs[i]));

                    cplex.AddGe(avnc[i], cplex.Diff(hs[i], hcc[i]));
                    cplex.AddLe(avnc[i], cplex.Diff(hs[i], hcc[i]));


                    cplex.AddGe(atrp[i],
                        cplex.Diff(
                            cplex.Diff(hcc[i],
                                cplex.Sum(
                                    cplex.ScalProd(vp[i], dv[i]),
                                    cplex.ScalProd(vp[i], dp[i]))),
                        tfp[i]));
                    cplex.AddLe(atrp[i],
                        cplex.Diff(
                            cplex.Diff(hcc[i],
                                cplex.Sum(
                                    cplex.ScalProd(vp[i], dv[i]),
                                    cplex.ScalProd(vp[i], dp[i]))),
                        tfp[i]));

                    cplex.AddLe(avnp[i],
                        cplex.Diff(
                            cplex.Sum(
                                cplex.ScalProd(vp[i], dv[i]),
                                cplex.ScalProd(vp[i], dp[i]),
                                tfp[i]),
                            hcc[i]));
                    cplex.AddGe(avnp[i],
                        cplex.Diff(
                            cplex.Sum(
                                cplex.ScalProd(vp[i], dv[i]),
                                cplex.ScalProd(vp[i], dp[i]),
                                tfp[i]),
                            hcc[i]));
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

                //maximize
                //    sum(v in I, p in K)(vp[v][p] * (f[v][p] - c[v][p]));
                INumExpr profit = cplex.NumExpr();
                for (int v = 0; v < qViagens; v++)
                {
                    for (int p = 0; p < qPontosCarga; p++)
                    {
                        profit = cplex.Sum(profit,
                            cplex.Diff(
                                cplex.Prod(vp[v][p], f[v][p]),
                                cplex.Prod(vp[v][p], c[v][p])));
                    }
                }

                cplex.AddMaximize(profit);

                if (cplex.Solve())
                {
                    System.Console.WriteLine("Solution status = " + cplex.GetStatus());
                    System.Console.WriteLine(" Optimal Value = " + cplex.ObjValue);
                }

                if (cplex.GetStatus().Equals(Cplex.Status.Infeasible))
                {
                    System.Console.WriteLine("No Solution");
                    return;
                }
                cplex.End();
            }
        }
        catch (ILOG.Concert.Exception exc)
        {
            System.Console.WriteLine("Concert exception '" + exc + "' caught");
        }
        catch (System.IO.IOException exc)
        {
            System.Console.WriteLine("Error reading file " + args[0] + ": " + exc);
        }
        catch (InputDataReader.InputDataReaderException exc)
        {
            System.Console.WriteLine(exc);
        }
    }
}
