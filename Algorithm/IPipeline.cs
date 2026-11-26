using ParameterDataLib;

namespace TelemetryHealthPredictor.Algorithm
{
    public interface IPipeline
    {
        public Task Completion();

        /// <summary>
        /// Sends one frame into the pipeline. If the frame starts a new flight, first waits for
        /// the previous flight's frames to finish, then resets the algorithms and rebuilds the blocks.
        /// </summary>
        public Task BeginAlgorithm(ParameterData[] parameters);

        public void Complete();

        public void Fault(Exception ex);
    }
}
