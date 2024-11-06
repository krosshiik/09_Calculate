﻿using Confluent.Kafka;
using _09_Calculate.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using _09_Calculate.Controllers;
using _09_Calculate.Data;

namespace _09_Calculate.Services
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System.Net.Http;

    public class KafkaConsumerService : BackgroundService
    {
        
        private readonly string _topic;
        private readonly IConsumer<Null, string> _kafkaConsumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _clientFactory;

        public KafkaConsumerService(IConfiguration config, IServiceProvider serviceProvider, IHttpClientFactory clientFactory)
        {
            // Конфигурирование настроек Kafka и инициализация компонентов
            var consumerConfig = new ConsumerConfig();
            config.GetSection("Kafka:ConsumerSettings").Bind(consumerConfig);
            _topic = config.GetValue<string>("Kafka:TopicName");
            _kafkaConsumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
            _serviceProvider = serviceProvider;
            _clientFactory = clientFactory;
        }

        /// <summary>
        /// Выполнение работы Kafka Consumer'а.
        /// </summary>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        /// <summary>
        /// Цикл обработки сообщений из Kafka.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        private async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            _kafkaConsumer.Subscribe(_topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _kafkaConsumer.Consume(cancellationToken);
                    var ip = cr.Message.Value;

                    // Исходные данные
                    var inputData = JsonSerializer.Deserialize<DataInputVariant>(cr.Message.Value);

                    // Выполнение расчета
                    var result = CalculatorLibrary.CalculateOperation(inputData.Operand_1, inputData.Operand_2, inputData.Type_operation);
                    inputData.Result = result.ToString();

                    var httpClient = _clientFactory.CreateClient();

                    await httpClient.PostAsJsonAsync($"http://localhost:5009/Calculator/Callback", inputData);

                    // Обработка сообщения...
                    Console.WriteLine($"Message key: {cr.Message.Key}, value: {cr.Message.Value}");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException e)
                {
                    if (e.Error.IsFatal)
                    {
                        // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                        break;
                    }
                }
                catch (Exception e)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Очистка ресурсов Consumer'а при завершении работы сервиса.
        /// </summary>
        public override void Dispose()
        {
            _kafkaConsumer.Close(); // Фиксация оффсетов и корректное выход из группы.
            _kafkaConsumer.Dispose();

            base.Dispose();
        }

    }
}
