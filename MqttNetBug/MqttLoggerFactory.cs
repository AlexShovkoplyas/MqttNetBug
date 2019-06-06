using Microsoft.Extensions.Logging;
using MQTTnet.Diagnostics;

namespace MqttNetBug
{
    class MqttLoggerFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public MqttLoggerFactory()
        {
            _loggerFactory = LoggerFactory.Create(loggerBuilder =>
            {
                loggerBuilder
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddConsole();
            });
        }

        public IMqttNetLogger Create(string loggerName)
        {
            var logger = _loggerFactory.CreateLogger(loggerName);

            var mqttNetLogger = new MqttNetLogger();

            mqttNetLogger.LogMessagePublished += (sender, e) =>
            {
                if (e.TraceMessage.Exception != null)
                {
                    logger.LogError(e.TraceMessage.Exception, e.TraceMessage.Message);
                }
                else
                {
                    logger.LogDebug(e.TraceMessage.Message);
                }
            };

            return mqttNetLogger;
        }
    }
}
