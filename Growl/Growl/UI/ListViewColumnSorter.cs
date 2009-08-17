using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Windows.Forms;

namespace Growl.UI
{
    public class ListViewColumnSorter : IComparer<ListViewItem>
    {
        /// <summary>
        /// Specifies the column to be sorted
        /// </summary>
        private int columnToSort;

        /// <summary>
        /// Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        private SortOrder sortOrder;

        /// <summary>
        /// The type of comparison to use
        /// </summary>
        private ComparisonType type;

        /// <summary>
        /// Class constructor.  Initializes various elements
        /// </summary>
        public ListViewColumnSorter()
        {
            columnToSort = 0;
            sortOrder = SortOrder.None;
            type = ComparisonType.String;
        }

        /// <summary>
        /// Compare two ListViewItem objects
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(ListViewItem listviewX, ListViewItem listviewY)
        {
            bool fallbackToStringComparison = true;
            int compareResult = 0;

            string s1 = listviewX.SubItems[this.ColumnToSort].Text;
            string s2 = listviewY.SubItems[this.ColumnToSort].Text;

            // Compare the two items using any special comparisons
            switch (this.Type)
            {
                case ComparisonType.Date :
                    DateTime d1;
                    DateTime d2;
                    bool d1OK = DateTime.TryParse(s1, out d1);
                    bool d2OK = DateTime.TryParse(s2, out d2);
                    if (d1OK && d2OK)
                    {
                        fallbackToStringComparison = false;
                        compareResult = DateTime.Compare(d1, d2);
                    }
                    break;
                case ComparisonType.Numeric :
                    int i1;
                    int i2;
                    bool i1OK = int.TryParse(s1, out i1);
                    bool i2OK = int.TryParse(s2, out i2);
                    if (i1OK && i2OK)
                    {
                        fallbackToStringComparison = false;
                        compareResult = i1.CompareTo(i2);
                    }
                    break;
            }

            // if this is a straight string comparison or the special comparison failed, do the string comparison now
            if (fallbackToStringComparison) compareResult = String.Compare(s1, s2, true);

            if (this.Order == SortOrder.Descending)
            {
                // Descending sort is selected, return negative result of compare operation
                compareResult = -compareResult;
            }
            return compareResult;
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int ColumnToSort
        {
            set
            {
                this.columnToSort = value;
            }
            get
            {
                return this.columnToSort;
            }
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order
        {
            set
            {
                this.sortOrder = value;
            }
            get
            {
                return this.sortOrder;
            }
        }

        public ComparisonType Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        public enum ComparisonType
        {
            String,
            Numeric,
            Date
        }
    }
}
