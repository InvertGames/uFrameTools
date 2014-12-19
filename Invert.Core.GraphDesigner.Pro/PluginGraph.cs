using System.Collections;

#if UNITY_DLL
public class PluginGraph : UnityGraphData<PluginGraphData>
{ }
#endif
public class PluginGraphData : GenericGraphData<ShellPluginNode>
{
    
}
