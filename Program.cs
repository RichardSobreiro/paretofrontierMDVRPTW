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
            string filename = "";
            ReadDataFromFile(filename);

            Cplex cplex = new Cplex();

            //Variables
            INumVar[] tfp = new INumVar[qViagens];
            INumVar[] tfb = new INumVar[qViagens];
            INumVar[] hcc = new INumVar[qViagens];

            INumVar[][][] x = new INumVar[qViagens][][];
            for (var i = 0; i < qViagens; i++)
            {
                for (var j = 0; j < qViagens; j++)
                {
                    x[i][j] = cplex.BoolVarArray(qPontosCarga);
                }
            }
            INumVar[][][] y = new INumVar[qViagens][][];
            for (var i = 0; i < qViagens; i++)
            {
                for (var j = 0; j < qViagens; j++)
                {
                    x[i][j] = cplex.BoolVarArray(qBetoneiras);
                }
            }
            INumVar[] atrc = new INumVar[qViagens];
            INumVar[] avnc = new INumVar[qViagens];
            INumVar[] atrp = new INumVar[qViagens];
            INumVar[] avnp = new INumVar[qViagens];
            INumVar[][] vp = new INumVar[qViagens][];
            for(var i = 0; i < qViagens; i++)
            {
                vp[i] = cplex.BoolVarArray(qPontosCarga);
            }
            INumVar[][] vb = new INumVar[qViagens][];
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
                    for (int k = 0; k < qViagens; k++)
                    {
                        if(i == j)
                        {
                            cplex.AddLe(0, x[i][j][k]);
                        }
                    }
                }
            }
            //ViagemNaoPodeSucederElaMesmaNaBetoneira:
            //forall(i in I, j in I, b in B : i == j){
            //    y[i][j][b] <= 0;
            //}

            //TodaViagemDeveAntecederSucederNoMaximoAlgumaViagemNaPesagemExcetoAPrimeira:
            //forall(p in K, v in I){
            //    sum(i in I)(x[i][v][p]) <= 1;
            //    sum(i in I)(x[v][i][p]) <= 1;
            //    sum(i in I)(x[v][i][p]) <= sum(i in I)(x[i][v][p]);
            //    sum(i in I)(x[v][i][p]) >= sum(i in I)(x[i][v][p]);
            //}

            //TodaViagemDeveAntecederSucederNoMaximoApenasUmaViagemEmUmaBetoneiraExcetoAPrimeira:
            //forall(b in B, v in I) {
            //    sum(i in I)(y[i][v][b]) <= 1;
            //    sum(i in I)(y[v][i][b]) <= 1;
            //    sum(i in I)(y[v][i][b]) <= sum(i in I)(y[i][v][b]);
            //    sum(i in I)(y[v][i][b]) >= sum(i in I)(y[i][v][b]);
            //}

            //ViagemFicticia1AntecedeNoMaximoAlgumaViagemEmCadaPontoDeCarga:
            //forall(p in K){
            //    sum(v in I : v > 1)(x[1][v][p]) <= 1;
            //}

            //ViagemFicticia1AntecedeNoMaximoAlgumaViagemEmCadaBetoneira:
            //forall(b in B) {
            //    sum(v in I : v > 1)(y[1][v][b]) <= 1;
            //}
            //SequenciamentoDePesagemDeBetoneirasNoMesmoPontoDeCarga:
            //forall(p in K, i in I, j in I : j > 1){
            //    (M * (1 - (x[i][j][p]))) + tfp[j] >=
            //        tfp[i] +
            //        sum(p in K)(vp[j][p] * dp[j][p]);
            //}
            //GarantiaTempoDeVidaDoConcreto:
            //forall(j in I){
            //    hs[j] - tfp[j] <= tmaxvc[j];
            //}
            //SequenciamentoDoAtendimentoDeViagensPelaMesmaBetoneira:
            //forall(b in B, i in I, j in I: j > 1){
            //    (M * (1 - y[i][j][b])) + tfb[j] >=
            //        tfb[i] +
            //        sum(p in K)(vp[j][p] * (dp[j][p] + (2 * dv[j][p]))) +
            //        td[j];

            //    hcc[j] >= tfb[j] - td[j] - sum(p in K)(vp[j][p] * (dv[j][p] + dp[j][p]));
            //    hcc[j] <= tfb[j] - td[j] - sum(p in K)(vp[j][p] * (dv[j][p] + dp[j][p]));
            //}
            //SeAViagemSucedeAlgumaViagemEmUmPontoDeCargaElaDeveSerAtribuidaAoPontoDeCarga:
            //forall(v in I, p in K){
            //    vp[v][p] >= sum(i in I)(x[i][v][p]);
            //    //vp[v][p] <= sum(i in I)(x[i][v][p]);	
            //}
            //SeAViagemEhAtendidaPelaBetoneiraAposAlgumaViagemEssaViagemSoPodeSerAtendidaPorEssaBetoneira:
            //forall(v in I, b in B){
            //    vb[v][b] >= sum(i in I)(y[i][v][b]);
            //    vb[v][b] <= sum(i in I)(y[i][v][b]);
            //}
            //AtribuicaoViagemPontoCarga:
            //forall(v in I){
            //    sum(p in K)(vp[v][p]) <= 1;
            //}
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
            //AtrasoAvancoDasViagens:
            //forall(i in I){
            //    sum(b in B)(vb[i][b]) >= sum(p in K)(vp[i][p]);
            //    //sum(b in B)(vb[i][b]) <= sum(p in K)(vp[i][p]);

            //    //sum(b in B)(vb[i][b]) <= sum(p in K)(vp[i][p]);

            //    atrc[i] >= hcc[i] - hs[i];
            //    atrc[i] <= hcc[i] - hs[i];

            //    avnc[i] >= hs[i] - hcc[i];
            //    avnc[i] <= hs[i] - hcc[i];

            //    atrp[i] >= hcc[i] - sum(p in K)((dv[i][p] + dp[i][p]) * vp[i][p]) - tfp[i];
            //    atrp[i] <= hcc[i] - sum(p in K)((dv[i][p] + dp[i][p]) * vp[i][p]) - tfp[i];

            //    avnp[i] >= sum(p in K)((dv[i][p] + dp[i][p]) * vp[i][p]) + tfp[i] - hcc[i];
            //    avnp[i] <= sum(p in K)((dv[i][p] + dp[i][p]) * vp[i][p]) + tfp[i] - hcc[i];
            //}
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
