namespace CrudBot.Main.Helpers;

public static class TemperatureConverter
{
    private const float NegativeCoefficient = 273.15F;

    //C = K - 273.15
    public static float ConvertKelvinToTemperature(float temperature)
    {
        return temperature - NegativeCoefficient;
    }
}