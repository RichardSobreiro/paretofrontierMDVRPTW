using System;
using System.IO;

namespace ParetoFrontier_MDVRPTW
{
    public static class InstanceProblemGenerator
    {
        public static void Execute(Parameters parameters, 
            bool? writeToDotnetDatFile = false, string pathDotnetDatFile = null,
            bool? writeToOptimizationStudioDataFile = false, string pathOptimizationStudioDataFile = null)
        {
            parameters.dp = new double[parameters.qViagens][parameters.qPontosCarga];
            CrossCutting.GenerateMatrixRowsByColumns(parameters.dp, 7, 7, parameters.qViagens, parameters.qPontosCarga);
            for (int i = 0; i < parameters.qPontosCarga; i++)
                parameters.dp[0, i] = 0;
            

            double[,] dv = new double[parameters.qViagens, parameters.qPontosCarga];
            CrossCutting.GenerateMatrixRowsByColumns(dv, 10, 50, parameters.qViagens, parameters.qPontosCarga);
            for (int i = 0; i < parameters.qPontosCarga; i++)
                dv[0, i] = 0;

            double[] td = new double[parameters.qViagens];
            CrossCutting.GenerateRandomArray(td, 10, 20, parameters.qViagens);
            td[0] = 0;

            double[] tmaxvc = new double[parameters.qViagens];
            CrossCutting.GenerateRandomArray(tmaxvc, 120, 120, parameters.qViagens);
            tmaxvc[0] = 0;

            double[] hs = new double[parameters.qViagens];
            CrossCutting.GenerateRandomArray(hs, 360, 1200, parameters.qViagens);
            hs[0] = 0;

            double[,] f = new double[parameters.qViagens, parameters.qPontosCarga];
            CrossCutting.GenerateMatrixRowsByColumns(f, 100, 140, parameters.qViagens, parameters.qPontosCarga);
            for (int i = 0; i < parameters.qPontosCarga; i++)
                f[0, i] = 0;

            double[,] c = new double[parameters.qViagens, parameters.qPontosCarga];
            CrossCutting.GenerateMatrixRowsByColumns(c, 40, 80, parameters.qViagens, parameters.qPontosCarga);
            for (int i = 0; i < parameters.qPontosCarga; i++)
                c[0, i] = 0;

            double[,] pb = new double[parameters.qPontosCarga, parameters.qBetoneiras];
            CrossCutting.Generate01MatrixRowsByColumnsByIntervals(pb, parameters.qPontosCarga, 
                parameters.qBetoneiras);

            StreamWriter file = null;
            if (writeToOptimizationStudioDataFile.HasValue && writeToOptimizationStudioDataFile.Value && 
                pathOptimizationStudioDataFile != null)
            {
                File.WriteAllText(pathOptimizationStudioDataFile, string.Empty);
                file = new StreamWriter(pathOptimizationStudioDataFile);

                CrossCutting.WriteToOptimizationStudioDataFile($"qViagens = {parameters.qViagens};");
                CrossCutting.WriteToOptimizationStudioDataFile($"qPontosCarga = {parameters.qPontosCarga};");
                CrossCutting.WriteToOptimizationStudioDataFile($"qBetoneiras = {parameters.qBetoneiras};");
                CrossCutting.WriteToOptimizationStudioDataFile($"M = {parameters.M};");

                CrossCutting.WriteMatrixNxNToFile(file, parameters.dp, parameters.qViagens, 
                    parameters.qPontosCarga, "dp");
                CrossCutting.WriteMatrixNxNToFile(file, dv, parameters.qViagens, parameters.qPontosCarga, "dv");
                CrossCutting.WriteArrayToFile(file, td, parameters.qViagens, "td");
                CrossCutting.WriteArrayToFile(file, tmaxvc, parameters.qViagens, "tmaxvc");
                CrossCutting.WriteArrayToFile(file, hs, parameters.qViagens, "hs");
                CrossCutting.WriteMatrixNxNToFile(file, f, parameters.qViagens, parameters.qPontosCarga, "f");
                CrossCutting.WriteMatrixNxNToFile(file, c, parameters.qViagens, parameters.qPontosCarga, "c");
                CrossCutting.WriteMatrixNxNToFile(file, pb, parameters.qPontosCarga, parameters.qBetoneiras, "pb");

                file.Close();
            }

            StreamWriter dotnetDatFile = null;
            if (writeToDotnetDatFile.HasValue && writeToDotnetDatFile.Value && pathDotnetDatFile != null)
            {
                File.WriteAllText(pathDotnetDatFile, string.Empty);
                dotnetDatFile = new StreamWriter(pathDotnetDatFile);

                CrossCutting.WriteToDotnetDatFile($"{parameters.qViagens}{Environment.NewLine}",
                writeToDotnetDatFile, dotnetDatFile);
                CrossCutting.WriteToDotnetDatFile($"{parameters.qPontosCarga}{Environment.NewLine}",
                    writeToDotnetDatFile, dotnetDatFile);
                CrossCutting.WriteToDotnetDatFile($"{parameters.qBetoneiras}{Environment.NewLine}",
                    writeToDotnetDatFile, dotnetDatFile);
                CrossCutting.WriteToDotnetDatFile($"{parameters.M}{Environment.NewLine}",
                    writeToDotnetDatFile, dotnetDatFile);

                CrossCutting.WriteMatrixNxNToFile(dotnetDatFile, parameters.dp, parameters.qViagens,
                    parameters.qPontosCarga);
                CrossCutting.WriteMatrixNxNToFile(dotnetDatFile, dv, parameters.qViagens, parameters.qPontosCarga);
                CrossCutting.WriteArrayToFile(dotnetDatFile, td, parameters.qViagens);
                CrossCutting.WriteArrayToFile(dotnetDatFile, tmaxvc, parameters.qViagens);
                CrossCutting.WriteArrayToFile(dotnetDatFile, hs, parameters.qViagens);
                CrossCutting.WriteMatrixNxNToFile(dotnetDatFile, f, parameters.qViagens, parameters.qPontosCarga);
                CrossCutting.WriteMatrixNxNToFile(dotnetDatFile, c, parameters.qViagens, parameters.qPontosCarga);
                CrossCutting.WriteMatrixNxNToFile(dotnetDatFile, pb, parameters.qPontosCarga, parameters.qBetoneiras);

                dotnetDatFile.Close();
            }
        }
    }
}
