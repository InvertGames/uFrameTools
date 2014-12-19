using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Invert.Core;
using Invert.GraphDesigner.WPF.Controls;

namespace DiagramDesigner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //InvertApplication.CachedAssemblies.Add(this.GetType().Assembly);
            //InvertApplication.CachedAssemblies.Add(typeof(ShellPluginNode).Assembly);
            //InvertApplication.CachedAssemblies.Add(typeof(DiagramControl).Assembly);
            InvertApplication.CachedAssemblies.Clear();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                InvertApplication.CachedAssemblies.Add(assembly);
            }
        }
    }
}
