using System;
using System.Reflection;
using System.Windows.Forms;

namespace CKAN
{
    public class KSPDataGridView : System.Windows.Forms.DataGridView
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyData & e.KeyCode)
            {
                case Keys.Down:
                    SelectNextVisibleRow();
                    break;
                case Keys.Up:
                    SelectPreviousVisibleRow();
                    break;
                case Keys.Space:
                    ToggleCurrentSelection();
                    break;
                default:
                    base.OnKeyDown(e);
                    return;
            }

            // Wo do not want anyone else to handle this event.
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        /// <summary>
        /// Gets the index of the currently selected row, or -1 if none is selected.
        /// </summary>
        /// <returns>The current index.</returns>
        private int GetCurrentIndex()
        {
            // Check for null.
            if (this.SelectedRows == null)
            {
                return -1;
            }
            if (this.SelectedRows[0] == null)
            {
                return -1;
            }

            // Get the currently selected row.
            return this.SelectedRows[0].Index;
        }

        /// <summary>
        /// Selects the next visible row.
        /// </summary>
        private void SelectNextVisibleRow()
        {
            // Get the currently selected row.
            int index = GetCurrentIndex();

            if (index < 0)
            {
                return;
            }

            // Do a boundary check.
            if (index == this.Rows.Count - 1)
            {
                return;
            }

            // Look for the next visible row.
            for (int i = index + 1; i < this.Rows.Count; i++)
            {
                if (this.Rows[i].Visible)
                {
                    // Deselect the current row.
                    this.Rows[index].Selected = false;

                    // Select the visible row.
                    this.Rows[i].Selected = true;

                    UpdateScroll();

                    return;
                }
            }
        }

        /// <summary>
        /// Selects the previous visible row.
        /// </summary>
        private void SelectPreviousVisibleRow()
        {
            // Get the currently selected row.
            int index = GetCurrentIndex();

            if (index < 0)
            {
                return;
            }

            // Look for the previous visible row.
            for (int i = index - 1; i >= 0; i--)
            {
                if (this.Rows[i].Visible)
                {
                    // Deselect the current row.
                    this.Rows[index].Selected = false;

                    // Select the visible row.
                    this.Rows[i].Selected = true;

                    UpdateScroll();

                    return;
                }
            }
        }

        /// <summary>
        /// Toggles the checkbox of the current row.
        /// </summary>
        private void ToggleCurrentSelection()
        {
            // Get the currently selected row.
            int index = GetCurrentIndex();

            if (index < 0)
            {
                return;
            }

            // Get the checkbox.
            var selectedRowCheckBox = (DataGridViewCheckBoxCell)this.Rows[index].Cells["Installed"];

            // Invert the value.
            bool selectedValue = (bool)selectedRowCheckBox.Value;
            selectedRowCheckBox.Value = !selectedValue;
        }

        /// <summary>
        /// Update the scroll view to include the currently selected row.
        /// </summary>
        public void UpdateScroll()
        {
            // Get the currently selected row.
            int index = GetCurrentIndex();

            if (index < 0)
            {
                return;
            }

            // Check if we are in the view.
            int startRow = this.FirstDisplayedScrollingRowIndex;
            int displayedRows = this.DisplayedRowCount(false);

            int currentRow = startRow + 1;
            int finalRow = 0;
            int foundRows = 0;

            // We need to keep in mind the invisible rows.
            do
            {
                // Check if we have reached the end.
                if (currentRow >= this.Rows.Count)
                {
                    break;
                }

                // Is this item visible?
                if(this.Rows[currentRow].Visible)
                {
                    foundRows++;
                    finalRow = currentRow;
                }

                // Increment the index.
                currentRow++;
            } while (foundRows < displayedRows);

            // Is the current row within the bounds?
            if (index > startRow && index < finalRow)
            {
                // Do nothing, we are visible.
                return;
            }
            else
            {
                // Mono workaround.
                if (!Platform.IsWindows)
                {
                    try
                    {
                        var first_row_index = this.GetType().BaseType.GetField("first_row_index", BindingFlags.NonPublic | BindingFlags.Instance);
                        var vertical_scroll_bar = this.GetType().BaseType.GetField("verticalScrollBar", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
                        var safe_set_method = vertical_scroll_bar.GetType().BaseType.GetMethod("SafeValueSet", BindingFlags.NonPublic | BindingFlags.Instance);

                        first_row_index.SetValue(this, index);
                        safe_set_method.Invoke(vertical_scroll_bar, new object[] { index * this.SelectedRows[0].Height });
                    }
                    catch
                    {
                        //Compared to crashing ignoring the keypress is fine.
                    }
                    this.FirstDisplayedScrollingRowIndex = index;
                    this.Refresh();
                }
                else
                {
                    // Move the startindex to this element.
                    this.FirstDisplayedScrollingRowIndex = index;
                }
            }
        }
    }
}
