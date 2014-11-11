using Invert.uFrame;

namespace Invert.Core
{
    public interface ICorePlugin
    {
        string Title { get; }
        bool Enabled { get; set; }
        decimal LoadPriority { get; }
        string PackageName { get; }
        bool Required { get; }
        void Initialize(uFrameContainer container);
        void Loaded();
    }

    public interface IDiagramPlugin : ICorePlugin
    {
        
    }

    public abstract class CorePlugin : ICorePlugin
    {
        public virtual string PackageName
        {
            get { return string.Empty; }
        }

        public virtual bool Required
        {
            get { return false; }
        }

        public string Title
        {
            get { return this.GetType().Name; }
        }

        public abstract bool Enabled { get; set; }

        public virtual bool EnabledByDefault
        {
            get { return true; }
        }

        public virtual decimal LoadPriority { get { return 1; } }
        public abstract void Initialize(uFrameContainer container);
        public abstract void Loaded();
    }
}