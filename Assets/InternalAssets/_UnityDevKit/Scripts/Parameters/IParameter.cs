namespace UnityDevKit.Parameters
{
    public interface IParameter
    {
        string GetName();
        float GetValue();
        string GetUnits();
    }
}