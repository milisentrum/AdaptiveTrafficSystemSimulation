namespace TrafficModule.Managers
{
    public abstract class ModuleSettings
    {
        public bool UseModule;

        protected ModuleSettings(bool useModule)
        {
            UseModule = useModule;
        }
    }
}