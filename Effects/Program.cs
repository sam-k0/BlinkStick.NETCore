using BlinkStickNETCore;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


BlinkStickFlex bs = new BlinkStickFlex();

int counter = 0;
double preamp = 1;

byte[] Rainbow(int i)
{
    var r = (byte)(Math.Sin(i) * 127 + 128 * preamp);
    var g = (byte)(Math.Sin(i + 2) * 127 + 128 * preamp);
    var b = (byte)(Math.Sin(i + 4) * 127 + 128 * preamp);
    return [r,g,b];
}

byte[] SineWave(int i)
{
    var r = (byte)(Math.Sin(i) * 127 + 128 * preamp);

    return [r,0,0];
}

while(true)
{

    //for (int i = 0; i < bs.GetNumLeds(); i++)
    {
        var inp = Rainbow(counter);
        bs.SetColor(0, 0, inp);
    }

    counter  ++;
    Thread.Sleep(300);
    System.Console.WriteLine(counter);
}