using System.Collections.Generic;

namespace Invert.uFrame.Editor.ViewModels
{
    public interface IConnectionStrategy
    {
  
        /// <summary>
        /// Try and connect a to b.  If it can't connect return null
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>The connection created if any. Null if no connection can be made</returns>
        ConnectionViewModel Connect(ConnectorViewModel a, ConnectorViewModel b);

        /// <summary>
        /// Iterate through connectors and find decorate the connections list with any found connections.
        /// </summary>
        /// <param name="connections"></param>
        /// <param name="info"></param>
        void GetConnections(List<ConnectionViewModel> connections, ConnectorInfo info);
    }
}