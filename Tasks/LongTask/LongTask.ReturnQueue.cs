namespace ClipHunta2.Tasks.LongTask;

public partial class LongTaskWithReturn<T, TR>
{
    public class ReturnQueue
    {
        private readonly CancellationTokenSource _source = new();
        private TR? _value;

        public void SetValue(TR? value)
        {
 
            _value = value;
            _source.Cancel();
        }

        public TR? GetReturn()
        {
            try
            {
                Task.Delay(-1, _source.Token).Wait();
            }
            catch (Exception e)
            {
            }

         

            return _value;
        }
    }
}