﻿using OfficeOpenXml;
using TableRW.Read;
using TableRW.Read.DataTableEx;

namespace TableRW.Read.Epplus.Tests;

public class ExcelWorksheetExTest : IDisposable {
    public void Dispose() => _excel.Dispose();
    readonly ExcelPackage _excel;
    readonly ExcelWorksheet _sheet;
    readonly ExcelWorksheet _sheetEmpty;

    /// <summary>
    /// FieldStr, FieldInt, Str, StructInt, NullableInt
    /// </summary>
    readonly List<RecordA> _entitySrc;
    const int HeaderRow = 3;
    public ExcelWorksheetExTest() {
        _excel = new ExcelPackage(new MemoryStream(2 * 1024));

        _entitySrc = new() {
            new() { FieldStr = "fs1", FieldInt = 11, Str = "str1", StructInt = 12, NullableInt = null },
            new() { FieldStr = "fs2", FieldInt = 21, Str = "str2", StructInt = 22, NullableInt = 23 }
        };

        var (e0, e1) = (_entitySrc[0], _entitySrc[1]);
        _sheet = _excel.Workbook.Worksheets.Add("sheet1");
        _sheet.WriteCells((HeaderRow, 1), [
            ["FieldStr", "FieldInt", "Str", "StructInt", "NullableInt"],
            [e0.FieldStr, e0.FieldInt, e0.Str, e0.StructInt, e0.NullableInt],
            [e1.FieldStr, e1.FieldInt, e1.Str, e1.StructInt, e1.NullableInt],
        ]);

        _sheetEmpty = _excel.Workbook.Worksheets.Add("sheet2");
    }

    void CkeckDataCount(ICollection<RecordA> data) {
        Assert.Equal(_entitySrc.Count, data.Count);
    }

    void CkeckData(IEnumerable<RecordA> data) {
        foreach (var (test, origin) in data.Zip(_entitySrc)) {
            test.TestIgnoreWrite();
            Assert.Equal(origin.FieldStr, test.FieldStr);
            Assert.Equal(origin.FieldInt, test.FieldInt);
            Assert.Equal(origin.Str, test.Str);
            Assert.Equal(origin.StructInt, test.StructInt);
            Assert.Equal(origin.NullableInt, test.NullableInt);
        }
    }



    [Fact]
    public void ReadToList_UseHeader() {
        var list = _sheet.ReadToList<RecordA>(HeaderRow);
        CkeckDataCount(list);
        CkeckData(list);
    }

    [Fact]
    public void ReadToList_UseHeader_EmptyTable() {
        Assert.Throws<ArgumentNullException>(() => {
            var list = _sheetEmpty.ReadToList<RecordA>(HeaderRow);
        });
    }

    [Fact]
    public void ReadToList_UseReader() {
        var list = _sheet.ReadToList<RecordA>(cacheKey: 0, reader => {
            reader.SetStart(HeaderRow + 1, 1).AddColumns((s, e) =>
                s(e.FieldStr, e.FieldInt, e.Str, e.StructInt, e.NullableInt));

            var lmd = reader.Lambda();
            return lmd.Compile();
        });
        CkeckDataCount(list);
        CkeckData(list);
    }

    [Fact]
    public void ReadToList_UseReader_AnotherKey() {
        var list = _sheet.ReadToList<RecordA>(cacheKey: 1, reader => {
            reader.SetStart(HeaderRow + 1, 1).AddColumns((s, e) =>
                s(e.FieldStr, e.FieldInt, e.Str, e.StructInt, e.NullableInt))
                .OnEndReadingRow(it => {
                    it.Entity.FieldInt *= 1000;
                });

            var lmd = reader.Lambda();
            return lmd.Compile();
        });

        CkeckDataCount(list);

        foreach (var (test, origin) in list.Zip(_entitySrc)) {
            Assert.Equal(origin.FieldStr, test.FieldStr);
            Assert.Equal(origin.FieldInt *= 1000, test.FieldInt);
            Assert.Equal(origin.Str, test.Str);
            Assert.Equal(origin.StructInt, test.StructInt);
            Assert.Equal(origin.NullableInt, test.NullableInt);
        }
    }

    [Fact]
    public void ReadToList_UseReader_WithData() {
        var list = _sheet.ReadToList<RecordA, (int, int)>(cacheKey: 0, reader => {
            reader.SetStart(HeaderRow + 1, 1).AddColumns((s, e) =>
                s(e.FieldStr, e.FieldInt, e.Str, e.StructInt, e.NullableInt));

            var lmd = reader.Lambda();
            return lmd.Compile();
        });
        CkeckDataCount(list);
        CkeckData(list);
    }

}
