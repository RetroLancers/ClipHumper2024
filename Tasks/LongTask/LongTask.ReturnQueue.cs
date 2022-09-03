using OpenCvSharp;

namespace ClipHunta2;

public partial class LongTaskWithReturn<T, TR>
{
    public class ReturnQueue 
    {
        private readonly AutoResetEvent _are = new(false);
        private TR? _value;

        public void SetValue(TR? value)
        {
            _value = value;
            _are.Set();
        }

        public TR? GetReturn()
        {
            _are.WaitOne();
            return _value;
        }
    }
}