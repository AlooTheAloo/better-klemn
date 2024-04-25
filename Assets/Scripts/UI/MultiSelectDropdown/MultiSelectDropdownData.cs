using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class MultiSelectDropDownData
{
    /// <summary>
    /// A list of indexes into the list of <see cref="Options"/> of the selected values.
    /// </summary>
    public List<int> SelectedIndexes = new List<int>();
    
    
    /// <summary>
    /// The list of selectable options the end-user can choose from for this given dropdown.
    /// </summary>
    public List<string> Options = new List<string>();
    
    /// <summary>
    /// The currently selected items, if any are selected, otherwise an empty list.
    /// </summary>
    public List<string> CurrentSelections
    {
        get
        {
            if (Options == null || SelectedIndexes.Count == 0)
            {
                // If the dropdown doesn't have any options or the
                // SelectedIndex is out of range, indicate nothing selected.
                return new List<string>();
            }
            
            // Return the selected option
            return SelectedIndexes.Select(x => Options[x]).ToList();
        }
    }
}