using System;

public interface IHasProgress
{
    public event EventHandler OnStartProgress;
    public event EventHandler OnEndProgress;
    public event EventHandler<OnProgressChangedEventArgs> OnProgressChanged;
    public class OnProgressChangedEventArgs : EventArgs
    {
        public float progressNormalized;
    }
}
