using UnityEngine;

namespace CompressorsModule.Workflow
{
    public abstract class WorkflowController<T> : MonoBehaviour
        where T : IWorkflowParams
    {
        [SerializeField] private T parameters;

        public static IWorkflow<T> CurrentWorkflow { get; private set; }

        private void Start()
        {
            Apply();
        }

        public static void SetupWorkflow(IWorkflow<T> workflow)
        {
            CurrentWorkflow = workflow;
        }

        private void Apply()
        {
            if (CurrentWorkflow == null)
            {
                Debug.LogError("[WorkflowController] There's no workflow.");
            }

            CurrentWorkflow?.Apply(parameters);
        }
    }
}