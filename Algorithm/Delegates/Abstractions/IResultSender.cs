using AlgoLib.Data.Result_Objects;

namespace TelemetryHealthPredictor.Algorithm.Delegates.Abstractions
{
    public interface IResultSender
    {
        public Task ResultSenderDelegate(AlgorithmResult result);
    }
}
