using System.Diagnostics;
using Clee.Text;


void test()
{
    Stopwatch sw = Stopwatch.StartNew();
    StringManager sm = new StringManager("@echo off\r\n\r\nfn main()\r\n{-\r\n\t; A=B\r\n-}\r\n");

    // sm.OnLog += Console.WriteLine;

    var wildcard = StringManager.ExportWildcards(";[name]=[value]\n");

    sm.ApplyWildcards(wildcard, modifyWildcard =>
    {
        var name = modifyWildcard.GetValue("name").Trim();
        var value = modifyWildcard.GetValue("value");
    
        modifyWildcard.Replace($"SET {name}={value}\n");
    }, wildcardName: "test card");

    sm.Dispose();

    // Console.WriteLine(sm.Text);
    Console.WriteLine(sw.Elapsed.TotalMilliseconds+"ms");
}

for (int i = 0;i < 10;i++)
    test();
