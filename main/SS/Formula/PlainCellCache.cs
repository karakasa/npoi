/* ====================================================================
   Licensed To the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for Additional information regarding copyright ownership.
   The ASF licenses this file To You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed To in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
==================================================================== */

namespace NPOI.SS.Formula
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using NPOI.Util;

    public class Loc : IEquatable<Loc>
    {

        private long _bookSheetColumn;

        private int _rowIndex;

        public Loc(int bookIndex, int sheetIndex, int rowIndex, int columnIndex)
        {
            _bookSheetColumn = ToBookSheetColumn(bookIndex, sheetIndex, columnIndex);
            _rowIndex = rowIndex;
        }

        public static long ToBookSheetColumn(int bookIndex, int sheetIndex, int columnIndex)
        {
            return ((bookIndex & 0xFFFFL) << 48) + ((sheetIndex & 0xFFFFL) << 32) + ((columnIndex & 0xFFFFL) << 0);
        }

        public Loc(int bookSheetColumn, int rowIndex)
        {
            _bookSheetColumn = bookSheetColumn;
            _rowIndex = rowIndex;
        }
        public override int GetHashCode()
        {
            return (int)(_bookSheetColumn ^ (Operator.UnsignedRightShift(_bookSheetColumn , 32))) + 17 * _rowIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is Loc loc && Equals(loc);
        }
        public bool Equals(Loc obj)
        {
            Loc other = (Loc)obj;
            return _bookSheetColumn == other._bookSheetColumn && _rowIndex == other._rowIndex;
        }

        public int RowIndex
        {
            get
            {
                return _rowIndex;
            }
        }
        public int ColumnIndex
        {
            get
            {
                return (int)(_bookSheetColumn & 0x000FFFF);
            }
        }
        public int SheetIndex
        {
            get
            {
                return (int)((_bookSheetColumn >> 32) & 0xFFFF);
            }
        }

        public int BookIndex
        {
            get
            {
                return (int)((_bookSheetColumn >> 48) & 0xFFFF);
            }
        }
    }
    /**
     *
     * @author Josh Micich
     */
    public class PlainCellCache
    {

        private Dictionary<Loc, PlainValueCellCacheEntry> _plainValueEntriesByLoc;

        public PlainCellCache()
        {
            _plainValueEntriesByLoc = new Dictionary<Loc, PlainValueCellCacheEntry>();
        }
        public void Put(Loc key, PlainValueCellCacheEntry cce)
        {
            _plainValueEntriesByLoc[key] = cce;
        }
        public void Clear()
        {
            _plainValueEntriesByLoc.Clear();
        }
        public PlainValueCellCacheEntry Get(Loc key)
        {
            return _plainValueEntriesByLoc.TryGetValue(key, out var o) ? o : null;
        }
        public void Remove(Loc key)
        {
            _plainValueEntriesByLoc.Remove(key);
        }
    }
}