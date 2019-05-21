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

            INumVar[][][] x;
            for(var i = 0; i < qViagens; i++)

            //dvar boolean y[I][I][B]; // Se a betoneira b atende a viagem j após a viagem i

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
