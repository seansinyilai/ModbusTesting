using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ModbusConnection
{
    public class BlockQueue<T>
    {
        public static Timer Timer;
        private Queue<T> _inner_queue = null;
        private ManualResetEvent _enqueue_wait = null;
        private ManualResetEvent _dequeue_wait = null;

        public BlockQueue()
        {
            this._inner_queue = new Queue<T>();
            this._inner_queue.Clear();
            this._enqueue_wait = new ManualResetEvent(false);
            this._dequeue_wait = new ManualResetEvent(false);
        }

        public void Clear()
        {
            this._inner_queue.Clear();
        }

        #region EnQueue
        /// <summary>
        /// 加入至queue
        /// </summary>
        /// <param name="item"></param>
        public void EnQueue(T item)
        {
            lock (this._inner_queue)
            {
                this._inner_queue.Enqueue(item);
                this._dequeue_wait.Set();
            }
        }

        #endregion

        #region Dequeue
        /// <summary>
        /// 當dequeue方法中沒有給時間參數 只要一dequeue執行緒就會停止一直等到下一次enqueue
        /// 如果有設時間，時間到的時候沒有東西enqueue進來就會造成timeout也就是回傳null
        /// </summary>
        /// <param name="TimeOut"></param>
        /// <returns></returns>
        public T DeQueue(int TimeOut = System.Threading.Timeout.Infinite)
        {
            bool TO = false;
            //this._enqueue_wait.Reset();
            this._dequeue_wait.Reset();
            while (true)
            {
                lock (this._inner_queue)
                {
                    if (this._inner_queue.Count > 0)
                    {
                        T item = this._inner_queue.Dequeue();
                        this._dequeue_wait.Reset();
                        return item;
                    }
                }
                TO = this._dequeue_wait.WaitOne(TimeOut);
                if ((TimeOut != System.Threading.Timeout.Infinite) && (TO == false))
                    return default(T);
            }
        }

        #endregion

        public int Count()
        {
            return this._inner_queue.Count;
        }

        //public bool ConTains(T item)
        //{
        //    return this._inner_queue.Contains(item); ;
        //}
    }
}
