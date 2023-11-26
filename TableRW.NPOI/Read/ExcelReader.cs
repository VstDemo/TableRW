
using NPOI.SS.UserModel;
using TableRW.Read.I;
using TableRW.Read.I.NPOI;

namespace TableRW.Read.NPOI;

public class ExcelReader<TEntity> : ExcelReaderImpl<IContext<ISheet, TEntity>> { }

public class ExcelReader<TEntity, TData> : ExcelReaderImpl<IContext<ISheet, TEntity, TData>> { }
