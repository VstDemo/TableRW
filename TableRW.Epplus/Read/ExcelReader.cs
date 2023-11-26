
using OfficeOpenXml;
using TableRW.Read.I;
using TableRW.Read.I.Epplus;

namespace TableRW.Read.Epplus;

public class ExcelReader<TEntity> : ExcelReaderImpl<IContext<ExcelWorksheet, TEntity>> { }

public class ExcelReader<TEntity, TData> : ExcelReaderImpl<IContext<ExcelWorksheet, TEntity, TData>> { }
