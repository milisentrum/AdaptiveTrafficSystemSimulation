namespace CompressorsModule.Workflow
{
    public interface IWorkflow<T>
        where T : IWorkflowParams
    {
        void Apply(T parameters);
        string GetName();
    }
}