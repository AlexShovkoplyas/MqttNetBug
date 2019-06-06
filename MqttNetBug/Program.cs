using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using MQTTnet.Diagnostics;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace MqttNetBug
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serverLogger = new MqttLoggerFactory().Create("Server");
            var clientLogger = new MqttLoggerFactory().Create("Client");

            IMqttFactory mqttFactory = new MqttFactory();

            //STARTING SERVER
            var mqttServer = mqttFactory.CreateMqttServer(serverLogger);
            await mqttServer.StartAsync(new MqttServerOptionsBuilder().Build());

            //STARTING CLIENT
            var mqttClient = mqttFactory.CreateMqttClient(clientLogger);

            var options = new MqttClientOptionsBuilder()
                .WithClientId("Client1")
                .WithTcpServer("localhost")
                .WithCleanSession(false).Build();
            await mqttClient.ConnectAsync(options);

            await mqttClient.SubscribeAsync("testtopic/1", MqttQualityOfServiceLevel.AtLeastOnce);
            await mqttClient.SubscribeAsync("testtopic/2", MqttQualityOfServiceLevel.AtLeastOnce);

            mqttClient.UseApplicationMessageReceivedHandler(async e =>
            {
                if (e.ApplicationMessage.Topic =="testtopic/1")
                {
                    var messageMqtt2 = new MqttApplicationMessageBuilder()
                        .WithTopic("testtopic/2")
                        .WithPayload("Hello2")
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag(false)
                        .Build();

                    await mqttClient.PublishAsync(messageMqtt2);
                }
            });

            var messageMqtt1 = new MqttApplicationMessageBuilder()
                .WithTopic("testtopic/1")
                .WithPayload("Hello1")
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            await mqttClient.PublishAsync(messageMqtt1);

            Console.WriteLine("Finish!");
            Console.ReadKey();
        }
    }
}
