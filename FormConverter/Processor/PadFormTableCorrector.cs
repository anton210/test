using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using FormConverter.Xsf;

namespace FormConverter.Processor
{
    class PadFormTableCorrector
    {
        public static void ApplyTableModes(XmlDocument document, XmlNamespaceManager xmlns)
        {
            var tables = document.DocumentElement.SelectNodes("//table");
            if (tables == null) return;
            foreach (XmlElement table in tables)
            {
                var rows = table.SelectNodes(@"tr | tbody/tr");
                if (rows == null) continue;
                var tableModel = new TableCell[100, 100];
                int tableModelRowCount = 0;
                int tableModelColCount = 0;
                var tableCellsList = new List<TableCell>();
                for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    var row = (XmlElement)rows[rowIndex];
                    var cells = row.SelectNodes("th | td");
                    if (cells == null) continue;
                    for (int colIndex = 0; colIndex < cells.Count; colIndex++)
                    {
                        var cell = (XmlElement)cells[colIndex];

                        var colSpan = cell.Attributes["colSpan"] != null
                                          ? Convert.ToInt32(cell.Attributes["colSpan"].Value)
                                          : 1;

                        var rowSpan = cell.Attributes["rowSpan"] != null
                                          ? Convert.ToInt32(cell.Attributes["rowSpan"].Value)
                                          : 1;

                        var styleAttr = cell.Attributes["style"];

                        var style = new StyleAttributeString(styleAttr != null ? styleAttr.Value : String.Empty);

                        var leftBorderVisible = IsCellBorderVisible(style, "border-left");
                        var rightBorderVisible = IsCellBorderVisible(style, "border-right");
                        var topBorderVisible = IsCellBorderVisible(style, "border-top");
                        var bottomBorderVisible = IsCellBorderVisible(style, "border-bottom");

                        var tableCell = new TableCell(cell)
                        {
                            RowNumber = rowIndex,
                            ColumnNumber = colIndex,
                            TopBorderVisible = topBorderVisible,
                            RightBorderVisible = rightBorderVisible,
                            BottomBorderVisible = bottomBorderVisible,
                            LeftBorderVisible = leftBorderVisible
                        };

                        tableCellsList.Add(tableCell);

                        tableModelRowCount = Math.Max(tableModelRowCount, rowIndex + rowSpan);
                        tableModelColCount = Math.Max(tableModelColCount, colIndex + colSpan);

                        for (int i = 0; i < rowSpan; i++)
                        {
                            var colOffset = 0;
                            for (int j = 0; j < colSpan; j++)
                            {
                                while (tableModel[rowIndex + i, colIndex + colOffset + j] != null)
                                {
                                    colOffset++;
                                }
                                tableModel[rowIndex + i, colIndex + colOffset + j] = tableCell;
                            }
                        }
                    }
                }

                for (int row = 0; row < tableModelRowCount; row++)
                {
                    for (int col = 0; col < tableModelColCount; col++)
                    {
                        var cell = tableModel[row, col];
                        if (!cell.LeftBorderVisible && (col == 0 || tableModel[row, col - 1].CellType == 0))
                        {
                            cell.CellType = InternalCellType.Outher;
                        }
                        else if (!cell.TopBorderVisible && (row == 0 || tableModel[row - 1, col].CellType == 0))
                        {
                            cell.CellType = InternalCellType.Outher;
                        }

                    }
                }

                for (int row = 0; row < tableModelRowCount; row++)
                {
                    for (int col = 0; col < tableModelColCount; col++)
                    {
                        var cell = tableModel[row, col];
                        if (cell.CellType == InternalCellType.Inner)
                        {
                            // Top 
                            if (row == 0 || tableModel[row - 1, col].CellType == InternalCellType.Outher)
                            {
                                cell.Edges |= CellEdgeType.Top;
                            }

                            // Left
                            if (col == 0 || tableModel[row, col - 1].CellType == InternalCellType.Outher)
                            {
                                cell.Edges |= CellEdgeType.Left;
                            }

                            // Bottom 
                            if (row == tableModelRowCount - 1 || tableModel[row + 1, col].CellType == InternalCellType.Outher)
                            {
                                cell.Edges |= CellEdgeType.Bottom;
                            }

                            // Right
                            if (col == tableModelColCount - 1 || tableModel[row, col + 1].CellType == InternalCellType.Outher)
                            {
                                cell.Edges |= CellEdgeType.Right;
                            }
                        }
                    }
                }

                foreach (var tableCell in tableCellsList)
                {
                    if (tableCell.CellType == InternalCellType.Inner) SetElementClass(tableCell.XmlElement, "inner-cell");
                    if (tableCell.CellType == InternalCellType.Outher) SetElementClass(tableCell.XmlElement, "outher-cell");

                    if ((tableCell.Edges & CellEdgeType.Top) == CellEdgeType.Top &&
                         (tableCell.Edges & CellEdgeType.Left) == CellEdgeType.Left) SetElementClass(tableCell.XmlElement, "ui-corner-tl");

                    if ((tableCell.Edges & CellEdgeType.Bottom) == CellEdgeType.Bottom &&
                         (tableCell.Edges & CellEdgeType.Left) == CellEdgeType.Left) SetElementClass(tableCell.XmlElement, "ui-corner-bl");

                    if ((tableCell.Edges & CellEdgeType.Top) == CellEdgeType.Top &&
                         (tableCell.Edges & CellEdgeType.Right) == CellEdgeType.Right) SetElementClass(tableCell.XmlElement, "ui-corner-tr");

                    if ((tableCell.Edges & CellEdgeType.Bottom) == CellEdgeType.Bottom &&
                         (tableCell.Edges & CellEdgeType.Right) == CellEdgeType.Right) SetElementClass(tableCell.XmlElement, "ui-corner-br");

                    if ((tableCell.Edges & CellEdgeType.Top) == CellEdgeType.Top) SetElementClass(tableCell.XmlElement, "cell-top");
                    if ((tableCell.Edges & CellEdgeType.Right) == CellEdgeType.Right) SetElementClass(tableCell.XmlElement, "cell-right");
                    if ((tableCell.Edges & CellEdgeType.Bottom) == CellEdgeType.Bottom) SetElementClass(tableCell.XmlElement, "cell-bottom");
                    if ((tableCell.Edges & CellEdgeType.Left) == CellEdgeType.Left) SetElementClass(tableCell.XmlElement, "cell-left");

                }

            }
        }

