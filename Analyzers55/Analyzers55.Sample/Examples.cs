// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

using System;

namespace Analyzers55.Sample;

// If you don't see warnings, build the Analyzers Project.
public class Ab5C  
{
    
}
public class Ea5a{
    public class MyCompanay23Class // Try to apply quick fix using the IDE.
    {
    }
    public class GoodE2xamp2le{}
    public void MetAhod1(){}
    public void ExamplMethod()
    {
        Console.WriteLine("badVar");
        var myComp = new MyCompanay23Class();
        var spaceship = new Spaceship();
        spaceship.SetSpeed(300000000); // Invalid value, it should be highlighted.
        spaceship.SetSpeed(42);
    }
}