/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
==================================================================== */

namespace NPOI.HSSF.Model
{
    using System;
    //using System.Collections;
    using System.Collections.Generic;
    using NPOI.SS.Util;
    using NPOI.HSSF.Record;
    using NPOI.HSSF.Record.Aggregates;
    using System.Collections;
    using NPOI.Util;

    /**
     * Segregates the 'Row Blocks' section of a single sheet into plain row/cell records and 
     * shared formula records.
     * 
     * @author Josh Micich
     */
    public class RowBlocksReader
    {

        private List<Record> _plainRecords;
        private SharedValueManager _sfm;
        private MergeCellsRecord[] _mergedCellsRecords;

        /**
         * Also collects any loose MergeCellRecords and puts them in the supplied
         * mergedCellsTable
         */
        public RowBlocksReader(RecordStream rs)
        {
            var plainRecords = new List<Record>();
            var shFrmRecords = new List<SharedFormulaRecord>();
            var arrayRecords = new List<ArrayRecord>();
            var tableRecords = new List<TableRecord>();
            var mergeCellRecords = new List<MergeCellsRecord>();
            List<CellReference> firstCellRefs = new List<CellReference>();
            Record prevRec = null;

            while (!RecordOrderer.IsEndOfRowBlock(rs.PeekNextSid()))
            {
                // End of row/cell records for the current sheet
                // Note - It is important that this code does not inadvertently add any sheet 
                // records from a subsequent sheet.  For example, if SharedFormulaRecords 
                // are taken from the wrong sheet, this could cause bug 44449.
                if (!rs.HasNext())
                {
                    throw new InvalidOperationException("Failed to find end of row/cell records");

                }
                Record rec = rs.GetNext();
                List<Record> dest;
                switch (rec.Sid)
                {
                    case MergeCellsRecord.sid:
                        mergeCellRecords.Add((MergeCellsRecord)rec);
                        break;
                    case SharedFormulaRecord.sid:
                        if (prevRec is not FormulaRecord)
                        {
                            throw new Exception("Shared formula record should follow a FormulaRecord");
                        }
                        FormulaRecord fr = (FormulaRecord)prevRec;
                        firstCellRefs.Add(new CellReference(fr.Row, fr.Column));

                        shFrmRecords.Add((SharedFormulaRecord)rec);
                        break;
                    case ArrayRecord.sid:
                        arrayRecords.Add((ArrayRecord)rec);
                        break;
                    case TableRecord.sid:
                        tableRecords.Add((TableRecord)rec);
                        break;
                    default:
                        plainRecords.Add(rec);
                        break;
                }
                prevRec = rec;
            }
            SharedFormulaRecord[] sharedFormulaRecs;
            List<ArrayRecord> arrayRecs;
            List<TableRecord> tableRecs;
            sharedFormulaRecs = shFrmRecords.ToArray();

            CellReference[] firstCells;
            firstCells = firstCellRefs.ToArray();
            arrayRecs = new List<ArrayRecord>(arrayRecords.ToArray());
            tableRecs = new List<TableRecord>(tableRecords.ToArray());

            _plainRecords = plainRecords;
            _sfm = SharedValueManager.Create(sharedFormulaRecs,firstCells, arrayRecs, tableRecs);
            _mergedCellsRecords = mergeCellRecords.ToArray();
        }

        /**
         * Some unconventional apps place {@link MergeCellsRecord}s within the row block.  They 
         * actually should be in the {@link MergedCellsTable} which is much later (see bug 45699).
         * @return any loose  <c>MergeCellsRecord</c>s found
         */
        public MergeCellsRecord[] LooseMergedCells
        {
            get
            {
                return _mergedCellsRecords;
            }
        }

        public SharedValueManager SharedFormulaManager
        {
            get
            {
                return _sfm;
            }
        }
        /**
         * @return a {@link RecordStream} containing all the non-{@link SharedFormulaRecord} 
         * non-{@link ArrayRecord} and non-{@link TableRecord} Records.
         */
        public RecordStream PlainRecordStream
        {
            get
            {
                return new RecordStream(_plainRecords, 0);
            }
        }
    }
}