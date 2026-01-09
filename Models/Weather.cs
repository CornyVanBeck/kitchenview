namespace kitchenview.Models;

public class Weather
{
    public double Latitude;

    public double Longitude;

    public double Elevation;

    public required WeatherDay Today;

    public required WeatherDay Tomorrow;

    public required WeatherDay DayAfterTomorrow;
}