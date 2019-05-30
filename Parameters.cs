namespace ParetoFrontier_MDVRPTW
{
    public class Parameters
    {
        public Parameters(int _qViagens, int _qPontosCarga, int _qBetoneiras, int _M, int? _qtdvnap = null)
        {
            qViagens = _qViagens;
            qPontosCarga = _qPontosCarga;
            qBetoneiras = _qBetoneiras;
            M = _M;
            qtdvnap = _qtdvnap;
        }
        public void ReadDataFromFile(string fileName)
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

        internal string dataFilename = "./Data.dat";
        internal string outputDirectoty = "./Results";

        internal int? qtdvnap; // Quantidade maxima de viagens nao atendidas

        internal int qViagens; // Quantidade de viagens
        internal int qPontosCarga; // Quantidade de pontos de carga que podem produzir concreto para atendimento das viagens
        internal int qBetoneiras; // Quantidade de betoneiras
        internal int M;

        // Conjuntos

        //range I = 1..qViagens;
        //range K = 1..qPontosCarga;
        //range B = 1..qBetoneiras;

        // Parâmetros

        internal double[][] dp; // Tempo gasto para pesagem da viagem v no ponto de carga p
        internal double[][] dv; // Tempo gasto no trajeto entre o ponto de carga p e o cliente da viagem i
        internal double[] td; // Tempo de descarga no cliente da viagem v
        internal double[] tmaxvc; // Tempo maximo de vida do concreto da viagem v
        internal double[] hs; // Horario solicitado pelo cliente para chegada da viagem v

        internal double[][] f; // Faturamento pelo atendimento da viagem v pelo ponto de carga p (Custo dos insumos + Custo rodoviário + Custo de pessoal)
        internal double[][] c; // Custo de atendimento da viagem v pelo ponto de carga p (Custo dos insumos + Custo rodoviário + Custo de pessoal)

        internal double[][] pb; // Se a betoneira b pertence ao ponto de carga p
    }
}
