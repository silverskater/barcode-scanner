using System.Collections;
using System.Windows.Forms;

public class ListViewColumnSorter : IComparer
{
    private int _columnToSort;
    private SortOrder _orderOfSort;
    private readonly CaseInsensitiveComparer _objectCompare;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListViewColumnSorter"/> class.
    /// </summary>
    public ListViewColumnSorter()
    {
        _columnToSort = 0;
        _orderOfSort = SortOrder.None;
        _objectCompare = new CaseInsensitiveComparer();
    }

    /// <summary>
    /// Compares two ListView items.
    /// </summary>
    /// <param name="x">First item to compare.</param>
    /// <param name="y">Second item to compare.</param>
    /// <returns>
    /// A signed integer that indicates the relative values of x and y.
    /// </returns>
    public int Compare(object x, object y)
    {
        int compareResult;
        var listViewX = (ListViewItem)x;
        var listViewY = (ListViewItem)y;

        // Compare the two items.
        compareResult = _objectCompare.Compare(listViewX.SubItems[_columnToSort].Text, listViewY.SubItems[_columnToSort].Text);

        // Calculate the correct return value based on the object comparison.
        if (_orderOfSort == SortOrder.Ascending)
        {
            return compareResult;
        }
        else if (_orderOfSort == SortOrder.Descending)
        {
            return -compareResult;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
    /// </summary>
    public int SortColumn
    {
        get { return _columnToSort; }
        set { _columnToSort = value; }
    }

    /// <summary>
    /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
    /// </summary>
    public SortOrder Order
    {
        get { return _orderOfSort; }
        set { _orderOfSort = value; }
    }
}
