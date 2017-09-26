using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace z.Diagnostics
{
    public class TaskEach<T>
    {
        private List<TaskEachItem<T>> ObjectList;

        public CancellationTokenSource Token { get; private set; } = new CancellationTokenSource();

        public void Execute()
        {

            Parallel.ForEach(ObjectList, new ParallelOptions() { CancellationToken = Token.Token, MaxDegreeOfParallelism = Environment.ProcessorCount }, v=> {
                //do some shit
            });


        }

      //  public abstract Action<T> OnExecute();

    }

    public sealed class TaskEachItem<T> : IDisposable
    {
        public T Item { get; set; }

        ~TaskEachItem()
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
}
