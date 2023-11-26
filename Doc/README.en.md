# TableRW
## Introduction ( English | [中文](../README.md) )
A library for reading and writing table data, using expression trees to generate delegates (Lambda), quickly and conveniently reading and writing data to entity objects (Entity), and mapping multi-layer entities to read and write.

(Currently the mapping for reading has been implemented, and development for writing has not yet started)

## Read from `DataTable` to Entity

### Add namespace
```cs
using TableRW.Read; // Read method
using TableRW.Read.DataTableEx; // DataTable extension method
```

### Simple reading (not cached)
```cs
public class Entity {
    public long Id { get; set; }
    public string Name;
    public string Tel; // it can be of a field
    public int? NullableInt { get; set; } // or a property
}

var reader = new DataTblReader<Entity>()
    .AddColumns((s, e) => s(e.Id, e.Name, e.Tel, e.NullableInt));

// When debugging, you can view the generated expression tree
var readLmd = reader.Lambda(); // Expression<Func<DataTable, List<Entity>>>
var readFn = readLmd.Compile(); // Func<DataTable, List<Entity>>
var list = readFn(tbl); // List<Entity>
```

### Reading with subtables (not cached)
``` cs
public class EntityA {
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public List<EntityB> SubB { get; set; }
}
public class EntityB {
    public int Id { get; set; }
    public string Text { get; set; }
    public string Remark { get; set; }
}
var reader2 = new DataTblReader<EntityA>()
    .AddColumns((s, e) => s(s.RowKey(e.Id), e.Name))
    .AddSubTable(e => e.SubList, (s, e) => s(e.Id, e.Text, e.Remark));

var readLmd = reader2.Lambda(); // Expression<Func<DataTable, List<EntityA>>>
var readFn = readLmd.Compile(); // Func<DataTable, List<EntityA>>

// table
// | 10  | name1 | 101  | text101 | remark101
// | 10  | name1 | 102  | text102 | remark102
// | 20  | name2 | 201  | text201 | remark201
var list = readFn(table); // List<EntityA>
_ = list.Count == 2;
_ = list[0].SubB.Count == 2;
// list[0].SubB[0] is EntityB { Id = 101, Text = "text101", Remark = "remark101" }
// list[0].SubB[1] is EntityB { Id = 102, Text = "text102", Remark = "remark102" }
_ = list[1].SubB.Count == 1;

```

### Cache Generated delegate
The `reader` above compiles the expression tree every time it is executed, and should actually cache the resulting `readFn` and call the delegate directly afterwards.
``` cs
// The user needs to create a new class to manage the Cache.
static class CacheReadFn<T> {
    internal static Func<DataTable, List<T>>? Fn;
}

static class CacheReadTbl {
    public static List<T> Read<T>(DataTable tbl, Action<DataTblReader<T>> buildRead) {
        if (CacheReadFn<T>.Fn == null) {
            var reader = new DataTblReader<T>();
            buildRead(reader);

            // When debugging, you can view the generated expression tree
            var readLmd = reader.Lambda();
            CacheReadFn<T>.Fn = readLmd.Compile();
        }
        return CacheReadFn<T>.Fn(tbl);
    }
}

var list = CacheReadTbl.Read<Entity>(table, reader => {
    reader.AddColumns((s, e) => s(e.Id, e.Name, e.Tel, e.NullableInt));
});
```

### Use the cache provided by the library
This library also has some simple encapsulation for user-friendly invocation:
``` cs
using TableRW.Read;
using TableRW.Read.DataTableEx; // DataTable extension method

void Example(DataTable tbl) {
    // Use the column name of the DataTable as the property mapping.
    // The column name and property name must be the same.
    var list1 = tbl.ReadToList<Entity>(); // List<Entity>

    var list2 = tbl.ReadToList<Entity>(cacheKey: 0, reader => {
        // Handle the mapping of properties and columns yourself
        reader.AddColumns((s, e) => s(e.Id, e.Name, e.Tel, e.NullableInt));

        // When debugging, you can view the generated expression tree
        var lmd = reader.Lambda();
        return lmd.Compile();
    });
}
```

### Events while reading
``` cs
static void Example(DataTable tbl) {
var list2 = tbl.ReadToList<Entity>(cacheKey: 0, reader => {
    reader.AddColumns((s, e) =>
        s(e.Id, e.Name, e.Tel))
        .OnStartReadingTable(it => {
            // Row.Count >= 100 will be read
            return it.Src.Rows.Count >= 100;
        })
        .OnStartReadingRow(it => {
            // If column 0 of row is DBNull, skip reading of row
            return it.SkipRow(it.Src.Rows[it.iRow][0] is DBNull);
        })
        .OnEndReadingRow(it => {
            // If entity.Id > 1000 then this row is skipped
            return it.SkipRow(it.Entity.Id > 1000);
        })
        .OnEndReadingTable(it => { });

    var lmd = reader.Lambda();
    return lmd.Compile();
});
}
```

