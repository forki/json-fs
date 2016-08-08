namespace JsonFs

type Json = 
    | Bool of bool
    | Null of unit
    | Number of decimal
    | String of string
    | Array of Json list
    | Object of Map<string, Json>