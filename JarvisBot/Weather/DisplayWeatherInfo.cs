namespace JarvisBot.Weather
{
    public class DisplayWeatherInfo
    {

        public static string DisplayFactInformation(string displayInfo)
        {
            return displayInfo switch
            {
                "night" => "Ночью 🌙",
                "morning" => "Утром 🌅",
                "day" => "Днем ☀️",
                "evening" => "Вечером 🌆",

                "clear" => "Ясно ☀️",
                "partly - cloudy" => "Малооблачно 🌤",
                "cloudy" => "Облачно с прояснениями 🌥",
                "overcast" => "Пасмурно ☁",
                "light-rain" => "Небольшой дождь 🌦",
                "rain" => "Дождь 🌧",
                "heavy-rain" => "Сильный дождь 🌧",
                "showers" => "Ливень 🌧",
                "wet-snow" => "Дождь со снегом 🌨",
                "light-snow" => "Небольшой снег 🌨",
                "snow" => "Снег ❄",
                "snow-showers" => "Снегопад 🌨",
                "hail" => "Град 🌨",
                "thunderstorm" => "Гроза 🌩",
                "thunderstorm-with-rain" => "Дождь с грозой ⛈",
                "thunderstorm-with-hail" => "Гроза с градом ⛈",

                "nw" => "Северо - западный ↖️",
                "n" => "Северный ⬆️",
                "ne" => "Северо - восточный ↗️",
                "e" => "Восточный ➡️",
                "se" => "Юго - восточный ↘️",
                "s" => "Южный ⬇️",
                "sw" => "Юго - западный ↙️",
                "w" => "Западный ⬅️",
                "c" => "Штиль",


                _ => "!!! значение не найдено !!!"
            };
        }

        public static string DetermineWindStrengthCategory(double windSpeed)
        {
            string category;

            if (windSpeed >= 0 && windSpeed < 6)
            {
                category = "слабый";
            }
            else if (windSpeed >= 6 && windSpeed < 15)
            {
                category = "умеренный";
            }
            else if (windSpeed >= 15 && windSpeed < 25)
            {
                category = "СИЛЬНЫЙ";
            }
            else if (windSpeed >= 25 && windSpeed < 33)
            {
                category = "ОЧЕНЬ СИЛЬНЫЙ";
            }
            else if (windSpeed >= 33)
            {
                category = "УРАГАННЫЙ !!!";
            }
            else
            {
                category = "Некорректная скорость ветра";
            }

            return category;
        }

    }
}
