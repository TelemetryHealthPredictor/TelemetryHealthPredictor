using System.Threading.Tasks.Dataflow;
using AlgoLib.Data.Result_Objects;
using ParameterDataLib;
using TelemetryHealthPredictor.Algorithm.Delegates.Abstractions;
using TelemetryHealthPredictor.Algorithm.Handlers;
using TelemetryHealthPredictor.Data;

namespace TelemetryHealthPredictor.Algorithm
{
    public class Pipeline : IPipeline
    {
        /// <summary>
        /// A frame more than this long after the latest one starts a new flight.
        /// Same rule as MongoConsumer's BatchHandler.
        /// </summary>
        private const ulong FlightGapMs = 2UL * 60UL * 1000UL;

        private readonly Baseline _baseline;
        private readonly KalmanHandler _kalmanHandler;
        private readonly MahalanobisHandler _mahalanobisHandler;
        private readonly CusumHandler _cusumHandler;
        private readonly FusionHandler _fusionHandler;
        private readonly IResultSender _sender;
        private readonly ILogger<Pipeline> _logger;

        // ---- Blocks of the current flight, rebuilt by Init (null before the first flight) ----
        private TransformManyBlock<ParameterData[], KalmanFilterResult>? _kalmanBlock;
        private ActionBlock<AlgorithmResult>? _senderBlock;

        // ---- Flight tracking ----
        private ulong? _latestTimestampMs;

        public Pipeline
        (
            Baseline baseline,
            KalmanHandler kalmanHandler,
            MahalanobisHandler mahalanobisHandler,
            CusumHandler cusumHandler,
            FusionHandler fusionHandler,
            IResultSender sender,
            ILogger<Pipeline> logger
        )
        {
            _baseline = baseline;
            _kalmanHandler = kalmanHandler;
            _mahalanobisHandler = mahalanobisHandler;
            _cusumHandler = cusumHandler;
            _fusionHandler = fusionHandler;
            _sender = sender;
            _logger = logger;
        }

        public async Task BeginAlgorithm(ParameterData[] parameters)
        {
            if (parameters.Length == 0)
                return;

            ulong timestampMs = parameters[0].TimestampMS;

            if (IsNewFlight(timestampMs))
            {
                await DrainCurrentFlight();
                Init();
            }

            _latestTimestampMs = timestampMs;

            await _kalmanBlock!.SendAsync(parameters);
        }

        public void Complete()
        {
            _kalmanBlock?.Complete();
        }

        public void Fault(Exception ex)
        {
            ((IDataflowBlock?)_kalmanBlock)?.Fault(ex);
        }

        public async Task Completion()
        {
            if (_senderBlock is not null)
                await _senderBlock.Completion;
        }

        /// <summary>
        /// New flight = the first frame ever, a frame more than FlightGapMs after the latest one,
        /// or a frame earlier than the latest one.
        /// </summary>
        private bool IsNewFlight(ulong timestampMs)
        {
            if (_latestTimestampMs is not ulong latestMs)
            {
                _logger.LogInformation("New flight: first frame received (timestamp {Timestamp} ms).", timestampMs);
                return true;
            }

            if (timestampMs < latestMs)
            {
                _logger.LogInformation("New flight: timestamp went back by {Seconds:F1} s.",
                    (latestMs - timestampMs) / 1000.0);
                return true;
            }

            if (timestampMs - latestMs > FlightGapMs)
            {
                _logger.LogInformation("New flight: gap of {Seconds:F1} s since the latest frame.",
                    (timestampMs - latestMs) / 1000.0);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Lets every frame of the previous flight leave the pipeline before the algorithms are reset.
        /// Completed blocks can't be reused, so Init builds new ones afterwards.
        /// </summary>
        private async Task DrainCurrentFlight()
        {
            if (_kalmanBlock is null)
                return;

            Complete();
            await Completion();
        }

        /// <summary>
        /// Resets every algorithm with the baseline values, then builds and links a fresh set of blocks.
        /// </summary>
        private void Init()
        {
            _kalmanHandler.Init(_baseline);
            _mahalanobisHandler.Init(_baseline);
            _cusumHandler.Init(_baseline);
            _fusionHandler.Init(_baseline);

            BuildBlocks();
        }

        private void BuildBlocks()
        {
            ExecutionDataflowBlockOptions blockOptions = new()
            {
                MaxDegreeOfParallelism = 1
            };

            GroupingDataflowBlockOptions groupingOptions = new()
            {
                EnsureOrdered = true
            };

            TransformManyBlock<ParameterData[], KalmanFilterResult> kalmanBlock = new(_kalmanHandler.KalmanDelegate, blockOptions);
            BroadcastBlock<KalmanFilterResult> broadcastBlock = new(result => result);
            TransformBlock<KalmanFilterResult, MahalanobisResult> mahalanobisBlock = new(_mahalanobisHandler.MahalanobisDelegate, blockOptions);
            TransformBlock<KalmanFilterResult, CusumResult> cusumBlock = new(_cusumHandler.CusumDelegate, blockOptions);
            JoinBlock<MahalanobisResult, CusumResult> joinBlock = new(groupingOptions);
            TransformBlock<Tuple<MahalanobisResult, CusumResult>, AlgorithmResult> fusionBlock = new(_fusionHandler.FusionDelegate, blockOptions);
            ActionBlock<AlgorithmResult> senderBlock = new(_sender.ResultSenderDelegate, blockOptions);

            DataflowLinkOptions linkOptions = new()
            {
                PropagateCompletion = true
            };

            kalmanBlock.LinkTo(broadcastBlock, linkOptions);
            broadcastBlock.LinkTo(mahalanobisBlock, linkOptions);
            broadcastBlock.LinkTo(cusumBlock, linkOptions);
            mahalanobisBlock.LinkTo(joinBlock.Target1, linkOptions);
            cusumBlock.LinkTo(joinBlock.Target2, linkOptions);
            joinBlock.LinkTo(fusionBlock, linkOptions);
            fusionBlock.LinkTo(senderBlock, linkOptions);

            _kalmanBlock = kalmanBlock;
            _senderBlock = senderBlock;
        }
    }
}
