using System;
using System.IO.Pipes;
using System.Threading;
using System.Collections.Concurrent;

namespace pl_token_ring
{
    [Serializable]
    public class Token
    {
        public string data;
        public int sender;
        public int recipent;

        public Token(string data, int sender, int recipent)
        {
            this.data = data;
            this.recipent = recipent;
            this.sender = sender;
        }
    }

    public class Params
    {
        public AnonymousPipeClientStream inPipe;
        public AnonymousPipeServerStream outPipe;
        public int threadNumber;

        public Params(AnonymousPipeClientStream inPipe, AnonymousPipeServerStream outPipe, int threadNumber)
        {
            this.inPipe = inPipe;
            this.outPipe = outPipe;
            this.threadNumber = threadNumber;
        }
    }

    class Program
    {
        private const int numThreads = 10;
        private const int sender = 3;
        private const int recipent = 2;
        private const string message = "Hello!";

        static void Main(string[] args)
        {
            var queues = new ConcurrentQueue<Token>[numThreads];
            for (var i = 0; i< numThreads; i++)
            {
                queues[i] = new ConcurrentQueue<Token>();
            }

            var threads = new Thread[numThreads];
            for (var i = 0; i < numThreads-1; i++)
            {
                var index = i;
                threads[i] = new Thread(() =>
                {
                    threadFunction(queues[index], queues[index + 1], index);
                });
            }
            threads[numThreads-1] = new Thread(() =>
            {
                threadFunction(queues[numThreads - 1], queues[0], numThreads - 1);
            });
            for (var i = 0; i < numThreads; i++)
            {
                threads[i].Start();
            }


            Token token = new Token(message, sender, recipent);
            Console.WriteLine("Thread {0}: send message", sender);
            queues[sender + 1].Enqueue(token);

        }


        public static void threadFunction(ConcurrentQueue<Token> inQueue, ConcurrentQueue<Token> outQueue, int threadNumber)
        {
            while (true)
            { 
                Token token;
                if (!inQueue.TryDequeue(out token))
                {
                    Thread.Sleep(100);
                    continue;
                }

                if (token.sender == threadNumber)
                {
                    Console.WriteLine("Thread {0}: recipent not found", threadNumber);
                    Environment.Exit(0);
                }
                else if (token.recipent == threadNumber)
                {
                    Console.WriteLine("Thread {0}: recived message: {1}", threadNumber, token.data);
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("Thread {0}: forvard token", threadNumber);
                    outQueue.Enqueue(token);
                }
            }
        }
    }
}
