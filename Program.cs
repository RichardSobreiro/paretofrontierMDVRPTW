using ParetoFrontier_MDVRPTW;

public class ParetoFrontier
{
    public static void Main(string[] args)
    {
        int qViagens = 50;
        int qPontosCarga = 1;
        int qBetoneiras = 1;
        int M = 10;
        int? qtdvnap = 50;
        GenerateParetoFrontier(Method.E_RESTRICTED, qViagens, qPontosCarga, qBetoneiras, M, qtdvnap);
    }

    public static void  GenerateParetoFrontier(Method method, int _qViagens, 
        int _qPontosCarga, int _qBetoneiras, int _M, int? _qtdvnap = null)
    {
        Parameters parameters = new Parameters(50, 1, 1, 10000, 50);
        InstanceProblemGenerator.Generate(parameters);
        MaximizeProfit.Solve(parameters);
    }
}
