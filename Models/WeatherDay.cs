namespace kitchenview.Models;
/*
    0	            Clear sky
    1, 2, 3	        Mainly clear, partly cloudy, and overcast
    45, 48	        Fog and depositing rime fog
    51, 53, 55	    Drizzle: Light, moderate, and dense intensity
    56, 57	        Freezing Drizzle: Light and dense intensity
    61, 63, 65	    Rain: Slight, moderate and heavy intensity
    66, 67	        Freezing Rain: Light and heavy intensity
    71, 73, 75	    Snow fall: Slight, moderate, and heavy intensity
    77	            Snow grains
    80, 81, 82	    Rain showers: Slight, moderate, and violent
    85, 86	        Snow showers slight and heavy
    95 *	        Thunderstorm: Slight or moderate
    96, 99 *	    Thunderstorm with slight and heavy hail
*/
public enum WeatherCode
{
    CLEAR_SKY = 0,
    MAINLY_CLEAR = 1,
    PARTLY_CLOUDY = 2,
    OVERCAST = 3,
    FOG = 45,
    DEPOSITING_RIME_FOG = 48,
    DRIZZLE_LIGHT = 51,
    DRIZZLE_MODERATE = 53,
    DRIZZLE_DENSE = 55,
    FREEZING_DRIZZLE_LIGHT = 56,
    FREEZING_DRIZZLE_DENSE = 57,
    RAIN_SLIGHTLY = 61,
    RAIN_LIGHT = 63,
    RAIN_HEAVY = 65,
    FREEZING_RAIN_LIGHT = 66,
    FREEZING_RAIN_HEAVY = 67,
    SNOW_FALL_LIGHT = 71,
    SNOW_FALL_MODERATE = 73,
    SNOW_FALL_HEAVY = 75,
    SNOW_GRAINS = 77,
    RAIN_SHOWERS_SLIGHT = 80,
    RAIN_SHOWERS_MODERATE = 81,
    RAIN_SHOWERS_VIOLENT = 82,
    SNOW_SHOWERS_SLIGHT = 88,
    SNOW_SHOWERS_HEAVY = 86,
    THUNDERSTORM_SLIGHT = 95,
    THUNDERSTORM_SLIGHT_HAIL = 96,
    THUNDERSTORM_HEAVY_HAIL = 99,
    UNSET = 100
}

public class WeatherDay
{
    public float Temperature;

    public float Pressure;

    public float Windspeed;

    public int WindDirection;

    public WeatherCode WeatherCode;

    public int PrecipitationProbability;
}