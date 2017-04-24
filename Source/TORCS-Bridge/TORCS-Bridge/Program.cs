﻿using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TORCS_Bridge.TorcsIntegration;

namespace TORCS_Bridge
{
    class Program
    {
        public static string HostName = "localhost";

        static void Main(string[] args)
        {
            //TODO REMOVE

            XMLIntegration.ChangeValueInTorcsXML("F_cars.F_car1-ow1.f_car1-ow1.S_Car.A_mass.T_val", 20);

            //TODO REMOVE



            var factory = new ConnectionFactory()
            {
                HostName = HostName
            };

            do
            {
                //On error, try to reconnect to server. Wait 5 seconds between reconnect attempts
                Console.WriteLine("Connecting to host " + HostName);
                try
                {
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: "rpc_queue",
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);
                        channel.BasicQos(0, 1, false);
                        var consumer = new QueueingBasicConsumer(channel);
                        channel.BasicConsume(queue: "rpc_queue",
                                             noAck: false,
                                             consumer: consumer);
                        Console.WriteLine(" [x] Awaiting RPC requests");

                        while (true)
                        {
                            string response = null;
                            var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                            var body = ea.Body;
                            var props = ea.BasicProperties;
                            var replyProps = channel.CreateBasicProperties();
                            replyProps.CorrelationId = props.CorrelationId;

                            try
                            {
                                var message = Encoding.UTF8.GetString(body);

                                //int n = int.Parse(message);
                                Console.WriteLine(" [.] RunGame()");

                                /*var serializer = new XmlSerializer(typeof(RPCData));
                                RPCData customData;

                                using (TextReader reader = new StringReader(message))
                                {
                                    customData = (RPCData)serializer.Deserialize(reader);
                                }*/
                                dynamic JResults = JsonConvert.DeserializeObject(message);

                                

                                foreach (var Param in JResults.parameters)
                                {
                                    if ((bool)Param.enabled == true)
                                    {
                                        //Params[(int)Param.custom.index] += (double)Param.value;
                                        //Find appropriate xml file in Torcs and apply changes
                                    }
                                }
                                
                                //Run TORCS

                                //Collect results

                                response = ""; //Send them here
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(" [.] " + e.Message);
                                response = "";
                            }
                            finally
                            {
                                var responseBytes = Encoding.UTF8.GetBytes(response);
                                channel.BasicPublish(exchange: "",
                                                     routingKey: props.ReplyTo,
                                                     basicProperties: replyProps,
                                                     body: responseBytes);
                                channel.BasicAck(deliveryTag: ea.DeliveryTag,
                                                 multiple: false);
                            }

                            Console.WriteLine(" [x] Awaiting RPC requests");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                }

                Thread.Sleep(5000);
            } while (true);
        }
    }
}
