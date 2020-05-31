using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Rocket.Core.Utils
{
    public class TaskDispatcher : MonoBehaviour
    {
        private static int numThreads;

        private static List<Action> actions = new List<Action>();
        private static List<DelayedQueueItem> delayed = new List<DelayedQueueItem>();

        public struct DelayedQueueItem
        {
            public float time;
            public Action action;
        }

        public static void QueueOnMainThread(Action action)
        {
            QueueOnMainThread(action, 0f);
        }

        public static void QueueOnMainThread(Action action, float time)
        {
            if (time != 0f)
            {
                lock (delayed)
                {
                    delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });
                }
            }
            else
            {
                lock (actions)
                {
                    actions.Add(action);
                }
            }
        }

        public static Thread RunAsync(Action a)
        {
            while (numThreads >= 8)
            {
                Thread.Sleep(1);
            }
            Interlocked.Increment(ref numThreads);
            ThreadPool.QueueUserWorkItem(RunAction, a);
            return null;
        }

        private static void RunAction(object action)
        {
            try
            {
                ((Action)action)();
            }
            catch(Exception ex)
            {
                Logging.Logger.LogException(ex,"Error while running action");
            }
            finally
            {
                Interlocked.Decrement(ref numThreads);
            }
        }

        private void FixedUpdate()
        {
            List<Action> currentActions;
            lock (delayed)
            {
                lock (actions)
                {
                    if (actions.Count == 0 && delayed.Count == 0)
                        return;
                    currentActions = new List<Action>();
                    currentActions.AddRange(actions);
                    actions.Clear();
                }
                for (int i = delayed.Count - 1; i >= 0; i--)
                {
                    if (delayed[i].time <= Time.time)
                    {
                        currentActions.Add(delayed[i].action);
                        delayed.RemoveAt(i);
                    }
                }
            }
            foreach (var a in currentActions)
            {
                a();
            }
        }
    }
}