### Adjust the generated Lambda
``` cs
var reader = new DataTblReader<Entity>()
    .AddColumns((s, e) => s(e.Id, e.Name, e.Tel, e.NullableInt));

// When debugging, you can view the generated expression tree
var lmd1 = reader.Lambda();
var fn1 = lmd1.Compile(); // Func<DataTable, List<Entity>>
fn1(table);


var lmd2 = reader.Lambda(f => f.StartRow());
var fn2 = lmd2.Compile(); // Func<DataTable, int, List<Entity>>
var startRow = 3; // Start reading from row 3
fn2(table, startRow);


var lmd3 = reader.Lambda(f => f.Start());
var fn3 = lmd3.Compile(); // Func<DataTable, int, int, List<Entity>>
(startRow, var startCol) = (3, 2); // Start reading from row 3, column 2
fn3(table, startRow, startCol);

var lmd4 = reader.Lambda(f => f.ToDictionary(entity => entity.Id));
var fn4 = lmd4.Compile(); // Func<DataTable, Dictionary<long, Entity>>
// Returns a Dictionary with entity.Id as key
var dic4 = fn4(table); // Dictionary<long, Entity>

// multiple combinations
var lmd5 = reader.Lambda(f => f.StartRow().ToDictionary(entity => entity.Id));
var fn5 = lmd5.Compile(); // Func<DataTable, int, int, Dictionary<long, Entity>>
startRow = 2;
var dic5 = fn5(table, startRow);
```

### More ways to read
```cs
static void Example(DataTable tbl) {
var list = tbl.ReadToList<Entity>(cacheKey: 0, reader => {
    var x = reader
        // Set the starting position to read
        .SetStart(row: 3, column: 2)
        // Add several column mapping reads
        .AddColumns((s, e) => s(e.Id, e.Name))
        // Skip 2 columns to read
        .AddSkipColumn(2)
        // Convert the value of this column to DateTime, and then execute a function
        .AddColumnRead((DateTime val) => it => {
            if (val.Year < 2000) {
                // If Year < 2000, skip reading this row
                return it.SkipRow();
            }
            it.Entity.Year = val.Year;
            return null; // No action to be done
        })
        //Add a few more columns to read
        .AddColumns((s, e) => s(e.Text1, e.Text2))
        // Execute an Action. There is no data column read here, and the entity can be processed.
        .AddActionRead(it => {
            it.Entity.Remark1 = it.Entity.Text1 + it.Entity.Text2;
            it.Entity.Remark2 = it.Entity.Id + " - " + it.Entity.Year;
        });


    var lmd = reader.Lambda();
    return lmd.Compile();
});
}
```

## Implementation of the `Epplus` library
[Code implementation (/TableRW.Epplus/Read/I/ExcelReaderImpl.cs)](/TableRW.Epplus/Read/I/ExcelReaderImpl.cs), it only takes less than 100 lines of code to have the abstraction mentioned above Read the insinuation.

There could be some specializations for the library, but there is currently no time to implement them.
### Add namespace
```cs
using TableRW.Read;
using TableRW.Read.EpplusEx;
```

### Simple usage example
```cs
using TableRW.Read;
using TableRW.Read.EpplusEx; // Epplus extension method

void Example(ExcelWorksheet sheet) {
     // headerRow specifies the first row as the column name as the attribute mapping, the column name and attribute name must be the same
     var list1 = sheet.ReadToList<Entity>(headerRow: 1); // List<Entity>

     var list2 = sheet.ReadToList<Entity>(cacheKey: 0, reader => {
         // Handle the mapping of attributes and columns yourself
         reader.AddColumns((s, e) => s(e.Id, e.Name, e.Tel, e.NullableInt));

         // When debugging, you can view the generated expression tree
         var lmd = reader.Lambda();
         return lmd.Compile();
     });
}
```

## Implementation of the `NPOI` library
[Code implementation (/TableRW.NPOI/Read/I/ExcelReaderImpl.cs)](/TableRW.NPOI/Read/I/ExcelReaderImpl.cs), it only takes less than 100 lines of code to have the abstraction mentioned above Read the insinuation.

There could be some specializations for the library, but there is currently no time to implement them.