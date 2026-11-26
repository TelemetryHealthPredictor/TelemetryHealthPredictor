using AlgoLib.Algorithms.Abstractions;
using AlgoLib.Data.Result_Objects;
using TelemetryHealthPredictor.Data;

namespace TelemetryHealthPredictor.Algorithm.Handlers
{
    public class MahalanobisHandler
    {
        private readonly IMahalanobis _mahalanobis;

        public MahalanobisHandler(IMahalanobis mahalanobis)
        {
            _mahalanobis = mahalanobis;
        }

        public void Init(Baseline baseline)
        {
            _mahalanobis.Init(baseline.MDOutliersStepSize, baseline.CovarianceMatrix, baseline.DistanceSquaredThreshold);
        }

        public MahalanobisResult MahalanobisDelegate(KalmanFilterResult kalmanResult)
        {
            _mahalanobis.ComputeD2(kalmanResult);

            // Only searches for outliers if d² is above its threshold
            _mahalanobis.FindOutlierParameters();

            return _mahalanobis.GetResult();
        }
    }
}
