namespace TableRW.Read.I.Epplus.Fs

open System
open System.Linq.Expressions
open TableRW
open TableRW.Read.I
open OfficeOpenXml

type E = Linq.Expressions.Expression


module internal ExcelReaderImpl =
    let GetSrcValueByIndex (ctx: Expression) =
        Utils.Expr.ExtractBody(
            (fun (c: ISource<ExcelWorksheet>) -> c.Src.Cells[c.iRow, c.iCol])
            , [| ctx |])


    let ConvertSrcValue (srcValue: Expression) (valueType: Type) : Expression =
       let range = Unchecked.defaultof<ExcelRange>

       if valueType = typeof<string>
       then E.Property(srcValue, nameof range.Text)
       else E.Call(srcValue, nameof range.GetValue, [| valueType |])


    let ReadSrcValueByIndex ctx valueType =
        ConvertSrcValue (GetSrcValueByIndex ctx) valueType

type ExcelReaderImpl<'C>() =
    inherit TableReaderImpl<'C>()

    static do
        ReadSource<ExcelWorksheet>.SetDefaultStart(1, 1)
        ReadSource<ExcelWorksheet>.Impl(
            ExcelReaderImpl.ReadSrcValueByIndex
            , fun src iRow -> iRow > src.Dimension.Rows
            , fun src iRow iCol -> src.Cells[iRow, iCol].Value = null)

