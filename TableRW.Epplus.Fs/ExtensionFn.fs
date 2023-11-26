namespace TableRW

open System.Collections.Generic
open OfficeOpenXml
open System.Linq

[<AutoOpen>]
module SystemEx =

    let defVal<'a> = Unchecked.defaultof<'a>

    type IEnumerable<'a> with
        member src.CastConcat src2 = src.Cast<'b>().Concat(src2)


    type Option<'a> with
        member x.OrDefult() = match x with Some(v) -> v | _ -> Unchecked.defaultof<'a>

    type IDictionary<'k, 'v> with
        member dic.GetValueOr key orValue =
            match dic.TryGetValue(key) with
            | (true, value) -> value
            | _ -> orValue
        // member dic.GetValueOr key = dic.GetValueOr(key, defVal)


