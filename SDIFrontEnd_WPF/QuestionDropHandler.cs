using GongSolutions.Wpf.DragDrop;
using ITCLib;
using System.Collections;
using System.Linq;
using System.Windows;

namespace SDIFrontEnd_WPF
{   
    public class QuestionRecordDropHandler : IDropTarget
    {

        private readonly Action _onReordered;

        public QuestionRecordDropHandler(Action onReordered)
        {
            _onReordered = onReordered;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.TargetCollection is IList &&
                (dropInfo.Data is SurveyQuestionRecord ||
                 dropInfo.Data is IEnumerable))
            {
                dropInfo.Effects = DragDropEffects.Move;
                dropInfo.NotHandled = false;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.TargetCollection is not IList list)
                return;

            List<SurveyQuestionRecord> draggedItems;

            if (dropInfo.Data is IEnumerable enumerable &&
                dropInfo.Data is not SurveyQuestionRecord)
            {
                draggedItems = enumerable.Cast<SurveyQuestionRecord>().ToList();
            }
            else if (dropInfo.Data is SurveyQuestionRecord item)
            {
                draggedItems = new List<SurveyQuestionRecord> { item };
            }
            else
            {
                return;
            }

            // Preserve original order
            draggedItems = draggedItems
                .OrderBy(i => list.IndexOf(i))
                .ToList();

            int insertIndex = dropInfo.InsertIndex;

            // Adjust insertion index for items removed before it
            foreach (var item in draggedItems)
            {
                int index = list.IndexOf(item);
                if (index >= 0 && index < insertIndex)
                    insertIndex--;
            }

            // Remove all dragged items
            foreach (var item in draggedItems)
                list.Remove(item);

            // Insert them back together
            foreach (var item in draggedItems)
                list.Insert(insertIndex++, item);

            _onReordered?.Invoke();
        }

    }
}