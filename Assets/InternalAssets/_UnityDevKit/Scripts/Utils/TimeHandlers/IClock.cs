namespace UnityDevKit.Utils.TimeHandlers
{
    public interface IClock
    {
        void Launch();
        Clock.Data Stop();
        void Pause();
        void Resume();
    }
}