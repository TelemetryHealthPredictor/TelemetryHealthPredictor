using MathNet.Numerics.LinearAlgebra;

namespace TelemetryHealthPredictor.Data
{
    /// <summary>
    /// Every value the algorithms need for Init: the profile written by the Baseline Builder,
    /// plus the hand-set scalars from scalars.json. Loaded once at startup.
    /// </summary>
    public class Baseline
    {
        // ---- Kalman ----
        public required Vector<double> QcVector { get; init; }
        public required Matrix<double> R0Matrix { get; init; }
        public required double ForgettingFactor { get; init; }

        // ---- Mahalanobis ----
        public required Matrix<double> CovarianceMatrix { get; init; }
        public required double MDOutliersStepSize { get; init; }
        public required double DistanceSquaredThreshold { get; init; }

        // ---- CUSUM ----
        public required double[] CusumSlacks { get; init; }
        public required double[] CusumThresholds { get; init; }

        // ---- Fusion ----
        public required double[] D2Ladder { get; init; }
        public required double[] CusumRawLadder { get; init; }
        public required double D2Weight { get; init; }
        public required double TDecay { get; init; }
        public required double HysteresisEntry { get; init; }
        public required double HysteresisExit { get; init; }
    }
}
