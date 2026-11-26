using AlgoLib.Algorithms.Abstractions;
using AlgoLib.Data.Result_Objects;
using TelemetryHealthPredictor.Data;

namespace TelemetryHealthPredictor.Algorithm.Handlers
{
    public class FusionHandler
    {
        private readonly IFusion _fusion;

        public FusionHandler(IFusion fusion)
        {
            _fusion = fusion;
        }

        public void Init(Baseline baseline)
        {
            _fusion.Init
            (
                baseline.CusumRawLadder,
                baseline.D2Ladder,
                baseline.D2Weight,
                baseline.TDecay,
                baseline.HysteresisEntry,
                baseline.HysteresisExit
            );
        }

        public AlgorithmResult FusionDelegate(Tuple<MahalanobisResult, CusumResult> combinedResult)
        {
            return _fusion.FusionDelegate(combinedResult);
        }
    }
}
