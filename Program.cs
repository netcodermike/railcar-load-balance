using System.Text.Json;

try
{
    if (args.Length != 1) throw new ArgumentException("Pass one JSON input file");
    using var data = JsonDocument.Parse(File.ReadAllText(args[0]));
    Console.WriteLine(JsonSerializer.Serialize(Analyze(data.RootElement)));
    return 0;
}
catch (Exception error) when (error is IOException or JsonException or FormatException
                               or ArgumentException or InvalidOperationException
                               or KeyNotFoundException or OverflowException)
{
    Console.Error.WriteLine(error.Message);
    return 2;
}

static object Analyze(JsonElement input)
{
    decimal left = 0, right = 0;
    decimal limit = input.GetProperty("maxAxle").GetDecimal();
    if (limit <= 0) throw new ArgumentException("Axle limit must be positive");
    foreach (var item in input.GetProperty("cargo").EnumerateArray())
    {
        decimal weight = item.GetProperty("weight").GetDecimal();
        decimal position = item.GetProperty("position").GetDecimal();
        if (weight < 0 || position < 0 || position > 1)
            throw new ArgumentException("Invalid weight or support position");
        left += weight * (1 - position);
        right += weight * position;
    }
    return new { total = left + right, left, right, imbalance = Math.Abs(left-right), overLimit = left > limit || right > limit };
}
