using System.Text.Json;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Extensions.Options;
using TelemetryHealthPredictor.Config.Settings;
using TelemetryHealthPredictor.Data;
using ThpUtils;
using ThpUtils.Naming;

namespace TelemetryHealthPredictor.Config.Builders
{
    public static class BaselineBuilder
    {
        /// <summary>
        /// Index in the d² ladder of the value exceeded by 1% of normal frames.
        /// Ladder: 0 = 100%, 1 = 10%, 2 = 1%, 3 = 0.1%, ...
        /// </summary>
        private const int D2OnePercentLevelIndex = 2;

        public static Baseline Build(IServiceProvider provider)
        {
            BaselineSettings settings = provider.GetRequiredService<IOptions<BaselineSettings>>().Value;
            int parameterCount = provider.GetRequiredService<NamingData>().ParameterNames.Length;

            string folder = settings.BaselineFolderPath;

            Vector<double> qc = CSVReader.Load(settings.NameForQc,
                Path.Combine(folder, settings.NameForQc), CSVReader.ReadVector);
            Matrix<double> r0 = CSVReader.Load(settings.NameForR0,
                Path.Combine(folder, settings.NameForR0), CSVReader.ReadMatrix);
            Matrix<double> cov = CSVReader.Load(settings.NameForCov,
                Path.Combine(folder, settings.NameForCov), CSVReader.ReadMatrix);
            double[] slacks = CSVReader.Load(settings.NameForCusumSlacks,
                Path.Combine(folder, settings.NameForCusumSlacks), CSVReader.ReadDoubles);
            double[] thresholds = CSVReader.Load(settings.NameForCusumThresholds,
                Path.Combine(folder, settings.NameForCusumThresholds), CSVReader.ReadDoubles);
            double[] d2Ladder = CSVReader.Load(settings.NameForMDThresholds,
                Path.Combine(folder, settings.NameForMDThresholds), CSVReader.ReadDoubles);
            double[] cusumRawLadder = CSVReader.Load(settings.NameForCusumRawThresholds,
                Path.Combine(folder, settings.NameForCusumRawThresholds), CSVReader.ReadDoubles);

            EnsureLength(settings.NameForQc, qc.Count, parameterCount);
            EnsureSquare(settings.NameForR0, r0, parameterCount);
            EnsureSquare(settings.NameForCov, cov, parameterCount);
            EnsureLength(settings.NameForCusumSlacks, slacks.Length, parameterCount);
            EnsureLength(settings.NameForCusumThresholds, thresholds.Length, parameterCount);

            if (d2Ladder.Length <= D2OnePercentLevelIndex)
                throw new InvalidDataException(
                    $"{settings.NameForMDThresholds}: has {d2Ladder.Length} levels, needs at least {D2OnePercentLevelIndex + 1}.");

            using JsonDocument scalars = ReadScalars(settings);

            return new Baseline()
            {
                QcVector = qc,
                R0Matrix = r0,
                ForgettingFactor = GetScalar(scalars, settings, settings.ForgettingFactorKey),

                CovarianceMatrix = cov,
                MDOutliersStepSize = GetScalar(scalars, settings, settings.MDOutliersStepSizeKey),
                DistanceSquaredThreshold = d2Ladder[D2OnePercentLevelIndex],

                CusumSlacks = slacks,
                CusumThresholds = thresholds,

                D2Ladder = d2Ladder,
                CusumRawLadder = cusumRawLadder,
                D2Weight = GetScalar(scalars, settings, settings.D2WeightKey),
                TDecay = GetScalar(scalars, settings, settings.TDecayKey),
                HysteresisEntry = GetScalar(scalars, settings, settings.HysteresisEntryKey),
                HysteresisExit = GetScalar(scalars, settings, settings.HysteresisExitKey)
            };
        }

        private static JsonDocument ReadScalars(BaselineSettings settings)
        {
            string path = Path.Combine(settings.BaselineFolderPath, settings.ScalarsFileName);

            return CSVReader.Load(settings.ScalarsFileName, path, p => JsonDocument.Parse(File.ReadAllText(p)));
        }

        private static double GetScalar(JsonDocument scalars, BaselineSettings settings, string key)
        {
            if (!scalars.RootElement.TryGetProperty(key, out JsonElement element))
                throw new KeyNotFoundException($"Key '{key}' was not found in '{settings.ScalarsFileName}'.");

            return element.GetDouble();
        }

        private static void EnsureLength(string fileName, int actual, int expected)
        {
            if (actual != expected)
                throw new InvalidDataException(
                    $"{fileName}: has {actual} values, expected {expected} (one per monitored parameter).");
        }

        private static void EnsureSquare(string fileName, Matrix<double> matrix, int expected)
        {
            if (matrix.RowCount != expected || matrix.ColumnCount != expected)
                throw new InvalidDataException(
                    $"{fileName}: is {matrix.RowCount}x{matrix.ColumnCount}, expected {expected}x{expected}.");
        }
    }
}
