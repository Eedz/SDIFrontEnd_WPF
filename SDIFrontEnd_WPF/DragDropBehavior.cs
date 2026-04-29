using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SDIFrontEnd_WPF
{
    public class DragDropBehavior : Behavior<ListBox>
    {
        private static object draggedItem;
        private static ListBox sourceListBox;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseLeftButtonDown += ListBox_PreviewMouseLeftButtonDown;
            AssociatedObject.Drop += ListBox_Drop;
            AssociatedObject.DragEnter += ListBox_DragEnter;
            AssociatedObject.DragOver += ListBox_DragOver;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseLeftButtonDown -= ListBox_PreviewMouseLeftButtonDown;
            AssociatedObject.Drop -= ListBox_Drop;
            AssociatedObject.DragEnter -= ListBox_DragEnter;
            AssociatedObject.DragOver -= ListBox_DragOver;
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            sourceListBox = (ListBox)sender;

            if (sourceListBox.Items.Count==0)
            {
                return;
            }

            draggedItem = GetDataFromListBox(sourceListBox, e.GetPosition(sourceListBox));

            if (draggedItem != null)
            {
                DataObject dataObject = new DataObject(draggedItem);
                DragDrop.DoDragDrop(sourceListBox, dataObject, DragDropEffects.Move);
            }
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            ListBox targetListBox = (ListBox)sender;
            
            object data = e.Data.GetData(typeof(string));
            
            if (data != null)
            {
                int index = GetIndexUnderMouse(targetListBox, e.GetPosition(targetListBox));
                
                // Remove item from the source ListBox
                if (sourceListBox.ItemsSource is ObservableCollection<string> sourceList)
                {
                    sourceList.Remove((string)draggedItem);
                }

                // Insert item into the target ListBox
                if (targetListBox.ItemsSource is ObservableCollection<string> targetList)
                {
                    if (index == -1)
                    {
                        targetList.Add((string)draggedItem);
                    }
                    else
                    {
                        targetList.Insert(index, (string)draggedItem);
                    }
                }
                draggedItem = null;
            }
        }

        private void ListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(string)) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(string)) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private object GetDataFromListBox(ListBox listBox, Point point)
        {
            UIElement element = listBox.InputHitTest(point) as UIElement;
            if (element == null)
            {
                return null;
            }

            ListBoxItem listBoxItem = listBox.ContainerFromElement(element) as ListBoxItem;

            if (listBoxItem == null)
            {
                return null;
            }

            return listBox.ItemContainerGenerator.ItemFromContainer(listBoxItem);

            
        }

        private int GetIndexUnderMouse(ListBox listBox, Point point)
        {
            UIElement element = listBox.InputHitTest(point) as UIElement;
            if (element != null)
            {
                ListBoxItem listBoxItem = listBox.ContainerFromElement(element) as ListBoxItem;
                if (listBoxItem != null)
                {
                    return listBox.ItemContainerGenerator.IndexFromContainer(listBoxItem);
                }
            }

            return -1;
        }
    }
}

