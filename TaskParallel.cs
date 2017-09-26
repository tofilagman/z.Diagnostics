using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace z.Diagnostics
{
    /// <summary>
    /// LJ 20160102
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TaskParallel<T> : IDisposable
    {
        public int MaxTask { set; private get; } = 20;

        public int ProcessCounter { get; private set; } = 0;
        public List<TaskParallelItem<T>> ObjectList { set; get; } = new List<TaskParallelItem<T>>();
        private SemaphoreSlim sempahore;
        //private System.Timers.Timer tmr;
        private Stopwatch sw; // = new Stopwatch();

        //public TaskParallel()
        //{
        //    this.tmr = new System.Timers.Timer();
        //    this.tmr.Interval = 1000;
        //    this.tmr.Elapsed += Tmr_Elapsed;
        //}


        public async void Execute()
        {
            sw = Stopwatch.StartNew();
            var actions = ObjectList.Select(x => new TaskAction<T>() { TaskObject = x.Item, action = () => DoTask(x.Item), Token = x.Token });

            await ProcessAll(actions).ContinueWith(x => TaskCompleted(sw.Elapsed, x));
        }

        public async void Execute(T item)
        {
            sw = Stopwatch.StartNew();
            var j = ObjectList.Where(x => x.Item.Equals(item)).Select(x => new TaskAction<T>() { TaskObject = x.Item, action = () => DoTask(x.Item), Token = x.Token });
            await ProcessAll(j).ContinueWith(x => TaskCompleted(sw.Elapsed, x));
        }

        public abstract void DoTask(T TaskObject);

        public abstract void TaskRemoved(T TaskObject);

        public abstract void TaskCompleted(TimeSpan ElapsedTime, Task task);// ryan was here

        public abstract void ItemCompleted(T current, TimeSpan ElapsedTime);

        public abstract void AllItemCompleted(TimeSpan ElapsedTime);

#pragma warning disable CS4014
        public virtual async Task ProcessAll(IEnumerable<TaskAction<T>> actions)
        {
            this.sempahore = new SemaphoreSlim(MaxTask);
            this.ProcessCounter = 0;
            // this.tmr.Start();

            foreach (TaskAction<T> action in actions)
            {
                await Process(action);
            }

            //for (int i = 0; i < MaxTask; i++)
            //    await sempahore.WaitAsync().ConfigureAwait(false);
        }

        public async virtual Task Process(TaskAction<T> action)
        {
            await sempahore.WaitAsync().ConfigureAwait(false);
            Task.Run<TaskAction<T>>(() =>
            {
                action.action();
                return action;
            }, action.Token.Token).ContinueWith(task =>
         {
             ItemCompleted(task.Result.TaskObject, sw.Elapsed);

             sempahore.Release();
             if (task.IsFaulted) Console.WriteLine(task.Exception.InnerException.Message);

             if (this.ObjectList.Count <= 0)
                 AllItemCompleted(sw.Elapsed);
         });
        }

#pragma warning restore CS4014

        /// <summary>
        /// Release the calls if the task is unterminated
        /// </summary>
        public void Release()
        {
            this.sempahore.Release();
        }

        public void Add(T Object)
        {
            ObjectList.Add(new TaskParallelItem<T>() { Item = Object });
            Task.Run(() => this.Execute(Object));
        }

        public void Delete(Func<T, bool> Compare)
        {
            //foreach (TaskParallelItem<T> obj in ObjectList)

            var obj = ObjectList.Where(x => Compare(x.Item)); //.FirstOrDefault();

            if (obj.Any())
            {
                var g = obj.FirstOrDefault();
                g.Token.Cancel(); //cancel the task before we remove it
                this.TaskRemoved(g.Item);
                ObjectList.Remove(g); 
            }

       

            //for (var i = ObjectList.Count; i <= 0; --i)
            //{
            //    if (Compare(obj.Item))
            //    {
            //        obj.Token.Cancel(); //cancel the task before we remove it
            //        this.TaskRemoved(obj.Item);
            //        ObjectList.Remove(obj);
            //        return;
            //    }
            //}
        }

        public void InitList(IEnumerable<T> lst)
        {
            foreach (T t in lst)
                ObjectList.Add(new TaskParallelItem<T>() { Item = t });
        }

        //private void Tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    if (ProcessCounter < this.ObjectList.Count) ItemCompleted(ProcessCounter, sw.Elapsed);
        //    else {
        //        tmr.Stop();
        //        AllItemCompleted(sw.Elapsed);
        //    }
        //}

        //public void Stop()
        //{

        //}

        public int Count
        {
            get
            {
                return this.ObjectList.Count;
            }
        }

        public int TaskCount
        {
            get
            {
                if (sempahore != null)
                    return this.sempahore.CurrentCount;
                return 0;
            }
        }

        public void Dispose()
        {
            //this.tmr?.Dispose();
            GC.Collect();
            GC.SuppressFinalize(this);
        }

    }

    public sealed class TaskParallelItem<T> : IDisposable
    {
        public T Item { get; set; }
        public CancellationTokenSource Token { get; private set; } = new CancellationTokenSource();

        ~TaskParallelItem()
        {
            Dispose();
        }

        public void Dispose()
        {
            var itm = Item as IDisposable; //try this object if implemented a disposable interface
            itm?.Dispose();
            Token?.Dispose();

            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }

    public sealed class TaskAction<T>
    {
        public T TaskObject { get; set; }
        public Action action { get; set; }
        public CancellationTokenSource Token { get; set; }
    }
}
