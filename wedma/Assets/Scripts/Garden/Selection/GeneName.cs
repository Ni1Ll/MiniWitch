public static class GeneName
{
    public static string Ru(GeneType type)
    {
        switch (type)
        {
            // Field
            case GeneType.Vigor: return "Бодрость";
            case GeneType.Calm: return "Спокойствие";
            case GeneType.Recovery: return "Восстановление";
            case GeneType.Energy: return "Энергия";
            case GeneType.Endurance: return "Выносливость";
            case GeneType.Concentration: return "Концентрация";
            case GeneType.Fortitude: return "Стойкость";
            case GeneType.Clarity: return "Ясность";
            case GeneType.Assimilation: return "Усвоение";

            // SunMoon
            case GeneType.Acceleration: return "Ускорение";
            case GeneType.Slowdown: return "Замедление";
            case GeneType.FireResist: return "Огнестойкость";
            case GeneType.ColdResist: return "Хладостойкость";
            case GeneType.Cleansing: return "Очищение";
            case GeneType.Decay: return "Распад";
            case GeneType.Fermentation: return "Брожение";
            case GeneType.Sensitivity: return "Чувствительность";
            case GeneType.Stimulation: return "Стимуляция";

            // Noble
            case GeneType.Charm: return "Очарование";
            case GeneType.Oblivion: return "Забвение";
            case GeneType.Insight: return "Проницательность";
            case GeneType.Confidence: return "Уверенность";
            case GeneType.Charisma: return "Харизма";
            case GeneType.Fun: return "Веселье";
            case GeneType.Fortune: return "Удача";
            case GeneType.Inspiration: return "Вдохновение";
            case GeneType.Logic: return "Логика";

            // Spring
            case GeneType.Scaring: return "Отпугивание";
            case GeneType.Attraction: return "Притяжение";
            case GeneType.Toxic: return "Токсичность";
            case GeneType.Suppression: return "Подавление";
            case GeneType.Disorientation: return "Дезориентация";
            case GeneType.Stickiness: return "Липкость";
            case GeneType.Paralysis: return "Паралич";
            case GeneType.Distortion: return "Искажение";
            case GeneType.Masking: return "Маскировка";

            default: return type.ToString();
        }
    }
}