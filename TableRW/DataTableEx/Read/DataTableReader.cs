using System.Data;
using TableRW.Read.I;
using TableRW.Read.I.DataTableEx;

namespace TableRW.Read.DataTableEx;


// There already exists a name `System.Data.DataTableReader`,
// which is prone to conflicts, so use `DataTblReader`
public class DataTblReader<TEntity> : DataTblReaderImpl<IContext<DataTable, TEntity>> { }

public class DataTblReader<TEntity, TData> : DataTblReaderImpl<IContext<DataTable, TEntity, TData>> { }
