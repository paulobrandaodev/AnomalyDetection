using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.AnomalyDetector;
using Microsoft.Azure.CognitiveServices.AnomalyDetector.Models;

namespace AzureAnomalyDetector
{
    class Program
    {
        static void Main(string[] args)
        {
            
            // Preparando nosso ambiente
            string endpoint = "MEUENDPOINT";
            string key      = "MINHACHAVEDEAPI";
            string datapath = "request-data.csv";

            IAnomalyDetectorClient client = createClient(endpoint, key); // autenticando usuario da API

            Request request = GetSeriesFromFile(datapath);  // Gerando uma request com os dados do CSV

            EntireDetectSampleAsync(client, request).Wait(); // Método assincrono
            LastDetectSampleAsync(client, request).Wait(); // Método assincrono

            Console.WriteLine("\nPressione ENTER para sair.");
            Console.ReadLine();

        }

        /// <summary>
        /// Autenticar o cliente da API
        /// </summary>
        /// <param name="endpoint">Endpoint da API Anomaly Detector</param>
        /// <param name="key">Chave da API</param>
        /// <returns></returns>
        static IAnomalyDetectorClient createClient(string endpoint, string key){
            IAnomalyDetectorClient client = new AnomalyDetectorClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };
            return client;
        }

        /// <summary>
        /// Cria a request para ser passada para o Endpoint de anomalias
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static Request GetSeriesFromFile(string path)
        {
            List<Point> list = File.ReadAllLines(path, Encoding.UTF8)
                .Where(e => e.Trim().Length != 0)
                .Select(e => e.Split(','))
                .Where(e => e.Length == 2)
                .Select(e => new Point(DateTime.Parse(e[0]), Double.Parse(e[1]))).ToList();
            
            return new Request(list, Granularity.Daily); 
        }

        /// <summary>
        /// Detectar anomalias no conjunto de dados inteiro
        /// </summary>
        /// <param name="client">Cliente autenticado</param>
        /// <param name="request">Request com os dados</param>
        /// <returns></returns>
        static async Task EntireDetectSampleAsync(IAnomalyDetectorClient client, Request request)
        {
            Console.WriteLine("\nDetectando anomalias no conjunto de dados inteiro");

            EntireDetectResponse result = await client.EntireDetectAsync(request).ConfigureAwait(false);

            if (result.IsAnomaly.Contains(true))
            {
                Console.WriteLine("Uma anomalia foi detectada no índice:");
                for (int i = 0; i < request.Series.Count; ++i)
                {
                    if (result.IsAnomaly[i])
                    {
                        Console.Write(i);
                        Console.Write(" ");
                    }
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Nenhuma anomalia foi detectada neste conjunto de dados.");
            }
        }

        /// <summary>
        /// Detectar o status de anomalias do último ponto de dados
        /// </summary>
        /// <param name="client">Cliente autenticado</param>
        /// <param name="request">Request com os dados</param>
        /// <returns></returns>
        static async Task LastDetectSampleAsync(IAnomalyDetectorClient client, Request request)
        {

            Console.WriteLine("\nDetectando o status de anomalias do último ponto de dados");
            LastDetectResponse result = await client.LastDetectAsync(request).ConfigureAwait(false);

            if (result.IsAnomaly)
            {
                Console.WriteLine("No último registro EXISTE uma anomalia.");
            }
            else
            {
                Console.WriteLine("No último registro NÃO existe uma anomalia.");
            }
        }

    }
}
