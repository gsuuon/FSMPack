module FSMPack.Compile.Generator.Enum

open System
open FSMPack.Format
open FSMPack.Compile.Generator.Common

let generateFormatEnum (enumType: Type) =
    let names = TypeName.getGeneratorNames enumType
    let underlyingType = Enum.GetUnderlyingType enumType

    let underlyingTypeAsField = {
        typ = underlyingType
        name = ""
        typeFullName = TypeName.field underlyingType
    }

    $"""type {names.formatTypeNamedArgs}() =
{__}interface Format<{names.dataTypeNamedArgs}> with
{__}{__}member _.Write bw (v: {names.dataTypeNamedArgs}) =
{__}{__}{__}let ev = LanguagePrimitives.EnumToValue v
{__}{__}{__}{getWriteFieldCall underlyingTypeAsField "ev"}
{__}{__}member _.Read (br, bytes) = 
{__}{__}{__}{getReadFieldCall underlyingTypeAsField "ev"}
{__}{__}{__}LanguagePrimitives.EnumOfValue<{underlyingTypeAsField.typeFullName}, {names.dataType}> ev
"""
