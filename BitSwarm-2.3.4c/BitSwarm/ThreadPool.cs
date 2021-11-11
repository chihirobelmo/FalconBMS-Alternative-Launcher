using System.Collections.Concurrent;
using System.Threading;

using SuRGeoNix.BitSwarmLib.BEP;

namespace SuRGeoNix.BitSwarmLib
{
    /// <summary>
    /// BitSwarm's Thread Pool For Peers Dispatching [Short/Long Run]
    /// </summary>
    class ThreadPool
    {
        public  bool            Stop        { get; internal set; }
        public  int             MaxThreads  { get; private  set; }
        public  int             MinThreads  { get; private  set; }
        public  int             Available   => MaxThreads- Running;
        public  int             Running;
        public  int             ShortRun    => Running   - LongRun;
        public  int             LongRun;

        public ThreadPeer[]     Threads;

        ConcurrentStack<Peer>   peersForDispatch;
        private readonly object lockerThreads = new object();

        public void Initialize(int minThreads, int maxThreads, ConcurrentStack<Peer> peersStack)
        {
            lock (lockerThreads)
            {
                Dispose();

                Stop        = false;
                MinThreads  = minThreads;
                MaxThreads  = maxThreads;
                Running     = maxThreads;
                Threads     = new ThreadPeer[MaxThreads];

                peersForDispatch = peersStack;

                for (int i=0; i<MaxThreads; i++)
                {
                    StartThread(i);

                    if (i % 25 == 0) Thread.Sleep(25);
                }
            }
        }
        public void SetMinThreads(int minThreads)
        {
            lock (lockerThreads)
            {
                if (minThreads > MaxThreads)
                    MinThreads = MaxThreads;
                else if (minThreads < 0)
                    MinThreads = 0;
                else
                    MinThreads = minThreads;
            }
        }
        private void StartThread(int i)
        {
            int cacheI = i;

            Threads[i]                      = new ThreadPeer();
            Threads[i].thread               = new Thread(_ => { ThreadRun(cacheI); });
            Threads[i].thread.IsBackground  = true;
            Threads[i].thread.Start();
        }
        private void ThreadRun(int index)
        {
            Interlocked.Decrement(ref Running);
            Threads[index].IsAlive  = true;

            while (!Stop)
            {
                Threads[index].resetEvent.WaitOne();
                if (Stop) break;

                do
                {
                    Threads[index].peer?.Run(this, index);
                    if (ShortRun > MinThreads || Stop || Threads == null || Threads[index] == null) break;
                    lock (peersForDispatch)
                        if (peersForDispatch.TryPop(out Peer tmp)) { Threads[index].peer = tmp; Threads[index].peer.status = Peer.Status.CONNECTING; } else break;

                } while (true);

                if (Threads != null && Threads[index] != null) Threads[index].IsRunning = false;
                Interlocked.Decrement(ref Running);
            }
        }

        public bool Dispatch(Peer peer)
        {
            lock (lockerThreads)
            {
                if (Stop || Running >= MaxThreads || ShortRun >= MinThreads) return false;

                foreach (var thread in Threads)
                    if (thread != null && !thread.IsRunning && thread.IsAlive)
                    {
                        if (Running >= MaxThreads || ShortRun >= MinThreads) return false;

                        if (peer != null) peer.status = Peer.Status.CONNECTING;
                        thread.peer     = peer;
                        thread.IsRunning= true;
                        Interlocked.Increment(ref Running);
                        thread.resetEvent.Set();

                        return true;
                    }

                return false;
            }
        }
        public void Dispose()
        {
            lock (lockerThreads)
            {
                if (peersForDispatch != null) lock (peersForDispatch) peersForDispatch.Clear();
                Stop = true;

                if (Threads != null)
                {
                    foreach (var thread in Threads) thread?.resetEvent.Set();

                    int escape = 150;
                    while (Running > 0 && escape > 0) { Thread.Sleep(20); escape--; }
                }

                MinThreads  = 0;
                MaxThreads  = 0;
                Running     = 0;
                Threads     = null;
            }
        }
    }

    class ThreadPeer
    {
        public AutoResetEvent   resetEvent = new AutoResetEvent(false);
        public bool             isLongRun   { get; internal set; }
        public bool             IsRunning   { get; internal set; }
        public bool             IsAlive     { get; internal set; }

        public Thread           thread;
        public Peer             peer;
    }
}
