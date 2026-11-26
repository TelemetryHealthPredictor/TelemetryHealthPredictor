using AlgoLib.Algorithms.Abstractions;
using AlgoLib.Algorithms.Implementations;
using TelemetryHealthPredictor.Algorithm;
using TelemetryHealthPredictor.Algorithm.Delegates.Abstractions;
using TelemetryHealthPredictor.Algorithm.Delegates.Implementations;
using TelemetryHealthPredictor.Algorithm.Handlers;
using TelemetryHealthPredictor.Config;
using TelemetryHealthPredictor.Config.Builders;
using TelemetryHealthPredictor.Config.Settings;
using TelemetryHealthPredictor.Kafka;
using ThpUtils.Naming;

namespace TelemetryHealthPredictor.Setup.BuilderExtensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddSignalR();

            builder.Services.Configure<KafkaConsumerConfig>
                (builder.Configuration.GetSection(nameof(KafkaConsumerConfig)));

            builder.Services.AddNaming(builder.Configuration);

            builder.Services.Configure<BaselineSettings>
                (builder.Configuration.GetSection(nameof(BaselineSettings)));
            builder.Services.AddSingleton(BaselineBuilder.Build);

            builder.Services.AddSingleton<IKalmanFilter, KalmanFilter>();
            builder.Services.AddSingleton<IMahalanobis, Mahalanobis>();
            builder.Services.AddSingleton<ICusum, Cusum>();
            builder.Services.AddSingleton<IFusion, Fusion>();

            builder.Services.AddSingleton<KalmanHandler>();
            builder.Services.AddSingleton<MahalanobisHandler>();
            builder.Services.AddSingleton<CusumHandler>();
            builder.Services.AddSingleton<FusionHandler>();

            builder.Services.AddSingleton<IResultSender, ResultSender>();

            builder.Services.AddSingleton<IPipeline, Pipeline>();

            builder.Services.AddHostedService<KafkaConsumerService>();

            return builder;
        }
    }
}
