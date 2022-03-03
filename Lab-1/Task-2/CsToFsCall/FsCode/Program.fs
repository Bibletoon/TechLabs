module FsProgram

open System
open System.Text.RegularExpressions

module Result =
    let apply s f = match f, s with
        | Ok f, Ok s -> Ok (f s)
        | Ok f, Error s -> Error s
        | Error f, Ok s -> Error f
        | Error f, Error s -> Error (f @ s)
    let pure = Ok

let (|Regex|_|) pattern input =
    let m = Regex.Match(input, pattern)
    if m.Success then seq m.Groups |> Seq.map (fun g -> g.Value) |> Seq.toList |>  List.tail |> Some
    else None

type Country =
    | Russia
    | USA
    | Canada

type UserDto = {
    Username : string
    Name : string
    Country : Country
}

type User = {
    Username : string
    FirstName : string
    LastName : string
    Country : Country
}

module User =
    let FirstName (user : User ) = user.FirstName

let validateName name =
    match name with
    | Regex "(^[A-Z]{1}[a-z]*) ([A-Z]{1}[a-z]*)$" [firstName; lastName] -> Ok (firstName, lastName)
    | _ -> Error "Name has wrong format"

let validateUsername username =
    match username with
    | Regex "^[A-Za-z0-9]+$" _ -> Ok username
    | _ -> Error "Username has wrong format"

let createUserFromDto userDto =
    let create (firstName, lastName) username country = {
        Username = username
        FirstName = firstName
        LastName = lastName
        Country = country}

    Ok create
    |> Result.apply (validateName userDto.Name |> Result.mapError List.singleton)
    |> Result.apply (validateUsername userDto.Username |> Result.mapError List.singleton)
    |> Result.apply (Result.Ok userDto.Country)

let dto = {
    Username = "bibletoon"
    Name = "Aboba Kekw"
    Country = Russia
}

match dto |> createUserFromDto |> Result.map User.FirstName |> Result.mapError (fun l ->  String.Join(",", l)) with
    | Ok res -> printfn $"Success %s{res}"
    | Error err -> printfn $"Error %s{err}"