        private class TableCell
        {
            public TableCell(XmlElement xmlElement)
            {
                XmlElement = xmlElement;
                CellType = InternalCellType.Inner;
                Edges = CellEdgeType.None;
            }

            public int RowNumber { get; set; }
            public int ColumnNumber { get; set; }
            public bool LeftBorderVisible { get; set; }
            public bool RightBorderVisible { get; set; }
            public bool TopBorderVisible { get; set; }
            public bool BottomBorderVisible { get; set; }

            public InternalCellType CellType { get; set; }
            public CellEdgeType Edges { get; set; }

            public XmlElement XmlElement { get; private set; }

            public override string ToString()
            {
                return String.Format("{0}:{1} {2}", RowNumber, ColumnNumber, CellType);
            }
        }

        private enum InternalCellType
        {
            Outher = 0,
            Inner
        }

        [Flags]
        private enum CellEdgeType
        {
            None = 0x0,
            Left = 0x1,
            Right = 0x2,
            Top = 0x4,
            Bottom = 0x8
        }

        private static bool IsCellBorderVisible(StyleAttributeString style, string styleProperty)
        {
            var solidBorders = new[] { "solid", "dashed", "dotted", "double", "groove", "ridge", "inset", "outset" };
            bool borderVisible = false;
            if (style.Properties.ContainsKey(styleProperty))
            {
                foreach (var solidBorder in solidBorders)
                {
                    borderVisible = style.Properties[styleProperty].Contains(solidBorder);
                    if (borderVisible) break;
                }
            }
            return borderVisible;
        }

        private static void SetElementClass(XmlElement cell, string cellClassName)
        {
            string @class = String.Empty;
            if (cell.HasAttribute("class"))
            {
                @class = cell.Attributes["class"].Value + " " + cellClassName;
            }
            else
            {
                @class = cellClassName;
            }

            cell.SetAttribute("class", @class);
        }
    }
}
