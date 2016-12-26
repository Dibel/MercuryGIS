using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace MercuryGIS
{
    class SSSPClient
    {
        private IConnection connection;
        private IModel channel;
        private string replyQueueName;
        private string methodname;
        private QueueingBasicConsumer consumer;

        private static Dictionary<string, SSSPClient> clients = new Dictionary<string, SSSPClient>();

        public static SSSPClient get(string methodname)
        {
            if (!clients.ContainsKey(methodname))
            {
                clients.Add(methodname, new SSSPClient(methodname));
            }
            return clients[methodname];
        }

        public static void closeAll()
        {
            foreach (var val in clients.Values)
            {
                val.Close();
            }
        }

        protected SSSPClient(string methodname)
        {
            this.methodname = methodname;
            var factory = new ConnectionFactory() { HostName = "mq.12414.tk" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queue: replyQueueName,
                                 noAck: true,
                                 consumer: consumer);

        }

        public List<int> call(double[,] data, int[] start, int[] end)
        {
            var corrId = Guid.NewGuid().ToString();
            var props = channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;
            props.CorrelationId = corrId;

            Dictionary<string, object> obj = new Dictionary<string, object>();
            obj.Add("data", data);
            obj.Add("start", start);
            obj.Add("end", end);
            var json = JsonConvert.SerializeObject(obj);

            byte[] payload;

            using (MemoryStream cms = new MemoryStream())
            {
                using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(cms, System.IO.Compression.CompressionMode.Compress))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    gzip.Write(bytes, 0, bytes.Length);
                }
                payload = cms.ToArray();
            }

            channel.BasicPublish(exchange: "",
                                 routingKey: methodname,
                                 basicProperties: props,
                                 body: payload);

            while (true)
            {
                var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                if (ea.BasicProperties.CorrelationId == corrId)
                {
                    using (MemoryStream dms = new MemoryStream())
                    {
                        using (MemoryStream cms = new MemoryStream(ea.Body))
                        {
                            using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(cms, System.IO.Compression.CompressionMode.Decompress))
                            {
                                byte[] bytes = new byte[1024 * 64];
                                int len = 0;
                                //读取压缩流，同时会被解压
                                while ((len = gzip.Read(bytes, 0, bytes.Length)) > 0)
                                {
                                    dms.Write(bytes, 0, len);
                                }
                            }
                        }
                        var respjson = Encoding.UTF8.GetString(dms.ToArray());
                        var result = JsonConvert.DeserializeObject<List<int>>(respjson);
                        return result;
                    }
                }
            }
        }

        public void Close()
        {
            connection.Close();
        }

    }
}
