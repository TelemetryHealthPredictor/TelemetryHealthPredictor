using AlgoLib.Algorithms.Abstractions;
using AlgoLib.Data.Result_Objects;
using ParameterDataLib;
using TelemetryHealthPredictor.Data;

namespace TelemetryHealthPredictor.Algorithm.Handlers
{
    public class KalmanHandler
    {
        private readonly IKalmanFilter _kalmanFilter;

        public KalmanHandler(IKalmanFilter kalmanFilter)
        {
            _kalmanFilter = kalmanFilter;
        }

        public void Init(Baseline baseline)
        {
            _kalmanFilter.Init(baseline.QcVector, baseline.R0Matrix, baseline.ForgettingFactor);
        }

        /// <returns>
        /// No result while the filter is settling (first minute of the flight)
        /// or until every parameter has been measured once; otherwise one result.
        /// </returns>
        public KalmanFilterResult[] KalmanDelegate(ParameterData[] measurements)
        {
            _kalmanFilter.Predict(measurements);
            _kalmanFilter.ComputeInnovation();
            _kalmanFilter.Update();
            _kalmanFilter.ComputeZScores();
            _kalmanFilter.SageHusa();

            KalmanFilterResult? result = _kalmanFilter.GetStepResult();

            if (result is null)
            {
                return [];
            }

            return [result];
        }
    }
}
