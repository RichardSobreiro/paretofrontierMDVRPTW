using System;
using System.IO;

namespace ParetoFrontier_MDVRPTW
{
    public static class InstanceProblemGenerator
    {
        public static void Generate(Parameters parameters, 
            bool? writeToDotnetDatFile = false, string pathDotnetDatFile = null,
            bool? writeToOptimizationStudioDataFile = false, string pathOptimizationStudioDataFile = null)
        {
            parameters.dp = CrossCutting.GetDoubleArrayArray(parameters.qViagens, 
                parameters.qBetoneiras);
            CrossCutting.GenerateMatrixRowsByColumns(parameters.dp, 7, 7, 
                parameters.qViagens, parameters.qPontosCarga);
            for (int i = 0; i < parameters.qPontosCarga; i++)
                parameters.dp[0][i] = 0;
            
            CrossCutting.GenerateMatrixRowsByColumns(parameters.dv, 10, 50, 
                parameters.qViagens, parameters.qPontosCarga);
            for (int i = 0; i < parameters.qPontosCarga; i++)
                parameters.dv[0][i] = 0;

            CrossCutting.GenerateRandomArray(parameters.td, 10, 20, parameters.qViagens);
            parameters.td[0] = 0;

            CrossCutting.GenerateRandomArray(parameters.tmaxvc, 120, 120, parameters.qViagens);
            parameters.tmaxvc[0] = 0;

            CrossCutting.GenerateRandomArray(parameters.hs, 360, 1200, parameters.qViagens);
            parameters.hs[0] = 0;

            CrossCutting.GenerateMatrixRowsByColumns(parameters.f, 100, 140, 
                parameters.qViagens, parameters.qPontosCarga);
            for (int i = 0; i < parameters.qPontosCarga; i++)
                parameters.f[0][i] = 0;

            CrossCutting.GenerateMatrixRowsByColumns(parameters.c, 40, 80, 
                parameters.qViagens, parameters.qPontosCarga);
            for (int i = 0; i < parameters.qPontosCarga; i++)
                parameters.c[0][i] = 0;

            CrossCutting.Generate01MatrixRowsByColumnsByIntervals(parameters.pb, parameters.qPontosCarga, 
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
                CrossCutting.WriteMatrixNxNToFile(file, parameters.dv, parameters.qViagens, parameters.qPontosCarga, "dv");
                CrossCutting.WriteArrayToFile(file, parameters.td, parameters.qViagens, "td");
                CrossCutting.WriteArrayToFile(file, parameters.tmaxvc, parameters.qViagens, "tmaxvc");
                CrossCutting.WriteArrayToFile(file, parameters.hs, parameters.qViagens, "hs");
                CrossCutting.WriteMatrixNxNToFile(file, parameters.f, parameters.qViagens, parameters.qPontosCarga, "f");
                CrossCutting.WriteMatrixNxNToFile(file, parameters.c, parameters.qViagens, parameters.qPontosCarga, "c");
                CrossCutting.WriteMatrixNxNToFile(file, parameters.pb, parameters.qPontosCarga, parameters.qBetoneiras, "pb");

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
                CrossCutting.WriteMatrixNxNToFile(dotnetDatFile, parameters.dv, parameters.qViagens, parameters.qPontosCarga);
                CrossCutting.WriteArrayToFile(dotnetDatFile, parameters.td, parameters.qViagens);
                CrossCutting.WriteArrayToFile(dotnetDatFile, parameters.tmaxvc, parameters.qViagens);
                CrossCutting.WriteArrayToFile(dotnetDatFile, parameters.hs, parameters.qViagens);
                CrossCutting.WriteMatrixNxNToFile(dotnetDatFile, parameters.f, parameters.qViagens, parameters.qPontosCarga);
                CrossCutting.WriteMatrixNxNToFile(dotnetDatFile, parameters.c, parameters.qViagens, parameters.qPontosCarga);
                CrossCutting.WriteMatrixNxNToFile(dotnetDatFile, parameters.pb, parameters.qPontosCarga, parameters.qBetoneiras);

                dotnetDatFile.Close();
            }
        }
    }
}
