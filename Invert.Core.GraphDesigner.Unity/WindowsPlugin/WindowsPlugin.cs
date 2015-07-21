using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Unity;
using Invert.IOC;
using Invert.Windows;
using Invert.Windows.Unity;
using UnityEditor;

public class WindowsPlugin : DiagramPlugin, IWindowsEvents {

    private static List<IWindowFactory> _laucherWindows;
    public static List<IWindowDrawer> _windowDrawers; 
    public override void Initialize(UFrameContainer container)
    {
        ListenFor<IWindowsEvents>();
    }

    public static Rect GetPosition(EditorWindow window)
    {
        var vector2 = new Vector2((Screen.currentResolution.width - window.position.width) / 2, (Screen.currentResolution.height - window.position.height) / 2);
        return new Rect(vector2.x, vector2.y, (float)window.position.width, (float)window.position.height);
    }

    public override void Loaded(UFrameContainer container)
    {
        base.Loaded(container);
        LaucherWindows = container.ResolveAll<IWindowFactory>().Where(c => c.ShowInLauncher).ToList();
    }

    public static List<IWindowFactory> LaucherWindows 
    {
        get { return _laucherWindows ?? (_laucherWindows = new List<IWindowFactory>()); }
        set { _laucherWindows = value; }
    }

    public static List<IWindowDrawer> WindowDrawers
    {
        get { return _windowDrawers ?? ( _windowDrawers = new List<IWindowDrawer>()); }
        set { _windowDrawers = value; }
    }

    public void WindowRequestCloseWithArea(Area drawer)
    {
        WindowDrawers.Where(d => d.Drawers.Contains(drawer)).ToList().ForEach(d =>
        {
            (d as EditorWindow).Close();
        });
    }

    public void WindowRequestCloseWithViewModel(IWindow windowViewModel)
    {
        WindowDrawers.Where(d => d.ViewModel == windowViewModel).ToList().ForEach(d =>
        {
            (d as EditorWindow).Close();
        });
    }

    public static SmartWindow GetWindowFor(string factoryId, IWindow viewModel = null, bool createNewIfMultipleAllowed = true)
    {

        SmartWindow drawer = null;

        var factory = InvertApplication.Container.Resolve<IWindowFactory>(factoryId);

        if (factory.Multiple && createNewIfMultipleAllowed)
        {
            
        }
        else if(factory.Multiple && !createNewIfMultipleAllowed)
        {
            drawer = GetByFactoryId(factoryId).FirstOrDefault() as SmartWindow;
        }
        else if (!factory.Multiple)
        {
            drawer = GetByFactoryId(factoryId).FirstOrDefault() as SmartWindow;
        }

        if (drawer == null)
        {
            drawer = ScriptableObject.CreateInstance<SmartWindow>();
            drawer.WindowFactoryId = factoryId;
            BindDrawerToWindow(drawer, factory, viewModel);
        }

        return drawer;
    }

    public static IEnumerable<IWindowDrawer> GetByFactoryId(string identifier)
    {
        return WindowDrawers.Where(d => d.WindowFactoryId == identifier);
    }

    public static void BindDrawerToWindow(IWindowDrawer drawer , IWindowFactory factory, IWindow window = null )
    {
        if(window == null) window = factory.GetDefaultViewModelObject(drawer.PersistedData); 
        if(window.GetType() != factory.ViewModelType) throw new Exception("Type of viewmodel != vm type of the factory");
        drawer.PersistedData = null;
        drawer.WindowFactoryId = factory.Identifier;
        drawer.ViewModel = window;
        if(!WindowDrawers.Contains(drawer))
            WindowDrawers.Add(drawer);
        factory.SetAreasFor(drawer);
    }

    public static void BindDrawerToWindow(IWindowDrawer drawer , string factoryId )
    {
        if (string.IsNullOrEmpty(factoryId)) throw new Exception("Bad bad bad");
        var factory = InvertApplication.Container.Resolve<IWindowFactory>(drawer.WindowFactoryId); 
        BindDrawerToWindow(drawer, factory);
    }

}

public interface IWindowFactory
{
    IWindow GetDefaultViewModelObject(string persistedData);
    Type ViewModelType { get; }
    string Identifier { get; set; }
    bool ShowInLauncher { get; set; }
    bool Multiple { get; set; }
    string LauncherTitle { get; set; }
    void SetAreasFor(IWindowDrawer drawer);
}

public interface IWindowFactory<TWindow> : IWindowFactory
{
    
}
public interface IWindowsEvents
{
    void WindowRequestCloseWithArea(Area drawer);
    void WindowRequestCloseWithViewModel(IWindow windowViewModel);
}

public interface IWindow
{
    string Identifier { get; set; }
}

public class KeyboardDispatcher : Area<IKeyHandler>
{
    public override void Draw(IKeyHandler data)
    {
        if (Event.current.type == EventType.KeyDown)
        {
            data.KeyPressed(Event.current.keyCode, Event.current.alt);
            EditorWindow.focusedWindow.Repaint();
        }
        if (Event.current.type == EventType.KeyUp)
        {
            data.KeyReleased(Event.current.keyCode, Event.current.alt);
            EditorWindow.focusedWindow.Repaint();
        }
    }
}

public interface IKeyHandler
{
    void KeyPressed(KeyCode code, bool withAlt);
    void KeyReleased(KeyCode code, bool withAlt);
}

