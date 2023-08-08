using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockPenSimWPF.Shared.Models
{
    internal class SortFilterForm
    {
        
        public string? ColumnName { get; set; }

        public SortDirection? ColumnSort { get; set; }

        public string? RowFilter { get; set; }

        public bool HighlightValues { get; set; }
    }
}
