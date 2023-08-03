# Simple usage

```csharp
var wildcard = StringManager.ExportWildcards(";[name]=[value]\n");

StringManager sm = new StringManager("@echo off\r\n\r\nfn main()\r\n{-\r\n\t; A=B\r\n-}\r\n");

sm.ApplyWildcards(wildcard, modifyWildcard =>
{
    // from, ; name=value
    // to  , set name=value
    var name = modifyWildcard.GetValue("name").Trim();
    var value = modifyWildcard.GetValue("value");
    
    modifyWildcard.Replace($"SET {name}={value}\n");
});

Console.WriteLine(sm.Text);
```
---
```rust
@echo off

fn main()
{-
        SET A=B
-}
```
