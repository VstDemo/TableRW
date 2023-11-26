using NPOI.SS.UserModel;
using E = System.Linq.Expressions.Expression;

namespace TableRW.Read.I.NPOI;

public class ExcelReaderImpl<C> : TableReaderImpl<C> {

    static ExcelReaderImpl() {
        ReadSource<ISheet>.Impl(
            ReadSrcValueByIndex,
            (src, iRow) => iRow > src.LastRowNum,
            (src, iRow, iCol) => iCol >= src.GetRow(iRow).LastCellNum 
                || src.GetRow(iRow).GetCell(iCol).CellType == CellType.Blank);
    }

    public static Expression ReadSrcValueByIndex(Expression ctx, Type valueType)
        => ConvertSrcValue(GetSrcValueByIndex(ctx), valueType);

    public static Expression GetSrcValueByIndex(Expression ctx)
        => Utils.Expr.ExtractBody((ISource<ISheet> ctx) =>
            ctx.Src.GetRow(ctx.iRow).GetCell(ctx.iCol), ctx);

    public static Expression ConvertSrcValue(Expression cell, Type valueType) {
        if (Nullable.GetUnderlyingType(valueType) is var vType and { }) {
            // cell == null || cell.CellType == CellType.Blank
            // ? null
            // : Nullalbe( (cell.CellValue) )
            return E.Condition(
                E.OrElse(
                    E.Equal(cell, E.Constant(null)),
                    E.Equal(E.Property(cell, nameof(ICell.CellType)), E.Constant(CellType.Blank))),
                E.Constant(null, valueType),
                E.Convert(ConvertSrcValue(cell, vType), valueType));
        }
        if (valueType == typeof(string)) {
            return E.Property(cell, nameof(ICell.StringCellValue));
        }
        if (valueType == typeof(bool)) {
            return E.Property(cell, nameof(ICell.BooleanCellValue));
        }

        if (valueType == typeof(DateTime)) {
            return E.Property(cell, nameof(ICell.DateCellValue));
        }
        if (valueType == typeof(DateTimeOffset)) {
            return E.Convert(E.Property(cell, nameof(ICell.DateCellValue)), valueType);
        }

        if (valueType == typeof(double)) {
            return E.Property(cell, nameof(ICell.NumericCellValue));
        }

        // #if net5, Half Type
        if (Type.GetTypeCode(valueType) is >= TypeCode.SByte and <= TypeCode.Decimal) {
            return E.Convert(E.Property(cell, nameof(ICell.NumericCellValue)), valueType);
        }

        throw new NotSupportedException($"Cell does not support this type({valueType}) of read.");
    }
}
