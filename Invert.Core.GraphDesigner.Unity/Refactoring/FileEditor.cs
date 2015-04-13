//using System;
//using System.Collections.Generic;

//namespace Invert.Core.GraphDesigner.Unity.Refactoring
//{
//    public class FileEditor
//    {
//        public string Filename { get; set; }

//        public List<string> Lines { get; set; }

//        public void InsertAtPosition(int line, int column, string text)
//        {
//            DocumentScript 
//            if (line >= Lines.Length) return;
//            var lineText = Lines[line];
//            var pre = lineText.Substring(0, column);
//            var post = lineText.Substring(column);

//            var newLines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

//            Lines[line] = pre;

//            var lIndex = line;
//            for (int index = 0; index < newLines.Length; index++, lIndex++)
//            {
//                var l = newLines[index];
//                Lines.Insert(line + index, l);
//            }
//            Lines.Insert(lIndex, post);

            
//        }
//    }


//}
