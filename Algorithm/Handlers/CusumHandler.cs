using AlgoLib.Algorithms.Abstractions;
using AlgoLib.Data.Result_Objects;
using TelemetryHealthPredictor.Data;

namespace TelemetryHealthPredictor.Algorithm.Handlers
{
    public class CusumHandler
    {
        private readonly ICusum _cusum;

        public CusumHandler(ICusum cusum)
        {
            _cusum = cusum;
        }

        public void Init(Baseline baseline)
        {
            _cusum.Init(baseline.CusumSlacks, baseline.CusumThresholds);
        }

        public CusumResult CusumDelegate(KalmanFilterResult kalmanResult)
        {
            _cusum.RecalculateCusums(kalmanResult);
            _cusum.ComputeAnomalies();
            _cusum.ComputeCusumRaw();

            return _cusum.GetStepResult();
        }
    }
}
