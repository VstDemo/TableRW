namespace TableRW.Read.Epplus.Fs

open TableRW.Read.I
open TableRW.Read.I.Epplus.Fs
open OfficeOpenXml
open System.Collections.Generic

type ExcelReader<'TEntity>() =
    inherit ExcelReaderImpl<IContext<ExcelWorksheet, 'TEntity>>()

type ExcelReader<'TEntity, 'TData>() =
    inherit ExcelReaderImpl<IContext<ExcelWorksheet, 'TEntity, 'TData>>()

//[<AutoOpen>]
//module SystemEx2 =
//    let FilterMap seq =
//        seq |> Seq.filter fst |> Seq.map snd
    
//    type IDictionary<'k, 'v> with 
//        member dic.GetValueOr (k, orValue) =
//            match dic.TryGetValue(k) with
//            | (true, value) -> value
//            | _ -> orValue