using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Invert.Core.GraphDesigner
{
    public interface IDiagramNodeItem : ISelectable, IJsonObject, IItem, IConnectable
    {
        bool Precompiled { get; set; }
        string Name { get; set; }
        string Highlighter { get; }
        string FullLabel { get; }
        bool IsSelectable { get;}
        DiagramNode Node { get; set; }
        [Browsable(false)]
        DataBag DataBag { get; set; }
        
        /// <summary>
        /// Is this node currently in edit mode/ rename mode.
        /// </summary>
        bool IsEditing { get; set; }

        FlagsDictionary Flags { get; set; }

        //void Remove(IDiagramNode diagramNode);
        void Rename(IDiagramNode data, string name);
        void NodeRemoved(IDiagramNode nodeData);
        void NodeItemRemoved(IDiagramNodeItem nodeItem);
        void NodeAdded(IDiagramNode data);
        void NodeItemAdded(IDiagramNodeItem data);
        void Validate(List<ErrorInfo> info);
        bool ValidateInput(IDiagramNodeItem arg1, IDiagramNodeItem arg2);
        bool ValidateOutput(IDiagramNodeItem arg1, IDiagramNodeItem arg2);
    }

    public class ErrorInfo
    {
        protected bool Equals(ErrorInfo other)
        {
            return string.Equals(Identifier, other.Identifier) && string.Equals(Message, other.Message);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ErrorInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Identifier != null ? Identifier.GetHashCode() : 0)*397) ^ (Message != null ? Message.GetHashCode() : 0);
            }
        }

        public string Identifier { get; set; }
        public string Message { get; set; }
        public Action AutoFix { get; set; }
        public ValidatorType Siverity { get; set; }
    }

    public static class ErrorInfoExtensions
    {
        public static ErrorInfo AddError(this List<ErrorInfo> list, string message, string identifier = null,
            Action autoFix = null)
        {
            var error = new ErrorInfo()
            {
                Message = message,
                Identifier = identifier,
                AutoFix = autoFix,
                Siverity = ValidatorType.Error
            };
            if (!list.Any(p=>p.Equals(error)))
            list.Add(error);
            return error;
        }
        public static ErrorInfo AddWarning(this List<ErrorInfo> list, string message, string identifier = null,
         Action autoFix = null)
        {
            var error = new ErrorInfo()
            {
                Message = message,
                Identifier = identifier,
                AutoFix = autoFix,
                Siverity = ValidatorType.Warning
            };
            list.Add(error);
            return error;
        }
        public static ErrorInfo AddInfo(this List<ErrorInfo> list, string message, string identifier = null,
         Action autoFix = null)
        {
            var error = new ErrorInfo()
            {
                Message = message,
                Identifier = identifier,
                AutoFix = autoFix,
                Siverity = ValidatorType.Info
            };
            list.Add(error);
            return error;
        }
    }
}