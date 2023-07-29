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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public interface IEntryOperation
    {
        void ProcessEntry(FormulaCellCacheEntry entry);
    }

    /**
     * 
     * @author Josh Micich
     */
    public class FormulaCellCache
    {

        private Dictionary<object, FormulaCellCacheEntry> _formulaEntriesByCell;

        public FormulaCellCache()
        {
            // assumes HSSFCell does not override HashCode or Equals, otherwise we need IdentityHashMap
            _formulaEntriesByCell = new Dictionary<object, FormulaCellCacheEntry>();
        }

        public CellCacheEntry[] GetCacheEntries()
        {
            return _formulaEntriesByCell.Values.ToArray();
        }

        public void Clear()
        {
            _formulaEntriesByCell.Clear();
        }

        /**
         * @return <c>null</c> if not found
         */
        public FormulaCellCacheEntry Get(IEvaluationCell cell)
        {
            return _formulaEntriesByCell.TryGetValue(cell.IdentityKey, out var entry) ? entry : null;
        }

        public void Put(IEvaluationCell cell, FormulaCellCacheEntry entry)
        {
            _formulaEntriesByCell[cell.IdentityKey] = entry;
        }

        public FormulaCellCacheEntry Remove(IEvaluationCell cell)
        {
            FormulaCellCacheEntry tmp = (FormulaCellCacheEntry)_formulaEntriesByCell[cell.IdentityKey];

            // Original code may be wrong.
            // _formulaEntriesByCell.Remove(cell);
            _formulaEntriesByCell.Remove(cell.IdentityKey);
            return tmp;
        }

        public void ApplyOperation(IEntryOperation operation)
        {
            foreach (var it in _formulaEntriesByCell.Values)
                operation.ProcessEntry(it);
        }
    }
}