namespace ParetoFrontier_MDVRPTW
{
    public static class GenerateParameters
    {
        public static Parameters Execute(string filename, int qViagens, int qPontosCarga,
            int qBetoneiras, int M)
        {
            Parameters parameters = new Parameters(qViagens, qPontosCarga, qBetoneiras, M);

            string path = filename;
            string pathDotnetDatFile = filename;

            InstanceProblemGenerator.Generate(parameters);

            return parameters;
        }
    }
}
