namespace TableRW.Read.Epplus.Fs

open OfficeOpenXml
open TableRW.Read.Epplus
open TableRW
open TableRW.Utils.Ex
open TableRW.Read.Epplus.Fs
open System.Reflection
open System.Linq

module ExcelWorksheetEx =
    type ExcelWorksheet with
        member sheet.ReadToList<'TEntity> headerRow =
            let reader = new ExcelReader<'TEntity>()
            let t_entity = typeof<'TEntity>

            let props =
                t_entity.GetProperties().Where(fun p -> p.CanWrite).Cast<MemberInfo>()
                |> Seq.append (t_entity.GetFields().Where(fun f -> not f.IsInitOnly).Cast())
                |> Seq.filter (fun m -> not (m.HasAttribute<IgnoreReadAttribute>()))
                |> Seq.map (fun m -> m.Name, m) |> Map.ofSeq

            let header =
                { 1..sheet.Dimension.Columns }
                |> Seq.map (fun i -> i, sheet.Cells[headerRow, i].Text)
                |> Seq.map (fun (i, text) -> i, props.GetValueOr text null)
                |> Seq.filter (fun (i, m) -> m <> null)


            0
