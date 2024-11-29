using System;

namespace Analyzers55.Sample;

public class Program2S
{
    public const int Not = 100;
    public const int YES = 200;
    static void Main(string[] args)
    {
        int carAnd2SDas2, good;
        carAnd2SDas2 = 5;
        good = 6;
        Console.WriteLine(good);
        Console.WriteLine(carAnd2SDas2);
        var foo = new Spaceship();
    }
}

// File 1
public static class Globals
{
    public const int GOOD = 5;
}



class BobFinder
{
    void Run() => Console.WriteLine(Globals.GOOD);
}

