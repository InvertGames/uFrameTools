﻿using System.ComponentModel;
using Invert.IOC;

namespace Invert.Core
{
    public interface ICorePlugin
    {
        string Title { get; }
        bool Enabled { get; set; }
        decimal LoadPriority { get; }
        string PackageName { get; }
        bool Required { get; }
        bool Ignore { get; }
        UFrameContainer Container { get; set; }
        void Initialize(UFrameContainer container);
        void Loaded(UFrameContainer container);
        
    }

    public interface IDiagramPlugin : ICorePlugin
    {
        
    }
    public interface IExecuteCommand<in TCommandType> where TCommandType : ICommand
    {
        void Execute(TCommandType command);
    }
    public interface ICommand
    {
        string Title { get; set; }
    }

    public interface IBackgroundCommand : ICommand
    {
        BackgroundWorker Worker { get; set; }
    }

    public abstract class CorePlugin : ICorePlugin
    {
        private UFrameContainer _container;

        public void Execute<TCommand>(TCommand command) where TCommand :  ICommand
        {
            InvertApplication.Execute(command);
        }
        public virtual string PackageName
        {
            get { return string.Empty; }
        }

        public virtual bool Required
        {
            get { return false; }
        }

        public virtual bool Ignore
        {
            get { return false; }
        }

        public virtual string Title
        {
            get { return this.GetType().Name; }
        }

        public abstract bool Enabled { get; set; }

        public virtual bool EnabledByDefault
        {
            get { return true; }
        }

        public virtual decimal LoadPriority { get { return 1; } }

        public virtual void Initialize(UFrameContainer container)
        {
            Container = container;
        }

        public UFrameContainer Container
        {
            get { return InvertApplication.Container; }
            set { _container = value; }
        }

        public abstract void Loaded(UFrameContainer container);
    }


    public interface IFeature
    {
        
    }
    
}